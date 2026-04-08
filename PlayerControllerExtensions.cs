using CounterStrikeSharp.API.Core;

namespace VipTags;

public static class PlayerControllerExtensions
{
    public static ulong GetAuthorizedSteamId(this CCSPlayerController player) =>
        player.AuthorizedSteamID?.SteamId64 ?? throw new InvalidOperationException("Player was not authorized");
}