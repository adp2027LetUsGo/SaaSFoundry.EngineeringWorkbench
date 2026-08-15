// VibeStock — Static job dispatch registration.
// This file provides the implementing half of the partial methods declared in
// StaticJobDispatcher.cs (Generated/backgroundjob/StaticJobDispatcher.cs).
//
// ARCHITECTURE:
//   StaticJobDispatcher.TryDispatch is a partial method — the Generated half declares
//   the extension point; this file provides the VibeStock-specific implementation.
//   This pattern ensures AOT compatibility: no reflection, no dynamic dispatch.
//
// HOW TO ADD A NEW JOB TYPE:
//   1. Create a new IBackgroundJob record in VibeStock.System.Cell/Jobs/.
//   2. Create a corresponding IBackgroundJobHandler<TJob> in VibeStock.System.Cell/Jobs/.
//   3. Add a case to TryDispatch below.
//   4. Register the handler in VibeStockJobRegistrations.cs (AddVibeStockJobs).
// ------------------------------------------------------------------------------------------
#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;
using VibeStock.System.Cell.Jobs;

namespace VibeStock.System.Cell.Generated.BackgroundProcessing;

// Extends the generated StaticJobDispatcher with VibeStock-specific job routing.
public partial class StaticJobDispatcher
{
    partial void TryDispatch(
        EnqueuedJob job,
        JobExecutionContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken,
        ref Task? dispatchTask)
    {
        switch (job.JobTypeId)
        {
            case SyncProductToShopifyJob.TypeId:
                dispatchTask = DispatchSyncProductToShopifyAsync(job, context, serviceProvider, cancellationToken);
                break;
            // Add additional VibeStock job types here when introduced.
        }
    }

    private static async Task DispatchSyncProductToShopifyAsync(
        EnqueuedJob job,
        JobExecutionContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var payloadSerializer = serviceProvider.GetRequiredService<IJobPayloadSerializer>();
        var typedJob = payloadSerializer.Deserialize<SyncProductToShopifyJob>(job.JobTypeId, job.SerializedPayload);
        var handler = serviceProvider.GetRequiredService<IBackgroundJobHandler<SyncProductToShopifyJob>>();
        await handler.ExecuteAsync(typedJob, context, cancellationToken);
    }
}
