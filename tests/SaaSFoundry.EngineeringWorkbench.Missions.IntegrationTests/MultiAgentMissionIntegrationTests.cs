using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.AgentCoordinator;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Engine;
using SaaSFoundry.EngineeringWorkbench.AgentGovernance.Policy;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Agent.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Packaging;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;
using SaaSFoundry.EngineeringWorkbench.Execution;
using SaaSFoundry.EngineeringWorkbench.Execution.Capabilities;
using SaaSFoundry.EngineeringWorkbench.Execution.Strategies;
using SaaSFoundry.EngineeringWorkbench.Missions.Packaging;
using SaaSFoundry.EngineeringWorkbench.Planning.DAG;
using SaaSFoundry.EngineeringWorkbench.Planning.Scheduling;

namespace SaaSFoundry.EngineeringWorkbench.Missions.IntegrationTests;

public class MultiAgentMissionIntegrationTests
{
    private sealed class GovernedAgent : IAgentOrchestrator
    {
        public AgentIdentity Identity { get; }
        public AgentMetadata Metadata { get; }
        public AgentGovernanceMetadata GovernanceMetadata { get; }
        private readonly AgentGovernanceEngine _governanceEngine;

        public GovernedAgent(string id, AgentRiskLevel riskLevel, AgentGovernanceEngine governanceEngine)
        {
            Identity = new AgentIdentity(id, "1.0", "Author", "Fingerprint", 1700000000000L);
            Metadata = new AgentMetadata(Identity, "GovernedReferenceAgent", "Desc", "Purpose", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), "None");
            GovernanceMetadata = new AgentGovernanceMetadata(id, riskLevel, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
            _governanceEngine = governanceEngine;
        }

        public async Task<AgentExecutionResult> ExecuteAsync(AgentExecutionContext context, CancellationToken cancellationToken = default)
        {
            var req = new AgentExecutionRequest(context.ExecutionId, Identity.AgentId, "PerformTask", GovernanceMetadata, Array.Empty<string>());
            var decision = await _governanceEngine.EvaluateRequestAsync(req, cancellationToken);

            if (!decision.CanExecute || decision.IsBlocked)
            {
                return new AgentExecutionResult(context.ExecutionId, context.AgentId, AgentExecutionStatus.Failed, 1000L, "Denied", Array.Empty<string>(), "Governance Denied");
            }

            return new AgentExecutionResult(
                context.ExecutionId,
                context.AgentId,
                AgentExecutionStatus.Succeeded,
                1000L,
                "Success",
                new[] { $"artifact-{Identity.AgentId}.json", "engineering-trace.log" },
                null
            );
        }
    }

    [Fact]
    public async Task CompleteMissionExecution_MultiAgentDag_ProducesVerifiedMissionPackage()
    {
        var governanceEngine = new AgentGovernanceEngine();
        var obsAgent = new GovernedAgent("ObservabilityAgent", AgentRiskLevel.Low, governanceEngine);
        var docAgent = new GovernedAgent("DocumentationAgent", AgentRiskLevel.Low, governanceEngine);
        var valAgent = new GovernedAgent("ValidationAgent", AgentRiskLevel.Low, governanceEngine);
        var pkgAgent = new GovernedAgent("PackagingAgent", AgentRiskLevel.Low, governanceEngine);

        var registry = new AgentRegistry();
        registry.Register(obsAgent);
        registry.Register(docAgent);
        registry.Register(valAgent);
        registry.Register(pkgAgent);

        var scheduler = new ExecutionScheduler(new ParallelExecutionStrategy(4));
        var coordinator = new MissionCoordinator(scheduler);
        coordinator.RegisterAgent(obsAgent);
        coordinator.RegisterAgent(docAgent);
        coordinator.RegisterAgent(valAgent);
        coordinator.RegisterAgent(pkgAgent);

        var missionId = new MissionIdentity("Mission-Full-001", "1.0", 1700000000000L, "FINGERPRINT");
        var opts = new MissionExecutionOptions(true, 4, false, 2, 60000L);
        var missionCtx = new MissionContext(missionId, new Dictionary<string, string> { ["Project"] = "SaaSFoundry.EngineeringWorkbench" }, new Dictionary<string, string>(), new[] { "final-package.zip" }, opts);
        var missionMeta = new MissionMetadata("Full Engineering Workflow", "End-to-end multi-agent orchestration", "Integration Test", new[] { "ObservabilityAgent", "DocumentationAgent", "ValidationAgent", "PackagingAgent" }, new[] { "Obs", "Doc", "Val", "Pkg" }, Array.Empty<string>(), MissionPriority.High);

        var execCtx = MissionExecutionContext.Create(missionCtx, missionMeta, registry, new GovernanceContext(governanceEngine, true), isDeterministicClock: true);

        // DAG setup: Obs and Doc can run in parallel; Val depends on Obs and Doc; Pkg depends on Val
        var n1 = ExecutionNode.Create("Node-Obs", "ObservabilityAgent", "Cap-Obs");
        var n2 = ExecutionNode.Create("Node-Doc", "DocumentationAgent", "Cap-Doc");
        var n3 = ExecutionNode.Create("Node-Val", "ValidationAgent", "Cap-Val", new[] { "Node-Obs", "Node-Doc" });
        var n4 = ExecutionNode.Create("Node-Pkg", "PackagingAgent", "Cap-Pkg", new[] { "Node-Val" });

        var planner = new MissionPlanner();
        var plan = planner.CreatePlan(execCtx, new[] { n1, n2, n3, n4 });

        Assert.Equal(4, plan.Nodes.Count);
        Assert.Equal(3, plan.Edges.Count);

        // Execute Mission via Coordinator
        var result = await coordinator.ExecuteMissionAsync(plan, execCtx);

        Assert.True(result.Succeeded);
        Assert.Equal(MissionExecutionStatus.Completed, result.MissionExecutionStatus);
        Assert.Contains("artifact-ObservabilityAgent.json", result.Artifacts);
        Assert.Contains("artifact-PackagingAgent.json", result.Artifacts);

        // Build Mission Package
        var engPackage = new EngineeringPackage("ENG-001", new ValidationReport(Array.Empty<ValidationEvidence>(), true, DateTimeOffset.UtcNow), result.Artifacts);
        var builder = new MissionPackageBuilder();
        var missionPkg = builder.BuildPackage(execCtx, plan, result, engPackage);

        Assert.NotNull(missionPkg);
        Assert.True(missionPkg.CertificationDescriptor.IsVerified);
        Assert.StartsWith("SHA256:", missionPkg.MissionHash);
        Assert.Equal("Completed", missionPkg.MissionDescriptor.ExecutionStatus);
        Assert.Equal("ObservabilityAgent", missionPkg.AgentAssignments["Node-Obs"]);
        Assert.Equal("PackagingAgent", missionPkg.AgentAssignments["Node-Pkg"]);
        Assert.NotEmpty(missionPkg.ExecutionTimeline);
    }

    [Fact]
    public async Task MissionExecution_DeterministicReplay_ProducesIdenticalHash()
    {
        var hash1 = await RunAndGetHashAsync();
        var hash2 = await RunAndGetHashAsync();

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public async Task MissionExecution_GovernanceBlockedAgent_AbortsMissionAndLogsAudit()
    {
        var governanceEngine = new AgentGovernanceEngine();
        // Configure agent with Critical risk level to trigger governance policy block without manual approval
        var criticalAgent = new GovernedAgent("CriticalAgent", AgentRiskLevel.Critical, governanceEngine);
        var registry = new AgentRegistry();
        registry.Register(criticalAgent);

        var scheduler = new ExecutionScheduler(new SequentialExecutionStrategy());
        var coordinator = new MissionCoordinator(scheduler);
        coordinator.RegisterAgent(criticalAgent);

        var missionId = new MissionIdentity("Gov-Block-Mission", "1.0", 1700000000000L, "FINGERPRINT");
        var opts = new MissionExecutionOptions(false, 1, false, 0, 30000L);
        var missionCtx = new MissionContext(missionId, new Dictionary<string, string>(), new Dictionary<string, string>(), Array.Empty<string>(), opts);
        var missionMeta = new MissionMetadata("Governance Test", "Gov", "Test", new[] { "CriticalAgent" }, new[] { "Cap1" }, Array.Empty<string>(), MissionPriority.High);
        var execCtx = MissionExecutionContext.Create(missionCtx, missionMeta, registry, new GovernanceContext(governanceEngine, true), isDeterministicClock: true);

        var n1 = ExecutionNode.Create("Node-Gov", "CriticalAgent", "Cap1", retryCount: 0);
        var planner = new MissionPlanner();
        var plan = planner.CreatePlan(execCtx, new[] { n1 });

        var result = await coordinator.ExecuteMissionAsync(plan, execCtx);

        Assert.False(result.Succeeded);
        Assert.Equal(MissionExecutionStatus.Failed, result.MissionExecutionStatus);
        Assert.Contains(result.Diagnostics, d => d.Contains("Governance Denied"));

        var builder = new MissionPackageBuilder();
        var pkg = builder.BuildPackage(execCtx, plan, result);
        Assert.False(pkg.CertificationDescriptor.IsVerified);
        Assert.Equal("Failed", pkg.MissionDescriptor.ExecutionStatus);
    }

    private async Task<string> RunAndGetHashAsync()
    {
        var governanceEngine = new AgentGovernanceEngine();
        var agent = new GovernedAgent("SingleAgent", AgentRiskLevel.Low, governanceEngine);
        var registry = new AgentRegistry();
        registry.Register(agent);

        var scheduler = new ExecutionScheduler(new SequentialExecutionStrategy());
        var coordinator = new MissionCoordinator(scheduler);
        coordinator.RegisterAgent(agent);

        var missionId = new MissionIdentity("Replay-Mission", "1.0", 1700000000000L, "FINGERPRINT");
        var opts = new MissionExecutionOptions(false, 1, false, 1, 30000L);
        var missionCtx = new MissionContext(missionId, new Dictionary<string, string>(), new Dictionary<string, string>(), Array.Empty<string>(), opts);
        var missionMeta = new MissionMetadata("Replay Test", "Replay", "Test", new[] { "SingleAgent" }, new[] { "Cap1" }, Array.Empty<string>(), MissionPriority.Normal);
        var execCtx = MissionExecutionContext.Create(missionCtx, missionMeta, registry, isDeterministicClock: true);

        var n1 = ExecutionNode.Create("Node-1", "SingleAgent", "Cap1");
        var planner = new MissionPlanner();
        var plan = planner.CreatePlan(execCtx, new[] { n1 });

        var result = await coordinator.ExecuteMissionAsync(plan, execCtx);
        var builder = new MissionPackageBuilder();
        var pkg = builder.BuildPackage(execCtx, plan, result);

        return pkg.MissionHash;
    }
}
