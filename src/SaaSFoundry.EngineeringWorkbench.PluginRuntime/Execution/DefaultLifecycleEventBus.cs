using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Events;

namespace SaaSFoundry.EngineeringWorkbench.PluginRuntime.Execution;

public sealed class DefaultLifecycleEventBus : ILifecycleEventBus
{
    private readonly ConcurrentQueue<ILifecycleEvent> _events = new();

    public Task PublishAsync(ILifecycleEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        _events.Enqueue(@event);
        return Task.CompletedTask;
    }

    public IReadOnlyList<ILifecycleEvent> GetAuditHistory()
    {
        return _events.ToArray();
    }

    public IReadOnlyList<ILifecycleEvent> GetEventsForPlugin(string pluginId)
    {
        if (string.IsNullOrEmpty(pluginId))
        {
            return Array.Empty<ILifecycleEvent>();
        }

        return _events.Where(e => string.Equals(e.PluginId, pluginId, StringComparison.OrdinalIgnoreCase)).ToArray();
    }
}
