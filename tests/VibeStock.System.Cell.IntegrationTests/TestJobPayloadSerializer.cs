using System;
using System.Text.Json;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

namespace VibeStock.System.Cell.IntegrationTests;

public class TestJobPayloadSerializer : IJobPayloadSerializer
{
    public string Serialize<TJob>(TJob job) where TJob : IBackgroundJob
    {
        if (job is TestJob testJob)
        {
            return JsonSerializer.Serialize(testJob);
        }
        throw new NotSupportedException($"Job type {typeof(TJob).Name} is not supported by TestJobPayloadSerializer.");
    }

    public TJob Deserialize<TJob>(string jobTypeId, string serializedPayload) where TJob : IBackgroundJob
    {
        if (jobTypeId == "test.job")
        {
            return (TJob)(object)JsonSerializer.Deserialize<TestJob>(serializedPayload)!;
        }
        throw new NotSupportedException($"JobTypeId {jobTypeId} is not supported by TestJobPayloadSerializer.");
    }
}
