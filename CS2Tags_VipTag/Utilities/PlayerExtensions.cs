using System.Diagnostics.CodeAnalysis;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;

namespace VipTags.Utilities;

internal static class PlayerExtensions
{
    public static async Task SafeColoredPrintToChat(this CCSPlayerController player, string message)
    {
        await Server.NextFrameAsync(() => player.PrintToChat(message
            .ReplaceColorTags()
            .Replace("{TeamColor}", ChatColors.ForTeam(player.Team).ToString())));
    }

    public static bool IsRealAuthorizedPerson([NotNullWhen(true)] this CCSPlayerController? player) =>
        player is { IsBot: false, IsHLTV: false, AuthorizedSteamID: not null };
}