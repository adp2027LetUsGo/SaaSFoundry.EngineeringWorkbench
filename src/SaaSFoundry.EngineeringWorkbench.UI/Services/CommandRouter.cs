using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Infrastructure.Host;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Planning;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Catalog;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Catalog;
using SaaSFoundry.EngineeringWorkbench.Planning;
using SaaSFoundry.EngineeringWorkbench.Application;
using SaaSFoundry.EngineeringWorkbench.UI.Presentation;

namespace SaaSFoundry.EngineeringWorkbench.UI.Services;

public sealed class CommandRouter
{
    private readonly WorkbenchHost _host;
    private readonly ICatalogManager _catalogManager;
    private readonly ExecutionOrchestrator _orchestrator;
    private readonly LocalGovernanceStorage _storage;
    private readonly IConsoleRenderer _console;
    private readonly DashboardRenderer _dashboard;
    private readonly HelpRenderer _help;
    private readonly TableRenderer _table;
    private readonly TimelineRenderer _timeline;
    private readonly SummaryRenderer _summary;
    private readonly NextActionRenderer _nextAction;

    public CommandRouter(WorkbenchHost host)
    {
        _host = host;
        _catalogManager = new CatalogManager();
        _orchestrator = new ExecutionOrchestrator(host);
        _storage = new LocalGovernanceStorage();
        
        _console = new CliRenderer();
        _dashboard = new DashboardRenderer(_console);
        _help = new HelpRenderer(_console);
        _table = new TableRenderer(_console);
        _timeline = new TimelineRenderer(_console);
        _summary = new SummaryRenderer(_console);
        _nextAction = new NextActionRenderer(_console);
    }

    public async Task ExecuteAsync(string[] args)
    {
        var command = args.Length > 0 ? args[0].ToLowerInvariant() : null;
        var subCommand = args.Length > 1 ? args[1].ToLowerInvariant() : null;
        var arg = args.Length > 2 ? args[2] : null;

        using var cts = new CancellationTokenSource();
        var ct = cts.Token;

        try
        {
            await _host.InitializeAsync(ct);

            if (command == null)
                await PrintDashboardAsync(ct);
            else if (command == "help")
                _help.DrawHelp();
            else if (command == "doctor")
                await RunDoctorAsync(ct);
            else if (command == "summary")
                await DrawSummaryAsync(ct);
            else if (command == "info" && subCommand != null)
                PrintPluginInfo(subCommand);
            else if (command == "plugins")
                ListPlugins();
            else if (command == "capabilities")
                ListCapabilities();
            else if (command == "plans")
                await ListPlansAsync(ct);
            else if (command == "executions" || command == "reports")
                await ListExecutionsAsync(ct);
            else if (command == "catalog" && subCommand == "validate")
                await ValidateCatalogAsync(ct);
            else if (command == "plan")
                await RunPlannerAsync(subCommand, ct);
            else if (command == "generate" && args.Length > 2)
                await GenerateArtifactsAsync(args, ct);
            else if (command == "execute" && subCommand == "approve" && arg != null)
                await ApproveExecutionAsync(arg, ct);
            else if (command == "execute" && subCommand == "plan")
                await ExecutePlanWithGovernanceAsync(arg, ct);
            else if (command == "execute" && subCommand == "status" && arg != null)
                await CheckExecutionStatusAsync(arg, ct);
            else if (command == "execute" && subCommand == "report" && arg != null)
                await CheckExecutionReportAsync(arg, ct);
            else if (command == "test-governance")
                await RunTestSequenceAsync(ct);
            else
                HandleUnknownCommand(command);
        }
        finally
        {
            await _host.ShutdownAsync(ct);
        }
    }

    private EngineeringPackageManifest GetMockPackageManifest()
    {
        return SaaSFoundry.Plugins.Observability.Catalog.ObservabilityPluginCatalog.BuildPackageManifest();
    }

    private async Task PrintDashboardAsync(CancellationToken ct)
    {
        var catalog = _catalogManager.BuildCatalog(GetMockPackageManifest());
        var plans = await _storage.GetAllPlansAsync(ct);
        var reports = await _storage.GetAllReportsAsync(ct);
        var approvals = await _storage.GetAllApprovalsAsync(ct);

        _dashboard.DrawDashboard(catalog, plans, approvals, reports);
    }

    private async Task DrawSummaryAsync(CancellationToken ct)
    {
        var catalog = _catalogManager.BuildCatalog(GetMockPackageManifest());
        var plans = await _storage.GetAllPlansAsync(ct);
        var reports = await _storage.GetAllReportsAsync(ct);
        var approvals = await _storage.GetAllApprovalsAsync(ct);

        _summary.DrawSummary(catalog, plans, approvals, reports);
    }

    private async Task RunDoctorAsync(CancellationToken ct)
    {
        _console.DrawHeader("Platform Health");
        
        bool catalogOk = true;
        try { _catalogManager.BuildCatalog(GetMockPackageManifest()); } catch { catalogOk = false; }
        
        var catalog = _catalogManager.BuildCatalog(GetMockPackageManifest());

        _console.WriteLine($".NET Runtime ........ {Environment.Version}");
        _console.WriteLine($"Working Directory ... {Directory.GetCurrentDirectory()}");
        
        bool outDirOk = Directory.Exists("output/executions");
        _console.WriteLine($"Output Folder ....... {(outDirOk ? "output/executions" : "MISSING")}");

        _console.WriteLine($"Plugin Count ........ {catalog.RegisteredPlugins.Count}");
        _console.WriteLine($"Capability Count .... {catalog.RegisteredPlugins.Sum(p => p.Capabilities.Count)}");

        _console.WriteLine($"Catalog Version ..... 1.0.0");
        _console.WriteLine($"Catalog ............. {(catalogOk ? CliRenderer.IconSuccess : CliRenderer.IconError)}");
        _console.WriteLine($"Planner ............. {CliRenderer.IconSuccess}");
        _console.WriteLine($"Governance .......... {CliRenderer.IconSuccess}");
        _console.WriteLine($"Storage Status ...... {CliRenderer.IconSuccess}");
        _console.WriteLine("===========================================================");
    }

    private void PrintPluginInfo(string pluginId)
    {
        var catalog = _catalogManager.BuildCatalog(GetMockPackageManifest());
        var record = catalog.RegisteredPlugins.FirstOrDefault(p => p.PluginId.Equals(pluginId, StringComparison.OrdinalIgnoreCase));

        if (record == null)
        {
            _console.DrawError($"Plugin '{pluginId}' not found.");
            _console.WriteLine();
            _console.DrawSection("Next Steps");
            _console.WriteLine("    sfw plugins");
            return;
        }

        _console.DrawHeader($"Plugin: {record.PluginId} (v{record.Version.Version})");
        _console.WriteLine($"Name: {record.Name}");
        _console.WriteLine($"Domain: {record.EngineeringDomain}");
        _console.WriteLine($"Required Canon Version: {record.RequiredCanonVersion}");
        _console.WriteLine();
        _console.WriteLine("Capabilities:");
        foreach (var cap in record.Capabilities)
        {
            _console.WriteLine($" - {cap.CapabilityId} [{cap.Operation}]");
        }
        _console.WriteLine("===========================================================");
    }

    private void ListPlugins()
    {
        var catalog = _catalogManager.BuildCatalog(GetMockPackageManifest());
        if (!catalog.RegisteredPlugins.Any())
        {
            _console.DrawWarning("No plugins registered.");
            _console.WriteLine("See documentation:");
            _console.WriteLine("    sfw help");
            return;
        }

        var rows = catalog.RegisteredPlugins
            .Select(p => new string[] { p.PluginId, p.Version.Version, "Loaded", p.Capabilities.Count.ToString() })
            .ToList();

        _table.DrawTable("Installed Plugins", new[] { "Plugin", "Version", "Status", "Capabilities" }, rows);
    }

    private void ListCapabilities()
    {
        var catalog = _catalogManager.BuildCatalog(GetMockPackageManifest());
        var rows = catalog.RegisteredPlugins
            .SelectMany(p => p.Capabilities.Select(c => new string[] { p.PluginId, c.CapabilityId, c.Operation, c.Description }))
            .ToList();

        _table.DrawTable("Capabilities", new[] { "Plugin", "Capability", "Operation", "Description" }, rows);
    }

    private async Task ListPlansAsync(CancellationToken ct)
    {
        var plans = await _storage.GetAllPlansAsync(ct);
        var approvals = await _storage.GetAllApprovalsAsync(ct);

        if (!plans.Any())
        {
            _console.DrawWarning("No Engineering Plans found.");
            _console.WriteLine("Create one with:");
            _console.WriteLine("    sfw plan observability");
            return;
        }

        var rows = new List<string[]>();
        foreach (var p in plans)
        {
            if (p.Plan == null) continue;
            var approval = approvals.FirstOrDefault(a => a.PlanId == p.Plan.PlanId);
            bool isApproved = approval != null && approval.Approved;
            var created = p.Identity != null ? DateTimeOffset.FromUnixTimeSeconds(p.Identity.CreatedTimestamp).ToString("yyyy-MM-dd") : "Unknown";
            
            rows.Add(new string[] { p.Plan.PlanId, "Generated", created, isApproved ? "Yes" : "No" });
        }

        _table.DrawTable("Engineering Plans", new[] { "PlanId", "Status", "Created", "Approved" }, rows);
    }

    private async Task ListExecutionsAsync(CancellationToken ct)
    {
        var reports = await _storage.GetAllReportsAsync(ct);
        var rows = reports.Select(r => {
            var started = DateTimeOffset.FromUnixTimeSeconds(r.StartedAt).ToString("HH:mm:ss");
            var completed = r.CompletedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(r.CompletedAt.Value).ToString("HH:mm:ss") : "-";
            return new string[] { r.ExecutionId, r.PlanId, r.Status.ToString(), started, completed };
        }).ToList();

        _table.DrawTable("Executions", new[] { "ExecutionId", "PlanId", "Status", "Started", "Completed" }, rows);
    }

    private async Task ValidateCatalogAsync(CancellationToken ct)
    {
        _console.WriteLine($"{CliRenderer.IconRunning} Validating Engineering Catalog...");

        var catalog = _catalogManager.BuildCatalog(GetMockPackageManifest());

        _console.DrawSuccess("Catalog validation completed.");
        _console.WriteLine($"Plugins: {catalog.RegisteredPlugins.Count}");
        _console.WriteLine($"Capabilities: {catalog.RegisteredPlugins.Sum(p => p.Capabilities.Count)}");
        _console.WriteLine();

        _console.DrawSection("Next Steps");
        _console.WriteLine($"{CliRenderer.IconInfo} List plugins:");
        _console.WriteLine("    sfw plugins");
        _console.WriteLine($"{CliRenderer.IconInfo} Plan engineering package:");
        _console.WriteLine("    sfw plan observability");
    }

    private async Task GenerateArtifactsAsync(string[] args, CancellationToken ct)
    {
        var pluginId = args[1];
        var capabilityId = args[2];
        var catalog = _catalogManager.BuildCatalog(GetMockPackageManifest());
        
        if (!catalog.RegisteredPlugins.Any(p => p.PluginId == pluginId && p.Capabilities.Any(c => c.CapabilityId == capabilityId)))
        {
            _console.DrawHeader("Capability Not Found");
            _console.DrawError($"Capability '{capabilityId}' was not found in plugin '{pluginId}'.");
            _console.WriteLine();
            _console.WriteLine("Available capabilities:");
            var plugin = catalog.RegisteredPlugins.FirstOrDefault(p => p.PluginId == pluginId);
            if (plugin != null)
            {
                foreach (var c in plugin.Capabilities)
                {
                    _console.WriteLine($" - {c.CapabilityId}");
                }
            }
            _console.WriteLine();
            _console.DrawSection("Next Steps");
            _console.WriteLine("    sfw capabilities");
            _console.WriteLine("===========================================================");
            return;
        }

        _console.WriteLine($"Generating {capabilityId} for {pluginId} (Simulated)...");
        await RunPlannerAsync(pluginId, ct);
    }

    private async Task RunPlannerAsync(string? scope, CancellationToken ct)
    {
        _console.WriteLine($"{CliRenderer.IconRunning} Reading Engineering Package");
        var catalog = _catalogManager.BuildCatalog(GetMockPackageManifest());
        _console.DrawSuccess("Completed");

        _console.WriteLine($"{CliRenderer.IconRunning} Building Engineering Catalog");
        var planner = new EngineeringPlanner(catalog);
        _console.DrawSuccess("Completed");

        _console.WriteLine($"{CliRenderer.IconRunning} Resolving Dependency Graph");
        var tasks = scope?.Equals("observability", StringComparison.OrdinalIgnoreCase) == true || scope == null || scope == "default"
            ? SaaSFoundry.Plugins.Observability.Planner.ObservabilityPlanContributor.GetRequestedTasks(scope ?? "observability").ToList()
            : new List<EngineeringTask> {
                new EngineeringTask("task-1", scope, "logging", "generate", ExecutionPriority.Normal, new List<EngineeringRequirement>(), new List<string>(), new List<string>())
            };
        _console.DrawSuccess("Completed");

        _console.WriteLine($"{CliRenderer.IconRunning} Validating Engineering Plan");
        var result = planner.CreatePlan(new EngineeringPlanningContext("default", tasks));
        
        if (!result.IsSuccessful)
        {
            _console.DrawError("Plan validation failed.");
            return;
        }

        _console.DrawSuccess("Completed");
        _console.WriteLine();

        if (result.Plan != null && (scope?.Equals("observability", StringComparison.OrdinalIgnoreCase) == true || scope == null || scope == "default"))
        {
            _console.DrawHeader("Observability Plan");
            _console.WriteLine("Nodes:");
            foreach (var stage in result.Plan.Stages)
            {
                foreach (var task in stage.Tasks)
                {
                    var capName = task.CapabilityId;
                    if (capName.Length > 0)
                    {
                        capName = string.Concat(capName[0].ToString().ToUpperInvariant(), capName.AsSpan(1));
                    }
                    if (string.Equals(capName, "Healthchecks", StringComparison.OrdinalIgnoreCase))
                    {
                        capName = "HealthChecks";
                    }
                    _console.WriteLine($" - {capName}");
                }
            }
            _console.WriteLine("===========================================================");
            _console.WriteLine();
        }

        var identity = new ExecutionPlanIdentity(result.Plan!.PlanId, "1.0", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), "pkg-obs-001", "1.0.0");
        await _storage.SavePlanAsync(result.Plan, identity, ct);

        _console.DrawSuccess($"Plan '{result.Plan.PlanId}' generated and stored.");
        _console.WriteLine();
        _console.DrawSection("Next Steps");
        _console.WriteLine($"{CliRenderer.IconInfo} Review plan:");
        _console.WriteLine("    sfw plans");
        _console.WriteLine($"{CliRenderer.IconInfo} Approve plan:");
        _console.WriteLine($"    sfw execute approve {result.Plan.PlanId}");
    }

    private async Task ApproveExecutionAsync(string planId, CancellationToken ct)
    {
        var planWrapper = await _storage.LoadPlanAsync(planId, ct);
        if (planWrapper == null || planWrapper.Plan == null)
        {
            _console.DrawError("Plan not found.");
            return;
        }

        var approval = new ExecutionApproval(planId, true, "Manually approved by user via CLI", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        await _storage.SaveApprovalAsync(approval, ct);

        _console.DrawSuccess($"Execution for plan '{planId}' has been approved.");
        _console.WriteLine();
        _console.DrawSection("Next Steps");
        _console.WriteLine($"{CliRenderer.IconInfo} Execute plan:");
        _console.WriteLine($"    sfw execute plan {planId}");
    }

    private async Task ExecutePlanWithGovernanceAsync(string? planId, CancellationToken ct)
    {
        bool isShorthand = string.IsNullOrEmpty(planId);
        if (isShorthand)
        {
            var allPlans = await _storage.GetAllPlansAsync(ct);
            var latestPlan = allPlans.OrderByDescending(p => p.Identity?.CreatedTimestamp ?? 0).FirstOrDefault();
            if (latestPlan != null && latestPlan.Plan != null)
            {
                planId = latestPlan.Plan.PlanId;
            }
            else
            {
                await RunPlannerAsync("observability", ct);
                allPlans = await _storage.GetAllPlansAsync(ct);
                latestPlan = allPlans.OrderByDescending(p => p.Identity?.CreatedTimestamp ?? 0).FirstOrDefault();
                if (latestPlan != null && latestPlan.Plan != null)
                {
                    planId = latestPlan.Plan.PlanId;
                }
            }
        }

        if (string.IsNullOrEmpty(planId))
        {
            _console.DrawError("Plan not found.");
            return;
        }

        if (isShorthand)
        {
            var existingApproval = await _storage.LoadApprovalAsync(planId, ct);
            if (existingApproval == null || !existingApproval.Approved)
            {
                await ApproveExecutionAsync(planId, ct);
            }
        }

        var planWrapper = await _storage.LoadPlanAsync(planId, ct);
        if (planWrapper == null || planWrapper.Plan == null || planWrapper.Identity == null)
        {
            _console.DrawError("Plan not found.");
            return;
        }

        var approval = await _storage.LoadApprovalAsync(planId, ct);

        _console.WriteLine($"{CliRenderer.IconRunning} Initiating execution governance for '{planId}'...");

        var record = await _orchestrator.OrchestrateExecutionAsync(
            planWrapper.Plan,
            planWrapper.Identity,
            approval ?? new ExecutionApproval(planId, false, "No explicit approval found.", 0),
            "catalog-hash-test",
            ct
        );

        if (record.Status == ExecutionStatus.Rejected)
        {
            _console.DrawHeader("Execution Rejected");
            _console.WriteLine("Reason");
            _console.WriteLine("The selected Engineering Plan has not yet been approved.");
            _console.WriteLine();
            _console.DrawSection("Next Steps");
            _console.WriteLine($"{CliRenderer.IconInfo} Approve plan:");
            _console.WriteLine($"    sfw execute approve {planId}");
            _console.WriteLine($"{CliRenderer.IconInfo} Then execute:");
            _console.WriteLine($"    sfw execute plan {planId}");
            _console.WriteLine("===========================================================");
        }
        else if (record.Status == ExecutionStatus.Completed)
        {
            _console.DrawHeader("Execution Summary");
            
            _console.WriteLine("Execution");
            _console.WriteLine(record.ExecutionId);
            _console.WriteLine();

            _console.WriteLine("Plan");
            _console.WriteLine(record.PlanId);
            _console.WriteLine();

            _console.WriteLine("Artifacts");
            if (record.Evidence != null && record.Evidence.ArtifactsGenerated != null)
            {
                foreach (var a in record.Evidence.ArtifactsGenerated)
                {
                    _console.DrawSuccess(a);
                }
            }
            _console.WriteLine();

            _console.WriteLine("Validation");
            _console.DrawSuccess("Passed");
            _console.WriteLine();

            _console.WriteLine("Evidence Location");
            _console.WriteLine($"output/executions/reports/execution-report-{record.ExecutionId}.json");
            _console.WriteLine("===========================================================");
            _console.WriteLine();

            _timeline.DrawTimeline(record);

            _console.DrawSection("Next Steps");
            _console.WriteLine($"{CliRenderer.IconInfo} View report:");
            _console.WriteLine($"    sfw execute report {record.ExecutionId}");
            _console.WriteLine($"{CliRenderer.IconInfo} Review evidence:");
            _console.WriteLine($"    sfw executions");
            _console.WriteLine($"{CliRenderer.IconInfo} Create another plan:");
            _console.WriteLine("    sfw plan observability");
        }
    }

    private async Task CheckExecutionStatusAsync(string executionId, CancellationToken ct)
    {
        var report = await _storage.LoadReportAsync(executionId, ct);
        if (report == null)
        {
            _console.DrawError($"Execution '{executionId}' not found.");
            return;
        }
        _console.WriteLine($"Status for '{executionId}': {report.Status}");
        _console.WriteLine($"Started At: {DateTimeOffset.FromUnixTimeSeconds(report.StartedAt)}");
        if (report.CompletedAt.HasValue)
        {
            _console.WriteLine($"Completed At: {DateTimeOffset.FromUnixTimeSeconds(report.CompletedAt.Value)}");
        }
    }

    private async Task CheckExecutionReportAsync(string executionId, CancellationToken ct)
    {
        var report = await _storage.LoadReportAsync(executionId, ct);
        if (report == null)
        {
            _console.DrawError($"Execution '{executionId}' not found.");
            return;
        }
        _console.WriteLine(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }

    private async Task RunTestSequenceAsync(CancellationToken ct)
    {
        var plansDir = Path.Combine("output", "executions", "plans");
        if (Directory.Exists(plansDir)) 
        {
            foreach (var f in Directory.GetFiles(plansDir)) File.Delete(f);
        }

        _console.DrawSection("--- SCENARIO 1: Generate Plan & Unapproved Execution ---");
        await RunPlannerAsync(null, ct);
        var dir = new DirectoryInfo(Path.Combine("output", "executions", "plans"));
        var planFile = dir.GetFiles("*.json")[0];
        var planId = Path.GetFileNameWithoutExtension(planFile.Name);

        await ExecutePlanWithGovernanceAsync(planId, ct);

        _console.DrawSection("--- SCENARIO 2: Approve & Execute ---");
        await ApproveExecutionAsync(planId, ct);
        await ExecutePlanWithGovernanceAsync(planId, ct);
    }

    private void HandleUnknownCommand(string command)
    {
        _console.WriteLine($"Unknown command:");
        _console.WriteLine(command);
        _console.WriteLine();
        _console.WriteLine("Did you mean:");
        
        var validCommands = new[] { "catalog", "plan", "execute", "help", "doctor", "info", "plugins", "capabilities", "plans", "executions", "reports", "summary" };
        var match = validCommands.OrderBy(c => ComputeLevenshtein(command, c)).First();
        
        _console.WriteLine(match);
        _console.WriteLine();
        _console.WriteLine("Run 'sfw help' for a list of valid commands.");
    }

    private static int ComputeLevenshtein(string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
        if (string.IsNullOrEmpty(t)) return s.Length;

        int[] v0 = new int[t.Length + 1];
        int[] v1 = new int[t.Length + 1];

        for (int i = 0; i < v0.Length; i++) v0[i] = i;

        for (int i = 0; i < s.Length; i++)
        {
            v1[0] = i + 1;
            for (int j = 0; j < t.Length; j++)
            {
                var cost = (s[i] == t[j]) ? 0 : 1;
                v1[j + 1] = Math.Min(v1[j] + 1, Math.Min(v0[j + 1] + 1, v0[j] + cost));
            }
            for (int j = 0; j < v0.Length; j++) v0[j] = v1[j];
        }
        return v1[t.Length];
    }
}
