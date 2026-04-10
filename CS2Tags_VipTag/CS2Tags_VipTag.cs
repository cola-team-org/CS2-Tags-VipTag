using CounterStrikeSharp.API.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using VipTags.Managers;

namespace VipTags;

// TODO: follow up on reload admins during map breaking tags.

public class VipTagsPlugin(IServiceProvider serviceProvider) : BasePlugin, IPluginConfig<VipTagsConfig>
{
    public override string ModuleName => "CS2Tags_VipTag";
    public override string ModuleVersion => "0.4.1";
    public override string ModuleAuthor => "Letaryat";
    public override string ModuleDescription => "Tag change for vip players";
    public required VipTagsConfig Config { get; set; }
    public override void Load(bool hotReload)
    {
        serviceProvider.GetRequiredService<EventManager>().InitializeEvents();
        serviceProvider.GetRequiredService<CommandManager>().InitializeCommands();

        Logger.LogInformation("CS2Tags_VipTag - Loaded");

        Task.Run(() => serviceProvider.GetRequiredService<DatabaseManager>().InitializeConnection());

    }

    public override void Unload(bool hotReload)
    {
        Logger.LogInformation("CS2Tags_VipTag - Unloaded");
        // TODO: save right away?
        _ = serviceProvider.GetRequiredService<DatabaseManager>().SaveAllTags();

    }
    public void OnConfigParsed(VipTagsConfig config)
    {
        Config = config;
    }

}