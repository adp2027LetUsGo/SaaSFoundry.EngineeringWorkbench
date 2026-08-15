using System.Collections.Generic;
using SaaSFoundry.Plugins.Persistence.Capabilities.Connection;
using SaaSFoundry.Plugins.Persistence.Capabilities.Query;
using SaaSFoundry.SDK.Core.Generators;
using SaaSFoundry.SDK.Testing.Determinism;
using Xunit;

namespace SaaSFoundry.Plugins.Persistence.UnitTests;

public class PersistenceDeterminismTests
{
    [Fact]
    public void ConnectionCapability_Artifacts_AreDeterministic()
    {
        var capability = new ConnectionCapability();
        
        DeterminismVerifier.AssertDeterministic(
            1, 
            _ => capability.GetArtifactDescriptors(),
            (a, b) => a.Count == b.Count
        );
    }

    [Fact]
    public void QueryCapability_Artifacts_AreDeterministic()
    {
        var capability = new QueryCapability();
        
        DeterminismVerifier.AssertDeterministic(
            1,
            _ => capability.GetArtifactDescriptors(),
            (a, b) => a.Count == b.Count
        );
    }
}
