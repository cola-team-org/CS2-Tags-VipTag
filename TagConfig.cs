using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace CS2Tags_VipTag;

public sealed class TagConfig : BasePluginConfig
{
    [JsonPropertyName("Vip_BaseFlag")] public string VipBaseFlag { get; set; } = "@vip/vipbaseflag";
    [JsonPropertyName("Vip_SetTagFlag")] public string VipSetTagFlag { get; set; } = "@vip/vipsettag";
    [JsonPropertyName("Vip_ToggleMenuFlag")] public string VipToggleMenuFlag { get; set; } = "@vip/viptogglemenu";
    [JsonPropertyName("Vip_ScoreboardFlag")] public string VipScoreboardFlag { get; set; } = "@vip/scoreboardflag";
    [JsonPropertyName("Vip_ChatFlag")] public string VipChatFlag { get; set; } = "@vip/chatflag";
    [JsonPropertyName("Vip_TagColorFlag")] public string VipTagColorFlag { get; set; } = "@vip/tagcolor";
    [JsonPropertyName("Vip_ChatColorFlag")] public string VipChatColorFlag { get; set; } = "@vip/chatcolor";
    [JsonPropertyName("Vip_NameColorFlag")] public string VipNameColorFlag { get; set; } = "@vip/namecolor";
    [JsonPropertyName("DBHost")] public string DbHost { get; set; } = "localhost";
    [JsonPropertyName("DBPort")] public uint DbPort { get; set; } = 3306;
    [JsonPropertyName("DBUsername")] public string DbUsername { get; set; } = "root";
    [JsonPropertyName("DBName")] public string DbName { get; set; } = "db_";
    [JsonPropertyName("DBPassword")] public string DbPassword { get; set; } = "123";

}