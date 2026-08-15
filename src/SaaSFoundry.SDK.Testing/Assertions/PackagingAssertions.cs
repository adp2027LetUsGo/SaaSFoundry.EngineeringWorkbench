using System;
using SaaSFoundry.SDK.Packaging.Results;

namespace SaaSFoundry.SDK.Testing.Assertions;

public static class PackagingAssertions
{
    public static void AssertValidPackage(PackagePreparationResult? result)
    {
        if (result == null)
            throw new InvalidOperationException("PackagePreparationResult is null.");

        if (!result.IsSuccess)
        {
            var msg = result.Diagnostics != null ? string.Join(", ", result.Diagnostics) : "Unknown error";
            throw new InvalidOperationException($"Package preparation failed: {msg}");
        }

        if (result.Package == null)
            throw new InvalidOperationException("Prepared package is null despite success status.");

        if (string.IsNullOrWhiteSpace(result.Package.PackageId))
            throw new InvalidOperationException("PackageId is missing.");

        if (string.IsNullOrWhiteSpace(result.Package.PackageHash))
            throw new InvalidOperationException("PackageHash is missing.");

        if (!result.Package.PackageHash.StartsWith("SHA256:", StringComparison.Ordinal))
            throw new InvalidOperationException($"PackageHash does not start with SHA256: '{result.Package.PackageHash}'");
    }
}
