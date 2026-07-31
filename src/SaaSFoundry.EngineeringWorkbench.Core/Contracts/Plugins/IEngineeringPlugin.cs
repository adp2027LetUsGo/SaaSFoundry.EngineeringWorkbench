namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

public interface IEngineeringPlugin
{
    IPluginManifest Manifest { get; }
    System.Collections.Generic.IReadOnlyCollection<IPluginCapability> Capabilities { get; }
    IPluginValidator Validator { get; }
    IPluginExecutor Executor { get; }
    IArtifactGenerator ArtifactGenerator { get; }

    System.Threading.Tasks.Task InitializeAsync(System.IServiceProvider services, System.Threading.CancellationToken cancellationToken);
    System.Threading.Tasks.Task ShutdownAsync(System.Threading.CancellationToken cancellationToken);
}
