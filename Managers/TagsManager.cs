using CounterStrikeSharp.API.Core;

using TagsApi;

using VipTags.Models;

using static TagsApi.Tags;

namespace VipTags.Managers;

public sealed class TagsManager
{
    private ITagApi? _tagApiCache;

    private ITagApi TagApi
    {
        get
        {
            _tagApiCache ??= ITagApi.Capability.Get() ?? throw new InvalidOperationException("Tags API not found!");

            return _tagApiCache;
        }
    }

    public void ApplyTags(CCSPlayerController player, TagSettings settings)
    {
        // TODO: compute permissions somewhere
        // TODO: run in server frame?
        TagApi.ResetAttribute(player, TagType.ScoreTag | TagType.ChatTag | TagType.NameColor | TagType.ChatColor);

        if (settings.Visibility != true)
        {
            return;
        }

        if (settings.Tag is not null)
        {
            var colorPrefix = settings.TagColor is not null ? $"{{{settings.TagColor}}}" : "";
            var tagWithColor = $"{colorPrefix}{settings.Tag} ";

            if (settings.ChatVisibility == true)
            {
                TagApi.SetAttribute(player, TagType.ChatTag, tagWithColor);
            }

            if (settings.ScoreVisibility == true)
            {
                TagApi.SetAttribute(player, TagType.ScoreTag, tagWithColor);
            }
        }



        SetColorIfPresent(player, TagType.NameColor,  settings.NameColor);
        SetColorIfPresent(player, TagType.ChatColor,  settings.ChatColor);
    }

    private void SetColorIfPresent(CCSPlayerController player, TagType type, string? color)
    {
        if (color is not null)
        {
            TagApi.SetAttribute(player, type, $"{{{color}}}");
        }
    }
}