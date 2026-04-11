using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

using Microsoft.Extensions.Logging;

using TagsApi;

using VipTags.Authorization;
using VipTags.Models;

using static TagsApi.Tags;

namespace VipTags.Managers;

public sealed class TagsManager(
    VipTagsPlugin plugin,
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

    public async Task UpdateTag(AuthorizationContext authorizationContext, string? tag)
    {
        if (!authorizationContext.CanSetCustomTag)
            throw new UnauthorizedAccessException("Player does not have permission to update tag");
        await UpdateAndApply(authorizationContext, settings => settings.Tag = tag);
    }

    public async Task UpdateColor(AuthorizationContext authorizationContext, ColorType colorType, string? color)
    {
        if (!authorizationContext.CanUpdate(colorType))
            throw new UnauthorizedAccessException("Player does not have permission to update tag color");

        await UpdateAndApply(authorizationContext, settings => settings.UpdateColor(colorType, color));
    }

    public async Task DeleteSettings(AuthorizationContext authorizationContext)
    {
        playerModelCache.Clear(authorizationContext.SteamId);
        await databaseManager.DeleteTags(authorizationContext.SteamId);
    }

    public async Task ReloadSettings(AuthorizationContext authorizationContext)
    {
        var settings = await databaseManager.FetchPlayerInfo(authorizationContext.SteamId);

        if (settings is null)
        {
            playerModelCache.Clear(authorizationContext.SteamId);
            return;
        }

        var authorizedSettings = ApplyAuthorization(settings, authorizationContext);
        playerModelCache.Set(authorizationContext.SteamId, authorizedSettings);
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

    public void Initialize()
    {
        logger.LogInformation("Installed message pre-processor into Tags API");
        TagApi.OnMessageProcessPre += MessagePreProcessor;
    }

    public void Uninitialize()
    {
        logger.LogInformation("Uninstalled message pre-processor from Tags API");
        TagApi.OnMessageProcessPre -= MessagePreProcessor;
    }

    private HookResult MessagePreProcessor(MessageProcess messageProcess)
    {
        var steamId = messageProcess.Player.AuthorizedSteamID?.SteamId64;

        if (steamId is null) return HookResult.Continue;

        var settings = playerModelCache.Get(steamId.Value);

        if (settings is null) return HookResult.Continue;

        messageProcess.Tag.ChatColor = WrapColor(settings.ChatColor) ?? messageProcess.Tag.ChatColor;
        messageProcess.Tag.NameColor = WrapColor(settings.NameColor) ?? messageProcess.Tag.NameColor;
        messageProcess.Tag.ChatTag = AddSpaceToEnd(settings.Tag) ?? messageProcess.Tag.ChatTag;

        if (settings.TagColor is not null && messageProcess.Tag.ChatTag is not null)
        {
            var tagWithColor = $"{{{settings.TagColor}}}{RemoveColorTags(messageProcess.Tag.ChatTag)}{{TeamColor}}";
            messageProcess.Tag.ChatTag = tagWithColor;
        }

        return HookResult.Continue;
    }

    private static readonly string[] Colors = typeof(ChatColors).GetFields().Select(f => f.Name).ToArray();

    private static string RemoveColorTags(string message)
    {
        var modifiedValue = message;
        foreach (var name in Colors)
        {
            string pattern = $"{{{name}}}";
            if (modifiedValue.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                modifiedValue = modifiedValue.Replace(pattern, string.Empty, StringComparison.OrdinalIgnoreCase);
            }
        }

        return modifiedValue.Equals(message) ? message : modifiedValue;
    }

    private static string? WrapColor(string? color) => color is null ? null : $"{{{color}}}";

    private static string? AddSpaceToEnd(string? text) => text is null ? null : $"{text} ";

    // private async Task ApplySettings(CCSPlayerController player, TagSettings settings)
    // {
    //     await Server.NextFrameAsync(() =>
    //     {
    //         TagApi.ResetAttribute(player, TagType.ScoreTag | TagType.ChatTag | TagType.NameColor | TagType.ChatColor);
    //
    //         if (settings.Tag is not null)
    //         {
    //             var colorPrefix = settings.TagColor is not null ? $"{{{settings.TagColor}}}" : "";
    //             var tagWithColor = $"{colorPrefix}{settings.Tag}{{White}} ";
    //             TagApi.SetAttribute(player, TagType.ChatTag, tagWithColor);
    //             TagApi.SetAttribute(player, TagType.ScoreTag, settings.Tag);
    //         }
    //
    //         SetColorIfPresent(player, TagType.NameColor,  settings.NameColor);
    //         SetColorIfPresent(player, TagType.ChatColor,  settings.ChatColor);
    //     });
    // }
    //
    // private void SetColorIfPresent(CCSPlayerController player, TagType type, string? color)
    // {
    //     if (color is not null)
    //     {
    //         TagApi.SetAttribute(player, type, $"{{{color}}}");
    //     }
    // }
    // private async Task ResetSettings(CCSPlayerController player)
    // {
    //     await Server.NextFrameAsync(() =>
    //     {
    //         TagApi.ResetAttribute(player, TagType.ScoreTag | TagType.ChatTag | TagType.NameColor | TagType.ChatColor);
    //     });
    // }

    private async Task UpdateAndApply(AuthorizationContext authorizationContext, Action<TagSettings> action)
    {
        var model = playerModelCache.Get(authorizationContext.SteamId) ??
                    playerModelCache.Set(authorizationContext.SteamId,
                        new TagSettings { SteamId = authorizationContext.SteamId });
        action(model);
        await databaseManager.SaveTags(model);
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