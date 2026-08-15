using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.Plugins.API.Capabilities;
using SaaSFoundry.SDK.Plugins.Authoring;

namespace SaaSFoundry.Plugins.API.Plugin;

public sealed class ApiPlugin : IPluginMetadataProvider
{
    private readonly IPluginMetadataProvider _innerPlugin;

    public ApiPlugin() : this(null)
    {
    }

    public ApiPlugin(IEnumerable<IPluginCapability>? injectedCapabilities)
    {
        var capabilitiesList = injectedCapabilities != null && injectedCapabilities.Any()
            ? injectedCapabilities.ToArray()
            : CreateDefaultCapabilities();

        _innerPlugin = new PluginBuilder()
            .WithIdentity("api", "1.0.0", "SaaSFoundry Engineering", "SHA256:8F3A2B7C1D2E5F0A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E4G")
            .WithManifest("SaaSFoundry API Engineering Plugin", "Production API infrastructure plugin providing Minimal API endpoint capabilities without reflection.", "SaaSFoundry.EngineeringWorkbench v1.0", "net10.0", "v1.0.0", "NativeAOT")
            .AddCapabilities(capabilitiesList)
            .Build();
    }

    public IPluginManifest Manifest => _innerPlugin.Manifest;

    public PluginIdentity Identity => _innerPlugin.Identity;

    public PluginMetadata Metadata => _innerPlugin.Metadata;

    public IReadOnlyCollection<IPluginCapability> Capabilities => _innerPlugin.Capabilities;

    public IPluginCapability? GetCapability(string capabilityId)
    {
        return _innerPlugin.GetCapability(capabilityId);
    }

    public Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        return _innerPlugin.InitializeAsync(services, cancellationToken);
    }

    public Task ShutdownAsync(CancellationToken cancellationToken)
    {
        return _innerPlugin.ShutdownAsync(cancellationToken);
    }

    private static IPluginCapability[] CreateDefaultCapabilities()
    {
        // Define standard API capabilities (e.g. a health check endpoint to prove it works)
        return new IPluginCapability[]
        {
            new HttpEndpointCapability(
                "health",
                "Health check endpoint",
                "/api/health",
                HttpMethod.Get,
                context => 
                {
                    context.Response.StatusCode = 200;
                    return context.Response.WriteAsync("OK");
                }
            ),
            new GrpcTransportCapability()
        };
    }
}
