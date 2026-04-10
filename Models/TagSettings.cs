using System.ComponentModel.DataAnnotations.Schema;

namespace VipTags.Models;

public class TagSettings // TODO: why so nullable?
{
    [Column("steamid")]
    public required ulong SteamId { get; set; }
    [Column("tag")]
    public required string Tag { get; set; }
    [Column("tagcolor")]
    public string? TagColor { get; set; }
    [Column("namecolor")]
    public string? NameColor { get; set; }
    [Column("chatcolor")]
    public string? ChatColor { get; set; }
    // TODO: remove below
    [Column("visibility")]
    public bool? Visibility { get; set; }
    [Column("chatvisibility")]
    public bool? ChatVisibility { get; set; }
    [Column("scorevisibility")]
    public bool? ScoreVisibility { get; set; }
}