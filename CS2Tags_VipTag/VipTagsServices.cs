using CounterStrikeSharp.API.Core;

using Microsoft.Extensions.DependencyInjection;

using VipTags.Authorization;
using VipTags.Managers;

namespace VipTags;

public sealed class VipTagsServices : IPluginServiceCollection<VipTagsPlugin>
{
    public void ConfigureServices(IServiceCollection serviceCollection) => serviceCollection
        .AddTransient<EventManager>()
        .AddTransient<CommandManager>()
        .AddTransient<TagsManager>()
        .AddTransient<DatabaseManager>()
        .AddTransient<AuthorizationComputer>()
        .AddSingleton<PlayerModelCache>();
}