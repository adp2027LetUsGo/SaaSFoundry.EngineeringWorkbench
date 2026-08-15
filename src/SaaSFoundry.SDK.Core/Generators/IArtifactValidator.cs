using System.Collections.Generic;
using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.SDK.Core.Generators;

/// <summary>
/// Defines a contract for domain-specific artifact validation.
/// </summary>
public interface IArtifactValidator
{
    IEnumerable<ValidationDiagnostic> Validate(GeneratedArtifactDescriptor descriptor);
}
