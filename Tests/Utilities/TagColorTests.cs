using CounterStrikeSharp.API.Modules.Utils;

using VipTags.Utilities;

namespace Tests.Utilities;

public sealed class TagColorTests
{
    [Test]
    public void AllColorsHaveHexValue()
    {
        foreach (var colorName in TagColors.Colors)
        {
            var hex = TagColors.ComputeColorHex(colorName, CsTeam.Terrorist);
            Assert.NotNull(hex);
        }
    }

    [Test]
    public async Task TeamColorRespectsTeam()
    {
        using (Assert.Multiple())
        {
            await Assert.That(TagColors.ComputeColorHex("TeamColor", CsTeam.None)).IsEqualTo("#FFFFFF");
            await Assert.That(TagColors.ComputeColorHex("TeamColor", CsTeam.Spectator)).IsEqualTo("#BB82F0");
            await Assert.That(TagColors.ComputeColorHex("TeamColor", CsTeam.Terrorist)).IsEqualTo("#FFA500");
            await Assert.That(TagColors.ComputeColorHex("TeamColor", CsTeam.CounterTerrorist)).IsEqualTo("#ADD8E6");
        }
    }
}