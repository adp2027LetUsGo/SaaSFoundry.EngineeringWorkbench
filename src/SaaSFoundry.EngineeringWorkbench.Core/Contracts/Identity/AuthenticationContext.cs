namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;

/// <summary>
/// Represents the state and mechanism of the current authentication session.
/// Architecture Freeze v1.0.1
/// </summary>
public record AuthenticationContext(
    string AuthenticationScheme,
    string AuthenticationStatus);
