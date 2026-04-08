using CounterStrikeSharp.API.Core;

using Microsoft.Extensions.DependencyInjection;

using VipTags.Managers;

namespace VipTags;

public sealed class VipTagsServices : IPluginServiceCollection<VipTagsPlugin>
{
    public void ConfigureServices(IServiceCollection serviceCollection) => serviceCollection
        .AddTransient<EventManager>()
        .AddTransient<CommandManager>()
        .AddTransient<MenuManager>()
        .AddTransient<TagsManager>()
        .AddSingleton<DatabaseManager>()
        .AddSingleton<PlayerModelCache>();
}