using System;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;

namespace SaaSFoundry.SDK.Testing.Fixtures;

/// <summary>
/// A reusable fixture pattern for testing plugins in an isolated fashion.
/// </summary>
public abstract class PluginTestFixture<TPlugin> where TPlugin : IEngineeringPlugin, new()
{
    public TPlugin CreatePlugin()
    {
        return new TPlugin();
    }
}
