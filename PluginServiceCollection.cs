using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.DependencyInjection;

namespace CS2Tags_VipTag;

public sealed class PluginServiceCollection : IPluginServiceCollection<CS2Tags_VipTag>
{
    public void ConfigureServices(IServiceCollection serviceCollection) => serviceCollection
        .AddSingleton<PlayerModelCache>();
}