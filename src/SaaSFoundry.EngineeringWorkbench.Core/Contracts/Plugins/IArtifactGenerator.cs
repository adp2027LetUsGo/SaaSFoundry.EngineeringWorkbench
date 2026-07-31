namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

public interface IArtifactGenerator
{
    System.Threading.Tasks.Task GenerateArtifactsAsync(System.Threading.CancellationToken cancellationToken);
    System.Collections.Generic.IReadOnlyList<string> ReportGeneratedFiles();
}
