using System.Threading;
using System.Threading.Tasks;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Lifecycle;

public enum PluginLifecycleState
{
    Created = 0,
    Registered = 1,
    Loaded = 2,
    Validated = 3,
    Active = 4,
    Executing = 5,
    Disabled = 6,
    Deprecated = 7
}

public interface IPluginLifecycleManager
{
    string PluginId { get; }
    PluginLifecycleState CurrentState { get; }
    bool CanTransitionTo(PluginLifecycleState nextState);
    Task<bool> TransitionToAsync(PluginLifecycleState nextState, CancellationToken cancellationToken = default);
}
