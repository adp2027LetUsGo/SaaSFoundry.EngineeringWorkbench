// VibeStock — Static job payload serialization registration.
// This file provides the implementing half of the partial methods declared in
// JobPayloadSerializer.cs (Generated/backgroundjob/JobPayloadSerializer.cs).
//
// ARCHITECTURE:
//   AOT-compatible serialization uses source-generated JsonSerializerContext.
//   No reflection. No dynamic type discovery.
// ------------------------------------------------------------------------------------------
#nullable enable

using System.Text.Json;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;
using VibeStock.System.Cell.Jobs;

namespace VibeStock.System.Cell.Generated.BackgroundProcessing;

// Extends the generated JobPayloadSerializer with VibeStock-specific type registrations.
public partial class JobPayloadSerializer
{
    partial void TrySerialize<TJob>(TJob job, ref string? serialized)
        where TJob : IBackgroundJob
    {
        if (job is SyncProductToShopifyJob syncJob)
        {
            serialized = JsonSerializer.Serialize(
                syncJob,
                SyncProductToShopifyJobSerializerContext.Default.SyncProductToShopifyJob);
        }
        // Add additional job types here.
    }

    partial void TryDeserialize<TJob>(string jobTypeId, string serializedPayload, ref TJob? typedJob)
        where TJob : IBackgroundJob
    {
        if (jobTypeId == SyncProductToShopifyJob.TypeId && typeof(TJob) == typeof(SyncProductToShopifyJob))
        {
            var deserialized = JsonSerializer.Deserialize(
                serializedPayload,
                SyncProductToShopifyJobSerializerContext.Default.SyncProductToShopifyJob);
            typedJob = (TJob?)(IBackgroundJob?)deserialized;
        }
        // Add additional job types here.
    }
}
