using CounterStrikeSharp.API.Core;

using Microsoft.Extensions.Logging;

using VipTags.Authorization;
using VipTags.Utilities;

namespace VipTags.Managers;

public class EventManager(
    VipTagsPlugin plugin,
    ILogger<EventManager> logger,
    AuthorizationComputer authorizationComputer,
    TagsManager tagsManager)
{
    public void InitializeEvents()
    {
        plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
    }

    private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsRealAuthorizedPerson())
            return HookResult.Continue;

        var authorizationContext = authorizationComputer.ComputeAuthorizationContext(player);

        if (!authorizationContext.CanSetAnything) return HookResult.Continue;
        AsyncHelpers.RunWithErrorLogging(logger, () => tagsManager.ReloadSettings(authorizationContext));
        return HookResult.Continue;
    }
}