using CounterStrikeSharp.API.Core;

using Microsoft.Extensions.Logging;

using VipTags.Authorization;

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
        var player = @event.Userid;
        if (player == null || player.IsBot || player.IsHLTV || player.AuthorizedSteamID == null)
            return HookResult.Continue;

        var authorizationContext = authorizationComputer.ComputeAuthorizationContext(player);

        if (!authorizationContext.CanSetAnything) return HookResult.Continue;
        Task.Run(() => tagsManager.ReloadSettings(authorizationContext));
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
}