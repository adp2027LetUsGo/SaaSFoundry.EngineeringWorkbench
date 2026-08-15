using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Infrastructure.Host;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Catalog;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Catalog;
using SaaSFoundry.EngineeringWorkbench.Application;
using SaaSFoundry.EngineeringWorkbench.UI.Presentation;

namespace SaaSFoundry.EngineeringWorkbench.UI.Services;

public sealed class InteractiveShell
{
    private readonly WorkbenchHost _host;
    private readonly CommandRouter _router;
    private readonly IConsoleRenderer _console;
    private readonly MenuRenderer _menu;
    private readonly PromptRenderer _prompt;
    private readonly WorkflowRenderer _workflow;
    private readonly ICatalogManager _catalogManager;
    private readonly LocalGovernanceStorage _storage;

    public InteractiveShell(WorkbenchHost host, CommandRouter router)
    {
        _host = host;
        _router = router;
        _console = new CliRenderer();
        _menu = new MenuRenderer(_console);
        _prompt = new PromptRenderer(_console);
        _workflow = new WorkflowRenderer(_console, _prompt);
        _catalogManager = new CatalogManager();
        _storage = new LocalGovernanceStorage();
    }

    public async Task RunAsync()
    {
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;

        await _host.InitializeAsync(ct);

        try
        {
            // Show initial dashboard
            await _router.ExecuteAsync(Array.Empty<string>());
            _console.WriteLine();
            _menu.DrawMenu();

            while (true)
            {
                _prompt.DrawPrompt();
                var input = Console.ReadLine();

                if (input == null) break; // EOF - stdin closed
                input = input.Trim();
                if (string.IsNullOrEmpty(input)) continue;

                var lower = input.ToLowerInvariant();

                if (lower == "exit" || lower == "quit" || lower == "0")
                {
                    _console.WriteLine("Goodbye.");
                    break;
                }

                // Map menu numbers to guided workflows
                var action = await HandleInput(lower, ct);

                if (action == PostAction.Exit) break;
                if (action == PostAction.Dashboard)
                {
                    await _router.ExecuteAsync(Array.Empty<string>());
                    _console.WriteLine();
                    _menu.DrawMenu();
                }
            }
        }
        finally
        {
            await _host.ShutdownAsync(ct);
        }
    }

    private async Task<PostAction> HandleInput(string input, CancellationToken ct)
    {
        switch (input)
        {
            case "1": // Create Engineering Plan
                return await HandleGuidedPlan(ct);

            case "2": // Review Plans
                await RunCommand("plans");
                return await ShowPostAction();

            case "3": // Approve Plan
                return await HandleGuidedApproval(ct);

            case "4": // Execute Plan
                return await HandleGuidedExecution(ct);

            case "5": // Review Reports
                return await HandleGuidedReport(ct);

            case "6": // Browse Plugins
                await RunCommand("plugins");
                return await ShowPostAction();

            case "7": // Browse Capabilities
                await RunCommand("capabilities");
                return await ShowPostAction();

            case "8": // Doctor
                await RunCommand("doctor");
                return await ShowPostAction();

            case "9": // Help
                await RunCommand("help");
                return await ShowPostAction();

            case "menu":
                _menu.DrawMenu();
                return PostAction.Continue;

            case "dashboard":
                return PostAction.Dashboard;

            default:
                // Pass through to the standard command router
                var args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                await _router.ExecuteAsync(args);
                return await ShowPostAction();
        }
    }

    private async Task<PostAction> HandleGuidedPlan(CancellationToken ct)
    {
        var manifest = GetMockPackageManifest();
        var catalog = _catalogManager.BuildCatalog(manifest);

        var pluginId = _workflow.GuidePlanCreation(catalog);
        if (pluginId == null) return PostAction.Continue;

        await RunCommand($"plan {pluginId}");
        return await ShowPostAction();
    }

    private async Task<PostAction> HandleGuidedApproval(CancellationToken ct)
    {
        var plans = await _storage.GetAllPlansAsync(ct);
        var approvals = await _storage.GetAllApprovalsAsync(ct);

        var planId = _workflow.GuideApproval(plans, approvals);
        if (planId == null) return PostAction.Continue;

        await RunCommand($"execute approve {planId}");
        return await ShowPostAction();
    }

    private async Task<PostAction> HandleGuidedExecution(CancellationToken ct)
    {
        var plans = await _storage.GetAllPlansAsync(ct);
        var approvals = await _storage.GetAllApprovalsAsync(ct);

        var planId = _workflow.GuideExecution(plans, approvals);
        if (planId == null) return PostAction.Continue;

        await RunCommand($"execute plan {planId}");
        return await ShowPostAction();
    }

    private async Task<PostAction> HandleGuidedReport(CancellationToken ct)
    {
        var reports = await _storage.GetAllReportsAsync(ct);

        var executionId = _workflow.GuideReportSelection(reports);
        if (executionId == null) return PostAction.Continue;

        await RunCommand($"execute report {executionId}");
        return await ShowPostAction();
    }

    private async Task RunCommand(string commandLine)
    {
        var args = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        await _router.ExecuteAsync(args);
    }

    private async Task<PostAction> ShowPostAction()
    {
        _prompt.DrawPostActionMenu();
        _prompt.DrawSubPrompt("Select");

        var input = Console.ReadLine();
        if (input == null) return PostAction.Exit; // EOF

        return input.Trim() switch
        {
            "1" => PostAction.Dashboard,
            "3" => PostAction.Exit,
            _ => PostAction.Continue
        };
    }

    private EngineeringPackageManifest GetMockPackageManifest()
    {
        return SaaSFoundry.Plugins.Observability.Catalog.ObservabilityPluginCatalog.BuildPackageManifest();
    }

    private enum PostAction
    {
        Continue,
        Dashboard,
        Exit
    }
}
