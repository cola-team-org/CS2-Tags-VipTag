using CounterStrikeSharp.API.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using TagsApi;

using VipTags.Managers;

namespace VipTags;

public class VipTagsPlugin(IServiceProvider serviceProvider) : BasePlugin, IPluginConfig<VipTagsConfig>
{
    public override string ModuleName => "CS2Tags_VipTag";
    public override string ModuleVersion => "0.4.1";
    public override string ModuleAuthor => "Letaryat";
    public override string ModuleDescription => "Tag change for vip players";
    internal ITagApi TagApi = null!;
    public required VipTagsConfig Config { get; set; }

    internal readonly List<string> Colors =
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
        TagApi = ITagApi.Capability.Get() ?? throw new InvalidOperationException("Tags API not found!");
    }

    public override void Unload(bool hotReload)
    {
        Logger.LogInformation("CS2Tags_VipTag - Unloaded");
        _ = serviceProvider.GetRequiredService<DatabaseManager>().SaveAllTags();

    }
    public void OnConfigParsed(VipTagsConfig config)
    {
        Config = config;
    }

}