namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

public interface IPluginExecutionContext
{
    string Operation { get; }
    string[] Arguments { get; }
}

public interface IPluginExecutionResult
{
    int StatusCode { get; }
}

public interface IPluginExecutor
{
    System.Threading.Tasks.Task<IPluginExecutionResult> ExecuteCapabilityAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken);
}
