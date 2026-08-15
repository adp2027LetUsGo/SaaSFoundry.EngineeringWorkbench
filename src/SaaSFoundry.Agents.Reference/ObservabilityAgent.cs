using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Pipeline;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using SaaSFoundry.Plugins.Observability.Plugin;
using SaaSFoundry.Plugins.Observability.Traceability;
using SaaSFoundry.SDK.Plugins.Abstractions;

namespace SaaSFoundry.Agents.Reference;

public sealed class ObservabilityAgent : IAgentOrchestrator
{
    public AgentIdentity Identity { get; }
    public AgentMetadata Metadata { get; }
    public AgentGovernanceMetadata GovernanceMetadata { get; }
    public GovernedAgentExecutionPipeline Pipeline { get; }

    public ObservabilityAgent(GovernedAgentExecutionPipeline? pipeline = null)
    {
        Pipeline = pipeline ?? new GovernedAgentExecutionPipeline();

        const long timestamp = 1720000000000L;

        Identity = new AgentIdentity(
            AgentId: "observability-agent",
            Version: "1.0.0",
            Author: "SaaSFoundry Engineering",
            Fingerprint: "SHA256:7B8D90E5A3C4D1E2F3A4B5C6D7E8F90A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6F",
            CreatedTimestamp: timestamp
        );

        var capabilities = new[] { "observability.generate", "observability.validate" };
        var permissions = new[] { "GenerateArtifacts", "ExecuteValidation" };

        Metadata = new AgentMetadata(
            Identity: Identity,
            Name: "SaaSFoundry Reference Observability Agent",
            Description: "Autonomous agent responsible for orchestrating canonical observability plugin generation and validation.",
            Purpose: "Demonstrate governed agentic orchestration of existing engineering plugins without reflection.",
            Capabilities: capabilities,
            Dependencies: new[] { "SaaSFoundry.Plugins.Observability v1.0.0" },
            RequiredPermissions: permissions,
            CompatibilityInformation: "SaaSFoundry.EngineeringWorkbench v1.0"
        );

        GovernanceMetadata = new AgentGovernanceMetadata(
            AgentId: Identity.AgentId,
            RiskLevel: AgentRiskLevel.Medium,
            RequiredPermissions: permissions,
            AllowedCapabilities: capabilities,
            ValidationRequirements: new[] { "AllCapabilitiesCovered", "DeterministicOutput" }
        );
    }

    public Task<AgentExecutionResult> ExecuteAsync(AgentExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (!string.Equals(context.AgentId, Identity.AgentId, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new AgentExecutionResult(
                ExecutionId: context.ExecutionId,
                AgentId: Identity.AgentId,
                Status: AgentExecutionStatus.Failed,
                FinishedTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                OutputSummary: null,
                GeneratedArtifacts: Array.Empty<string>(),
                ErrorMessage: $"Execution context AgentId '{context.AgentId}' does not match orchestrator ID '{Identity.AgentId}'."
            ));
        }

        // Execute capabilities through the governed execution pipeline
        return Pipeline.ExecuteGovernedCapabilityAsync(
            agent: this,
            context: context,
            capabilityId: "observability.generate",
            requestedPermissions: new[] { "GenerateArtifacts" },
            capabilityAction: async (ct) =>
            {
                // Explicitly orchestrate SaaSFoundry.Plugins.Observability without reflection or assembly scanning
                var plugin = new ObservabilityPlugin();
                var generatedArtifacts = new List<string>();

                foreach (var capability in plugin.Capabilities)
                {
                    if (capability is ITraceablePluginCapability traceable)
                    {
                        var descriptors = traceable.GetArtifactDescriptors();
                        foreach (var descriptor in descriptors)
                        {
                            generatedArtifacts.Add(descriptor.FileName);
                        }
                    }
                }

                return await Task.FromResult(generatedArtifacts.AsReadOnly());
            },
            cancellationToken: cancellationToken
        );
    }
}
