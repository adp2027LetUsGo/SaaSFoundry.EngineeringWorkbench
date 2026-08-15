using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Engine;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Pipeline;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Providers;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent.Governance;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.AgentGovernance.UnitTests;

public class ApprovalWorkflowTests
{
    private class MockHighRiskAgent : IAgentOrchestrator
    {
        public AgentIdentity Identity { get; } = new("high-risk-agent", "1.0.0", "Tester", "hash", 1000);
        public AgentMetadata Metadata { get; }
        public AgentGovernanceMetadata GovernanceMetadata { get; } = new("high-risk-agent", AgentRiskLevel.High, new[] { "Execute" }, new[] { "high.cap" }, Array.Empty<string>());

        public MockHighRiskAgent()
        {
            Metadata = new AgentMetadata(Identity, "High Risk Mock", "Test", "Test", new[] { "high.cap" }, Array.Empty<string>(), new[] { "Execute" }, "v1");
        }

        public Task<AgentExecutionResult> ExecuteAsync(AgentExecutionContext context, System.Threading.CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    [Fact]
    [Trait("Category", "Governance")]
    public async Task ApprovalProvider_AutoApprove_GrantsDecision()
    {
        var provider = new DefaultAgentApprovalProvider(autoApprove: true, approverId: "TestAdmin");
        var req = new ApprovalRequest("appr-1", "agent-1", "cap-1", AgentRiskLevel.High, "High risk test", 1000);

        var decision = await provider.RequestApprovalAsync(req);

        Assert.True(decision.IsApproved);
        Assert.Equal("TestAdmin", decision.ApproverId);
        Assert.Null(decision.DenialReason);
    }

    [Fact]
    [Trait("Category", "Governance")]
    public async Task ApprovalProvider_Denied_ReturnsDenialReason()
    {
        var provider = new DefaultAgentApprovalProvider(autoApprove: false, approverId: "SecOpsGate");
        var req = new ApprovalRequest("appr-2", "agent-1", "cap-1", AgentRiskLevel.High, "High risk test", 1000);

        var decision = await provider.RequestApprovalAsync(req);

        Assert.False(decision.IsApproved);
        Assert.Equal("SecOpsGate", decision.ApproverId);
        Assert.NotNull(decision.DenialReason);
    }

    [Fact]
    [Trait("Category", "Governance")]
    public async Task Pipeline_HighRiskWithApprovedGate_ExecutesSuccessfully()
    {
        var provider = new DefaultAgentApprovalProvider(autoApprove: true, approverId: "ApprovedAdmin");
        var pipeline = new GovernedAgentExecutionPipeline(new AgentGovernanceEngine(), provider);
        var agent = new MockHighRiskAgent();
        var context = new AgentExecutionContext("exec-high-1", agent.Identity.AgentId, "High Risk Run", 2000L, new Dictionary<string, string>());

        var result = await pipeline.ExecuteGovernedCapabilityAsync(
            agent,
            context,
            "high.cap",
            new[] { "Execute" },
            async (ct) => await Task.FromResult(new[] { "artifact-after-approval.json" })
        );

        Assert.Equal(AgentExecutionStatus.Succeeded, result.Status);
        Assert.Single(result.GeneratedArtifacts);
        Assert.Contains("artifact-after-approval.json", result.GeneratedArtifacts);
    }

    [Fact]
    [Trait("Category", "Governance")]
    public async Task Pipeline_HighRiskWithDeniedGate_CancelsExecution()
    {
        var provider = new DefaultAgentApprovalProvider(autoApprove: false, approverId: "DeniedAdmin");
        var pipeline = new GovernedAgentExecutionPipeline(new AgentGovernanceEngine(), provider);
        var agent = new MockHighRiskAgent();
        var context = new AgentExecutionContext("exec-high-2", agent.Identity.AgentId, "High Risk Run", 2000L, new Dictionary<string, string>());

        bool capabilityInvoked = false;
        var result = await pipeline.ExecuteGovernedCapabilityAsync(
            agent,
            context,
            "high.cap",
            new[] { "Execute" },
            async (ct) =>
            {
                capabilityInvoked = true;
                return await Task.FromResult(new[] { "forbidden.json" });
            }
        );

        Assert.False(capabilityInvoked, "Capability action must not be invoked when approval gate denies execution.");
        Assert.Equal(AgentExecutionStatus.Cancelled, result.Status);
        Assert.Contains("Execution denied", result.ErrorMessage);
        Assert.Empty(result.GeneratedArtifacts);
    }
}
