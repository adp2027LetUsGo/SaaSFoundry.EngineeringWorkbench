using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport;
using SaaSFoundry.Persistence.Idempotency;

namespace VibeStock.System.Cell.IntegrationTests;

public class NpgsqlIdempotencyEnforcerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS sys_idempotency_keys (
                tenant_id VARCHAR(100) NOT NULL,
                idempotency_key VARCHAR(100) NOT NULL,
                status VARCHAR(50) NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
                PRIMARY KEY (tenant_id, idempotency_key)
            );
        ";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public async Task Test1_NewKey_Acquired()
    {
        var enforcer = new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5));
        var status = await enforcer.TryAcquireAsync("tenant-a", "key-1");

        Assert.Equal(IdempotencyAcquisitionStatus.Acquired, status);

        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT status, expires_at FROM sys_idempotency_keys WHERE tenant_id = 'tenant-a' AND idempotency_key = 'key-1'";
        await using var reader = await cmd.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal("InProgress", reader.GetString(0));
        Assert.True(reader.GetDateTime(1).ToUniversalTime() > DateTime.UtcNow);
    }

    [Fact]
    public async Task Test2_ExistingInProgressKey_ReturnsInProgress()
    {
        var enforcer = new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5));
        var status1 = await enforcer.TryAcquireAsync("tenant-a", "key-2");
        Assert.Equal(IdempotencyAcquisitionStatus.Acquired, status1);

        var status2 = await enforcer.TryAcquireAsync("tenant-a", "key-2");
        Assert.Equal(IdempotencyAcquisitionStatus.InProgress, status2);
    }

    [Fact]
    public async Task Test3_Complete_TransitionsToAlreadyProcessed()
    {
        var enforcer = new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5));
        await enforcer.TryAcquireAsync("tenant-a", "key-3");
        await enforcer.CompleteAsync("tenant-a", "key-3");

        var status = await enforcer.TryAcquireAsync("tenant-a", "key-3");
        Assert.Equal(IdempotencyAcquisitionStatus.AlreadyProcessed, status);

        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT status FROM sys_idempotency_keys WHERE tenant_id = 'tenant-a' AND idempotency_key = 'key-3'";
        var dbStatus = (string)await cmd.ExecuteScalarAsync()!;
        Assert.Equal("AlreadyProcessed", dbStatus);
    }

    [Fact]
    public async Task Test4_UnknownComplete_DoesNotThrow()
    {
        var enforcer = new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5));
        
        // Completing an unknown key silently does nothing as it updates 0 rows
        await enforcer.CompleteAsync("tenant-a", "unknown-key");
        
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM sys_idempotency_keys WHERE tenant_id = 'tenant-a' AND idempotency_key = 'unknown-key'";
        var count = (long)await cmd.ExecuteScalarAsync()!;
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Test5_CrossTenantIsolation()
    {
        var enforcer = new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5));
        var statusA = await enforcer.TryAcquireAsync("tenant-a", "shared-key");
        var statusB = await enforcer.TryAcquireAsync("tenant-b", "shared-key");

        Assert.Equal(IdempotencyAcquisitionStatus.Acquired, statusA);
        Assert.Equal(IdempotencyAcquisitionStatus.Acquired, statusB);
    }

    [Fact]
    public async Task Test6_SameTenantDifferentKeys()
    {
        var enforcer = new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5));
        var statusX = await enforcer.TryAcquireAsync("tenant-c", "key-x");
        var statusY = await enforcer.TryAcquireAsync("tenant-c", "key-y");

        Assert.Equal(IdempotencyAcquisitionStatus.Acquired, statusX);
        Assert.Equal(IdempotencyAcquisitionStatus.Acquired, statusY);
    }

    [Fact]
    public async Task Test7_AtomicConcurrency()
    {
        var enforcer = new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5));
        int concurrencyLevel = 50;
        var tasks = new Task<IdempotencyAcquisitionStatus>[concurrencyLevel];
        var startGate = new SemaphoreSlim(0, concurrencyLevel);
        
        for (int i = 0; i < concurrencyLevel; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                await startGate.WaitAsync();
                return await enforcer.TryAcquireAsync("tenant-atomic", "atomic-key");
            });
        }

        startGate.Release(concurrencyLevel);
        var results = await Task.WhenAll(tasks);

        int acquiredCount = results.Count(r => r == IdempotencyAcquisitionStatus.Acquired);
        Assert.Equal(1, acquiredCount);
        Assert.True(results.All(r => r == IdempotencyAcquisitionStatus.Acquired || r == IdempotencyAcquisitionStatus.InProgress));
    }

    [Fact]
    public async Task Test8_ExpiredInProgressKey()
    {
        var enforcer = new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5));
        await enforcer.TryAcquireAsync("tenant-exp", "exp-key");

        // Manually expire the key
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE sys_idempotency_keys SET expires_at = @Past WHERE tenant_id = 'tenant-exp' AND idempotency_key = 'exp-key'";
        cmd.Parameters.AddWithValue("Past", DateTimeOffset.UtcNow.AddMinutes(-10));
        await cmd.ExecuteNonQueryAsync();

        var status = await enforcer.TryAcquireAsync("tenant-exp", "exp-key");
        Assert.Equal(IdempotencyAcquisitionStatus.Acquired, status);
    }

    [Fact]
    public async Task Test9_ConcurrentTtlRecovery()
    {
        var enforcer = new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5));
        await enforcer.TryAcquireAsync("tenant-ttl", "ttl-key");

        // Manually expire the key
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE sys_idempotency_keys SET expires_at = @Past WHERE tenant_id = 'tenant-ttl' AND idempotency_key = 'ttl-key'";
        cmd.Parameters.AddWithValue("Past", DateTimeOffset.UtcNow.AddMinutes(-10));
        await cmd.ExecuteNonQueryAsync();

        int concurrencyLevel = 50;
        var tasks = new Task<IdempotencyAcquisitionStatus>[concurrencyLevel];
        var startGate = new SemaphoreSlim(0, concurrencyLevel);
        
        for (int i = 0; i < concurrencyLevel; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                await startGate.WaitAsync();
                return await enforcer.TryAcquireAsync("tenant-ttl", "ttl-key");
            });
        }

        startGate.Release(concurrencyLevel);
        var results = await Task.WhenAll(tasks);

        int acquiredCount = results.Count(r => r == IdempotencyAcquisitionStatus.Acquired);
        Assert.Equal(1, acquiredCount);
        Assert.True(results.All(r => r == IdempotencyAcquisitionStatus.Acquired || r == IdempotencyAcquisitionStatus.InProgress));
    }

    [Fact]
    public async Task Test10_CompletedKeyNeverReacquires()
    {
        var enforcer = new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5));
        await enforcer.TryAcquireAsync("tenant-never", "never-key");
        await enforcer.CompleteAsync("tenant-never", "never-key");

        // Expire the already processed key (shouldn't matter)
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE sys_idempotency_keys SET expires_at = @Past WHERE tenant_id = 'tenant-never' AND idempotency_key = 'never-key'";
        cmd.Parameters.AddWithValue("Past", DateTimeOffset.UtcNow.AddMinutes(-10));
        await cmd.ExecuteNonQueryAsync();

        var status = await enforcer.TryAcquireAsync("tenant-never", "never-key");
        Assert.Equal(IdempotencyAcquisitionStatus.AlreadyProcessed, status);
    }

    [Fact]
    public async Task Test11_12_DatabaseUniquenessAndTenantCollision()
    {
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO sys_idempotency_keys (tenant_id, idempotency_key, status, created_at, expires_at) VALUES ('tenant-a', 'key-x', 'InProgress', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";
        await cmd.ExecuteNonQueryAsync();

        await using var cmdConflict = connection.CreateCommand();
        cmdConflict.CommandText = "INSERT INTO sys_idempotency_keys (tenant_id, idempotency_key, status, created_at, expires_at) VALUES ('tenant-a', 'key-x', 'InProgress', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";
        
        await Assert.ThrowsAsync<PostgresException>(() => cmdConflict.ExecuteNonQueryAsync());

        // Cross-tenant collision should be valid
        await using var cmdValid = connection.CreateCommand();
        cmdValid.CommandText = "INSERT INTO sys_idempotency_keys (tenant_id, idempotency_key, status, created_at, expires_at) VALUES ('tenant-b', 'key-x', 'InProgress', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";
        await cmdValid.ExecuteNonQueryAsync(); // Should not throw
    }

    [Fact]
    public async Task Test16_MultiInstanceSafety()
    {
        int instanceCount = 10;
        var enforcers = Enumerable.Range(0, instanceCount)
            .Select(_ => new NpgsqlIdempotencyEnforcer(_dbContainer.GetConnectionString(), TimeSpan.FromMinutes(5)))
            .ToList();

        var tasks = new Task<IdempotencyAcquisitionStatus>[instanceCount];
        var startGate = new SemaphoreSlim(0, instanceCount);
        
        for (int i = 0; i < instanceCount; i++)
        {
            var enforcer = enforcers[i];
            tasks[i] = Task.Run(async () =>
            {
                await startGate.WaitAsync();
                return await enforcer.TryAcquireAsync("tenant-multi", "multi-key");
            });
        }

        startGate.Release(instanceCount);
        var results = await Task.WhenAll(tasks);

        int acquiredCount = results.Count(r => r == IdempotencyAcquisitionStatus.Acquired);
        Assert.Equal(1, acquiredCount);
    }
}
