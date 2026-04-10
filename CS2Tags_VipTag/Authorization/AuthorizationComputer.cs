using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace VipTags.Authorization;

public class AuthorizationComputer(VipTagsPlugin plugin)
{
    public AuthorizationContext ComputeAuthorizationContext(CCSPlayerController player)
    {
        return new AuthorizationContext
        {
            Player = player,
            SteamId = player.AuthorizedSteamID?.SteamId64 ??
                      throw new InvalidOperationException($"Player {player.PlayerName} not authorized"),
            CanSetCustomTag = AdminManager.PlayerHasPermissions(player,
                plugin.Config.VipSetTagFlag),
            CanSetChatColor = AdminManager.PlayerHasPermissions(player,
                plugin.Config.VipChatColorFlag),
            CanSetNameColor = AdminManager.PlayerHasPermissions(player,
                plugin.Config.VipNameColorFlag),
            CanSetTagColor = AdminManager.PlayerHasPermissions(player,
                plugin.Config.VipTagColorFlag),
        };
    }
}