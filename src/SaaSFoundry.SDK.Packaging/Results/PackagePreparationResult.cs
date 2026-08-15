using System.Collections.Generic;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Packaging.Models;

namespace SaaSFoundry.SDK.Packaging.Results;

/// <summary>
/// Deterministic result pattern for package preparation.
/// </summary>
public sealed class PackagePreparationResult
{
    public bool IsSuccess { get; }
    public EngineeringPackageDescriptor? Package { get; }
    public IReadOnlyList<ValidationDiagnostic> Diagnostics { get; }

    private PackagePreparationResult(bool isSuccess, EngineeringPackageDescriptor? package, IReadOnlyList<ValidationDiagnostic> diagnostics)
    {
        IsSuccess = isSuccess;
        Package = package;
        Diagnostics = diagnostics ?? new List<ValidationDiagnostic>();
    }

    public static PackagePreparationResult Success(EngineeringPackageDescriptor package, IReadOnlyList<ValidationDiagnostic>? warnings = null)
    {
        return new PackagePreparationResult(true, package, warnings ?? new List<ValidationDiagnostic>());
    }

    public static PackagePreparationResult Failure(IReadOnlyList<ValidationDiagnostic> errors)
    {
        return new PackagePreparationResult(false, null, errors);
    }
}
