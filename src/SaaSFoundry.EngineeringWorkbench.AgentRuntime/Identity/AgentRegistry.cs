using System;
using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;

namespace SaaSFoundry.EngineeringWorkbench.AgentRuntime.Identity;

public sealed class AgentRegistry
{
    private readonly Dictionary<string, IAgentMetadataProvider> _agents = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public void Register(IAgentMetadataProvider agent)
    {
        if (agent == null) throw new ArgumentNullException(nameof(agent));
        if (agent.Identity == null) throw new ArgumentException("Agent identity cannot be null.", nameof(agent));
        if (string.IsNullOrWhiteSpace(agent.Identity.AgentId)) throw new ArgumentException("AgentId cannot be null or empty.", nameof(agent));
        if (string.IsNullOrWhiteSpace(agent.Identity.Version)) throw new ArgumentException("Agent version cannot be null or empty.", nameof(agent));
        if (string.IsNullOrWhiteSpace(agent.Identity.Author)) throw new ArgumentException("Agent author cannot be null or empty.", nameof(agent));
        if (string.IsNullOrWhiteSpace(agent.Identity.Fingerprint)) throw new ArgumentException("Agent fingerprint cannot be null or empty.", nameof(agent));

        if (agent.Metadata == null) throw new ArgumentException("Agent metadata cannot be null.", nameof(agent));
        if (string.IsNullOrWhiteSpace(agent.Metadata.Name)) throw new ArgumentException("Agent metadata name cannot be null or empty.", nameof(agent));
        if (agent.Metadata.Capabilities == null) throw new ArgumentException("Agent metadata capabilities list cannot be null.", nameof(agent));
        if (agent.Metadata.RequiredPermissions == null) throw new ArgumentException("Agent metadata permissions list cannot be null.", nameof(agent));

        lock (_lock)
        {
            if (_agents.ContainsKey(agent.Identity.AgentId))
            {
                throw new InvalidOperationException($"An agent with ID '{agent.Identity.AgentId}' is already registered.");
            }

            _agents[agent.Identity.AgentId] = agent;
        }
    }

    public bool TryGetAgent(string agentId, out IAgentMetadataProvider? agent)
    {
        if (string.IsNullOrWhiteSpace(agentId)) throw new ArgumentException("AgentId cannot be null or empty.", nameof(agentId));

        lock (_lock)
        {
            return _agents.TryGetValue(agentId, out agent);
        }
    }

    public AgentMetadata GetMetadata(string agentId)
    {
        if (TryGetAgent(agentId, out var agent) && agent != null)
        {
            return agent.Metadata;
        }
        throw new KeyNotFoundException($"Agent with ID '{agentId}' was not found in the registry.");
    }

    public IReadOnlyList<IAgentMetadataProvider> GetAllRegisteredAgents()
    {
        lock (_lock)
        {
            return _agents.Values.ToList().AsReadOnly();
        }
    }
}
