using System.Linq;
using SaaSFoundry.Plugins.Observability.Plugin;
using SaaSFoundry.SDK.Validation.Registration;
using SaaSFoundry.SDK.Validation.Validators;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using Xunit;

namespace SaaSFoundry.SDK.Validation.UnitTests;

public class ValidationEngineTests
{
    [Fact]
    public void Test_GoldenReference_Identity_IsValid()
    {
        var plugin = new ObservabilityPlugin();
        var pipeline = new ValidationPipeline<IPluginMetadataProvider>(
            new PluginIdentityRule()
        );

        var report = pipeline.Validate(plugin);

        Assert.True(report.IsValid, "Golden reference should pass identity validation.");
        Assert.DoesNotContain(report.Diagnostics, d => d.IsError);
    }

    [Fact]
    public void Test_GoldenReference_Capabilities_AreValid()
    {
        var plugin = new ObservabilityPlugin();
        var pipeline = new ValidationPipeline<IPluginCapability>(
            new CapabilityTraceabilityRule(),
            new CapabilityGovernanceRule()
        );

        foreach (var cap in plugin.Capabilities)
        {
            var report = pipeline.Validate(cap);
            Assert.True(report.IsValid, $"Capability {cap.Id} failed validation: " + string.Join(", ", report.Diagnostics.Select(d => d.Message)));
        }
    }

    [Fact]
    public void Test_GoldenReference_Manifest_IsValid()
    {
        var plugin = new ObservabilityPlugin();
        var pipeline = new ValidationPipeline<IPluginManifest>(
            new ManifestIdentityRule()
        );

        var report = pipeline.Validate(plugin.Manifest);

        Assert.True(report.IsValid, "Manifest identity rule failed: " + string.Join(", ", report.Diagnostics.Select(d => d.Message)));
    }

    [Fact]
    public void Test_CapabilityRegistration_NoDuplicates()
    {
        var plugin = new ObservabilityPlugin();
        var pipeline = new ValidationPipeline<IPluginMetadataProvider>(
            new CapabilityRegistrationRule()
        );

        var report = pipeline.Validate(plugin);

        Assert.True(report.IsValid, "Capability registration rule failed.");
    }
}
