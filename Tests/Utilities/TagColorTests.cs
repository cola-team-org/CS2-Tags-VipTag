using System.Text.RegularExpressions;

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
            await Assert.That(TagColors.Colors).Count().IsEqualTo(20);
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

    [Test]
    public async Task RainbowColorsEveryLetterAndKeepsText()
    {
        const string tag = "[VIP cOLa]";
        var result = TagColors.Rainbowify(tag);
        var colorTags = Regex.Matches(result, @"\{(\w+)\}").Select(m => m.Groups[1].Value).ToArray();

        using (Assert.Multiple())
        {
            await Assert.That(Regex.Replace(result, @"\{\w+\}", "")).IsEqualTo(tag);
            await Assert.That(colorTags.Length).IsEqualTo(tag.Count(c => !char.IsWhiteSpace(c)));
            for (var i = 1; i < colorTags.Length; i++)
            {
                await Assert.That(colorTags[i]).IsNotEqualTo(colorTags[i - 1]);
            }
        }
    }

    [Test]
    public async Task RainbowOrderIsRandom()
    {
        const string tag = "RAINBOWTAG";
        var results = Enumerable.Range(0, 20).Select(_ => TagColors.Rainbowify(tag)).Distinct().Count();
        await Assert.That(results).IsGreaterThan(1);
    }
}
