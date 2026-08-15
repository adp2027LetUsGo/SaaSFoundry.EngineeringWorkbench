using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SaaSFoundry.Plugins.API.Capabilities;
using SaaSFoundry.Plugins.API.Contracts;

namespace SaaSFoundry.Plugins.API.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapPluginCapability(this IEndpointRouteBuilder endpoints, HttpEndpointCapability capability)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(capability);

        // We use explicit mapping of the HttpMethod and Route to the provided RequestDelegate.
        // This avoids any assembly scanning or reflection-based dynamic binding.
        
        var builder = endpoints.MapMethods(capability.Route, new[] { capability.Method.Method }, capability.Handler)
            .WithName(capability.Id)
            .WithDescription(capability.Description);

        // Map authentication/authorization integration point without implementing it
        if (capability.RequiresAuthentication)
        {
            builder.RequireAuthorization();
        }

        // We can add metadata derived from the plugin capability (e.g. Governance and Traceability metadata).
        builder.WithMetadata(capability.GovernanceMetadata);
        
        return endpoints;
    }
}
