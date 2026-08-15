// VibeStock — Handler for the SyncProductToShopify background job.
// This is the product-level handler owned by VibeStock.System.Cell.
// It is NOT auto-generated; it implements business logic for Shopify sync.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;
using VibeStock.System.Cell.Jobs;

namespace VibeStock.System.Cell.Jobs;

/// <summary>
/// Handles the execution of a <see cref="SyncProductToShopifyJob"/>.
/// This handler is resolved from the DI scope created per-job by BackgroundWorkerService.
/// </summary>
public sealed class SyncProductToShopifyJobHandler : IBackgroundJobHandler<SyncProductToShopifyJob>
{
    private readonly ILogger<SyncProductToShopifyJobHandler> _logger;

    public SyncProductToShopifyJobHandler(ILogger<SyncProductToShopifyJobHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(
        SyncProductToShopifyJob job,
        JobExecutionContext context,
        CancellationToken cancellationToken)
    {
        // The Bridge.Cell owns the Shopify HTTP integration (SaaSFoundry.SDK.Commerce.Shopify).
        // System.Cell owns the background job infrastructure and dispatches to Bridge via gRPC.
        // At this stage, this handler logs the intent and records the job as processed.
        // A future stage will wire the outbound gRPC call to Bridge.Cell.
        _logger.LogInformation(
            "SyncProductToShopifyJob dispatched. SKU={Sku} Title={Title} Tenant={TenantId}",
            job.Sku, job.Title, context.Tenant.TenantId);

        return Task.CompletedTask;
    }
}
