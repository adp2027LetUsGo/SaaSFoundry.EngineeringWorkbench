using System;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Policy;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent.Governance;

namespace SaaSFoundry.EngineeringWorkbench.AgentGovernance.Engine;

public sealed class AgentGovernanceEngine
{
    private readonly IAgentExecutionPolicy _policy;

    public IAgentExecutionPolicy Policy => _policy;

    public AgentGovernanceEngine(IAgentExecutionPolicy? policy = null)
    {
        _policy = policy ?? new StandardAgentExecutionPolicy();
    }

    public Task<AgentExecutionDecision> EvaluateRequestAsync(AgentExecutionRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        return _policy.EvaluateAsync(request, cancellationToken);
    }
}
