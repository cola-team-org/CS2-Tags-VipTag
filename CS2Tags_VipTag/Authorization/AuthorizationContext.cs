using CounterStrikeSharp.API.Core;

namespace VipTags.Authorization;

public class AuthorizationContext
{
    public required CCSPlayerController Player { get; set; }
    public required ulong SteamId { get; set; }
    public required bool CanSetCustomTag { get; init; }
    public required bool CanSetChatColor { get; init; }
    public required bool CanSetNameColor { get; init; }
    public required bool CanSetTagColor { get; init; }
    public bool CanSetAnything => CanSetChatColor || CanSetNameColor || CanSetTagColor || CanSetCustomTag;
}