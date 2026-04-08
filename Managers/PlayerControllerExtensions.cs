using CounterStrikeSharp.API.Core;

namespace CS2Tags_VipTag;

public static class PlayerControllerExtensions
{
    public static ulong GetAuthorizedSteamId(this CCSPlayerController player) =>
        player.AuthorizedSteamID?.SteamId64 ?? throw new InvalidOperationException("Player was not authorized");
}