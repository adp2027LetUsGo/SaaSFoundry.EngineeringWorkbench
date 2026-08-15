using SaaSFoundry.EngineeringWorkbench.PluginRuntime.Execution;
using SaaSFoundry.EngineeringWorkbench.PluginRuntime.Registration;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Planning;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Packaging;
using SaaSFoundry.EngineeringWorkbench.Validation;
using SaaSFoundry.EngineeringWorkbench.Packaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SaaSFoundry.EngineeringWorkbench.Infrastructure.Host;

public sealed class WorkbenchHost
{
    private readonly PluginRegistry _registry;
    private readonly PluginExecutionEngine _engine;

    public System.Collections.Generic.IReadOnlyCollection<IEngineeringPlugin> Plugins => _registry.Plugins;

    public WorkbenchHost(IServiceProvider services)
    {
        _registry = PluginCompositionRoot.Compose();
        _engine = new PluginExecutionEngine(_registry, services);
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        return _engine.InitializeAllAsync(cancellationToken);
    }

    public Task<CapabilityExecutionResult?> ExecuteCapabilityAsync(string pluginId, string capabilityId, IPluginExecutionContext context, CancellationToken cancellationToken)
    {
        return _engine.ExecuteCapabilityAsync(pluginId, capabilityId, context, cancellationToken);
    }

    public async Task<EngineeringPackage?> ExecutePlanAsync(EngineeringPlan plan, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[WorkbenchHost] Executing Plan: {plan.PlanId}");
        var allEvidence = new List<SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence>();
        var allArtifacts = new List<string>();

        foreach (var stage in plan.Stages)
        {
            Console.WriteLine($"[WorkbenchHost] Executing Stage {stage.StageNumber} ({stage.Tasks.Count} tasks)");
            
            foreach (var task in stage.Tasks)
            {
                Console.WriteLine($"  -> Task {task.TaskId} [{task.PluginId}.{task.CapabilityId} {task.Operation}]");
                var context = new DefaultExecutionContext(task.Operation, Array.Empty<string>());
                
                var result = await _engine.ExecuteCapabilityAsync(task.PluginId, task.CapabilityId, context, cancellationToken);
                
                if (result != null)
                {
                    allEvidence.AddRange(result.Evidence);
                    allArtifacts.AddRange(result.Artifacts);
                }
                else
                {
                    Console.WriteLine($"  [Warning] Task {task.TaskId} failed to resolve capability.");
                }
            }
        }

        var validationEngine = new ValidationEngine();
        var report = validationEngine.AggregateAndValidate(allEvidence);
        
        var packagingEngine = new PackagingEngine();
        var packageId = $"pkg-plan-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var package = packagingEngine.CreatePackage(packageId, report, allArtifacts);

        return package;
    }

    public Task ShutdownAsync(CancellationToken cancellationToken)
    {
        return _engine.ShutdownAllAsync(cancellationToken);
    }
}
