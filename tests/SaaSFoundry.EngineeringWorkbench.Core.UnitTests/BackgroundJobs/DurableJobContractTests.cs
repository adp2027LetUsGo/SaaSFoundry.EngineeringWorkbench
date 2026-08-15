using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.Core.UnitTests.BackgroundJobs;

public class DurableJobContractTests
{
    [Fact]
    public void EnqueuedJob_IsImmutableRecord()
    {
        var identity = new IdentityContext("sub", "user", new System.Collections.Generic.Dictionary<string, string>(), "tenantId");
        var tenant = new TenantContext("tenantId");
        var auth = new AuthorizationContext(Array.Empty<string>(), Array.Empty<string>());
        var executionContext = new JobExecutionContext(identity, tenant, auth);

        var job = new EnqueuedJob(
            "job123",
            "TestJob",
            "{}",
            "{}",
            JobStatus.Queued,
            0,
            null,
            DateTimeOffset.UtcNow,
            null,
            null,
            null
        );

        Assert.Equal("job123", job.JobId);
        Assert.Equal("TestJob", job.JobTypeId);
        Assert.Equal("{}", job.SerializedPayload);
        Assert.Equal("{}", job.SerializedContext);
        Assert.Equal(JobStatus.Queued, job.Status);
        Assert.Equal(0, job.AttemptCount);
    }

    [Fact]
    public void JobStatus_HasCanonicalValues()
    {
        var values = Enum.GetNames(typeof(JobStatus));
        Assert.Contains("Queued", values);
        Assert.Contains("Started", values);
        Assert.Contains("Completed", values);
        Assert.Contains("Failed", values);
        Assert.Contains("Cancelled", values);
        Assert.DoesNotContain("Retried", values);
    }

    [Fact]
    public void JobFailureInformation_IsImmutableRecord()
    {
        var failedAt = DateTimeOffset.UtcNow;
        var info = new JobFailureInformation("Error", "Stack", failedAt);
        Assert.Equal("Error", info.Message);
        Assert.Equal("Stack", info.StackTrace);
        Assert.Equal(failedAt, info.FailedAt);
    }

    [Fact]
    public void IJobPayloadSerializer_HasCanonicalSignature()
    {
        var type = typeof(IJobPayloadSerializer);
        var serializeMethod = type.GetMethod("Serialize");
        Assert.NotNull(serializeMethod);
        Assert.True(serializeMethod.IsGenericMethod);
        
        var deserializeMethod = type.GetMethod("Deserialize");
        Assert.NotNull(deserializeMethod);
        Assert.True(deserializeMethod.IsGenericMethod);
        
        var parameters = deserializeMethod.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(string), parameters[0].ParameterType);
        Assert.Equal(typeof(string), parameters[1].ParameterType);
    }

    [Fact]
    public void IJobContextSerializer_HasCanonicalSignature()
    {
        var type = typeof(IJobContextSerializer);
        var serializeMethod = type.GetMethod("Serialize");
        Assert.NotNull(serializeMethod);
        var serializeParams = serializeMethod.GetParameters();
        Assert.Single(serializeParams);
        Assert.Equal(typeof(JobExecutionContext), serializeParams[0].ParameterType);
        Assert.Equal(typeof(string), serializeMethod.ReturnType);

        var deserializeMethod = type.GetMethod("Deserialize");
        Assert.NotNull(deserializeMethod);
        var deserializeParams = deserializeMethod.GetParameters();
        Assert.Single(deserializeParams);
        Assert.Equal(typeof(string), deserializeParams[0].ParameterType);
        Assert.Equal(typeof(JobExecutionContext), deserializeMethod.ReturnType);
    }

    [Fact]
    public void IJobStorageCapability_HasCanonicalOperations()
    {
        var type = typeof(IJobStorageCapability);
        var enqueueMethod = type.GetMethod("EnqueueAsync");
        Assert.NotNull(enqueueMethod);
        
        var parameters = enqueueMethod.GetParameters();
        Assert.Equal(typeof(string), parameters[0].ParameterType); // jobTypeId
        Assert.Equal(typeof(string), parameters[1].ParameterType); // serializedPayload
        Assert.Equal(typeof(string), parameters[2].ParameterType); // serializedContext
        Assert.Equal(typeof(DateTimeOffset?), parameters[3].ParameterType); // nextExecutionTime
        Assert.Equal(typeof(CancellationToken), parameters[4].ParameterType); // cancellationToken

        Assert.NotNull(type.GetMethod("ClaimNextAsync"));
        Assert.NotNull(type.GetMethod("CompleteAsync"));
        Assert.NotNull(type.GetMethod("FailAsync"));
        Assert.NotNull(type.GetMethod("RetryAsync"));
        Assert.NotNull(type.GetMethod("CancelAsync"));
    }
}
