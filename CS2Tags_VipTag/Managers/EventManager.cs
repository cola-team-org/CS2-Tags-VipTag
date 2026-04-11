using CounterStrikeSharp.API.Core;

using VipTags.Authorization;

namespace VipTags.Managers;

public class EventManager(
    VipTagsPlugin plugin,
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
        Task.Run(() => tagsManager.ReloadSettings(authorizationContext));
        return HookResult.Continue;
    }
}