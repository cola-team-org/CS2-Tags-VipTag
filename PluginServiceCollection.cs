using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CS2Tags_VipTag;

public sealed class PluginServiceCollection : IPluginServiceCollection<CS2Tags_VipTag>
{
    public void ConfigureServices(IServiceCollection serviceCollection) => serviceCollection
        .AddTransient<EventManager>()
        .AddTransient<CommandManager>()
        .AddTransient<MenuManager>()
        .AddTransient<TagsManager>()
        .AddSingleton<DatabaseManager>()
        .AddSingleton<PlayerModelCache>();
}