using CounterStrikeSharp.API.Core;

using Microsoft.Extensions.DependencyInjection;

using VipTags.Managers;

namespace VipTags;

public sealed class VipTagsPlugin(IServiceProvider serviceProvider) : BasePlugin, IPluginConfig<VipTagsConfig>
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

        Task.Run(async () =>
        {
            await serviceProvider.GetRequiredService<DatabaseManager>().InitializeConnection();
            await serviceProvider.GetRequiredService<TagsManager>().ReloadAllSettings();
        });
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        serviceProvider.GetRequiredService<TagsManager>().Initialize();
    }

    public override void Unload(bool hotReload)
    {
        serviceProvider.GetRequiredService<TagsManager>().Uninitialize();
    }

    public void OnConfigParsed(VipTagsConfig config)
    {
        Config = config;
    }

}