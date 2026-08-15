using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Planning;

namespace SaaSFoundry.EngineeringWorkbench.Application;

public sealed class LocalGovernanceStorage
{
    private readonly string _basePath;

    public LocalGovernanceStorage(string basePath = "output/executions")
    {
        _basePath = basePath;
        Directory.CreateDirectory(Path.Combine(_basePath, "plans"));
        Directory.CreateDirectory(Path.Combine(_basePath, "approvals"));
        Directory.CreateDirectory(Path.Combine(_basePath, "reports"));
    }

    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task SavePlanAsync(EngineeringPlan plan, ExecutionPlanIdentity identity, CancellationToken ct)
    {
        var wrapper = new PlanStorageWrapper { Plan = plan, Identity = identity };
        var path = Path.Combine(_basePath, "plans", $"{plan.PlanId}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(wrapper, Options), ct);
    }

    public async Task<PlanStorageWrapper?> LoadPlanAsync(string planId, CancellationToken ct)
    {
        var path = Path.Combine(_basePath, "plans", $"{planId}.json");
        if (!File.Exists(path)) return null;
        var content = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<PlanStorageWrapper>(content, Options);
    }

    public async Task<IReadOnlyList<PlanStorageWrapper>> GetAllPlansAsync(CancellationToken ct)
    {
        var results = new List<PlanStorageWrapper>();
        foreach (var file in Directory.GetFiles(Path.Combine(_basePath, "plans"), "*.json"))
        {
            var content = await File.ReadAllTextAsync(file, ct);
            var item = JsonSerializer.Deserialize<PlanStorageWrapper>(content, Options);
            if (item != null) results.Add(item);
        }
        return results;
    }

    public async Task SaveApprovalAsync(ExecutionApproval approval, CancellationToken ct)
    {
        var path = Path.Combine(_basePath, "approvals", $"{approval.PlanId}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(approval, Options), ct);
    }

    public async Task<ExecutionApproval?> LoadApprovalAsync(string planId, CancellationToken ct)
    {
        var path = Path.Combine(_basePath, "approvals", $"{planId}.json");
        if (!File.Exists(path)) return null;
        var content = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<ExecutionApproval>(content, Options);
    }

    public async Task<IReadOnlyList<ExecutionApproval>> GetAllApprovalsAsync(CancellationToken ct)
    {
        var results = new List<ExecutionApproval>();
        foreach (var file in Directory.GetFiles(Path.Combine(_basePath, "approvals"), "*.json"))
        {
            var content = await File.ReadAllTextAsync(file, ct);
            var item = JsonSerializer.Deserialize<ExecutionApproval>(content, Options);
            if (item != null) results.Add(item);
        }
        return results;
    }

    public async Task SaveReportAsync(ExecutionRecord record, CancellationToken ct)
    {
        var path = Path.Combine(_basePath, "reports", $"execution-report-{record.ExecutionId}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(record, Options), ct);
    }

    public async Task<ExecutionRecord?> LoadReportAsync(string executionId, CancellationToken ct)
    {
        var path = Path.Combine(_basePath, "reports", $"execution-report-{executionId}.json");
        if (!File.Exists(path)) return null;
        var content = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<ExecutionRecord>(content, Options);
    }

    public async Task<IReadOnlyList<ExecutionRecord>> GetAllReportsAsync(CancellationToken ct)
    {
        var results = new List<ExecutionRecord>();
        foreach (var file in Directory.GetFiles(Path.Combine(_basePath, "reports"), "*.json"))
        {
            var content = await File.ReadAllTextAsync(file, ct);
            var item = JsonSerializer.Deserialize<ExecutionRecord>(content, Options);
            if (item != null) results.Add(item);
        }
        return results;
    }
}

public class PlanStorageWrapper
{
    public EngineeringPlan? Plan { get; set; }
    public ExecutionPlanIdentity? Identity { get; set; }
}
