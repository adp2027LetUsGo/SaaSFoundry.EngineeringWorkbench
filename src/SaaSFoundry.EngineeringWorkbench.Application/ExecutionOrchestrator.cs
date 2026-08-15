using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Planning;
using SaaSFoundry.EngineeringWorkbench.Governance;
using SaaSFoundry.EngineeringWorkbench.Infrastructure.Host;

namespace SaaSFoundry.EngineeringWorkbench.Application;

public sealed class ExecutionOrchestrator
{
    private readonly WorkbenchHost _host;
    private readonly IExecutionGovernanceEngine _governanceEngine;

    public ExecutionOrchestrator(WorkbenchHost host)
    {
        _host = host;
        _governanceEngine = new ExecutionGovernanceEngine();
    }

    public async Task<ExecutionRecord> OrchestrateExecutionAsync(
        EngineeringPlan plan, 
        ExecutionPlanIdentity identity, 
        ExecutionApproval approval, 
        string catalogHash, 
        CancellationToken ct)
    {
        var execId = "exec-" + DateTimeOffset.UtcNow.Ticks;
        var startedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var fingerprint = _governanceEngine.GenerateFingerprint(identity, catalogHash, "plan-hash-123");
        var governanceResult = _governanceEngine.ValidateApproval(identity, approval, fingerprint);
        
        if (!governanceResult.IsSuccessful)
        {
            return new ExecutionRecord(execId, plan.PlanId, ExecutionStatus.Rejected, startedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), Array.Empty<string>(), null);
        }

        Console.WriteLine($"[Orchestrator] Execution '{execId}' approved for Plan '{plan.PlanId}'. Fingerprint: {fingerprint.Hash}");
        
        var package = await _host.ExecutePlanAsync(plan, ct);

        var evidence = new ExecutionEvidence(
            package?.ArtifactPaths ?? new List<string>(),
            package?.Report?.Evidence?.ToList() ?? new List<SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence>(),
            new List<string> { "Execution completed." },
            new List<string>()
        );

        var finalRecord = new ExecutionRecord(execId, plan.PlanId, ExecutionStatus.Completed, startedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), new List<string>(), evidence);

        var storage = new LocalGovernanceStorage();
        await storage.SaveReportAsync(finalRecord, ct);
        
        return finalRecord;
    }
}
