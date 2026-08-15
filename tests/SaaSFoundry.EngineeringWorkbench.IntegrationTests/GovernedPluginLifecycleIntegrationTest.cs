using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Events;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Lifecycle;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Governance.Policies;
using SaaSFoundry.EngineeringWorkbench.Infrastructure.Host;
using SaaSFoundry.EngineeringWorkbench.PluginRuntime.Execution;
using SaaSFoundry.Plugins.Observability.Plugin;

namespace SaaSFoundry.EngineeringWorkbench.IntegrationTests;

public sealed class GovernedPluginLifecycleIntegrationTest
{
    [Fact]
    public async Task Validate_Governed_Lifecycle_Execution_Pipeline()
    {
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // 1. Initialize Event Bus and Lifecycle Manager
        var eventBus = new DefaultLifecycleEventBus();
        var plugin = new ObservabilityPlugin();
        var metadataProvider = Assert.IsAssignableFrom<IPluginMetadataProvider>(plugin);
        var identity = metadataProvider.Identity;

        var lifecycleManager = new PluginLifecycleManager(identity.PluginId, identity.Version, eventBus);
        Assert.Equal(PluginLifecycleState.Created, lifecycleManager.CurrentState);

        // 2. Register Observability Plugin and Activate Lifecycle
        Assert.True(await lifecycleManager.TransitionToAsync(PluginLifecycleState.Registered, cancellationToken));
        Assert.True(await lifecycleManager.TransitionToAsync(PluginLifecycleState.Loaded, cancellationToken));

        var services = new EmptyServiceProvider();
        await plugin.InitializeAsync(services, cancellationToken);

        Assert.True(await lifecycleManager.TransitionToAsync(PluginLifecycleState.Validated, cancellationToken));
        Assert.True(await lifecycleManager.TransitionToAsync(PluginLifecycleState.Active, cancellationToken));
        Assert.Equal(PluginLifecycleState.Active, lifecycleManager.CurrentState);

        // 3. Evaluate Governance Policy
        var governancePolicy = new StandardGovernancePolicy();
        var pluginDecision = await governancePolicy.EvaluatePluginAsync(plugin, cancellationToken);
        Assert.True(pluginDecision.CanExecutePlugin);
        Assert.False(pluginDecision.IsApprovalRequired);

        var capability = plugin.GetCapability("documentation");
        Assert.NotNull(capability);

        var governedCap = Assert.IsAssignableFrom<IGovernedPluginCapability>(capability);
        Assert.Equal(RiskLevel.Low, governedCap.GovernanceMetadata.Risk);

        var capabilityDecision = await governancePolicy.EvaluateCapabilityAsync(plugin, capability!, cancellationToken);
        Assert.True(capabilityDecision.CanExecuteCapability);
        Assert.False(capabilityDecision.IsApprovalRequired); // Low risk does not require approval
        Assert.True(capabilityDecision.IsValidationMandatory);

        // 4. Execute Capability under Governed Lifecycle
        Assert.True(await lifecycleManager.TransitionToAsync(PluginLifecycleState.Executing, cancellationToken));
        var eventId = Guid.NewGuid().ToString("N");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await eventBus.PublishAsync(new PluginExecutionStartedEvent(eventId, timestamp, identity.PluginId, capability!.Id), cancellationToken);

        var context = new DefaultExecutionContext("generate", Array.Empty<string>());
        await capability.ValidateConfigurationAsync(cancellationToken);
        await capability.ValidateInputAsync(context, cancellationToken);
        var execResult = await capability.ExecuteAsync(context, cancellationToken);
        await capability.GenerateArtifactsAsync(context, cancellationToken);
        await capability.ValidateOutputAsync(context, cancellationToken);
        var evidence = await capability.ProduceValidationEvidenceAsync(context, cancellationToken);

        Assert.NotNull(execResult);
        Assert.Equal(0, execResult.StatusCode);
        Assert.Equal(3, evidence.Count);

        await eventBus.PublishAsync(new PluginExecutionCompletedEvent(
            Guid.NewGuid().ToString("N"), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), identity.PluginId, capability.Id, true), cancellationToken);
        
        await eventBus.PublishAsync(new PluginValidationCompletedEvent(
            Guid.NewGuid().ToString("N"), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), identity.PluginId, true, evidence.Count), cancellationToken);

        Assert.True(await lifecycleManager.TransitionToAsync(PluginLifecycleState.Active, cancellationToken));

        // 5. Capture & Verify Lifecycle Audit Events
        var history = eventBus.GetAuditHistory();
        Assert.NotNull(history);
        Assert.NotEmpty(history);
        Assert.True(history.Count >= 8);

        Assert.Contains(history, e => e is PluginRegisteredEvent);
        Assert.Contains(history, e => e is PluginActivatedEvent);
        Assert.Contains(history, e => e is PluginExecutionStartedEvent);
        Assert.Contains(history, e => e is PluginExecutionCompletedEvent);
        Assert.Contains(history, e => e is PluginValidationCompletedEvent);
        Assert.Contains(history, e => e is PluginStateTransitionEvent transition && transition.NewState == PluginLifecycleState.Executing);

        await plugin.ShutdownAsync(cancellationToken);
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
