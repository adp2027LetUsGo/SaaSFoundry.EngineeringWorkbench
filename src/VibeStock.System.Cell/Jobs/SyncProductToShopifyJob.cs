// VibeStock — Shopify product synchronization background job.
// This is the product-level job type owned by VibeStock.System.Cell.
// It is NOT auto-generated; it is the VibeStock-specific job payload.

using System.Text.Json;
using System.Text.Json.Serialization;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

namespace VibeStock.System.Cell.Jobs;

/// <summary>
/// Background job payload for a single product-to-Shopify synchronisation operation.
/// The job is enqueued by the Bridge.Cell and dispatched by System.Cell's BackgroundWorkerService.
/// </summary>
public sealed record SyncProductToShopifyJob : IBackgroundJob
{
    /// <summary>
    /// Stable job type identifier — stored in the DB and used for static dispatch.
    /// </summary>
    public const string TypeId = "VibeStock.SyncProductToShopify";

    /// <summary>Implements <see cref="IBackgroundJob.JobTypeId"/>.</summary>
    public string JobTypeId => TypeId;

    /// <summary>The product SKU to synchronise.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>The product title to synchronise.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The vendor name.</summary>
    public string Vendor { get; init; } = string.Empty;

    /// <summary>The product description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Price in minor units (e.g., cents).</summary>
    public decimal Price { get; init; }

    /// <summary>Available inventory quantity.</summary>
    public int InventoryQuantity { get; init; }
}

/// <summary>
/// AOT-compatible JSON serializer context for <see cref="SyncProductToShopifyJob"/>.
/// </summary>
[JsonSerializable(typeof(SyncProductToShopifyJob))]
internal partial class SyncProductToShopifyJobSerializerContext : JsonSerializerContext
{
}
