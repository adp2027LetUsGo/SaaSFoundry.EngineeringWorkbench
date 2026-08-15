namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

public interface IJobPayloadSerializer
{
    string Serialize<TJob>(TJob job)
        where TJob : IBackgroundJob;

    TJob Deserialize<TJob>(
        string jobTypeId,
        string serializedPayload)
        where TJob : IBackgroundJob;
}
