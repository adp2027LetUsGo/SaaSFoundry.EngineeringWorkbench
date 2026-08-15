using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;
using VibeStock.System.Cell.Generated.Persistence;
using VibeStock.System.Cell.Generated.BackgroundProcessing;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using System.Collections.Generic;
using Npgsql;

namespace VibeStock.System.Cell.IntegrationTests;

public class BackgroundWorkerServiceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Create the necessary tables manually for the test
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE SystemCellJobs (
                JobId text PRIMARY KEY,
                JobTypeId text NOT NULL,
                SerializedPayload text NOT NULL,
                SerializedContext text NOT NULL,
                Status integer NOT NULL,
                AttemptCount integer NOT NULL DEFAULT 0,
                NextExecutionTime timestamp with time zone,
                StartedAt timestamp with time zone,
                CompletedAt timestamp with time zone,
                FailureErrorMessage text,
                FailureStackTrace text,
                FailedAt timestamp with time zone,
                CreatedAt timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
        ";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public async Task EndToEndJobTest()
    {
        // 1. Arrange Services
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());

        services.AddSingleton<IJobStorageCapability>(new SystemCellJobStorage(_dbContainer.GetConnectionString()));

        services.AddSingleton<IJobContextSerializer, JobContextSerializer>();
        services.AddSingleton<IJobPayloadSerializer, TestJobPayloadSerializer>();
        services.AddSingleton<IJobDispatcher, TestJobDispatcher>();

        var testHandler = new MockTestJobHandler();
        services.AddSingleton<IBackgroundJobHandler<TestJob>>(testHandler);

        services.AddHostedService<BackgroundWorkerService>();

        await using var provider = services.BuildServiceProvider();

        // 2. Enqueue the job via IJobStorageCapability
        var jobStorage = provider.GetRequiredService<IJobStorageCapability>();
        
        var payloadSerializer = provider.GetRequiredService<IJobPayloadSerializer>();
        var contextSerializer = provider.GetRequiredService<IJobContextSerializer>();

        var job = new TestJob("Hello, World!");
        var context = new JobExecutionContext(
            new IdentityContext("user-1", "user@example.com", new Dictionary<string, string>(), "Tenant 1"),
            new TenantContext("tenant-1"),
            new AuthorizationContext(Array.Empty<string>(), Array.Empty<string>())
        );

        var serializedPayload = payloadSerializer.Serialize(job);
        var serializedContext = contextSerializer.Serialize(context);

        var jobId = await jobStorage.EnqueueAsync(job.JobTypeId, serializedPayload, serializedContext, null, CancellationToken.None);

        // 3. Start the Hosted Service (Worker)
        var hostedServices = provider.GetServices<IHostedService>();
        var worker = (BackgroundWorkerService)hostedServices.First();
        
        var cts = new CancellationTokenSource();
        var workerTask = worker.StartAsync(cts.Token); // BackgroundWorkerService starts executing ExecuteAsync

        // Wait a little bit for the job to be picked up and processed
        await Task.Delay(2000);

        // 4. Assert Job State
        Assert.True(testHandler.WasExecuted);
        Assert.Equal("Hello, World!", testHandler.ReceivedMessage);
        Assert.NotNull(testHandler.ReceivedContext);
        Assert.Equal("user-1", testHandler.ReceivedContext.Identity.SubjectId);
        Assert.Equal("tenant-1", testHandler.ReceivedContext.Tenant.TenantId);

        // 5. Verify the state in DB
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Status FROM SystemCellJobs WHERE JobId = @id";
        cmd.Parameters.AddWithValue("id", jobId);
        
        var status = (int)await cmd.ExecuteScalarAsync()!;
        Assert.Equal((int)JobStatus.Completed, status);

        await worker.StopAsync(CancellationToken.None);
    }

    private class FailingJobHandler : IBackgroundJobHandler<TestJob>
    {
        public int InvocationCount { get; private set; } = 0;
        public bool ShouldThrow { get; set; } = true;
        public bool ThrowPermanent { get; set; } = false;

        public Task ExecuteAsync(TestJob job, JobExecutionContext context, CancellationToken cancellationToken)
        {
            InvocationCount++;
            if (ShouldThrow)
            {
                if (ThrowPermanent)
                {
                    throw new InvalidOperationException("Permanent failure.");
                }
                throw new Exception("Transient failure.");
            }
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task RetryTest()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());

        services.AddSingleton<IJobStorageCapability>(new SystemCellJobStorage(_dbContainer.GetConnectionString()));

        services.AddSingleton<IJobContextSerializer, JobContextSerializer>();
        services.AddSingleton<IJobPayloadSerializer, TestJobPayloadSerializer>();
        services.AddSingleton<IJobDispatcher, TestJobDispatcher>();

        var testHandler = new FailingJobHandler { ShouldThrow = true };
        services.AddSingleton<IBackgroundJobHandler<TestJob>>(testHandler);

        services.AddHostedService<BackgroundWorkerService>();

        await using var provider = services.BuildServiceProvider();
        var jobStorage = provider.GetRequiredService<IJobStorageCapability>();
        var payloadSerializer = provider.GetRequiredService<IJobPayloadSerializer>();
        var contextSerializer = provider.GetRequiredService<IJobContextSerializer>();

        var job = new TestJob("Fail me");
        var context = new JobExecutionContext(new IdentityContext("user", "test", new Dictionary<string, string>(), "tenant"), new TenantContext("tenant"), new AuthorizationContext(Array.Empty<string>(), Array.Empty<string>()));

        var jobId = await jobStorage.EnqueueAsync(job.JobTypeId, payloadSerializer.Serialize(job), contextSerializer.Serialize(context), null, CancellationToken.None);

        var worker = (BackgroundWorkerService)provider.GetServices<IHostedService>().First();
        var cts = new CancellationTokenSource();
        var workerTask = worker.StartAsync(cts.Token);

        await Task.Delay(2000); // Wait for failure and retry scheduling

        // Check if queued again with next execution time
        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Status, AttemptCount, NextExecutionTime, FailureErrorMessage FROM SystemCellJobs WHERE JobId = @id";
        cmd.Parameters.AddWithValue("id", jobId);
        
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            Assert.True(await reader.ReadAsync());
            Assert.Equal((int)JobStatus.Queued, reader.GetInt32(0));
            Assert.Equal(1, reader.GetInt32(1));
            Assert.NotNull(reader.GetValue(2));
            Assert.Contains("Transient failure.", reader.GetString(3));
        }

        // Now make it succeed
        testHandler.ShouldThrow = false;
        
        // Let's force next_execution_time to now so worker picks it up
        await using var updateCmd = connection.CreateCommand();
        updateCmd.CommandText = "UPDATE SystemCellJobs SET NextExecutionTime = @now WHERE JobId = @id";
        updateCmd.Parameters.AddWithValue("now", DateTimeOffset.UtcNow.AddMinutes(-1));
        updateCmd.Parameters.AddWithValue("id", jobId);
        await updateCmd.ExecuteNonQueryAsync();

        await Task.Delay(6000); // Wait for pickup and success

        await using var cmd2 = connection.CreateCommand();
        cmd2.CommandText = "SELECT Status FROM SystemCellJobs WHERE JobId = @id";
        cmd2.Parameters.AddWithValue("id", jobId);
        Assert.Equal((int)JobStatus.Completed, (int)await cmd2.ExecuteScalarAsync()!);

        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task PermanentFailureTest()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());

        services.AddSingleton<IJobStorageCapability>(new SystemCellJobStorage(_dbContainer.GetConnectionString()));

        services.AddSingleton<IJobContextSerializer, JobContextSerializer>();
        services.AddSingleton<IJobPayloadSerializer, TestJobPayloadSerializer>();
        services.AddSingleton<IJobDispatcher, TestJobDispatcher>();

        var testHandler = new FailingJobHandler { ShouldThrow = true, ThrowPermanent = true };
        services.AddSingleton<IBackgroundJobHandler<TestJob>>(testHandler);

        services.AddHostedService<BackgroundWorkerService>();

        await using var provider = services.BuildServiceProvider();
        var jobStorage = provider.GetRequiredService<IJobStorageCapability>();
        var payloadSerializer = provider.GetRequiredService<IJobPayloadSerializer>();
        var contextSerializer = provider.GetRequiredService<IJobContextSerializer>();

        var job = new TestJob("Fail me permanently");
        var context = new JobExecutionContext(new IdentityContext("user", "test", new Dictionary<string, string>(), "tenant"), new TenantContext("tenant"), new AuthorizationContext(Array.Empty<string>(), Array.Empty<string>()));

        var jobId = await jobStorage.EnqueueAsync(job.JobTypeId, payloadSerializer.Serialize(job), contextSerializer.Serialize(context), null, CancellationToken.None);

        // Wait... wait, how does it know it's a permanent failure vs transient?
        // Wait, looking at the code for BackgroundWorkerService:
        // MaxAttempts is 5. It will just retry 5 times. Wait, the prompt says:
        // "Create a handler that fails permanently according to the existing retry behavior."
        // Let's update attempt_count in the DB to 5 before running!
        await using var setupConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await setupConnection.OpenAsync();
        await using var setupCmd = setupConnection.CreateCommand();
        setupCmd.CommandText = "UPDATE SystemCellJobs SET AttemptCount = 5 WHERE JobId = @id";
        setupCmd.Parameters.AddWithValue("id", jobId);
        await setupCmd.ExecuteNonQueryAsync();

        var worker = (BackgroundWorkerService)provider.GetServices<IHostedService>().First();
        var cts = new CancellationTokenSource();
        var workerTask = worker.StartAsync(cts.Token);

        await Task.Delay(2000);

        await using var checkCmd = setupConnection.CreateCommand();
        checkCmd.CommandText = "SELECT Status, FailureErrorMessage, FailedAt FROM SystemCellJobs WHERE JobId = @id";
        checkCmd.Parameters.AddWithValue("id", jobId);
        
        await using (var reader = await checkCmd.ExecuteReaderAsync())
        {
            Assert.True(await reader.ReadAsync());
            Assert.Equal((int)JobStatus.Failed, reader.GetInt32(0));
            Assert.Contains("Permanent failure.", reader.GetString(1));
            Assert.False(reader.IsDBNull(2));
        }
        await worker.StopAsync(CancellationToken.None);
    }

    private class CancellingJobHandler : IBackgroundJobHandler<TestJob>
    {
        public async Task ExecuteAsync(TestJob job, JobExecutionContext context, CancellationToken cancellationToken)
        {
            // Wait to be cancelled
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    [Fact]
    public async Task CancellationTest()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole());

        services.AddSingleton<IJobStorageCapability>(new SystemCellJobStorage(_dbContainer.GetConnectionString()));

        services.AddSingleton<IJobContextSerializer, JobContextSerializer>();
        services.AddSingleton<IJobPayloadSerializer, TestJobPayloadSerializer>();
        services.AddSingleton<IJobDispatcher, TestJobDispatcher>();

        var testHandler = new CancellingJobHandler();
        services.AddSingleton<IBackgroundJobHandler<TestJob>>(testHandler);

        services.AddHostedService<BackgroundWorkerService>();

        await using var provider = services.BuildServiceProvider();
        var jobStorage = provider.GetRequiredService<IJobStorageCapability>();
        var payloadSerializer = provider.GetRequiredService<IJobPayloadSerializer>();
        var contextSerializer = provider.GetRequiredService<IJobContextSerializer>();

        var job = new TestJob("Cancel me");
        var context = new JobExecutionContext(new IdentityContext("user", "test", new Dictionary<string, string>(), "tenant"), new TenantContext("tenant"), new AuthorizationContext(Array.Empty<string>(), Array.Empty<string>()));

        var jobId = await jobStorage.EnqueueAsync(job.JobTypeId, payloadSerializer.Serialize(job), contextSerializer.Serialize(context), null, CancellationToken.None);

        var worker = (BackgroundWorkerService)provider.GetServices<IHostedService>().First();
        var cts = new CancellationTokenSource();
        var workerTask = worker.StartAsync(cts.Token);

        await Task.Delay(1000); // Wait for it to be claimed and start waiting

        // Now cancel it
        cts.Cancel();
        try { await workerTask; } catch { }

        await Task.Delay(1000); // Let cancellation propagate

        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Status FROM SystemCellJobs WHERE JobId = @id";
        cmd.Parameters.AddWithValue("id", jobId);
        Assert.Equal((int)JobStatus.Cancelled, (int)await cmd.ExecuteScalarAsync()!);
    }

    private class ConcurrencyJobHandler : IBackgroundJobHandler<TestJob>
    {
        private int _count = 0;
        public int ExecutedCount => _count;

        public async Task ExecuteAsync(TestJob job, JobExecutionContext context, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _count);
            await Task.Delay(500, cancellationToken); // Simulate some work
        }
    }

    [Fact]
    public async Task ConcurrencyTest()
    {
        var services1 = new ServiceCollection();
        services1.AddLogging(b => b.AddConsole());
        services1.AddSingleton<IJobStorageCapability>(new SystemCellJobStorage(_dbContainer.GetConnectionString()));
        services1.AddSingleton<IJobContextSerializer, JobContextSerializer>();
        services1.AddSingleton<IJobPayloadSerializer, TestJobPayloadSerializer>();
        services1.AddSingleton<IJobDispatcher, TestJobDispatcher>();
        
        var handler = new ConcurrencyJobHandler();
        services1.AddSingleton<IBackgroundJobHandler<TestJob>>(handler);
        services1.AddHostedService<BackgroundWorkerService>();
        await using var provider1 = services1.BuildServiceProvider();

        var services2 = new ServiceCollection();
        services2.AddLogging(b => b.AddConsole());
        services2.AddSingleton<IJobStorageCapability>(new SystemCellJobStorage(_dbContainer.GetConnectionString()));
        services2.AddSingleton<IJobContextSerializer, JobContextSerializer>();
        services2.AddSingleton<IJobPayloadSerializer, TestJobPayloadSerializer>();
        services2.AddSingleton<IJobDispatcher, TestJobDispatcher>();
        services2.AddSingleton<IBackgroundJobHandler<TestJob>>(handler); // share handler instance for counting
        services2.AddHostedService<BackgroundWorkerService>();
        await using var provider2 = services2.BuildServiceProvider();

        var jobStorage = provider1.GetRequiredService<IJobStorageCapability>();
        var payloadSerializer = provider1.GetRequiredService<IJobPayloadSerializer>();
        var contextSerializer = provider1.GetRequiredService<IJobContextSerializer>();

        var jobIds = new List<string>();
        for (int i = 0; i < 20; i++)
        {
            var job = new TestJob($"Job {i}");
            var context = new JobExecutionContext(new IdentityContext("user", "test", new Dictionary<string, string>(), "tenant"), new TenantContext("tenant"), new AuthorizationContext(Array.Empty<string>(), Array.Empty<string>()));
            var jobId = await jobStorage.EnqueueAsync(job.JobTypeId, payloadSerializer.Serialize(job), contextSerializer.Serialize(context), null, CancellationToken.None);
            jobIds.Add(jobId);
        }

        var worker1 = (BackgroundWorkerService)provider1.GetServices<IHostedService>().First();
        var worker2 = (BackgroundWorkerService)provider2.GetServices<IHostedService>().First();

        var cts = new CancellationTokenSource();
        var task1 = worker1.StartAsync(cts.Token);
        var task2 = worker2.StartAsync(cts.Token);

        await Task.Delay(8000); // Wait for them to churn through jobs

        cts.Cancel();
        try { await task1; } catch { }
        try { await task2; } catch { }

        await using var connection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await connection.OpenAsync();
        
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM SystemCellJobs WHERE Status = " + (int)JobStatus.Completed;
        var completedCount = (long)await cmd.ExecuteScalarAsync()!;
        
        Assert.Equal(20, completedCount);
        Assert.Equal(20, handler.ExecutedCount);
    }
}

public class MockTestJobHandler : IBackgroundJobHandler<TestJob>
{
    public bool WasExecuted { get; private set; }
    public string? ReceivedMessage { get; private set; }
    public JobExecutionContext? ReceivedContext { get; private set; }
    public bool ShouldThrow { get; set; }
    public bool PermanentFailure { get; set; }

    public Task ExecuteAsync(TestJob job, JobExecutionContext context, CancellationToken cancellationToken)
    {
        if (ShouldThrow)
        {
            if (PermanentFailure)
                throw new InvalidOperationException("Permanent failure.");
            throw new Exception("Transient failure.");
        }

        WasExecuted = true;
        ReceivedMessage = job.Message;
        ReceivedContext = context;
        return Task.CompletedTask;
    }
}

