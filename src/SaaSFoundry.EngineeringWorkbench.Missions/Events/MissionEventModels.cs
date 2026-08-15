#nullable enable

using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;

namespace SaaSFoundry.EngineeringWorkbench.Missions.Events;

/// <summary>Emitted when a new multi-agent engineering mission is initialized in memory.</summary>
public sealed record MissionCreatedEvent(string MissionId, long Timestamp) : IMissionEvent;

/// <summary>Emitted when a deterministic execution plan has been generated and validated.</summary>
public sealed record MissionPlannedEvent(string MissionId, int TotalNodes, long Timestamp) : IMissionEvent;

/// <summary>Emitted when the execution scheduler commences processing DAG nodes.</summary>
public sealed record MissionStartedEvent(string MissionId, long Timestamp) : IMissionEvent;

/// <summary>Emitted when all required mission execution nodes conclude successfully.</summary>
public sealed record MissionCompletedEvent(string MissionId, long DurationMilliseconds, long Timestamp) : IMissionEvent;

/// <summary>Emitted when an active mission execution is explicitly cancelled.</summary>
public sealed record MissionCancelledEvent(string MissionId, long Timestamp) : IMissionEvent;

/// <summary>Emitted when mission execution terminates due to an unrecoverable failure or critical error.</summary>
public sealed record MissionFailedEvent(string MissionId, string Reason, long Timestamp) : IMissionEvent;

/// <summary>Emitted when an individual agent worker begins execution of a scheduled DAG node.</summary>
public sealed record NodeExecutionStartedEvent(string MissionId, string NodeId, long Timestamp) : IMissionEvent;

/// <summary>Emitted upon termination of an individual node execution task.</summary>
public sealed record NodeExecutionCompletedEvent(string MissionId, string NodeId, bool Succeeded, long Timestamp) : IMissionEvent;

/// <summary>Emitted when a prerequisite dependency relationship between two nodes is satisfied.</summary>
public sealed record DependencySatisfiedEvent(string MissionId, string SourceNodeId, string TargetNodeId, long Timestamp) : IMissionEvent;

/// <summary>Emitted when a participating agent generates a new deliverable artifact.</summary>
public sealed record ArtifactProducedEvent(string MissionId, string ArtifactId, long Timestamp) : IMissionEvent;

/// <summary>Emitted upon conclusion of automated engineering validation checks.</summary>
public sealed record ValidationCompletedEvent(string MissionId, bool Passed, long Timestamp) : IMissionEvent;

/// <summary>Emitted when an associated Engineering Package is generated and certified.</summary>
public sealed record EngineeringPackageGeneratedEvent(string MissionId, string PackageHash, long Timestamp) : IMissionEvent;

/// <summary>Emitted when the complete Mission Package is compiled and sealed with a SHA256 cryptographic digest.</summary>
public sealed record MissionPackageGeneratedEvent(string MissionId, string MissionPackageHash, long Timestamp) : IMissionEvent;
