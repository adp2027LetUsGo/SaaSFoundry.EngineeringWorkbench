namespace SaaSFoundry.EngineeringWorkbench.Core.Configuration;

public sealed class WorkbenchPaths
{
    public string Root { get; }

    public string Plugins => Path.Combine(Root, "plugins");

    public string Output => Path.Combine(Root, "output");

    public string Reports => Path.Combine(Root, "reports");

    public string Packages => Path.Combine(Root, "packages");

    public WorkbenchPaths(string root)
    {
        Root = root;
    }
}