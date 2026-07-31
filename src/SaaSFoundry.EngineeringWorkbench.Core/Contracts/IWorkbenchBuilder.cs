namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts;


public interface IWorkbenchBuilder
{

    Task ListPluginsAsync();


    Task GeneratePluginAsync(
        string pluginName);


    Task ValidatePluginAsync(
        string pluginName);


    Task PackagePluginAsync(
        string pluginName);


    Task ReportPluginAsync(
        string pluginName);

}
