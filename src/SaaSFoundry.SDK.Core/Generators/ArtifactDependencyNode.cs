using System.Collections.Generic;

namespace SaaSFoundry.SDK.Core.Generators;

/// <summary>
/// Represents a node within the immutable artifact dependency graph.
/// </summary>
public sealed record ArtifactDependencyNode(
    string ArtifactId,
    IReadOnlyList<string> Dependencies
);
