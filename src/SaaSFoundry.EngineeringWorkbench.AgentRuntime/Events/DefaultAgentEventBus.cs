using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;

namespace SaaSFoundry.EngineeringWorkbench.AgentRuntime.Events;

public sealed class DefaultAgentEventBus : IAgentEventBus
{
    private readonly List<IAgentLifecycleEvent> _events = new();
    private readonly object _lock = new();

    public Task PublishAsync(IAgentLifecycleEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        lock (_lock)
        {
            _events.Add(@event);
        }
        return Task.CompletedTask;
    }

    public IReadOnlyList<IAgentLifecycleEvent> GetAuditHistory()
    {
        lock (_lock)
        {
            return _events.ToList().AsReadOnly();
        }
    }

    public IReadOnlyList<IAgentLifecycleEvent> GetEventsForAgent(string agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("AgentId cannot be null or empty.", nameof(agentId));

        lock (_lock)
        {
            return _events.Where(e => string.Equals(e.AgentId, agentId, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
        }
    }
}
