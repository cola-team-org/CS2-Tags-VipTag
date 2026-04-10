using System.Drawing;

using CounterStrikeSharp.API.Modules.Utils;

namespace VipTags.Utilities;

public static class TagColors
{
    public static readonly string[] Colors =
    [
        "TeamColor",
        "White",
        "DarkRed",
        "Green",
        "LightYellow",
        "LightBlue",
        "Olive",
        "Lime",
        "Red",
        "LightPurple",
        "Purple",
        "Grey",
        "Yellow",
        "Gold",
        "Silver",
        "Blue",
        "DarkBlue",
        "BlueGrey",
        "Magenta",
        "LightRed",
        "Orange",
    ];

    private static readonly Dictionary<string, string> PredefinedColors = new()
    {
        { "BlueGrey", "#B1C4D9" },
        { "Grey", "#C6CBD0" },
        { "LightPurple", "#BB82F0" },
        { "LightRed", "#EB4C4C" },
    };

    public static string ComputeColorHex(string name, CsTeam team)
    {
        // TODO: find exact colors...
        if (name == "TeamColor")
        {
            name = GetTeamColorName(team);
        }

        if (PredefinedColors.TryGetValue(name, out string? hex))
        {
            return hex;
        }

        Color color = Color.FromName(name);
        if (!color.IsKnownColor) throw new ArgumentException($"{name} is not a known color",  nameof(name));

        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
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