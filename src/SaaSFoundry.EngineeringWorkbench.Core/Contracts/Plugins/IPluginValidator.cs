namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

public interface IPluginValidator
{
    System.Threading.Tasks.Task ValidateConfigurationAsync(System.Threading.CancellationToken cancellationToken);
    System.Threading.Tasks.Task ValidateInputAsync(System.Threading.CancellationToken cancellationToken);
    System.Threading.Tasks.Task ValidateOutputAsync(System.Threading.CancellationToken cancellationToken);
    System.Threading.Tasks.Task ProduceValidationEvidenceAsync(System.Threading.CancellationToken cancellationToken);
}
