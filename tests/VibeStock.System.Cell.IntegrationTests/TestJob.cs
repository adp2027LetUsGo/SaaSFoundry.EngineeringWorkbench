using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

namespace VibeStock.System.Cell.IntegrationTests;

public sealed record TestJob(string Message) : IBackgroundJob
{
    public string JobTypeId => "test.job";
}
