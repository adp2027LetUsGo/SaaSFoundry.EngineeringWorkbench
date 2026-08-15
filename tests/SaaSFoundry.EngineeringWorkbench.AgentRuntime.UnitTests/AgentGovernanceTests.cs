using SaaSFoundry.Agents.Reference;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.AgentRuntime.UnitTests;

public sealed class AgentGovernanceTests
{
    [Fact]
    public void ObservabilityAgent_GovernanceMetadata_HasExpectedPermissionsAndRisk()
    {
        var agent = new ObservabilityAgent();
        var gov = agent.GovernanceMetadata;

        Assert.Equal("observability-agent", gov.AgentId);
        Assert.Equal(AgentRiskLevel.Medium, gov.RiskLevel);
        Assert.Contains("GenerateArtifacts", gov.RequiredPermissions);
        Assert.Contains("ExecuteValidation", gov.RequiredPermissions);
        Assert.Contains("observability.generate", gov.AllowedCapabilities);
        Assert.Contains("observability.validate", gov.AllowedCapabilities);
    }
}
