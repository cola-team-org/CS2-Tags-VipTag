using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TagsApi;
using Microsoft.Extensions.DependencyInjection;

namespace CS2Tags_VipTag;

public class TagConfig : BasePluginConfig
{
    [JsonPropertyName("Vip_BaseFlag")] public string Vip_BaseFlag { get; set; } = "@vip/vipbaseflag";
    [JsonPropertyName("Vip_SetTagFlag")] public string VipSetTagFlag { get; set; } = "@vip/vipsettag";
    [JsonPropertyName("Vip_ToggleMenuFlag")] public string VipToggleMenuFlag { get; set; } = "@vip/viptogglemenu";
    [JsonPropertyName("Vip_ScoreboardFlag")] public string VipScoreboardFlag { get; set; } = "@vip/scoreboardflag";
    [JsonPropertyName("Vip_ChatFlag")] public string VipChatFlag { get; set; } = "@vip/chatflag";
    [JsonPropertyName("Vip_TagColorFlag")] public string VipTagColorFlag { get; set; } = "@vip/tagcolor";
    [JsonPropertyName("Vip_ChatColorFlag")] public string VipChatColorFlag { get; set; } = "@vip/chatcolor";
    [JsonPropertyName("Vip_NameColorFlag")] public string VipNameColorFlag { get; set; } = "@vip/namecolor";
    [JsonPropertyName("DBHost")] public string DBHost { get; set; } = "localhost";
    [JsonPropertyName("DBPort")] public uint DBPort { get; set; } = 3306;
    [JsonPropertyName("DBUsername")] public string DBUsername { get; set; } = "root";
    [JsonPropertyName("DBName")] public string DBName { get; set; } = "db_";
    [JsonPropertyName("DBPassword")] public string DBPassword { get; set; } = "123";

}

public class CS2Tags_VipTag(IServiceProvider serviceProvider) : BasePlugin, IPluginConfig<TagConfig>
{
    public override string ModuleName => "CS2Tags_VipTag";
    public override string ModuleVersion => "0.4.1";
    public override string ModuleAuthor => "Letaryat";
    public override string ModuleDescription => "Tag change for vip players";
    internal ITagApi _tagApi = null!;
    public required TagConfig Config { get; set; }

    internal DatabaseManager? DatabaseManager { get; private set; }
    private EventManager? EventManager { get; set; }
    internal MenuManager? MenuManager { get; private set; }
    private CommandManager? CmdManager { get; set; }
    internal TagsManager? TagsManager { get; private set; }
    internal List<string> Colors =
        [
        "TeamColor", "White", "DarkRed", "Green", "LightYellow", "LightBlue", "Olive", "Lime", "Red", "LightPurple", "Purple", "Grey", "Yellow", "Gold", "Silver", "Blue","DarkBlue", "BlueGrey", "Magenta", "LightRed", "Orange"
        ];
    public override void Load(bool hotReload)
    {
        var playerManager = serviceProvider.GetRequiredService<PlayerModelCache>();
        
        DatabaseManager = new DatabaseManager(this, playerManager);
        EventManager = new EventManager(this, playerManager);
        MenuManager = new MenuManager(this, playerManager);
        CmdManager = new CommandManager(this, playerManager);
        TagsManager = new TagsManager(this, playerManager);

        _ = DatabaseManager.InitializeConnection(); // TODO: make this wait properly
        EventManager.InitializeEvents();
        CmdManager.InitializeCommands();

        Logger.LogInformation("CS2Tags_VipTag - Loaded");

    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _tagApi = ITagApi.Capability.Get() ?? throw new Exception("Tags API not found!");
    }

    public override void Unload(bool hotReload)
    {
        Logger.LogInformation("CS2Tags_VipTag - Unloaded");
        if(DatabaseManager != null)
        {
            _ = DatabaseManager!.SaveAllTags();
        }
        
    }
    public void OnConfigParsed(TagConfig config)
    {
        Config = config;
    }

}
