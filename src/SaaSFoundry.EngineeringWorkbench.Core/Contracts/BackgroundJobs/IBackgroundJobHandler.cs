using System.Threading;
using System.Threading.Tasks;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

public interface IBackgroundJobHandler<in TJob>
    where TJob : IBackgroundJob
{
    Task ExecuteAsync(
        TJob job,
        JobExecutionContext context,
        CancellationToken cancellationToken);
}
