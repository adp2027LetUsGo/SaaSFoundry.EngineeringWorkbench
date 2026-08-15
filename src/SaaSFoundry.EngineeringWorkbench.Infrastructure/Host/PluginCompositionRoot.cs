using SaaSFoundry.EngineeringWorkbench.PluginRuntime.Registration;
using SaaSFoundry.Plugins.Observability.Plugin;
using SaaSFoundry.Plugins.API.Plugin;
using SaaSFoundry.Plugins.Authentication.Plugin;
using SaaSFoundry.Plugins.BackgroundProcessing.Plugin;
using SaaSFoundry.Plugins.Persistence.Plugin;
using SaaSFoundry.Plugins.Import;

namespace SaaSFoundry.EngineeringWorkbench.Infrastructure.Host;

public static class PluginCompositionRoot
{
    public static PluginRegistry Compose()
    {
        var registry = new PluginRegistry();
        
        // Explicitly register plugins here
        // No reflection, no assembly scanning, Native AOT compatible
        registry.RegisterPlugin(new ObservabilityPlugin());
        registry.RegisterPlugin(new ApiPlugin());
        registry.RegisterPlugin(new AuthenticationPlugin());
        registry.RegisterPlugin(new BackgroundProcessingPlugin());
        registry.RegisterPlugin(new PersistencePlugin());
        registry.RegisterPlugin(new ImportPlugin());
        
        return registry;
    }
}
