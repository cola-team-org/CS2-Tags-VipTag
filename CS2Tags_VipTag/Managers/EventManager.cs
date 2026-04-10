using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

using Microsoft.Extensions.Logging;

using VipTags.Authorization;
using VipTags.Models;

namespace VipTags.Managers;

public class EventManager(
    VipTagsPlugin plugin,
    ILogger<EventManager> logger,
    DatabaseManager databaseManager,
    AuthorizationComputer authorizationComputer,
    TagsManager tagsManager,
    PlayerModelCache playerModelCache)
{
    public void InitializeEvents()
    {
        plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
        plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
    }

    private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        try
        {
            var player = @event.Userid;
            if (player == null || player.IsBot || player.IsHLTV || player.AuthorizedSteamID == null) return HookResult.Continue;

            var authorizationContext = authorizationComputer.ComputeAuthorizationContext(player);

            if (!authorizationContext.CanSetAnything) return HookResult.Continue;
            Task.Run(async () =>
            {
                try
                {
                    await OnClientAuthorizedAsync(authorizationContext.SteamId);

                    //if (!_plugin.Players.ContainsKey(steamid64)) return;

                    Server.NextFrame(() =>
                    {
                        var model = playerModelCache.Get(authorizationContext.SteamId);

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

            var authorizationContext = authorizationComputer.ComputeAuthorizationContext(player);
            var model = playerModelCache.Get(authorizationContext.SteamId);

            if (model is null || !authorizationContext.CanSetAnything) return HookResult.Continue;

            Task.Run(async () =>
            {
                try
                {
                    logger.LogInformation("Saving player {SteamId} into DB", model.SteamId);
                    await databaseManager.SaveTags(model);
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
            SteamId = user.SteamId,
            Tag = user.Tag,
            TagColor = user.TagColor,
            NameColor = user.NameColor,
            ChatColor = user.ChatColor,
        });
    }
}