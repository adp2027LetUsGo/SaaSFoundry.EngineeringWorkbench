using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Engine;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Policy;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Providers;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent.Governance;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.AgentGovernance.UnitTests;

public class AgentGovernancePolicyTests
{
    private readonly StandardAgentExecutionPolicy _policy = new();
    private readonly AgentGovernanceEngine _engine = new();

    private AgentExecutionRequest CreateRequest(AgentRiskLevel riskLevel, string capabilityId = "test.capability", IReadOnlyList<string>? requestedPermissions = null, IReadOnlyList<string>? allowedPermissions = null, IReadOnlyList<string>? allowedCapabilities = null)
    {
        var governance = new AgentGovernanceMetadata(
            AgentId: "test-agent",
            RiskLevel: riskLevel,
            RequiredPermissions: allowedPermissions ?? new[] { "Execute", "Generate" },
            AllowedCapabilities: allowedCapabilities ?? new[] { capabilityId },
            ValidationRequirements: new[] { "DeterministicOutput" }
        );

        return new AgentExecutionRequest(
            ExecutionId: "exec-001",
            AgentId: "test-agent",
            CapabilityId: capabilityId,
            Governance: governance,
            RequestedPermissions: requestedPermissions ?? new[] { "Execute" }
        );
    }

    [Theory]
    [InlineData(AgentRiskLevel.None)]
    [InlineData(AgentRiskLevel.Low)]
    public async Task Policy_NoneOrLowRisk_GrantsAutomaticApprovalWithoutAudit(AgentRiskLevel riskLevel)
    {
        var request = CreateRequest(riskLevel);
        var decision = await _engine.EvaluateRequestAsync(request);

        Assert.True(decision.CanExecute);
        Assert.False(decision.IsBlocked);
        Assert.False(decision.RequiresAudit);
        Assert.False(decision.ApprovalRequirement.IsRequired);
        Assert.Empty(decision.Violations);
    }

    [Fact]
    [Trait("Category", "Governance")]
    public async Task Policy_MediumRisk_GrantsApprovalWithMandatoryAudit()
    {
        var request = CreateRequest(AgentRiskLevel.Medium);
        var decision = await _engine.EvaluateRequestAsync(request);

        Assert.True(decision.CanExecute);
        Assert.False(decision.IsBlocked);
        Assert.True(decision.RequiresAudit);
        Assert.False(decision.ApprovalRequirement.IsRequired);
        Assert.Empty(decision.Violations);
    }

    [Fact]
    [Trait("Category", "Governance")]
    public async Task Policy_HighRisk_RequiresExplicitApprovalAndAudit()
    {
        var request = CreateRequest(AgentRiskLevel.High);
        var decision = await _engine.EvaluateRequestAsync(request);

        Assert.False(decision.CanExecute);
        Assert.False(decision.IsBlocked);
        Assert.True(decision.RequiresAudit);
        Assert.True(decision.ApprovalRequirement.IsRequired);
        Assert.Empty(decision.Violations);
        Assert.Equal("EngineeringManager", decision.ApprovalRequirement.RequiredApproverRole);
    }

    [Fact]
    [Trait("Category", "Governance")]
    public async Task Policy_CriticalRisk_IsStrictlyBlocked()
    {
        var request = CreateRequest(AgentRiskLevel.Critical);
        var decision = await _engine.EvaluateRequestAsync(request);

        Assert.False(decision.CanExecute);
        Assert.True(decision.IsBlocked);
        Assert.True(decision.RequiresAudit);
        Assert.False(decision.ApprovalRequirement.IsRequired);
        Assert.Single(decision.Violations);
        Assert.Contains(decision.Violations, v => v.Severity == "Critical");
    }

    [Fact]
    [Trait("Category", "Governance")]
    public async Task Policy_UnauthorizedPermissionRequest_GeneratesViolationAndBlocks()
    {
        var request = CreateRequest(
            AgentRiskLevel.Low,
            requestedPermissions: new[] { "Execute", "AdminDelete" },
            allowedPermissions: new[] { "Execute" }
        );

        var decision = await _policy.EvaluateAsync(request);

        Assert.False(decision.CanExecute);
        Assert.True(decision.IsBlocked);
        Assert.Single(decision.Violations);
        Assert.Contains(decision.Violations, v => v.Description.Contains("AdminDelete"));
    }

    [Fact]
    [Trait("Category", "Governance")]
    public async Task Policy_UnlistedCapability_GeneratesViolationAndBlocks()
    {
        var request = CreateRequest(
            AgentRiskLevel.Medium,
            capabilityId: "unauthorized.hack",
            allowedCapabilities: new[] { "safe.generate" }
        );

        var decision = await _policy.EvaluateAsync(request);

        Assert.False(decision.CanExecute);
        Assert.True(decision.IsBlocked);
        Assert.Single(decision.Violations);
        Assert.Contains(decision.Violations, v => v.Description.Contains("unauthorized.hack"));
    }
}
