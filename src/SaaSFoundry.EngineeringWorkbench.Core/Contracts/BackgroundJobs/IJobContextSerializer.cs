namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

public interface IJobContextSerializer
{
    string Serialize(JobExecutionContext context);

    JobExecutionContext Deserialize(string serializedContext);
}
