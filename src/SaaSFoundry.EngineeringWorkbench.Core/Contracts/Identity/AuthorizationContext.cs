namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;

using System.Collections.Generic;

/// <summary>
/// Represents the evaluated permissions for the requested operation (the "What").
/// Architecture Freeze v1.0.1
/// </summary>
public record AuthorizationContext(
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Roles);
