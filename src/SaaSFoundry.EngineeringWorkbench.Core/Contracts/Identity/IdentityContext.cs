namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;

using System.Collections.Generic;

/// <summary>
/// Represents the authenticated caller's identity (the "Who").
/// Architecture Freeze v1.0.1
/// </summary>
public record IdentityContext(
    string SubjectId,
    string IdentityType,
    IReadOnlyDictionary<string, string> Claims,
    string TenantAssociation);
