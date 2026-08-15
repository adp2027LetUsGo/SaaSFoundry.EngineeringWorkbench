using System;

namespace SaaSFoundry.SDK.Core.Diagnostics;

/// <summary>
/// Generic representation of validation evidence.
/// </summary>
public sealed record ValidationEvidence(
    string PluginId,
    string CapabilityId,
    string Stage,
    bool IsSuccess,
    string Message,
    DateTimeOffset Timestamp
);
