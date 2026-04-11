namespace VipTags.Models;

public class TagSettings
{
    public required ulong SteamId { get; set; }
    public string? Tag { get; set; }
    public string? TagColor { get; set; }
    public string? NameColor { get; set; }
    public string? ChatColor { get; set; }

    public bool Equals(TagSettings other)
    {
        return SteamId == other.SteamId && Tag == other.Tag && TagColor == other.TagColor && NameColor == other.NameColor && ChatColor == other.ChatColor;
    }

    public override string ToString()
    {
        return
            $"{nameof(SteamId)}: {SteamId}, {nameof(Tag)}: {Tag}, {nameof(TagColor)}: {TagColor}, {nameof(NameColor)}: {NameColor}, {nameof(ChatColor)}: {ChatColor}";
    }
}