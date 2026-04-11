using System.Diagnostics.CodeAnalysis;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace VipTags;

internal static class PlayerExtensions
{
    public static async Task SafePrintToChat(this CCSPlayerController player, string message)
    {
        await Server.NextFrameAsync(() => player.PrintToChat(message));
    }

    public static bool IsRealAuthorizedPerson([NotNullWhen(true)] this CCSPlayerController? player) =>
        player is { IsBot: false, IsHLTV: false, AuthorizedSteamID: not null };
}