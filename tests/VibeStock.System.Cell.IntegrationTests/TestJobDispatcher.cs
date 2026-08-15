using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;
using VibeStock.System.Cell.Generated.BackgroundProcessing;

namespace VibeStock.System.Cell.IntegrationTests;

public class TestJobDispatcher : IJobDispatcher
{
    private readonly IJobPayloadSerializer _payloadSerializer;

    public TestJobDispatcher(IJobPayloadSerializer payloadSerializer)
    {
        _payloadSerializer = payloadSerializer;
    }

    public async Task DispatchAsync(EnqueuedJob job, JobExecutionContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        if (job.JobTypeId == "test.job")
        {
            var testJob = _payloadSerializer.Deserialize<TestJob>("test.job", job.SerializedPayload);
            var handler = serviceProvider.GetRequiredService<IBackgroundJobHandler<TestJob>>();
            await handler.ExecuteAsync(testJob, context, cancellationToken);
            return;
        }
        
        throw new InvalidOperationException($"Unknown JobTypeId: {job.JobTypeId}");
    }
}
