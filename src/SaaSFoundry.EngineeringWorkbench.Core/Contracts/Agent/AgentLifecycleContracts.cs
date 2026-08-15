using System.Threading;
using System.Threading.Tasks;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;

public enum AgentLifecycleState
{
    Created = 0,
    Registered = 1,
    Loaded = 2,
    Validated = 3,
    Active = 4,
    Planning = 5,
    Executing = 6,
    WaitingForInput = 7,
    Completed = 8,
    Failed = 9,
    Disabled = 10,
    Deprecated = 11
}

public interface IAgentLifecycleManager
{
    string AgentId { get; }
    AgentLifecycleState CurrentState { get; }
    bool CanTransitionTo(AgentLifecycleState nextState);
    bool ValidateTransitions(AgentLifecycleState sourceState, AgentLifecycleState targetState);
    Task<bool> TransitionToAsync(AgentLifecycleState nextState, CancellationToken cancellationToken = default);

    Task<bool> RegisterAgentAsync(IAgentMetadataProvider agent, CancellationToken cancellationToken = default);
    Task<bool> LoadAgentAsync(CancellationToken cancellationToken = default);
    Task<bool> ValidateAgentAsync(CancellationToken cancellationToken = default);
    Task<bool> ActivateAgentAsync(CancellationToken cancellationToken = default);
    Task<bool> StartPlanningAsync(CancellationToken cancellationToken = default);
    Task<bool> StartExecutionAsync(CancellationToken cancellationToken = default);
    Task<bool> WaitForInputAsync(CancellationToken cancellationToken = default);
    Task<bool> CompleteExecutionAsync(CancellationToken cancellationToken = default);
    Task<bool> FailExecutionAsync(string reason, CancellationToken cancellationToken = default);
    Task<bool> DisableAgentAsync(CancellationToken cancellationToken = default);
}
