using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using System.Threading.Tasks;

namespace SaaSFoundry.EngineeringWorkbench.Plugins.Observability.Capabilities;

public sealed class ValidationCapability : IPluginCapability
{
    public string Id => "";
    public string Description => "Minimal reference capability for Validation";
    public System.Collections.Generic.IReadOnlyList<string> SupportedOperations => new[] { "generate" };

    public Task ValidateConfigurationAsync(System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
    public Task ValidateInputAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
    
    public Task<IPluginExecutionResult> ExecuteAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken)
    {
        return Task.FromResult<IPluginExecutionResult>(new Result(0));
    }
    
    public Task GenerateArtifactsAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
    public Task ValidateOutputAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<System.Collections.Generic.IReadOnlyCollection<SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence>> ProduceValidationEvidenceAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken) 
        => Task.FromResult<System.Collections.Generic.IReadOnlyCollection<SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence>>(System.Array.Empty<SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence>());
    public System.Collections.Generic.IReadOnlyList<string> ReportGeneratedFiles() => System.Array.Empty<string>();
}

file class Result : IPluginExecutionResult
{
    public Result(int statusCode) => StatusCode = statusCode;
    public int StatusCode { get; }
}

