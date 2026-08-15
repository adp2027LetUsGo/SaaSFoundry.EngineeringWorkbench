using System;
using System.Collections.Generic;

namespace SaaSFoundry.SDK.Core.Generators;

/// <summary>
/// Immutable engineering artifact dependency graph for packaging, deployment, and validation.
/// </summary>
public sealed record ArtifactDependencyGraph(
    IReadOnlyList<ArtifactDependencyNode> Nodes
)
{
    public static readonly ArtifactDependencyGraph Empty = new(Array.Empty<ArtifactDependencyNode>());
}
