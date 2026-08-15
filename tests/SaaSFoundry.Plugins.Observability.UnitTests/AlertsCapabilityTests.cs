using SaaSFoundry.SDK.Core.Generators;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.Plugins.Observability.Capabilities.Alerts;
using SaaSFoundry.Plugins.Observability.Traceability;
using Xunit;

namespace SaaSFoundry.Plugins.Observability.UnitTests;

public sealed class AlertsCapabilityTests
{
    [Fact]
    public void AlertsCapability_GeneratesThreeArtifacts_WithCorrectTraceabilityAndDependencies()
    {
        var capability = new AlertsCapability();
        Assert.Equal("alerts", capability.Id);

        var descriptors = capability.GetArtifactDescriptors();
        Assert.Equal(3, descriptors.Count);

        // Verify universal canonical traceability
        Assert.All(descriptors, d =>
        {
            Assert.Equal("OBS-008", d.CanonReference);
            Assert.Equal("OBS-108", d.ImplementationReference);
            Assert.Equal("alerts", d.CapabilityId);
            Assert.False(string.IsNullOrWhiteSpace(d.Content));
        });

        // 1. Prometheus Alerting Rules
        var rules = descriptors.Single(d => d.ArtifactId == "obs.alerts.rules.prometheus");
        Assert.Equal("prometheus-alert-rules.yaml", rules.FileName);
        Assert.Equal(ArtifactCategory.Configuration, rules.Category);
        Assert.NotNull(rules.Dependencies);
        Assert.Contains("obs.metrics.config.prometheus", rules.Dependencies!);
        Assert.Contains("obs.collector.config", rules.Dependencies!);
        Assert.Contains("http_server_requests_seconds_count", rules.Content); // Verifies reference to existing metrics sources

        // 2. Alerting Runbook
        var runbook = descriptors.Single(d => d.ArtifactId == "obs.alerts.runbook");
        Assert.Equal("alerting-runbook.md", runbook.FileName);
        Assert.Equal(ArtifactCategory.Documentation, runbook.Category);
        Assert.NotNull(runbook.Dependencies);
        Assert.Single(runbook.Dependencies!);
        Assert.Contains("obs.alerts.rules.prometheus", runbook.Dependencies!);

        // 3. Alerts Evidence
        var evidence = descriptors.Single(d => d.ArtifactId == "obs.alerts.evidence");
        Assert.Equal("Evidence-Alerts.json", evidence.FileName);
        Assert.Equal(ArtifactCategory.Evidence, evidence.Category);
        Assert.NotNull(evidence.Dependencies);
        Assert.Equal(2, evidence.Dependencies!.Count);
        Assert.Contains("obs.alerts.rules.prometheus", evidence.Dependencies!);
        Assert.Contains("obs.alerts.runbook", evidence.Dependencies!);
    }

    [Fact]
    public void AlertsCapability_HasValidGovernanceMetadata()
    {
        var capability = new AlertsCapability();
        var metadata = capability.GovernanceMetadata;

        Assert.NotNull(metadata);
        Assert.Equal("observability.alerts.generate", metadata.CapabilityId);
        Assert.Equal("alerts.generate", metadata.OperationType);
        Assert.Equal(RiskLevel.Medium, metadata.Risk);
        Assert.Contains("GenerateMonitoringArtifacts", metadata.RequiredPermissions);
        Assert.Contains("Alerts configuration must reference existing metrics sources.", metadata.ValidationRequirements);
    }
}
