using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.SDK.Core.Generators;
using SaaSFoundry.SDK.Plugins.Abstractions;

namespace SaaSFoundry.Plugins.API.Capabilities;

public sealed class HttpEndpointCapability : ITraceablePluginCapability
{
    private static readonly IReadOnlyList<GeneratedArtifactDescriptor> _descriptors = new GeneratedArtifactDescriptor[]
    {
        new(
            ArtifactId: "api.endpoint.docs",
            FileName: "README-Endpoint.md",
            RelativePath: "docs/README-Endpoint.md",
            ContentType: "text/markdown",
            CapabilityId: "http_endpoint",
            CanonReference: "API-001",
            ImplementationReference: "API-101",
            Description: "Engineering documentation describing endpoint explicit registration.",
            Generator: "ApiPlugin v1.0.0",
            ValidationEvidenceId: "ev.api.endpoint.docs",
            Content: """
            # SaaSFoundry API — Endpoint Capability (API-001 / API-101)

            ## Engineering Traceability
            - **Canonical Architecture**: API-001 (API Architecture)
            - **Engineering Implementation**: API-101 (HTTP Endpoint)
            - **Capability**: `http_endpoint`

            ## Overview
            This capability defines explicitly registered NativeAOT-compatible Minimal API endpoints.
            """
        )
    };

    public string CanonReference => _descriptors[0].CanonReference;
    public string ImplementationReference => _descriptors[0].ImplementationReference;

    public string Id { get; }
    public string Description { get; }
    public IReadOnlyList<string> SupportedOperations => new[] { "generate", "validate", "serve" };

    public string Route { get; }
    public HttpMethod Method { get; }
    public RequestDelegate Handler { get; }
    public bool RequiresAuthentication { get; }

    public CapabilityGovernanceMetadata GovernanceMetadata { get; }

    public HttpEndpointCapability(
        string id,
        string description,
        string route,
        HttpMethod method,
        RequestDelegate handler,
        bool requiresAuthentication = false)
    {
        Id = id;
        Description = description;
        Route = route;
        Method = method;
        Handler = handler;
        RequiresAuthentication = requiresAuthentication;

        GovernanceMetadata = new CapabilityGovernanceMetadata(
            $"api.endpoint.{id}",
            "serve",
            new[] { "ServeHttpTraffic", "EndpointRegistration" },
            new[] { "API-001-Compliance" },
            RiskLevel.Medium
        );
    }

    public IReadOnlyList<GeneratedArtifactDescriptor> GetArtifactDescriptors() => _descriptors;

    public Task ValidateConfigurationAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task ValidateInputAsync(IPluginExecutionContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    
    public Task<IPluginExecutionResult> ExecuteAsync(IPluginExecutionContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult<IPluginExecutionResult>(new CapabilityExecutionResult(0));
    }

        public Task GenerateArtifactsAsync(IPluginExecutionContext context, CancellationToken cancellationToken)
    {
        var stagingOpt = System.Linq.Enumerable.FirstOrDefault(context.Arguments, a => a.StartsWith("--extraction-path="));
        if (stagingOpt != null)
        {
            var path = stagingOpt.Substring("--extraction-path=".Length);
            var generator = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator(Id, "1.0.0", "1.0.0");
            var result = generator.Generate(_descriptors);
            var json = System.Text.Json.JsonSerializer.Serialize(result.GeneratedArtifacts, SaaSFoundry.SDK.Core.Generators.ArtifactGenerationJsonContext.Default.IReadOnlyListGeneratedArtifactDescriptor);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, json);
        }
        return Task.CompletedTask;
    }
    public Task ValidateOutputAsync(IPluginExecutionContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<IReadOnlyCollection<SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence>> ProduceValidationEvidenceAsync(IPluginExecutionContext context, CancellationToken cancellationToken)
    {
        var generator = new SaaSFoundry.SDK.Core.Generators.ArtifactGenerator("api", "1.0.0", "1.0.0");
        var result = generator.Generate(_descriptors);
        return Task.FromResult<IReadOnlyCollection<SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence>>(
            result.ValidationEvidence.Select(e => new SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence(e.PluginId, e.CapabilityId, e.Stage, e.IsSuccess, e.Message, e.Timestamp)).ToList());
    }

    public IReadOnlyList<string> ReportGeneratedFiles()
    {
        return _descriptors.Select(d => d.FileName).ToList();
    }
}

public sealed class CapabilityExecutionResult : IPluginExecutionResult
{
    public bool IsSuccess => StatusCode == 0;
    public int StatusCode { get; }
    public string Message => IsSuccess ? "Success" : "Failure";
    public IReadOnlyDictionary<string, object> Artifacts => new Dictionary<string, object>();

    public CapabilityExecutionResult(int statusCode)
    {
        StatusCode = statusCode;
    }
}
