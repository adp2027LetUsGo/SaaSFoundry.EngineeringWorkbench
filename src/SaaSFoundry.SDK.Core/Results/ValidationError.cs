using System.Collections.Generic;
using System.Linq;

namespace SaaSFoundry.SDK.Core.Results;

/// <summary>
/// Immutable, deterministic validation error.
/// </summary>
public sealed record ValidationError(
    string Code,
    string Message,
    string? Context = null
);
