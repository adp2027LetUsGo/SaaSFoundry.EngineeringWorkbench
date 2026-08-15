namespace SaaSFoundry.SDK.Core.Generators;

/// <summary>
/// Canonical artifact classification model replacing free-text types with deterministic enumerations.
/// </summary>
public enum ArtifactCategory
{
    SourceCode,
    Configuration,
    Documentation,
    Dashboard,
    Alert,
    Validation,
    Evidence,
    Manifest,
    Infrastructure,
    Deployment,
    Template,
    Script,
    Package,
    Report,
    Metadata
}
