using CounterStrikeSharp.API.Core;

using Microsoft.Extensions.DependencyInjection;

using VipTags.Authorization;
using VipTags.Managers;

namespace VipTags;

public sealed class VipTagsServices : IPluginServiceCollection<VipTagsPlugin>
{
    public void ConfigureServices(IServiceCollection serviceCollection) => serviceCollection
        .AddSingleton<EventManager>()
        .AddSingleton<CommandManager>()
        .AddSingleton<TagsManager>()
        .AddSingleton<DatabaseManager>()
        .AddSingleton<AuthorizationComputer>()
        .AddSingleton<PlayerModelCache>();
}