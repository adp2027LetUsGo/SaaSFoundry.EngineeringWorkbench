namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

public enum JobStatus
{
    Queued,
    Started,
    Completed,
    Failed,
    Cancelled
}
