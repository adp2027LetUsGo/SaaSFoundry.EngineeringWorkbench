using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Lifecycle;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Events;

public interface ILifecycleEvent
{
    string EventId { get; }
    long Timestamp { get; }
    string PluginId { get; }
    string EventType { get; }
}

public sealed record PluginRegisteredEvent(string EventId, long Timestamp, string PluginId, string Version) : ILifecycleEvent
{
    public string EventType => nameof(PluginRegisteredEvent);
}

public sealed record PluginActivatedEvent(string EventId, long Timestamp, string PluginId) : ILifecycleEvent
{
    public string EventType => nameof(PluginActivatedEvent);
}

public sealed record PluginExecutionStartedEvent(string EventId, long Timestamp, string PluginId, string CapabilityId) : ILifecycleEvent
{
    public string EventType => nameof(PluginExecutionStartedEvent);
}

public sealed record PluginExecutionCompletedEvent(string EventId, long Timestamp, string PluginId, string CapabilityId, bool IsSuccess) : ILifecycleEvent
{
    public string EventType => nameof(PluginExecutionCompletedEvent);
}

public sealed record PluginValidationCompletedEvent(string EventId, long Timestamp, string PluginId, bool IsPassed, int EvidenceCount) : ILifecycleEvent
{
    public string EventType => nameof(PluginValidationCompletedEvent);
}

public sealed record PluginStateTransitionEvent(string EventId, long Timestamp, string PluginId, PluginLifecycleState PreviousState, PluginLifecycleState NewState) : ILifecycleEvent
{
    public string EventType => nameof(PluginStateTransitionEvent);
}

public interface ILifecycleEventBus
{
    Task PublishAsync(ILifecycleEvent @event, CancellationToken cancellationToken = default);
    IReadOnlyList<ILifecycleEvent> GetAuditHistory();
    IReadOnlyList<ILifecycleEvent> GetEventsForPlugin(string pluginId);
}
