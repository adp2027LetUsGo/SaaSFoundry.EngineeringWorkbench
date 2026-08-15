namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

public interface IPluginExecutionContext
{
    string Operation { get; }
    string[] Arguments { get; }
}
