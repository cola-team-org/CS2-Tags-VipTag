using System.Collections.Frozen;
using System.Globalization;
using System.Text;

using CounterStrikeSharp.API.Modules.Utils;

namespace VipTags.Utilities;

public static class TagColors
{
    private static readonly FrozenDictionary<string, string> ColorMap = (new Dictionary<string, string>() {
        { "White", "#FFFFFF" },
        { "DarkRed", "#E24D1B" },
        { "Green", "#8DF664" },
        { "LightYellow", "#E8E386" },
        { "LightBlue", "#7095D3" },
        { "Olive", "#CDF9A0" },
        { "Lime", "#BDF868" },
        { "Red", "#E36246" },
        { "LightPurple", "#AF87EA" },
        { "Purple", "#BD50DF" },
        { "Grey", "#C5C9CE" },
        { "Yellow", "#EAE588" },
        { "Gold", "#D7B150" },
        { "Silver", "#B5C1D6" },
        { "Blue", "#7095D3" },
        { "DarkBlue", "#576AF6" },
        { "BlueGrey", "#B5C1D6" },
        { "Magenta", "#BD50DF" },
        { "LightRed", "#D2644F" },
        { "Orange", "#D8B350" },
    }).ToFrozenDictionary();

    // Keep the internal LightPurple preview for spectator TeamColor only.
    public static readonly string[] Colors = ColorMap.Keys
        .Where(name => name != "LightPurple")
        .Prepend("TeamColor")
        .ToArray();

    /// <summary>
    /// Special tag color: every letter gets a random color, in a different order on every message.
    /// Only available to players with the custom tag permission (!settag).
    /// </summary>
    public const string Rainbow = "Rainbow";

    // Only colors with a distinct in-game chat code (Orange==Gold, Purple==Magenta, Blue==LightBlue,
    // LightPurple is the team color), so two neighbouring letters never look the same.
    private static readonly string[] RainbowPalette =
    [
        "Red", "LightRed", "Orange", "Yellow", "Lime", "Green", "Olive", "LightBlue", "DarkBlue", "Purple",
    ];

    public static bool IsRainbow(string? name) =>
        string.Equals(name, Rainbow, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Colors each letter of <paramref name="text"/> with a chat color tag (e.g. "{Red}H{Lime}i").
    /// Colors are drawn from a shuffled palette, so the order changes every call and two
    /// consecutive letters never share a color. Whitespace is kept as-is.
    /// </summary>
    public static string Rainbowify(string text, Random? random = null) =>
        Colorize(text, random, (color, letter) => $"{{{color}}}{letter}");

    /// <summary>Same as <see cref="Rainbowify"/> but with HTML font tags, for center/menu HUD.</summary>
    public static string RainbowifyHtml(string text, Random? random = null) =>
        Colorize(text, random, (color, letter) => $"<font color='{ColorMap[color]}'>{letter}</font>");

    private static string Colorize(string text, Random? random, Func<string, string, string> format)
    {
        random ??= Random.Shared;

        var builder = new StringBuilder(text.Length * 8);
        string[] order = [];
        var index = 0;
        string? last = null;

        // Text elements so emojis / multi-byte characters are not split in half.
        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            var letter = enumerator.GetTextElement();

            if (string.IsNullOrWhiteSpace(letter))
            {
                builder.Append(letter);
                continue;
            }

            if (index >= order.Length)
            {
                order = ShufflePalette(random, last);
                index = 0;
            }

            var color = order[index++];
            builder.Append(format(color, letter));
            last = color;
        }

        return builder.ToString();
    }

    private static string[] ShufflePalette(Random random, string? avoidFirst)
    {
        var palette = RainbowPalette.ToArray();
        random.Shuffle(palette);

        // Avoid repeating the previous letter's color when a new round of the palette starts.
        if (avoidFirst is not null && palette[0] == avoidFirst)
        {
            (palette[0], palette[^1]) = (palette[^1], palette[0]);
        }

        return palette;
    }

    public static string? WrapChatColor(string? name) => name is null ? null : $"{{{name}}}";

    public static bool IsGreen(string? name) =>
        string.Equals(name, "Green", StringComparison.OrdinalIgnoreCase);

    public static string ComputeColorHex(string name, CsTeam team)
    {
        if (name == "TeamColor")
        {
            name = GetTeamColorName(team);
        }

        return ColorMap.GetValueOrDefault(name) ??
               throw new ArgumentException($"{name} is not a known color", nameof(name));
    }

    private static string GetTeamColorName(CsTeam team) => team switch
    {
        CsTeam.None => "White",
        CsTeam.Spectator => "LightPurple",
        CsTeam.CounterTerrorist => "LightBlue",
        CsTeam.Terrorist => "Orange",
        _ => throw new ArgumentException($"Invalid team: ${team}")
    };
}