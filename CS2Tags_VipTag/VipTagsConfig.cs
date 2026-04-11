using System.Text.Json.Serialization;

using CounterStrikeSharp.API.Core;

namespace VipTags;

public sealed class VipTagsConfig : BasePluginConfig
{
    [JsonPropertyName("Vip_SetTagFlag")] public string VipSetTagFlag { get; set; } = "@vip/vipsettag";
    [JsonPropertyName("Vip_TagColorFlag")] public string VipTagColorFlag { get; set; } = "@vip/tagcolor";
    [JsonPropertyName("Vip_ChatColorFlag")] public string VipChatColorFlag { get; set; } = "@vip/chatcolor";
    [JsonPropertyName("Vip_NameColorFlag")] public string VipNameColorFlag { get; set; } = "@vip/namecolor";
    [JsonPropertyName("DBHost")] public string DbHost { get; set; } = "localhost";
    [JsonPropertyName("DBPort")] public uint DbPort { get; set; } = 3306;
    [JsonPropertyName("DBUsername")] public string DbUsername { get; set; } = "root";
    [JsonPropertyName("DBName")] public string DbName { get; set; } = "db_";
    [JsonPropertyName("DBPassword")] public string DbPassword { get; set; } = "123";
    public bool CustomTagOnScoreboard { get; set; } = true;

}