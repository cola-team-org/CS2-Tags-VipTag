using System.Collections.Frozen;

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