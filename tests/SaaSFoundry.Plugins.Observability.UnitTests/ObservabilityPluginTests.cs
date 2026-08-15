using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Lifecycle;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.PluginRuntime.Execution;
using SaaSFoundry.Plugins.Observability.Catalog;
using SaaSFoundry.Plugins.Observability.DependencyInjection;

using SaaSFoundry.Plugins.Observability.Planner;
using SaaSFoundry.Plugins.Observability.Plugin;
using Xunit;

using SaaSFoundry.SDK.Testing.Assertions;

namespace SaaSFoundry.Plugins.Observability.UnitTests;

public sealed class ObservabilityPluginTests
{
    [Fact]
    public void PluginCanBeInstantiated_Directly_Succeeds()
    {
        var plugin = new ObservabilityPlugin();
        PluginAssertions.AssertValid(plugin);
    }

    [Fact]
    public void ManifestIsValid_ContainsRequiredMetadata()
    {
        var plugin = new ObservabilityPlugin();
        ManifestAssertions.AssertExactMatch(plugin.Manifest, "observability", "1.0.0");
        ManifestAssertions.AssertCompatible(plugin.Manifest, "net10.0");
    }

    [Fact]
    public void CapabilitiesCanBeDiscovered_ReturnsAllTenExpectedCapabilities()
    {
        var plugin = new ObservabilityPlugin();
        Assert.Equal(10, plugin.Capabilities.Count);

        var expectedIds = new[]
        {
            "logging",
            "metrics",
            "tracing",
            "healthchecks",
            "collector",
            "configuration",
            "dashboards",
            "alerts",
            "documentation",
            "validation"
        };

        foreach (var id in expectedIds)
        {
            var capability = plugin.GetCapability(id);
            Assert.NotNull(capability);
            Assert.Equal(id, capability!.Id);
            Assert.False(string.IsNullOrWhiteSpace(capability.Description));
            Assert.Equal(2, capability.SupportedOperations.Count);
            
            int expectedFileCount = (id is "alerts" or "documentation" or "validation") ? 3 : 4;
            Assert.Equal(expectedFileCount, capability.ReportGeneratedFiles().Count);
        }
    }

    [Fact]
    public async Task CapabilitiesCanBeExecuted_Deterministically_NoIo()
    {
        var plugin = new ObservabilityPlugin();
        var logging = plugin.GetCapability("logging");
        Assert.NotNull(logging);

        using var cts = new CancellationTokenSource();
        await logging!.ValidateConfigurationAsync(cts.Token);
        
        var evidence = await logging.ProduceValidationEvidenceAsync(null!, cts.Token);
        Assert.NotNull(evidence);
        Assert.Equal(4, evidence.Count);
        Assert.All(evidence, e => Assert.True(e.IsSuccess));

        var execResult = await logging.ExecuteAsync(null!, cts.Token);
        Assert.NotNull(execResult);
        Assert.Equal(0, execResult.StatusCode);
    }

    [Fact]
    public void DependencyInjectionRegistrationWorks_ResolvesPluginAndCapabilities()
    {
        var services = new ServiceCollection();
        services.AddObservabilityPlugin();

        using var provider = services.BuildServiceProvider();

        var plugin = provider.GetService<IEngineeringPlugin>();
        Assert.NotNull(plugin);
        Assert.IsType<ObservabilityPlugin>(plugin);

        var capabilities = provider.GetServices<IPluginCapability>().ToList();
        Assert.Equal(10, capabilities.Count);
    }

    [Fact]
    public void CatalogContributionIsValid_ContainsTenCapabilities()
    {
        var packageManifest = ObservabilityPluginCatalog.BuildPackageManifest();
        Assert.Single(packageManifest.Plugins);
        var plugin = packageManifest.Plugins[0];
        Assert.Equal("observability", plugin.PluginId);
        Assert.Equal(10, plugin.Capabilities.Count);
    }

    [Fact]
    public void PlannerContributionIsValid_ReturnsTenTasksInLogicalOrder()
    {
        var tasks = ObservabilityPlanContributor.GetRequestedTasks();
        Assert.Equal(10, tasks.Count);
        Assert.Equal("logging", tasks[0].CapabilityId);
        Assert.Empty(tasks[0].Dependencies);
        Assert.Equal("metrics", tasks[1].CapabilityId);
        Assert.Single(tasks[1].Dependencies);
        Assert.Equal("logging", tasks[1].Dependencies[0].CapabilityId);
        Assert.Equal("validation", tasks[9].CapabilityId);
    }

    [Fact]
    public async Task LifecycleTransitionsTest()
    {
        var manager = new PluginLifecycleManager("observability", "1.0.0", new DefaultLifecycleEventBus());
        Assert.Equal(PluginLifecycleState.Created, manager.CurrentState);

        // Assert rejection of invalid transitions
        Assert.False(manager.CanTransitionTo(PluginLifecycleState.Active));
        var invalidResult = await manager.TransitionToAsync(PluginLifecycleState.Executing);
        Assert.False(invalidResult);
        Assert.Equal(PluginLifecycleState.Created, manager.CurrentState);

        // Validate complete sequence: Created -> Registered -> Loaded -> Validated -> Active -> Executing
        Assert.True(await manager.TransitionToAsync(PluginLifecycleState.Registered));
        Assert.Equal(PluginLifecycleState.Registered, manager.CurrentState);

        Assert.True(await manager.TransitionToAsync(PluginLifecycleState.Loaded));
        Assert.Equal(PluginLifecycleState.Loaded, manager.CurrentState);

        Assert.True(await manager.TransitionToAsync(PluginLifecycleState.Validated));
        Assert.Equal(PluginLifecycleState.Validated, manager.CurrentState);

        Assert.True(await manager.TransitionToAsync(PluginLifecycleState.Active));
        Assert.Equal(PluginLifecycleState.Active, manager.CurrentState);

        Assert.True(await manager.TransitionToAsync(PluginLifecycleState.Executing));
        Assert.Equal(PluginLifecycleState.Executing, manager.CurrentState);
    }

    [Fact]
    public void MetadataValidationTest()
    {
        var plugin = new ObservabilityPlugin();
        var provider = Assert.IsAssignableFrom<IPluginMetadataProvider>(plugin);

        var identity = provider.Identity;
        Assert.NotNull(identity);
        Assert.Equal("observability", identity.PluginId);
        Assert.Equal("1.0.0", identity.Version);
        Assert.Equal("SaaSFoundry Engineering", identity.Author);
        Assert.StartsWith("SHA256:", identity.Fingerprint);

        var metadata = provider.Metadata;
        Assert.NotNull(metadata);
        Assert.Equal("observability", metadata.Id);
        Assert.Equal("SaaSFoundry Observability Engineering Plugin", metadata.Name);
        Assert.Equal("1.0.0", metadata.Version);
        Assert.Equal(10, metadata.Capabilities.Count);
        Assert.Empty(metadata.Dependencies);
        Assert.Equal("SaaSFoundry Engineering", metadata.Author);
        Assert.Equal("SaaSFoundry.EngineeringWorkbench v1.0", metadata.Compatibility);

        var capIdentity = new CapabilityIdentity(identity.PluginId, "alerts", identity.Version);
        Assert.Equal("observability", capIdentity.PluginId);
        Assert.Equal("alerts", capIdentity.CapabilityId);
        Assert.Equal("1.0.0", capIdentity.Version);
    }

    [Fact]
    public void CapabilityGovernanceValidationTest()
    {
        var plugin = new ObservabilityPlugin();
        Assert.Equal(10, plugin.Capabilities.Count);

        foreach (var cap in plugin.Capabilities)
        {
            GovernanceAssertions.AssertGoverned(cap);
        }
    }
}
