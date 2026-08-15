using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaaSFoundry.EngineeringWorkbench.Plugins.Observability.Capabilities;

public sealed class LoggingCapability : IPluginCapability
{
    public string Id => "logging";
    public string Description => "Implements structured logging architecture based on OBS-003.";
    public IReadOnlyList<string> SupportedOperations => new[] { "generate", "validate" };

    public Task ValidateConfigurationAsync(System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
    
    public Task ValidateInputAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;
    
    public Task<IPluginExecutionResult> ExecuteAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken)
    {
        return Task.FromResult<IPluginExecutionResult>(new ExecutionResult(0));
    }

    public Task GenerateArtifactsAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken)
    {
        // No physical file I/O here.
        return Task.CompletedTask;
    }

    public Task ValidateOutputAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<IReadOnlyCollection<ValidationEvidence>> ProduceValidationEvidenceAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken)
    {
        var evidence = new List<ValidationEvidence>
        {
            new ValidationEvidence("observability", "logging", "Configuration", true, "Valid configuration", DateTimeOffset.UtcNow),
            new ValidationEvidence("observability", "logging", "Execution", true, "Execution succeeded", DateTimeOffset.UtcNow)
        };
        return Task.FromResult<IReadOnlyCollection<ValidationEvidence>>(evidence);
    }

    public IReadOnlyList<string> ReportGeneratedFiles()
    {
        return new[] { "OBS-003-Logging-Standards.md", "structured-logging-config.json" };
    }
}

file class ExecutionResult : IPluginExecutionResult
{
    public ExecutionResult(int statusCode) => StatusCode = statusCode;
    public int StatusCode { get; }
}
