using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.Core.UnitTests.BackgroundJobs;

public class BackgroundJobContractTests
{
    private sealed class TestJob : IBackgroundJob
    {
        public string JobTypeId => "test.job";
    }

    private sealed class TestJobHandler : IBackgroundJobHandler<TestJob>
    {
        public Task ExecuteAsync(TestJob job, JobExecutionContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void IBackgroundJob_ExposesStringJobTypeId()
    {
        // Assert
        IBackgroundJob job = new TestJob();
        Assert.Equal("test.job", job.JobTypeId);
        Assert.IsType<string>(job.JobTypeId);
    }

    [Fact]
    public void IBackgroundJobHandler_RequiresIBackgroundJobAndReturnsTask()
    {
        // Arrange
        IBackgroundJobHandler<TestJob> handler = new TestJobHandler();
        var job = new TestJob();
        var identity = new IdentityContext("user1", "System", new Dictionary<string, string>(), "tenant1");
        var tenant = new TenantContext("tenant1");
        var authorization = new AuthorizationContext(new[] { "admin" }, new[] { "AdminRole" });
        var context = new JobExecutionContext(identity, tenant, authorization);
        var cts = new CancellationTokenSource();

        // Act
        var result = handler.ExecuteAsync(job, context, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsCompleted);
    }

    [Fact]
    public void JobExecutionContext_IsImmutableRecord()
    {
        // Arrange
        var identity = new IdentityContext("user1", "System", new Dictionary<string, string>(), "tenant1");
        var tenant = new TenantContext("tenant1");
        var authorization = new AuthorizationContext(new[] { "admin" }, new[] { "AdminRole" });

        // Act
        var context = new JobExecutionContext(identity, tenant, authorization);

        // Assert
        Assert.NotNull(context.Identity);
        Assert.NotNull(context.Tenant);
        Assert.NotNull(context.Authorization);
        Assert.Equal("user1", context.Identity.SubjectId);
        Assert.Equal("tenant1", context.Tenant.TenantId);
        Assert.Contains("admin", context.Authorization.Permissions);
    }
}
