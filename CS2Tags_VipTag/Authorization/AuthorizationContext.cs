using CounterStrikeSharp.API.Core;

using VipTags.Models;
using VipTags.Utilities;

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

    /// <summary>Rainbow tag color is reserved for players with the custom tag permission (!settag).</summary>
    public bool CanSetRainbowTag => CanSetCustomTag;

    public bool CanUpdate(ColorType colorType) => colorType switch
    {
        ColorType.TagColor => CanSetTagColor,
        ColorType.NameColor => CanSetNameColor,
        ColorType.ChatColor => CanSetChatColor,
        _ => throw new ArgumentOutOfRangeException(nameof(colorType), colorType, null)
    };

    /// <summary>Whether the color selector for <paramref name="colorType"/> can be opened at all.</summary>
    public bool CanOpenColorMenu(ColorType colorType) =>
        CanUpdate(colorType) || (colorType == ColorType.TagColor && CanSetRainbowTag);

    /// <summary>Whether the player may set <paramref name="colorType"/> to <paramref name="color"/> (null = reset).</summary>
    public bool CanSetColor(ColorType colorType, string? color)
    {
        if (color is null) return CanOpenColorMenu(colorType);

        if (TagColors.IsRainbow(color)) return colorType == ColorType.TagColor && CanSetRainbowTag;

        return CanUpdate(colorType);
    }
}