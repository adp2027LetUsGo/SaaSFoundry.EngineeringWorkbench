using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport;
using SaaSFoundry.SDK.Commerce;
using SaaSFoundry.SDK.Commerce.Models;

namespace VibeStock.Bridge.Cell.IntegrationTests;

// Mock Idempotency Enforcer
public class MockIdempotencyEnforcer : IIdempotencyEnforcer
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _keys = new();

    public Task<IdempotencyAcquisitionStatus> TryAcquireAsync(string tenantId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        if (_keys.TryAdd(idempotencyKey, "InProgress"))
        {
            return Task.FromResult(IdempotencyAcquisitionStatus.Acquired);
        }
        return Task.FromResult(IdempotencyAcquisitionStatus.AlreadyProcessed);
    }

    public Task CompleteAsync(string tenantId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        _keys[idempotencyKey] = "Completed";
        return Task.CompletedTask;
    }
}

// Mock Commerce Manager
public class MockCommerceProductManager : ICommerceProductManager
{
    public int CreateCount { get; private set; }

    public Task<CommerceResult<CommerceProduct>> CreateAsync(CommerceProduct product, CancellationToken cancellationToken = default)
    {
        CreateCount++;
        product.ExternalId = "gid://mock/" + Guid.NewGuid();
        return Task.FromResult(CommerceResult<CommerceProduct>.Success(product));
    }

    public Task<CommerceResult<CommerceProduct>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

// Simulated Bridge orchestrator
public class VibeStockShopifyBridgeService
{
    private readonly ICommerceProductManager _commerceManager;
    private readonly IIdempotencyEnforcer _idempotencyEnforcer;

    public VibeStockShopifyBridgeService(ICommerceProductManager commerceManager, IIdempotencyEnforcer idempotencyEnforcer)
    {
        _commerceManager = commerceManager;
        _idempotencyEnforcer = idempotencyEnforcer;
    }

    public async Task<CommerceResult<CommerceProduct>> SyncProductAsync(string idempotencyKey, string title, string vendor)
    {
        var status = await _idempotencyEnforcer.TryAcquireAsync("tenant", idempotencyKey);
        if (status == IdempotencyAcquisitionStatus.AlreadyProcessed)
        {
            return CommerceResult<CommerceProduct>.Failure(new CommerceError(CommerceErrorType.Conflict, "Duplicate sync intercepted by idempotency."));
        }

        var product = new CommerceProduct { Title = title, Vendor = vendor };
        var result = await _commerceManager.CreateAsync(product);

        if (result.IsSuccess)
        {
            await _idempotencyEnforcer.CompleteAsync("tenant", idempotencyKey);
        }
        return result;
    }
}

public class VibeStockBridgeIdempotencyTests
{
    [Fact]
    public async Task SyncProductAsync_DuplicateCalls_BlockedByIdempotencyEnforcer()
    {
        var manager = new MockCommerceProductManager();
        var enforcer = new MockIdempotencyEnforcer();
        var service = new VibeStockShopifyBridgeService(manager, enforcer);

        var key = "sync_req_12345";

        // First call should succeed
        var result1 = await service.SyncProductAsync(key, "Test Shirt", "VibeStock");
        Assert.True(result1.IsSuccess);
        Assert.Equal(1, manager.CreateCount);

        // Second call with same idempotency key should be blocked
        var result2 = await service.SyncProductAsync(key, "Test Shirt", "VibeStock");
        Assert.False(result2.IsSuccess);
        Assert.Equal(CommerceErrorType.Conflict, result2.Errors[0].Type);
        
        // Ensure Commerce manager was NOT called a second time
        Assert.Equal(1, manager.CreateCount);
    }
}
