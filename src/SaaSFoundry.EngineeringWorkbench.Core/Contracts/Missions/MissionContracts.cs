#nullable enable

using System;
using System.Collections.Generic;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;

/// <summary>
/// Specifies the execution priority of a multi-agent engineering mission.
/// </summary>
public enum MissionPriority
{
    /// <summary>Low priority execution.</summary>
    Low = 0,
    /// <summary>Standard execution priority.</summary>
    Normal = 1,
    /// <summary>High priority execution.</summary>
    High = 2,
    /// <summary>Urgent execution requiring preferential scheduling.</summary>
    Urgent = 3,
    /// <summary>Critical priority requiring immediate resources.</summary>
    Critical = 4
}

/// <summary>
/// Defines the lifecycle execution state of an engineering mission.
/// </summary>
public enum MissionExecutionStatus
{
    /// <summary>Mission is initialized in memory.</summary>
    Created = 0,
    /// <summary>Execution plan and DAG topology have been verified.</summary>
    Planned = 1,
    /// <summary>Mission is awaiting scheduler resources.</summary>
    Queued = 2,
    /// <summary>Mission DAG execution is actively progressing.</summary>
    Executing = 3,
    /// <summary>Execution is temporarily suspended awaiting intervention or approval.</summary>
    Paused = 4,
    /// <summary>All required mission nodes completed successfully.</summary>
    Completed = 5,
    /// <summary>Mission execution was explicitly aborted.</summary>
    Cancelled = 6,
    /// <summary>Mission terminated due to unrecoverable errors or critical node failures.</summary>
    Failed = 7
}

/// <summary>
/// Represents the immutable cryptographic identity and version of an engineering mission.
/// </summary>
/// <param name="MissionId">Unique identifier of the mission.</param>
/// <param name="MissionVersion">Semantic version of the mission workflow.</param>
/// <param name="CreationTimestamp">Epoch timestamp in milliseconds when the mission identity was originated.</param>
/// <param name="MissionFingerprint">SHA-256 cryptographic hash identifying the mission specification.</param>
public sealed record MissionIdentity(
    string MissionId,
    string MissionVersion,
    long CreationTimestamp,
    string MissionFingerprint
);

/// <summary>
/// Encapsulates operational metadata describing an engineering mission's structure and prerequisites.
/// </summary>
/// <param name="Name">Human-readable mission name.</param>
/// <param name="Description">Detailed operational statement of work.</param>
/// <param name="RequestedBy">Identity of the user or automated supervisor initiating the mission.</param>
/// <param name="RequiredAgents">Explicit list of agent IDs required to fulfill mission tasks.</param>
/// <param name="RequiredCapabilities">Explicit capability tokens required across participating agents.</param>
/// <param name="Dependencies">List of prerequisite artifact IDs or external mission requirements.</param>
/// <param name="Priority">Assigned execution priority.</param>
public sealed record MissionMetadata(
    string Name,
    string Description,
    string RequestedBy,
    IReadOnlyList<string> RequiredAgents,
    IReadOnlyList<string> RequiredCapabilities,
    IReadOnlyList<string> Dependencies,
    MissionPriority Priority
);

/// <summary>
/// Specifies scheduling and resiliency controls governing mission execution.
/// </summary>
/// <param name="AllowParallelExecution">Whether non-dependent DAG nodes may execute concurrently.</param>
/// <param name="MaximumParallelAgents">Maximum number of concurrent agent threads allowed.</param>
/// <param name="ContinueOnFailure">Whether non-critical branches should continue executing after a node failure.</param>
/// <param name="RetryPolicy">Maximum attempts allowed for transient node execution failures.</param>
/// <param name="TimeoutMilliseconds">Global timeout limit for the entire mission execution.</param>
public sealed record MissionExecutionOptions(
    bool AllowParallelExecution,
    int MaximumParallelAgents,
    bool ContinueOnFailure,
    int RetryPolicy,
    long TimeoutMilliseconds
);

/// <summary>
/// Represents the initialized parameter runtime context for a mission execution.
/// </summary>
/// <param name="MissionIdentity">The target mission identity.</param>
/// <param name="Inputs">Input key-value configuration parameters passed into the execution.</param>
/// <param name="Configuration">System configuration environment variables for participating agents.</param>
/// <param name="RequestedArtifacts">List of target deliverable filenames expected upon mission completion.</param>
/// <param name="ExecutionOptions">Runtime scheduling options.</param>
public sealed record MissionContext(
    MissionIdentity MissionIdentity,
    IReadOnlyDictionary<string, string> Inputs,
    IReadOnlyDictionary<string, string> Configuration,
    IReadOnlyList<string> RequestedArtifacts,
    MissionExecutionOptions ExecutionOptions
);

/// <summary>
/// Provides an aggregated snapshot descriptor of an active or completed mission.
/// </summary>
/// <param name="Identity">Mission cryptographic identity.</param>
/// <param name="Metadata">Associated mission metadata.</param>
/// <param name="ExecutionStatus">String representation of the latest operational execution state.</param>
/// <param name="UpdatedTimestamp">Timestamp in milliseconds of the last recorded state change.</param>
public sealed record MissionDescriptor(
    MissionIdentity Identity,
    MissionMetadata Metadata,
    string ExecutionStatus,
    long UpdatedTimestamp
);

/// <summary>
/// Certifies the cryptographic consistency and validation status of a mission delivery artifact.
/// </summary>
/// <param name="MissionId">The certified mission ID.</param>
/// <param name="CertificationHash">SHA-256 hash across all execution logs, timeline events, and artifacts.</param>
/// <param name="IsVerified">Whether validation engine checks completed without errors.</param>
/// <param name="CertifiedTimestamp">Epoch timestamp when the certification was issued.</param>
public sealed record MissionCertificationDescriptor(
    string MissionId,
    string CertificationHash,
    bool IsVerified,
    long CertifiedTimestamp
);

/// <summary>
/// Represents the canonical conclusion record of a completed engineering mission.
/// </summary>
/// <param name="MissionIdentity">The executed mission identity.</param>
/// <param name="MissionExecutionStatus">Final terminal execution state achieved.</param>
/// <param name="Artifacts">Collection of generated output file identifiers or paths.</param>
/// <param name="Diagnostics">Collection of warning or informative trace logs generated during execution.</param>
/// <param name="ValidationEvidence">Cryptographically verifiable evidence IDs from validation audits.</param>
/// <param name="ExecutionDurationMilliseconds">Total clock elapsed time for mission fulfillment.</param>
/// <param name="Succeeded">True if all required deliverables and critical nodes completed without uncaught error.</param>
public sealed record MissionResult(
    MissionIdentity MissionIdentity,
    MissionExecutionStatus MissionExecutionStatus,
    IReadOnlyList<string> Artifacts,
    IReadOnlyList<string> Diagnostics,
    IReadOnlyList<string> ValidationEvidence,
    long ExecutionDurationMilliseconds,
    bool Succeeded
);

/// <summary>
/// Represents the foundational contract for any event emitted during mission lifecycle transition or progress.
/// </summary>
public interface IMissionEvent
{
    /// <summary>Gets the unique identifier of the mission generating the event.</summary>
    string MissionId { get; }
    /// <summary>Gets the UTC epoch timestamp in milliseconds when the event occurred.</summary>
    long Timestamp { get; }
}
