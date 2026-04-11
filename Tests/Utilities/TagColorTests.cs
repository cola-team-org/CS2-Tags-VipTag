using CounterStrikeSharp.API.Modules.Utils;

using VipTags.Utilities;

namespace Tests.Utilities;

public sealed class TagColorTests
{
    [Test]
    public async Task AllColorsHaveHexValue()
    {
        using (Assert.Multiple())
        {
            await Assert.That(TagColors.Colors).Count().IsEqualTo(21);
            foreach (var colorName in TagColors.Colors)
            {
                var hex = TagColors.ComputeColorHex(colorName, CsTeam.Terrorist);
                await Assert.That(hex).IsNotNullOrWhiteSpace()
                    .And.StartsWith("#")
                    .And.Length().IsEqualTo(7);
            }
        }
    }

    [Test]
    public async Task TeamColorRespectsTeam()
    {
        using (Assert.Multiple())
        {
            await Assert.That(TagColors.ComputeColorHex("TeamColor", CsTeam.None)).IsEqualTo("#FFFFFF");
            await Assert.That(TagColors.ComputeColorHex("TeamColor", CsTeam.Spectator)).IsEqualTo("#AF87EA");
            await Assert.That(TagColors.ComputeColorHex("TeamColor", CsTeam.Terrorist)).IsEqualTo("#D8B350");
            await Assert.That(TagColors.ComputeColorHex("TeamColor", CsTeam.CounterTerrorist)).IsEqualTo("#7095D3");
        }
    }
}