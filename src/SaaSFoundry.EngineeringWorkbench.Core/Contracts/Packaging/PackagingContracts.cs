using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;
using System.Collections.Generic;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Packaging;

public sealed record EngineeringPackage(
    string PackageId,
    ValidationReport Report,
    IReadOnlyList<string> ArtifactPaths
);

public interface IPackageExporter
{
    string ExporterName { get; }
    System.Threading.Tasks.Task ExportAsync(EngineeringPackage package, string destinationPath);
}

public interface IPackagingEngine
{
    EngineeringPackage CreatePackage(string packageId, ValidationReport report, IReadOnlyList<string> artifacts);
    System.Threading.Tasks.Task ExportPackageAsync(EngineeringPackage package, IPackageExporter exporter, string destinationPath);
}
