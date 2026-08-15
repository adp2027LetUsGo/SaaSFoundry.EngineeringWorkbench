namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

public sealed class DefaultExecutionContext : IPluginExecutionContext
{
    public DefaultExecutionContext(string operation, string[] arguments)
    {
        Operation = operation;
        Arguments = arguments;
    }

    public string Operation { get; }
    public string[] Arguments { get; }
}
