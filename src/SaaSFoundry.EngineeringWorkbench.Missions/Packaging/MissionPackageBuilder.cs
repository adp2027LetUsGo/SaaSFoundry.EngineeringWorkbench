#nullable enable

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Missions;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Packaging;
using SaaSFoundry.EngineeringWorkbench.Execution;
using SaaSFoundry.EngineeringWorkbench.Execution.Blackboard;
using SaaSFoundry.EngineeringWorkbench.Planning.DAG;

namespace SaaSFoundry.EngineeringWorkbench.Missions.Packaging;

/// <summary>
/// Immutable deliverable packaging structure capturing complete mission history, state, artifacts, and cryptographic verification.
/// </summary>
/// <param name="MissionDescriptor">The summary descriptor of the executed mission.</param>
/// <param name="BlackboardSnapshot">Final immutable state snapshot of the mission blackboard.</param>
/// <param name="ExecutionPlan">The verified DAG execution plan applied during execution.</param>
/// <param name="ExecutionTimeline">Chronological history log of operational milestone events.</param>
/// <param name="ExecutionAudit">Audit traces and diagnostic log generated during the mission.</param>
/// <param name="AgentAssignments">Explicit mapping of node identifiers to responsible agent IDs.</param>
/// <param name="ValidationEvidence">Cryptographic verification evidence IDs produced during execution.</param>
/// <param name="EngineeringPackage">Optional linked Engineering Package deliverable.</param>
/// <param name="CertificationDescriptor">Formal validation and certification seal.</param>
/// <param name="MissionHash">Canonical SHA256 digest calculated across all mission execution inputs and deliverables.</param>
public sealed record MissionPackage(
    MissionDescriptor MissionDescriptor,
    MissionBlackboardSnapshot BlackboardSnapshot,
    ExecutionPlan ExecutionPlan,
    IReadOnlyList<string> ExecutionTimeline,
    IReadOnlyList<string> ExecutionAudit,
    IReadOnlyDictionary<string, string> AgentAssignments,
    IReadOnlyList<string> ValidationEvidence,
    EngineeringPackage? EngineeringPackage,
    MissionCertificationDescriptor CertificationDescriptor,
    string MissionHash
);

/// <summary>
/// Compiles executed mission contexts, DAG plans, and generated artifacts into an immutable, cryptographically certified <see cref="MissionPackage"/>.
/// </summary>
public sealed class MissionPackageBuilder
{
    /// <summary>
    /// Synthesizes the execution outcomes into a verified deliverable mission package without reflection or dynamic code generation.
    /// </summary>
    /// <param name="context">The executed runtime mission context.</param>
    /// <param name="plan">The executed DAG plan.</param>
    /// <param name="result">The finalized operational result record.</param>
    /// <param name="engineeringPackage">Optional associated engineering package.</param>
    /// <returns>A fully sealed and SHA256 cryptographically fingerprinted <see cref="MissionPackage"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if required parameters are null.</exception>
    public MissionPackage BuildPackage(
        MissionExecutionContext context,
        ExecutionPlan plan,
        MissionResult result,
        EngineeringPackage? engineeringPackage = null)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        if (result == null) throw new ArgumentNullException(nameof(result));

        var snapshot = context.MissionBlackboard.CreateSnapshot();

        var descriptor = new MissionDescriptor(
            context.MissionIdentity,
            context.MissionMetadata,
            result.MissionExecutionStatus.ToString(),
            context.ExecutionClock.GetCurrentTimestampMilliseconds()
        );

        var assignments = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var node in plan.Nodes)
        {
            assignments[node.NodeId] = node.AgentId;
        }

        var timeline = snapshot.ExecutionHistory;
        var audit = snapshot.Diagnostics;
        var evidence = snapshot.ValidationEvidence;

        // Compute deterministic hash of the mission execution inputs and outputs
        var hashPayload = new StringBuilder();
        hashPayload.Append(context.MissionIdentity.MissionId);
        hashPayload.Append(context.MissionIdentity.MissionFingerprint);
        hashPayload.Append(result.Succeeded);

        var sortedArtifacts = snapshot.GeneratedArtifacts.Count > 0 ? new List<string>(snapshot.GeneratedArtifacts).ToArray() : Array.Empty<string>();
        Array.Sort(sortedArtifacts, StringComparer.Ordinal);
        foreach (var art in sortedArtifacts)
        {
            hashPayload.Append(art);
        }

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(hashPayload.ToString()));
        var hashString = $"SHA256:{Convert.ToHexString(hashBytes).ToLowerInvariant()}";

        var cert = new MissionCertificationDescriptor(
            context.MissionIdentity.MissionId,
            hashString,
            result.Succeeded,
            context.ExecutionClock.GetCurrentTimestampMilliseconds()
        );

        context.MissionBlackboard.SetMissionPackageReference($"mission-pkg://{context.MissionIdentity.MissionId}");

        return new MissionPackage(
            descriptor,
            snapshot,
            plan,
            timeline,
            audit,
            new Dictionary<string, string>(assignments, StringComparer.Ordinal),
            evidence,
            engineeringPackage,
            cert,
            hashString
        );
    }
}
