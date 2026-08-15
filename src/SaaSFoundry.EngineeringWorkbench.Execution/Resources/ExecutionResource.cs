#nullable enable

using System.Collections.Generic;

namespace SaaSFoundry.EngineeringWorkbench.Execution.Resources;

/// <summary>
/// Categorizes hardware, storage, and networking resources required during mission execution.
/// </summary>
public enum ExecutionResource
{
    /// <summary>Central processing computation cores.</summary>
    CPU = 0,
    /// <summary>System volatile RAM capacity.</summary>
    Memory = 1,
    /// <summary>Local disk file system operations.</summary>
    Disk = 2,
    /// <summary>Hardware accelerated GPU compute.</summary>
    GPU = 3,
    /// <summary>External network communications capability.</summary>
    Network = 4,
    /// <summary>Third-party cloud or LLM inference API endpoint access.</summary>
    ExternalAPI = 5,
    /// <summary>Persistent structured volume storage.</summary>
    Storage = 6
}

/// <summary>
/// Specifies quantitative hardware and runtime limits required by an execution node or capability.
/// </summary>
/// <param name="RequiredResources">List of strictly required system execution resources.</param>
/// <param name="MaximumResources">Maximum ceiling of allocable execution resources.</param>
/// <param name="EstimatedMemoryBytes">Expected peak volatile RAM consumption in bytes.</param>
/// <param name="EstimatedCPUCores">Expected CPU cores utilization coefficient.</param>
/// <param name="EstimatedDurationMilliseconds">Expected task runtime duration in milliseconds.</param>
public sealed record ResourceDescriptor(
    IReadOnlyList<ExecutionResource> RequiredResources,
    IReadOnlyList<ExecutionResource> MaximumResources,
    long EstimatedMemoryBytes,
    double EstimatedCPUCores,
    long EstimatedDurationMilliseconds
)
{
    /// <summary>
    /// Gets the standard baseline execution resource configuration.
    /// </summary>
    public static ResourceDescriptor Default => new(
        new[] { ExecutionResource.CPU, ExecutionResource.Memory },
        new[] { ExecutionResource.CPU, ExecutionResource.Memory, ExecutionResource.Storage },
        512 * 1024 * 1024L, // 512 MB
        1.0,
        5000L
    );
}
