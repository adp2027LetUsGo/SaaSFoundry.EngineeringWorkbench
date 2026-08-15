using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

public interface IPluginCapability
{
    string Id { get; }
    string Description { get; }
    System.Collections.Generic.IReadOnlyList<string> SupportedOperations { get; }

    System.Threading.Tasks.Task ValidateConfigurationAsync(System.Threading.CancellationToken cancellationToken);
    System.Threading.Tasks.Task ValidateInputAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken);
    
    System.Threading.Tasks.Task<IPluginExecutionResult> ExecuteAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken);
    
    System.Threading.Tasks.Task GenerateArtifactsAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken);
    
    System.Threading.Tasks.Task ValidateOutputAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken);
    
    // CRITICAL DEFECT FIX: The capability must return its evidence so the Validation Engine can aggregate it.
    System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<ValidationEvidence>> ProduceValidationEvidenceAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken);
    
    System.Collections.Generic.IReadOnlyList<string> ReportGeneratedFiles();
}
