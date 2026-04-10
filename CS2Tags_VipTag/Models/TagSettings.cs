using System.ComponentModel.DataAnnotations.Schema;

namespace VipTags.Models;

public class TagSettings // TODO: why so nullable?
{
    [Column("steamid")]
    public required ulong SteamId { get; set; }
    [Column("tag")]
    public string? Tag { get; set; }
    [Column("tagcolor")]
    public string? TagColor { get; set; }
    [Column("namecolor")]
    public string? NameColor { get; set; }
    [Column("chatcolor")]
    public string? ChatColor { get; set; }
}