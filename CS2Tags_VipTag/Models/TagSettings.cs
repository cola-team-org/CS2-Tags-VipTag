namespace VipTags.Models;

public class TagSettings
{
    public required ulong SteamId { get; set; }
    public string? Tag { get; set; }
    public string? TagColor { get; set; }
    public string? NameColor { get; set; }
    public string? ChatColor { get; set; }
}