using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;

namespace SaaSFoundry.EngineeringWorkbench.AgentRuntime.Lifecycle;

public sealed class AgentLifecycleManager : IAgentLifecycleManager
{
    private readonly IAgentEventBus _eventBus;
    private readonly object _lock = new();
    private AgentLifecycleState _currentState = AgentLifecycleState.Created;

    public string AgentId { get; }
    public AgentLifecycleState CurrentState
    {
        get
        {
            lock (_lock)
            {
                return _currentState;
            }
        }
    }

    public AgentLifecycleManager(string agentId, IAgentEventBus eventBus)
    {
        if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("AgentId cannot be null or empty.", nameof(agentId));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        AgentId = agentId;
    }

    public bool CanTransitionTo(AgentLifecycleState nextState)
    {
        lock (_lock)
        {
            return ValidateTransitions(_currentState, nextState);
        }
    }

    public bool ValidateTransitions(AgentLifecycleState sourceState, AgentLifecycleState targetState)
    {
        if (sourceState == targetState) return true;

        return (sourceState, targetState) switch
        {
            // Sequential registration & initialization phase
            (AgentLifecycleState.Created, AgentLifecycleState.Registered) => true,
            (AgentLifecycleState.Registered, AgentLifecycleState.Loaded) => true,
            (AgentLifecycleState.Loaded, AgentLifecycleState.Validated) => true,
            (AgentLifecycleState.Validated, AgentLifecycleState.Active) => true,

            // Active execution loop
            (AgentLifecycleState.Active, AgentLifecycleState.Planning) => true,
            (AgentLifecycleState.Active, AgentLifecycleState.Executing) => true,
            (AgentLifecycleState.Planning, AgentLifecycleState.Executing) => true,
            (AgentLifecycleState.Planning, AgentLifecycleState.Active) => true,
            (AgentLifecycleState.Executing, AgentLifecycleState.WaitingForInput) => true,
            (AgentLifecycleState.Executing, AgentLifecycleState.Completed) => true,
            (AgentLifecycleState.Executing, AgentLifecycleState.Active) => true,
            (AgentLifecycleState.WaitingForInput, AgentLifecycleState.Executing) => true,
            (AgentLifecycleState.WaitingForInput, AgentLifecycleState.Completed) => true,
            (AgentLifecycleState.WaitingForInput, AgentLifecycleState.Active) => true,
            (AgentLifecycleState.Completed, AgentLifecycleState.Active) => true,
            (AgentLifecycleState.Completed, AgentLifecycleState.Planning) => true,

            // Failure transitions
            (AgentLifecycleState.Planning, AgentLifecycleState.Failed) => true,
            (AgentLifecycleState.Executing, AgentLifecycleState.Failed) => true,
            (AgentLifecycleState.WaitingForInput, AgentLifecycleState.Failed) => true,
            (AgentLifecycleState.Failed, AgentLifecycleState.Active) => true,

            // Administrative transitions (Disabling / Deprecation)
            (_, AgentLifecycleState.Disabled) when sourceState != AgentLifecycleState.Deprecated => true,
            (AgentLifecycleState.Disabled, AgentLifecycleState.Active) => true,
            (_, AgentLifecycleState.Deprecated) => true,

            // All other out-of-order transitions rejected deterministically
            _ => false
        };
    }

    public async Task<bool> TransitionToAsync(AgentLifecycleState nextState, CancellationToken cancellationToken = default)
    {
        AgentLifecycleState prevState;
        lock (_lock)
        {
            if (!ValidateTransitions(_currentState, nextState))
            {
                return false;
            }

            prevState = _currentState;
            _currentState = nextState;
        }

        if (prevState != nextState)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string eventId = $"evt-trans-{Guid.NewGuid():N}";
            var transEvent = new AgentStateTransitionEvent(eventId, timestamp, AgentId, prevState, nextState);
            await _eventBus.PublishAsync(transEvent, cancellationToken);
        }

        return true;
    }

    public async Task<bool> RegisterAgentAsync(IAgentMetadataProvider agent, CancellationToken cancellationToken = default)
    {
        if (agent == null) throw new ArgumentNullException(nameof(agent));
        if (!await TransitionToAsync(AgentLifecycleState.Registered, cancellationToken)) return false;

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string eventId = $"evt-reg-{Guid.NewGuid():N}";
        await _eventBus.PublishAsync(new AgentRegisteredEvent(eventId, timestamp, AgentId, agent.Identity.Version), cancellationToken);
        return true;
    }

    public Task<bool> LoadAgentAsync(CancellationToken cancellationToken = default) =>
        TransitionToAsync(AgentLifecycleState.Loaded, cancellationToken);

    public Task<bool> ValidateAgentAsync(CancellationToken cancellationToken = default) =>
        TransitionToAsync(AgentLifecycleState.Validated, cancellationToken);

    public async Task<bool> ActivateAgentAsync(CancellationToken cancellationToken = default)
    {
        if (!await TransitionToAsync(AgentLifecycleState.Active, cancellationToken)) return false;

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string eventId = $"evt-act-{Guid.NewGuid():N}";
        await _eventBus.PublishAsync(new AgentActivatedEvent(eventId, timestamp, AgentId), cancellationToken);
        return true;
    }

    public async Task<bool> StartPlanningAsync(CancellationToken cancellationToken = default)
    {
        if (!await TransitionToAsync(AgentLifecycleState.Planning, cancellationToken)) return false;

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string eventId = $"evt-plan-{Guid.NewGuid():N}";
        await _eventBus.PublishAsync(new AgentPlanningStartedEvent(eventId, timestamp, AgentId), cancellationToken);
        return true;
    }

    public async Task<bool> StartExecutionAsync(CancellationToken cancellationToken = default)
    {
        if (!await TransitionToAsync(AgentLifecycleState.Executing, cancellationToken)) return false;

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string eventId = $"evt-exec-{Guid.NewGuid():N}";
        await _eventBus.PublishAsync(new AgentExecutionStartedEvent(eventId, timestamp, AgentId, "primary"), cancellationToken);
        return true;
    }

    public async Task<bool> StartExecutionAsync(string capabilityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(capabilityId)) capabilityId = "primary";
        if (!await TransitionToAsync(AgentLifecycleState.Executing, cancellationToken)) return false;

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string eventId = $"evt-exec-{Guid.NewGuid():N}";
        await _eventBus.PublishAsync(new AgentExecutionStartedEvent(eventId, timestamp, AgentId, capabilityId), cancellationToken);
        return true;
    }

    public Task<bool> WaitForInputAsync(CancellationToken cancellationToken = default) =>
        TransitionToAsync(AgentLifecycleState.WaitingForInput, cancellationToken);

    public async Task<bool> CompleteExecutionAsync(CancellationToken cancellationToken = default)
    {
        if (!await TransitionToAsync(AgentLifecycleState.Completed, cancellationToken)) return false;

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string eventId = $"evt-comp-{Guid.NewGuid():N}";
        await _eventBus.PublishAsync(new AgentExecutionCompletedEvent(eventId, timestamp, AgentId, "primary", IsSuccess: true), cancellationToken);
        return true;
    }

    public async Task<bool> CompleteExecutionAsync(string capabilityId, bool isSuccess, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(capabilityId)) capabilityId = "primary";
        if (!await TransitionToAsync(AgentLifecycleState.Completed, cancellationToken)) return false;

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string eventId = $"evt-comp-{Guid.NewGuid():N}";
        await _eventBus.PublishAsync(new AgentExecutionCompletedEvent(eventId, timestamp, AgentId, capabilityId, isSuccess), cancellationToken);
        return true;
    }

    public async Task<bool> FailExecutionAsync(string reason, CancellationToken cancellationToken = default)
    {
        if (!await TransitionToAsync(AgentLifecycleState.Failed, cancellationToken)) return false;

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string eventId = $"evt-fail-{Guid.NewGuid():N}";
        await _eventBus.PublishAsync(new AgentFailedEvent(eventId, timestamp, AgentId, reason ?? "Unknown failure"), cancellationToken);
        return true;
    }

    public Task<bool> DisableAgentAsync(CancellationToken cancellationToken = default) =>
        TransitionToAsync(AgentLifecycleState.Disabled, cancellationToken);
}
