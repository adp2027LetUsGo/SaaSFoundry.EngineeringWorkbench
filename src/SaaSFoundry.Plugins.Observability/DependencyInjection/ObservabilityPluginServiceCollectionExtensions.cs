using Microsoft.Extensions.DependencyInjection;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.Plugins.Observability.Capabilities.Alerts;
using SaaSFoundry.Plugins.Observability.Capabilities.Collector;
using SaaSFoundry.Plugins.Observability.Capabilities.Configuration;
using SaaSFoundry.Plugins.Observability.Capabilities.Dashboards;
using SaaSFoundry.Plugins.Observability.Capabilities.Documentation;
using SaaSFoundry.Plugins.Observability.Capabilities.HealthChecks;
using SaaSFoundry.Plugins.Observability.Capabilities.Logging;
using SaaSFoundry.Plugins.Observability.Capabilities.Metrics;
using SaaSFoundry.Plugins.Observability.Capabilities.Tracing;
using SaaSFoundry.Plugins.Observability.Capabilities.Validation;
using SaaSFoundry.Plugins.Observability.Plugin;

namespace SaaSFoundry.Plugins.Observability.DependencyInjection;

public static class ObservabilityPluginServiceCollectionExtensions
{
    public static IServiceCollection AddObservabilityPlugin(this IServiceCollection services)
    {
        // Explicit registrations without runtime reflection for full NativeAOT compatibility
        services.AddSingleton<IPluginCapability, LoggingCapability>();
        services.AddSingleton<IPluginCapability, MetricsCapability>();
        services.AddSingleton<IPluginCapability, TracingCapability>();
        services.AddSingleton<IPluginCapability, HealthChecksCapability>();
        services.AddSingleton<IPluginCapability, CollectorCapability>();
        services.AddSingleton<IPluginCapability, ConfigurationCapability>();
        services.AddSingleton<IPluginCapability, DashboardsCapability>();
        services.AddSingleton<IPluginCapability, AlertsCapability>();
        services.AddSingleton<IPluginCapability, DocumentationCapability>();
        services.AddSingleton<IPluginCapability, ValidationCapability>();
        
        services.AddSingleton<IEngineeringPlugin, ObservabilityPlugin>();
        
        return services;
    }
}
