using System;

namespace SaaSFoundry.SDK.Core.Diagnostics;

/// <summary>
/// Represents a deterministic validation diagnostic produced during artifact generation or structural validation.
/// </summary>
public sealed record ValidationDiagnostic(
    string Code,
    string Message,
    bool IsError,
    string? ArtifactId = null
);
