using System;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Events;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Lifecycle;

namespace SaaSFoundry.EngineeringWorkbench.PluginRuntime.Execution;

public sealed class PluginLifecycleManager : IPluginLifecycleManager
{
    private readonly string _pluginVersion;
    private readonly ILifecycleEventBus? _eventBus;
    private PluginLifecycleState _currentState = PluginLifecycleState.Created;

    public PluginLifecycleManager(string pluginId, string pluginVersion, ILifecycleEventBus? eventBus = null)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            throw new ArgumentException("PluginId cannot be null or whitespace.", nameof(pluginId));

        PluginId = pluginId;
        _pluginVersion = pluginVersion ?? "1.0.0";
        _eventBus = eventBus;
    }

    public string PluginId { get; }

    public PluginLifecycleState CurrentState => _currentState;

    public bool CanTransitionTo(PluginLifecycleState nextState)
    {
        if (_currentState == nextState)
        {
            return false;
        }

        return _currentState switch
        {
            PluginLifecycleState.Created => nextState is PluginLifecycleState.Registered or PluginLifecycleState.Disabled or PluginLifecycleState.Deprecated,
            PluginLifecycleState.Registered => nextState is PluginLifecycleState.Loaded or PluginLifecycleState.Disabled or PluginLifecycleState.Deprecated,
            PluginLifecycleState.Loaded => nextState is PluginLifecycleState.Validated or PluginLifecycleState.Disabled or PluginLifecycleState.Deprecated,
            PluginLifecycleState.Validated => nextState is PluginLifecycleState.Active or PluginLifecycleState.Disabled or PluginLifecycleState.Deprecated,
            PluginLifecycleState.Active => nextState is PluginLifecycleState.Executing or PluginLifecycleState.Disabled or PluginLifecycleState.Deprecated,
            PluginLifecycleState.Executing => nextState is PluginLifecycleState.Active or PluginLifecycleState.Disabled or PluginLifecycleState.Deprecated,
            PluginLifecycleState.Disabled => nextState is PluginLifecycleState.Loaded or PluginLifecycleState.Deprecated,
            PluginLifecycleState.Deprecated => false, // Terminal state
            _ => false
        };
    }

    public async Task<bool> TransitionToAsync(PluginLifecycleState nextState, CancellationToken cancellationToken = default)
    {
        if (!CanTransitionTo(nextState))
        {
            return false;
        }

        var previousState = _currentState;
        _currentState = nextState;

        if (_eventBus != null)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var eventId = Guid.NewGuid().ToString("N");

            await _eventBus.PublishAsync(new PluginStateTransitionEvent(
                eventId, timestamp, PluginId, previousState, nextState), cancellationToken);

            if (nextState == PluginLifecycleState.Registered)
            {
                await _eventBus.PublishAsync(new PluginRegisteredEvent(
                    Guid.NewGuid().ToString("N"), timestamp, PluginId, _pluginVersion), cancellationToken);
            }
            else if (nextState == PluginLifecycleState.Active)
            {
                await _eventBus.PublishAsync(new PluginActivatedEvent(
                    Guid.NewGuid().ToString("N"), timestamp, PluginId), cancellationToken);
            }
        }

        return true;
    }
}
