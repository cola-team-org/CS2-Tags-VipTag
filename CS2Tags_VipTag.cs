using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagsApi;

namespace CS2Tags_VipTag;

public class CS2Tags_VipTag(IServiceProvider serviceProvider) : BasePlugin, IPluginConfig<TagConfig>
{
    public override string ModuleName => "CS2Tags_VipTag";
    public override string ModuleVersion => "0.4.1";
    public override string ModuleAuthor => "Letaryat";
    public override string ModuleDescription => "Tag change for vip players";
    internal ITagApi _tagApi = null!;
    public required TagConfig Config { get; set; }

    internal List<string> Colors =
        [
        "TeamColor", "White", "DarkRed", "Green", "LightYellow", "LightBlue", "Olive", "Lime", "Red", "LightPurple", "Purple", "Grey", "Yellow", "Gold", "Silver", "Blue","DarkBlue", "BlueGrey", "Magenta", "LightRed", "Orange"
        ];
    public override void Load(bool hotReload)
    {
        _ = serviceProvider.GetRequiredService<DatabaseManager>().InitializeConnection(); // 🤮
        serviceProvider.GetRequiredService<EventManager>().InitializeEvents();
        serviceProvider.GetRequiredService<CommandManager>().InitializeCommands();

        Logger.LogInformation("CS2Tags_VipTag - Loaded");

    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _tagApi = ITagApi.Capability.Get() ?? throw new Exception("Tags API not found!");
    }

    public override void Unload(bool hotReload)
    {
        Logger.LogInformation("CS2Tags_VipTag - Unloaded");
        _ = serviceProvider.GetRequiredService<DatabaseManager>().SaveAllTags();
        
    }
    public void OnConfigParsed(TagConfig config)
    {
        Config = config;
    }

}
