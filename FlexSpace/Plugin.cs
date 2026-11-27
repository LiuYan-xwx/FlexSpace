using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlexSpace;

[PluginEntrance]
public sealed class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        services.AddComponent<FlexSpaceComponent, FlexSpaceSettingsControl>();
    }
}
