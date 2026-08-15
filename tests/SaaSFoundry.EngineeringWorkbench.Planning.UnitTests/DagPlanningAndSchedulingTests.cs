using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.AgentRuntime.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;
using SaaSFoundry.EngineeringWorkbench.Execution;
using SaaSFoundry.EngineeringWorkbench.Execution.Strategies;
using SaaSFoundry.EngineeringWorkbench.Planning.DAG;
using SaaSFoundry.EngineeringWorkbench.Planning.Scheduling;

namespace SaaSFoundry.EngineeringWorkbench.Planning.UnitTests;

public class DagPlanningAndSchedulingTests
{
    [Fact]
    public void DependencyResolver_ValidDag_ReturnsDeterministicLexicographicOrder()
    {
        var resolver = new DependencyResolver();
        // C and B both depend on A. Lexicographically, B should come before C.
        var nodeA = ExecutionNode.Create("NodeA", "Agent1", "CapA");
        var nodeC = ExecutionNode.Create("NodeC", "Agent1", "CapC", new[] { "NodeA" });
        var nodeB = ExecutionNode.Create("NodeB", "Agent1", "CapB", new[] { "NodeA" });

        var order = resolver.ResolveDeterministicOrder(new[] { nodeC, nodeA, nodeB });

        Assert.Equal(3, order.Count);
        Assert.Equal("NodeA", order[0]);
        Assert.Equal("NodeB", order[1]);
        Assert.Equal("NodeC", order[2]);
    }

    [Fact]
    public void DependencyResolver_CyclicDag_ThrowsInvalidOperationException()
    {
        var resolver = new DependencyResolver();
        var nodeX = ExecutionNode.Create("NodeX", "Agent1", "CapX", new[] { "NodeY" });
        var nodeY = ExecutionNode.Create("NodeY", "Agent1", "CapY", new[] { "NodeX" });

        var ex = Assert.Throws<InvalidOperationException>(() => resolver.ResolveDeterministicOrder(new[] { nodeX, nodeY }));
        Assert.Contains("Cyclic dependency detected", ex.Message);
    }

    [Fact]
    public void ExecutionTopologyValidator_OrphanNode_ThrowsInvalidOperationException()
    {
        var validator = new ExecutionTopologyValidator();
        var node1 = ExecutionNode.Create("Node1", "Agent1", "Cap1");
        var node2 = ExecutionNode.Create("Node2", "Agent1", "Cap2", new[] { "Node1" });
        var orphan = ExecutionNode.Create("Orphan", "Agent1", "Cap3"); // No incoming or outgoing dependencies in a multi-node DAG

        var ex = Assert.Throws<InvalidOperationException>(() =>
            validator.ValidateTopology(new[] { node1, node2, orphan }, new[] { "Agent1" }, new[] { "Cap1", "Cap2", "Cap3" }));
        Assert.Contains("Orphan node detected", ex.Message);
    }

    [Fact]
    public void ExecutionTopologyValidator_UnregisteredAgent_ThrowsException()
    {
        var validator = new ExecutionTopologyValidator();
        var node1 = ExecutionNode.Create("Node1", "MissingAgent", "Cap1");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            validator.ValidateTopology(new[] { node1 }, new[] { "Agent1" }, new[] { "Cap1" }));
        Assert.Contains("targets unregistered agent", ex.Message);
    }

    [Fact]
    public void ExecutionTopologyValidator_UnsupportedCapability_ThrowsException()
    {
        var validator = new ExecutionTopologyValidator();
        var node1 = ExecutionNode.Create("Node1", "Agent1", "Cap-Invalid");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            validator.ValidateTopology(new[] { node1 }, new[] { "Agent1" }, new[] { "Cap-Supported" }));
        Assert.Contains("targets unsupported capability", ex.Message);
    }

    [Fact]
    public void ExecutionTopologyValidator_DuplicateNodeId_ThrowsException()
    {
        var validator = new ExecutionTopologyValidator();
        var node1 = ExecutionNode.Create("Node1", "Agent1", "Cap1");
        var node2 = ExecutionNode.Create("Node1", "Agent1", "Cap2");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            validator.ValidateTopology(new[] { node1, node2 }, new[] { "Agent1" }, new[] { "Cap1", "Cap2" }));
        Assert.Contains("Duplicate node identifier", ex.Message);
    }

    [Fact]
    public async Task ExecutionScheduler_ParallelStrategy_ExecutesWithoutViolatingOrder()
    {
        var scheduler = new ExecutionScheduler(new ParallelExecutionStrategy(2));
        var id = new MissionIdentity("M-101", "1.0", 1000L, "hash");
        var opts = new MissionExecutionOptions(true, 2, false, 1, 30000L);
        var ctx = new MissionContext(id, new Dictionary<string, string>(), new Dictionary<string, string>(), Array.Empty<string>(), opts);
        var execCtx = MissionExecutionContext.Create(ctx, new MissionMetadata("Test", "D", "U", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), MissionPriority.Normal), new AgentRegistry(), isDeterministicClock: true);

        var nodeA = ExecutionNode.Create("NodeA", "A1", "Cap1");
        var nodeB = ExecutionNode.Create("NodeB", "A1", "Cap2", new[] { "NodeA" });
        var plan = new ExecutionPlan(id, new[] { nodeA, nodeB }, new[] { new ExecutionEdge("NodeA", "NodeB") }, new[] { "NodeA", "NodeB" }, 2000L);

        var executedOrder = new List<string>();
        bool success = await scheduler.ScheduleAndExecuteAsync(plan, execCtx, (node, ct) =>
        {
            lock (executedOrder) { executedOrder.Add(node.NodeId); }
            return Task.FromResult(true);
        });

        Assert.True(success);
        Assert.Equal(2, executedOrder.Count);
        Assert.Equal("NodeA", executedOrder[0]);
        Assert.Equal("NodeB", executedOrder[1]);
    }

    [Fact]
    public async Task ExecutionScheduler_RetryPolicy_SucceedsAfterTransientFailure()
    {
        var scheduler = new ExecutionScheduler(new SequentialExecutionStrategy());
        var id = new MissionIdentity("M-Retry", "1.0", 1000L, "hash");
        var opts = new MissionExecutionOptions(false, 1, false, 2, 30000L);
        var ctx = new MissionContext(id, new Dictionary<string, string>(), new Dictionary<string, string>(), Array.Empty<string>(), opts);
        var execCtx = MissionExecutionContext.Create(ctx, new MissionMetadata("Test", "D", "U", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), MissionPriority.Normal), new AgentRegistry(), isDeterministicClock: true);

        var node = ExecutionNode.Create("Node1", "A1", "Cap1", retryCount: 2);
        var plan = new ExecutionPlan(id, new[] { node }, Array.Empty<ExecutionEdge>(), new[] { "Node1" }, 1000L);

        int attempts = 0;
        bool success = await scheduler.ScheduleAndExecuteAsync(plan, execCtx, (n, ct) =>
        {
            attempts++;
            return Task.FromResult(attempts >= 2); // Fails first attempt, succeeds second attempt
        });

        Assert.True(success);
        Assert.Equal(2, attempts);
    }
}
