using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

using Microsoft.Extensions.Logging;

using VipTags.Models;

namespace VipTags.Managers;

public class EventManager(
    VipTagsPlugin plugin,
    ILogger<EventManager> logger,
    DatabaseManager databaseManager,
    TagsManager tagsManager,
    PlayerModelCache playerModelCache)
{
    public void InitializeEvents()
    {
        plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
        plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    }

    /*
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        try
        {
            var player = @event.Userid;
            if (player == null || player.IsBot || player.IsHLTV) return HookResult.Continue;
            var steamid64 = player!.AuthorizedSteamID!.SteamId64;
            if (!_plugin.Players.ContainsKey(steamid64)) return HookResult.Continue;
            if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.VipFlag)) return HookResult.Continue;
            //var VipTag = $" {_plugin.Players[steamid64]!.tag}";
            if (_plugin.Players[steamid64]!.visibility == false) { return HookResult.Continue; }
            //_plugin._tagApi?.SetAttribute(player!, Tags.TagType.ScoreTag, VipTag);

            _plugin.TagsManager!.SetEverythingTagRelated(player, 0);

        }
        catch (Exception ex)
        {
            _plugin.Logger.LogInformation($"On_plugin.Playerspawn - {ex}");
        }

        return HookResult.Continue;
    }

    */
    private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        try
        {
            var player = @event.Userid;
            if (player == null || player.IsBot || player.IsHLTV || player.AuthorizedSteamID == null) return HookResult.Continue;
            var steamid64 = player.GetAuthorizedSteamId();

            if (!AdminManager.PlayerHasPermissions(player, plugin.Config.VipBaseFlag)) return HookResult.Continue;
            Task.Run(async () =>
            {
                try
                {
                    await OnClientAuthorizedAsync(steamid64);

                    //if (!_plugin.Players.ContainsKey(steamid64)) return;

                    Server.NextFrame(() =>
                    {
                        var model = playerModelCache.Get(steamid64);

                        if (model is null) return;

                        tagsManager.ApplyTags(player, model);
                    });

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to fetch tags on connect");
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OnPlayerConnectFull failed");
        }
        return HookResult.Continue;
    }


    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        try
        {
            var player = @event.Userid;

            if (player == null || player.IsBot || player.IsHLTV || player.AuthorizedSteamID is null)
                return HookResult.Continue;

            var model = playerModelCache.Get(player.GetAuthorizedSteamId());

            if (model is null) return HookResult.Continue;

            if (!AdminManager.PlayerHasPermissions(player, plugin.Config.VipBaseFlag))
                return HookResult.Continue;

            Task.Run(async () =>
            {
                try
                {
                    logger.LogInformation("Saving player {SteamId} into DB", model.SteamId);
                    await databaseManager.SaveTags(model.SteamId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Saving to DB failed");
                }
                finally
                {
                    playerModelCache.Clear(model.SteamId);
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "OnPlayerDisconnect failed");
        }

        return HookResult.Continue;
    }


    private async Task OnClientAuthorizedAsync(ulong steamid)
    {
        var user = await databaseManager.FetchPlayerInfo(steamid);
        if (user == null) return;

        playerModelCache.Set(steamid, new TagSettings
        {
            SteamId = user!.SteamId,
            Tag = user.Tag,
            TagColor = user.TagColor,
            NameColor = user.NameColor,
            ChatColor = user.ChatColor,
        });
    }
}