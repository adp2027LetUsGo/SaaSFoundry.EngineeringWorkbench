#nullable enable

namespace SaaSFoundry.EngineeringWorkbench.Execution.Strategies;

/// <summary>
/// Represents an algorithm governing DAG concurrency, task scheduling, and prioritization.
/// </summary>
public interface IExecutionStrategy
{
    /// <summary>Gets the unique human-readable identifier of the execution strategy.</summary>
    string StrategyName { get; }
    /// <summary>Gets a value indicating whether independent nodes may execute concurrently.</summary>
    bool AllowParallel { get; }
    /// <summary>Gets the maximum allowed number of concurrent agent node executions.</summary>
    int MaxConcurrency { get; }
}

/// <summary>
/// Enforces strict sequential processing of DAG nodes in deterministic topological order.
/// </summary>
public sealed class SequentialExecutionStrategy : IExecutionStrategy
{
    /// <inheritdoc />
    public string StrategyName => "Sequential";
    /// <inheritdoc />
    public bool AllowParallel => false;
    /// <inheritdoc />
    public int MaxConcurrency => 1;
}

/// <summary>
/// Enables concurrent multi-agent execution across independent branches of the DAG.
/// </summary>
public sealed class ParallelExecutionStrategy : IExecutionStrategy
{
    /// <inheritdoc />
    public string StrategyName => "Parallel";
    /// <inheritdoc />
    public bool AllowParallel => true;
    /// <inheritdoc />
    public int MaxConcurrency { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelExecutionStrategy"/> class.
    /// </summary>
    /// <param name="maxConcurrency">Maximum allowed concurrent worker threads (defaults to 4 if non-positive).</param>
    public ParallelExecutionStrategy(int maxConcurrency = 4)
    {
        MaxConcurrency = maxConcurrency > 0 ? maxConcurrency : 4;
    }
}

/// <summary>
/// Schedules node execution strictly according to assigned priority weighting and dependencies.
/// </summary>
public sealed class PriorityExecutionStrategy : IExecutionStrategy
{
    /// <inheritdoc />
    public string StrategyName => "Priority";
    /// <inheritdoc />
    public bool AllowParallel => false;
    /// <inheritdoc />
    public int MaxConcurrency => 1;
}

/// <summary>
/// Evaluates and tests DAG mission execution topologies in dry-run simulation mode without real I/O mutations.
/// </summary>
public sealed class SimulationExecutionStrategy : IExecutionStrategy
{
    /// <inheritdoc />
    public string StrategyName => "Simulation";
    /// <inheritdoc />
    public bool AllowParallel => true;
    /// <inheritdoc />
    public int MaxConcurrency => 10;
}
