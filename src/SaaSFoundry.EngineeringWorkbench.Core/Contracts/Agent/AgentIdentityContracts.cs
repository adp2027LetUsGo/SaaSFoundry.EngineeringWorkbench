using System.Collections.Generic;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;

public sealed record AgentIdentity(
    string AgentId,
    string Version,
    string Author,
    string Fingerprint,
    long CreatedTimestamp
);

public sealed record AgentCapabilityIdentity(
    string AgentId,
    string CapabilityId,
    string Version
);

public sealed record AgentVersionIdentity(
    string SemanticVersion,
    long BuildTimestamp,
    string CompatibilityTarget
);

public sealed record AgentMetadata(
    AgentIdentity Identity,
    string Name,
    string Description,
    string Purpose,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> Dependencies,
    IReadOnlyList<string> RequiredPermissions,
    string CompatibilityInformation
);

public interface IAgentMetadataProvider
{
    AgentIdentity Identity { get; }
    AgentMetadata Metadata { get; }
}
