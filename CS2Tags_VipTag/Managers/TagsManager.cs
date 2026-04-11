using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

using Microsoft.Extensions.Logging;

using TagsApi;

using VipTags.Authorization;
using VipTags.Models;

using static TagsApi.Tags;

namespace VipTags.Managers;

public sealed class TagsManager(
    ILogger<TagsManager> logger,
    DatabaseManager databaseManager,
    AuthorizationComputer authorizationComputer,
    PlayerModelCache playerModelCache)
{
    private ITagApi? _tagApiCache;

    private ITagApi TagApi
    {
        get
        {
            _tagApiCache ??= ITagApi.Capability.Get() ?? throw new InvalidOperationException("Tags API not found!");

            return _tagApiCache;
        }
    }

    public async Task UpdateTag(AuthorizationContext authorizationContext, string tag)
    {
        if (!authorizationContext.CanSetCustomTag)
            throw new UnauthorizedAccessException("Player does not have permission to update tag");
        await UpdateAndApply(authorizationContext, settings => settings.Tag = tag);
    }

    public async Task UpdateTagColor(AuthorizationContext authorizationContext, string color)
    {
        if (!authorizationContext.CanSetTagColor)
            throw new UnauthorizedAccessException("Player does not have permission to update tag color");
        await UpdateAndApply(authorizationContext, settings => settings.TagColor = color);
    }

    public async Task UpdateNameColor(AuthorizationContext authorizationContext, string color)
    {
        if (!authorizationContext.CanSetNameColor)
            throw new UnauthorizedAccessException("Player does not have permission to update name color");
        await UpdateAndApply(authorizationContext, settings => settings.NameColor = color);
    }

    public async Task UpdateChatColor(AuthorizationContext authorizationContext, string color)
    {
        if (!authorizationContext.CanSetChatColor)
            throw new UnauthorizedAccessException("Player does not have permission to update chat color");
        await UpdateAndApply(authorizationContext, settings => settings.ChatColor = color);
    }

    public async Task DeleteSettings(AuthorizationContext authorizationContext)
    {
        playerModelCache.Clear(authorizationContext.SteamId);
        await databaseManager.DeleteTags(authorizationContext.SteamId);
        await ResetSettings(authorizationContext.Player);
    }

    public async Task ReloadSettings(AuthorizationContext authorizationContext)
    {
        var settings = await databaseManager.FetchPlayerInfo(authorizationContext.SteamId);

        if (settings is null)
        {
            playerModelCache.Clear(authorizationContext.SteamId);
            await ResetSettings(authorizationContext.Player);
            return;
        }

        var authorizedSettings = ApplyAuthorization(settings, authorizationContext);
        playerModelCache.Set(authorizationContext.SteamId, authorizedSettings);
        await ApplySettings(authorizationContext.Player, authorizedSettings);
    }

    public async Task ReloadAllSettings()
    {
        AuthorizationContext[] contexts = [];
        await Server.NextFrameAsync(() =>
        {
            contexts = CounterStrikeSharp.API.Utilities.GetPlayers()
                .Where(p => p.IsRealAuthorizedPerson())
                .Select(authorizationComputer.ComputeAuthorizationContext)
                .ToArray();
        });

        foreach (var context in contexts)
        {
            await ReloadSettings(context);
        }
    }

    // TODO: consider using message processor instead
    private async Task ApplySettings(CCSPlayerController player, TagSettings settings)
    {
        await Server.NextFrameAsync(() =>
        {
            TagApi.ResetAttribute(player, TagType.ScoreTag | TagType.ChatTag | TagType.NameColor | TagType.ChatColor);

            if (settings.Tag is not null)
            {
                var colorPrefix = settings.TagColor is not null ? $"{{{settings.TagColor}}}" : "";
                var tagWithColor = $"{colorPrefix}{settings.Tag} ";
                // TODO: fix issue with colors bleeding over
                TagApi.SetAttribute(player, TagType.ChatTag, tagWithColor);
                TagApi.SetAttribute(player, TagType.ScoreTag, settings.Tag);
            }

            SetColorIfPresent(player, TagType.NameColor,  settings.NameColor);
            SetColorIfPresent(player, TagType.ChatColor,  settings.ChatColor);
        });
    }

    private async Task ResetSettings(CCSPlayerController player)
    {
        await Server.NextFrameAsync(() =>
        {
            TagApi.ResetAttribute(player, TagType.ScoreTag | TagType.ChatTag | TagType.NameColor | TagType.ChatColor);
        });
    }

    private void SetColorIfPresent(CCSPlayerController player, TagType type, string? color)
    {
        if (color is not null)
        {
            TagApi.SetAttribute(player, type, $"{{{color}}}");
        }
    }

    private async Task UpdateAndApply(AuthorizationContext authorizationContext, Action<TagSettings> action)
    {
        var model = playerModelCache.Get(authorizationContext.SteamId) ??
                    playerModelCache.Set(authorizationContext.SteamId,
                        new TagSettings { SteamId = authorizationContext.SteamId });
        action(model);
        await databaseManager.SaveTags(model);
        await ApplySettings(authorizationContext.Player, model);
    }

    private TagSettings ApplyAuthorization(TagSettings settings, AuthorizationContext authorizationContext)
    {
        var newSettings = new TagSettings
        {
            SteamId = settings.SteamId,
            Tag = authorizationContext.CanSetCustomTag ? settings.Tag : null,
            TagColor = authorizationContext.CanSetTagColor ? settings.TagColor : null,
            NameColor = authorizationContext.CanSetNameColor ? settings.NameColor : null,
            ChatColor = authorizationContext.CanSetChatColor ? settings.ChatColor : null,
        };

        if (!settings.Equals(newSettings))
        {
            logger.LogWarning(
                "Tag settings for {SteamId} was changed during authorization from {Before} to {After}",
                authorizationContext.SteamId, settings, newSettings);
        }

        return newSettings;
    }
}