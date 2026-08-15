using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;
using SaaSFoundry.Plugins.BackgroundProcessing.Plugin;
using Xunit;
using SaaSFoundry.Plugins.BackgroundProcessing.Capabilities.BackgroundJobs;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;

namespace SaaSFoundry.Plugins.BackgroundProcessing.UnitTests;

public class BackgroundProcessingPluginTests
{
    [Fact]
    public void Plugin_IdentityAndManifest_AreValid()
    {
        var plugin = new BackgroundProcessingPlugin();
        
        Assert.Equal("background-processing", plugin.Identity.PluginId);
        Assert.Equal("1.0.0", plugin.Identity.Version);
        Assert.Equal("SaaSFoundry Background Processing Engineering Plugin", plugin.Manifest.Name);
        Assert.Equal("SaaSFoundry.EngineeringWorkbench v1.0", plugin.Metadata.Compatibility);
    }

    [Fact]
    public void Plugin_ContainsExpectedCapabilities()
    {
        var plugin = new BackgroundProcessingPlugin();
        
        Assert.Single(plugin.Capabilities);
        Assert.IsType<BackgroundJobCapability>(plugin.Capabilities.First());
    }

    [Fact]
    public void BackgroundJobCapability_ProvidesCorrectGovernance()
    {
        var capability = new BackgroundJobCapability();
        
        Assert.Equal("backgroundjob", capability.Id);
        Assert.Equal("BP-001", capability.CanonReference);
        Assert.Equal("BP-101", capability.ImplementationReference);
        
        var gov = capability.GovernanceMetadata;
        Assert.Equal("backgroundprocessing.backgroundjob.generate", gov.CapabilityId);
        Assert.Equal(RiskLevel.High, gov.Risk);
        Assert.Contains("BP-001-Compliance", gov.ValidationRequirements);
    }

    [Fact]
    public async Task Capability_GeneratesDeterministicArtifacts()
    {
        var capability = new BackgroundJobCapability();
        
        var artifacts = capability.GetArtifactDescriptors();
        Assert.Equal(4, artifacts.Count);
        
        Assert.Contains(artifacts, a => a.FileName == "BackgroundWorkerService.cs");
        Assert.Contains(artifacts, a => a.FileName == "StaticJobDispatcher.cs");
        Assert.Contains(artifacts, a => a.FileName == "JobPayloadSerializer.cs");
        Assert.Contains(artifacts, a => a.FileName == "JobContextSerializer.cs");

        var workerContent = artifacts.Single(a => a.FileName == "BackgroundWorkerService.cs").Content;
        Assert.Contains("public class BackgroundWorkerService : BackgroundService", workerContent);
        
        var dispatcherContent = artifacts.Single(a => a.FileName == "StaticJobDispatcher.cs").Content;
        Assert.Contains("public partial class StaticJobDispatcher : IJobDispatcher", dispatcherContent);
        Assert.Contains("partial void TryDispatch", dispatcherContent);
        
        var payloadSerializerContent = artifacts.Single(a => a.FileName == "JobPayloadSerializer.cs").Content;
        Assert.Contains("public partial class JobPayloadSerializer : IJobPayloadSerializer", payloadSerializerContent);
        Assert.Contains("partial void TrySerialize<TJob>", payloadSerializerContent);
        Assert.Contains("partial void TryDeserialize<TJob>", payloadSerializerContent);
        
        var contextSerializerContent = artifacts.Single(a => a.FileName == "JobContextSerializer.cs").Content;
        Assert.Contains("public class JobContextSerializer : IJobContextSerializer", contextSerializerContent);
        Assert.Contains("new AuthorizationContext", contextSerializerContent); // Ensures auth isn't serialized directly
    }

    [Fact]
    public void GeneratedCode_DoesNotUseReflection()
    {
        var capability = new BackgroundJobCapability();
        var artifacts = capability.GetArtifactDescriptors();

        foreach (var artifact in artifacts)
        {
            Assert.DoesNotContain("System.Reflection", artifact.Content);
            Assert.DoesNotContain("Activator.CreateInstance", artifact.Content);
            Assert.DoesNotContain("Type.GetType", artifact.Content);
            Assert.DoesNotContain("Assembly.GetTypes", artifact.Content);
            Assert.DoesNotContain(" dynamic ", artifact.Content);
        }
    }

    [Fact]
    public void GeneratedCode_DoesNotContainVibeStockLogic()
    {
        var capability = new BackgroundJobCapability();
        var artifacts = capability.GetArtifactDescriptors();

        foreach (var artifact in artifacts)
        {
            Assert.DoesNotContain("TestJob", artifact.Content);
            Assert.DoesNotContain("test.job", artifact.Content);
            Assert.DoesNotContain("Shopify", artifact.Content);
            Assert.DoesNotContain("Inventory", artifact.Content);
        }
    }
}
