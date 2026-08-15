using System;
using System.Collections.Generic;

namespace SaaSFoundry.SDK.Core.Generators;

/// <summary>
/// Describes an engineering artifact produced by a plugin capability without generating physical files directly.
/// </summary>
public sealed record GeneratedArtifactDescriptor(
    string ArtifactId,
    string FileName,
    string RelativePath,
    string ContentType,
    string CapabilityId,
    string CanonReference,
    string ImplementationReference,
    string Description,
    string Generator,
    string ValidationEvidenceId,
    string Content,
    string? Hash = null,
    ArtifactCategory Category = ArtifactCategory.Metadata,
    IReadOnlyList<string>? Dependencies = null
);
