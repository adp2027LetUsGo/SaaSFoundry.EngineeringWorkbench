using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.SDK.Core.Generators;
using SaaSFoundry.SDK.Plugins.Abstractions;

namespace SaaSFoundry.Plugins.API.Capabilities;

public sealed class GrpcTransportCapability : ITraceablePluginCapability
{
    public string CanonReference => "TRN-001";
    public string ImplementationReference => "TRN-101";

    public string Id { get; } = "grpc_transport";
    public string Description { get; } = "Generic gRPC + mTLS inter-cell transport generation.";
    public IReadOnlyList<string> SupportedOperations => new[] { "generate", "validate" };

    public CapabilityGovernanceMetadata GovernanceMetadata { get; }

    public GrpcTransportCapability()
    {
        GovernanceMetadata = new CapabilityGovernanceMetadata(
            "api.transport.grpc",
            "generate",
            new[] { "InterCellCommunication", "mTLS", "SpiffeIdentity", "GraphAuthorization" },
            new[] { "TRN-001-Compliance" },
            RiskLevel.High
        );
    }

    public IReadOnlyList<GeneratedArtifactDescriptor> GetArtifactDescriptors() => Array.Empty<GeneratedArtifactDescriptor>();

    public Task ValidateConfigurationAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task ValidateInputAsync(IPluginExecutionContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    
    public Task<IPluginExecutionResult> ExecuteAsync(IPluginExecutionContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult<IPluginExecutionResult>(new CapabilityExecutionResult(0));
    }

    public Task GenerateArtifactsAsync(IPluginExecutionContext context, CancellationToken cancellationToken)
    {
        var stagingOpt = context.Arguments.FirstOrDefault(a => a.StartsWith("--extraction-path="));
        var topologyOpt = context.Arguments.FirstOrDefault(a => a.StartsWith("--topology-path="));
        var cellOpt = context.Arguments.FirstOrDefault(a => a.StartsWith("--target-cell="));

        if (stagingOpt == null || topologyOpt == null || cellOpt == null) return Task.CompletedTask;

        var path = stagingOpt.Substring("--extraction-path=".Length);
        var topologyPath = topologyOpt.Substring("--topology-path=".Length);
        var targetCell = cellOpt.Substring("--target-cell=".Length);

        var topologyJson = File.ReadAllText(topologyPath);
        var topology = JsonSerializer.Deserialize<TopologyDef>(topologyJson, TopologyJsonContext.Default.TopologyDef);

        if (topology == null) return Task.CompletedTask;

        var descriptors = GenerateArtifacts(topology, targetCell);

        var generator = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator(Id, "1.0.0", "1.0.0");
        var result = generator.Generate(descriptors);
        
        var json = JsonSerializer.Serialize(result.GeneratedArtifacts, SaaSFoundry.SDK.Core.Generators.ArtifactGenerationJsonContext.Default.IReadOnlyListGeneratedArtifactDescriptor);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json);

        return Task.CompletedTask;
    }

    private List<GeneratedArtifactDescriptor> GenerateArtifacts(TopologyDef topology, string targetCell)
    {
        var list = new List<GeneratedArtifactDescriptor>();
        
        var allowedCallers = new List<string>();
        var allowedDestinations = new List<string>();

        foreach(var edge in topology.Communications)
        {
            if (edge.Mode.Equals("Bidirectional", StringComparison.OrdinalIgnoreCase))
            {
                if (edge.Destination == targetCell) allowedCallers.Add(edge.Source);
                if (edge.Source == targetCell) allowedCallers.Add(edge.Destination);
                if (edge.Source == targetCell) allowedDestinations.Add(edge.Destination);
                if (edge.Destination == targetCell) allowedDestinations.Add(edge.Source);
            }
            else
            {
                if (edge.Destination == targetCell) allowedCallers.Add(edge.Source);
                if (edge.Source == targetCell) allowedDestinations.Add(edge.Destination);
            }
        }
        
        allowedCallers = allowedCallers.Distinct().ToList();
        allowedDestinations = allowedDestinations.Distinct().ToList();

        list.Add(new GeneratedArtifactDescriptor(
            "proto.transport",
            "TransportInfrastructure.proto",
            "Protos/TransportInfrastructure.proto",
            "text/plain",
            "grpc_transport",
            "TRN-001",
            "TRN-101",
            "Protobuf transport definition",
            "ApiPlugin v1.0.0",
            "ev.proto.transport",
            GetProtoContent(),
            null,
            ArtifactCategory.SourceCode
        ));

        list.Add(new GeneratedArtifactDescriptor(
            "cs.middleware",
            "MtlsAuthorizationMiddleware.cs",
            "Generated/MtlsAuthorizationMiddleware.cs",
            "text/plain",
            "grpc_transport",
            "TRN-001",
            "TRN-101",
            "mTLS validation middleware",
            "ApiPlugin v1.0.0",
            "ev.cs.middleware",
            GetMiddlewareContent(topology.ProductId, targetCell, allowedCallers),
            null,
            ArtifactCategory.SourceCode
        ));

        list.Add(new GeneratedArtifactDescriptor(
            "cs.services",
            "GeneratedGrpcServices.cs",
            "Generated/GeneratedGrpcServices.cs",
            "text/plain",
            "grpc_transport",
            "TRN-001",
            "TRN-101",
            "gRPC Server definition",
            "ApiPlugin v1.0.0",
            "ev.cs.services",
            GetServicesContent(),
            null,
            ArtifactCategory.SourceCode
        ));

        list.Add(new GeneratedArtifactDescriptor(
            "cs.clients",
            "GeneratedGrpcClients.cs",
            "Generated/GeneratedGrpcClients.cs",
            "text/plain",
            "grpc_transport",
            "TRN-001",
            "TRN-101",
            "gRPC Clients",
            "ApiPlugin v1.0.0",
            "ev.cs.clients",
            GetClientsContent(topology.ProductId, allowedDestinations),
            null,
            ArtifactCategory.SourceCode
        ));

        list.Add(new GeneratedArtifactDescriptor(
            "cs.extensions",
            "GrpcServiceCollectionExtensions.cs",
            "Generated/GrpcServiceCollectionExtensions.cs",
            "text/plain",
            "grpc_transport",
            "TRN-001",
            "TRN-101",
            "DI Extensions",
            "ApiPlugin v1.0.0",
            "ev.cs.extensions",
            GetExtensionsContent((IReadOnlyList<string>)allowedDestinations),
            null,
            ArtifactCategory.SourceCode
        ));

        return list;
    }

    private string GetProtoContent() => 
@"syntax = ""proto3"";
package SaaSFoundry.Transport;
option csharp_namespace = ""SaaSFoundry.Transport.Generated"";

service InfrastructureService {
  rpc Ping (PingRequest) returns (PingResponse);
  rpc ProcessCommand (CommandRequest) returns (CommandResponse);
}

message PingRequest {}
message PingResponse { 
  string cell_identity = 1; 
}
message CommandRequest { 
  string payload = 1; 
}
message CommandResponse { 
  string result = 1; 
}";

    private string GetMiddlewareContent(string productId, string targetCell, List<string> callers)
    {
        var callerArray = string.Join(", ", callers.Select(c => $"\"spiffe://saasfoundry/{productId.ToLowerInvariant()}/{c.ToLowerInvariant()}\""));
        
        return $@"
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SaaSFoundry.Transport.Generated;

public class MtlsAuthorizationMiddleware
{{
    private readonly RequestDelegate _next;
    private readonly ILogger<MtlsAuthorizationMiddleware> _logger;
    private static readonly string[] AllowedCallers = new[] {{ {callerArray} }};

    public MtlsAuthorizationMiddleware(RequestDelegate next, ILogger<MtlsAuthorizationMiddleware> logger)
    {{
        _next = next;
        _logger = logger;
    }}

    public async Task InvokeAsync(HttpContext context)
    {{
        // 1. Verify connection is mTLS
        var cert = context.Connection.ClientCertificate;
        if (cert == null)
        {{
            _logger.LogWarning(""mTLS authorization failed: No client certificate."");
            context.Response.StatusCode = 401;
            return;
        }}

        // 2. Validate SPIFFE URI SAN
        var sanExtensions = cert.Extensions[""2.5.29.17""];
        if (sanExtensions == null)
        {{
            _logger.LogWarning(""mTLS authorization failed: No Subject Alternative Name extension."");
            context.Response.StatusCode = 401;
            return;
        }}

        var spiffeUri = ExtractSpiffeUri(sanExtensions.Format(false));
        if (string.IsNullOrEmpty(spiffeUri))
        {{
            _logger.LogWarning(""mTLS authorization failed: No SPIFFE URI SAN."");
            context.Response.StatusCode = 401;
            return;
        }}

        // 3. Topology Authorization
        if (!AllowedCallers.Contains(spiffeUri, StringComparer.OrdinalIgnoreCase))
        {{
            _logger.LogWarning($""mTLS authorization failed: Caller {{spiffeUri}} is not authorized by the communication graph."");
            context.Response.StatusCode = 403;
            return;
        }}

        await _next(context);
    }}

    private string? ExtractSpiffeUri(string sanFormatted)
    {{
        var parts = sanFormatted.Split(new[] {{ ',', ' ' }}, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {{
            if (p.StartsWith(""URI="", StringComparison.OrdinalIgnoreCase) || p.StartsWith(""URL="", StringComparison.OrdinalIgnoreCase))
            {{
                var val = p.Substring(4);
                if (val.StartsWith(""spiffe://"", StringComparison.OrdinalIgnoreCase)) return val;
            }}
        }}
        return null;
    }}
}}";
    }

    private string GetServicesContent() => 
@"using System;
using System.Threading.Tasks;
using Grpc.Core;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport;

namespace SaaSFoundry.Transport.Generated;

public class InfrastructureServiceImpl : InfrastructureService.InfrastructureServiceBase
{
    private readonly IIdempotencyEnforcer _idempotency;

    public InfrastructureServiceImpl(IIdempotencyEnforcer idempotency)
    {
        _idempotency = idempotency;
    }

    public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new PingResponse { CellIdentity = ""Ok"" });
    }

    public override async Task<CommandResponse> ProcessCommand(CommandRequest request, ServerCallContext context)
    {
        var idempotencyKey = context.RequestHeaders.Get(""x-idempotency-key"")?.Value ?? Guid.NewGuid().ToString();

        await _idempotency.TryAcquireAsync(""system"", idempotencyKey, context.CancellationToken);
        
        try
        {
            var res = new CommandResponse { Result = ""Processed: "" + request.Payload };
            await _idempotency.CompleteAsync(""system"", idempotencyKey, context.CancellationToken);
            return res;
        }
        catch
        {
            throw;
        }
    }
}";

    private string GetExtensionsContent(IReadOnlyList<string> allowedDestinations)
    {
        if (allowedDestinations.Count == 0)
        {
            return """
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SaaSFoundry.Transport.Generated;

public static class GrpcServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcTransport(this IServiceCollection services)
    {
        services.AddGrpc();
        services.AddTransient<IStartupFilter, GrpcTransportStartupFilter>();
        return services;
    }
}

public class GrpcTransportStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseRouting();
            app.UseMiddleware<MtlsAuthorizationMiddleware>();
            app.UseEndpoints(endpoints => 
            {
                endpoints.MapGrpcService<InfrastructureServiceImpl>();
            });
            next(app);
        };
    }
}
""";
        }

        return """
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SaaSFoundry.Transport.Generated;

public static class GrpcServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcTransport(this IServiceCollection services)
    {
        services.AddGrpc();
        GrpcClientRegistrations.AddGrpcClients(services);
        services.AddTransient<IStartupFilter, GrpcTransportStartupFilter>();
        return services;
    }
}

public class GrpcTransportStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseRouting();
            app.UseMiddleware<MtlsAuthorizationMiddleware>();
            app.UseEndpoints(endpoints => 
            {
                endpoints.MapGrpcService<InfrastructureServiceImpl>();
            });
            next(app);
        };
    }
}
""";
    }

    private string GetClientsContent(string productId, List<string> destinations)
    {
        var setup = @"
using System;
using System.Net.Http;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;

namespace SaaSFoundry.Transport.Generated;

public static class GrpcClientRegistrations
{
    public static void AddGrpcClients(IServiceCollection services)
    {
";
        foreach(var dest in destinations)
        {
            setup += $"\n        services.AddGrpcClient<InfrastructureService.InfrastructureServiceClient>(\"{dest}\", o => o.Address = new Uri(\"https://{dest.ToLowerInvariant()}:5001\"))\n";
            setup += $@"            .AddPolicyHandler(GetRetryPolicy())
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {{
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => 
                {{
                    return true; 
                }}
            }});";
        }
        setup += @"
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
";
        return setup;
    }

    public Task ValidateOutputAsync(IPluginExecutionContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<IReadOnlyCollection<SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence>> ProduceValidationEvidenceAsync(IPluginExecutionContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence>>(new SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence[0]);
    }

    public IReadOnlyList<string> ReportGeneratedFiles()
    {
        return new List<string>();
    }
}

public class TopologyDef
{
    public string ProductId { get; set; } = string.Empty;
    public List<CommunicationEdgeDef> Communications { get; set; } = new();
}

public class CommunicationEdgeDef
{
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Mode { get; set; } = "Outbound";
}

[JsonSerializable(typeof(TopologyDef))]
public partial class TopologyJsonContext : JsonSerializerContext
{
}
