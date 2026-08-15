#nullable enable

using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;
using SaaSFoundry.EngineeringWorkbench.Execution;
using SaaSFoundry.EngineeringWorkbench.Execution.Capabilities;
using SaaSFoundry.EngineeringWorkbench.Planning.DAG;

namespace SaaSFoundry.EngineeringWorkbench.AgentCoordinator;

/// <summary>
/// Defines the canonical contract for orchestrating and dispatching multi-agent mission DAG executions.
/// </summary>
public interface IAgentCoordinator
{
    /// <summary>Explicitly registers a governed agent instance for mission execution without dynamic reflection.</summary>
    void RegisterAgent(IAgentOrchestrator agent);
    /// <summary>Explicitly registers a verified capability descriptor supported by participating agents.</summary>
    void RegisterCapability(AgentCapabilityDescriptor capability);
    /// <summary>Executes a validated mission DAG plan across registered agent capabilities.</summary>
    Task<MissionResult> ExecuteMissionAsync(ExecutionPlan plan, MissionExecutionContext context, CancellationToken cancellationToken = default);
    /// <summary>Requests graceful cancellation and termination of an active mission execution.</summary>
    Task<bool> CancelMissionAsync(string missionId);
    /// <summary>Retrieves the current execution lifecycle state for the specified mission identifier.</summary>
    MissionExecutionStatus GetMissionStatus(string missionId);
}
