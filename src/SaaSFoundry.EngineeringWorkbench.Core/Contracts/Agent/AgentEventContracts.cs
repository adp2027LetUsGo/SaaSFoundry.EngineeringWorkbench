using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;

public interface IAgentLifecycleEvent
{
    string EventId { get; }
    long Timestamp { get; }
    string AgentId { get; }
    string EventType { get; }
}

public sealed record AgentRegisteredEvent(string EventId, long Timestamp, string AgentId, string Version) : IAgentLifecycleEvent
{
    public string EventType => nameof(AgentRegisteredEvent);
}

public sealed record AgentActivatedEvent(string EventId, long Timestamp, string AgentId) : IAgentLifecycleEvent
{
    public string EventType => nameof(AgentActivatedEvent);
}

public sealed record AgentPlanningStartedEvent(string EventId, long Timestamp, string AgentId) : IAgentLifecycleEvent
{
    public string EventType => nameof(AgentPlanningStartedEvent);
}

public sealed record AgentExecutionStartedEvent(string EventId, long Timestamp, string AgentId, string CapabilityId) : IAgentLifecycleEvent
{
    public string EventType => nameof(AgentExecutionStartedEvent);
}

public sealed record AgentExecutionCompletedEvent(string EventId, long Timestamp, string AgentId, string CapabilityId, bool IsSuccess) : IAgentLifecycleEvent
{
    public string EventType => nameof(AgentExecutionCompletedEvent);
}

public sealed record AgentFailedEvent(string EventId, long Timestamp, string AgentId, string Reason) : IAgentLifecycleEvent
{
    public string EventType => nameof(AgentFailedEvent);
}

public sealed record AgentStateTransitionEvent(string EventId, long Timestamp, string AgentId, AgentLifecycleState PreviousState, AgentLifecycleState NewState) : IAgentLifecycleEvent
{
    public string EventType => nameof(AgentStateTransitionEvent);
}

public interface IAgentEventBus
{
    Task PublishAsync(IAgentLifecycleEvent @event, CancellationToken cancellationToken = default);
    IReadOnlyList<IAgentLifecycleEvent> GetAuditHistory();
    IReadOnlyList<IAgentLifecycleEvent> GetEventsForAgent(string agentId);
}
