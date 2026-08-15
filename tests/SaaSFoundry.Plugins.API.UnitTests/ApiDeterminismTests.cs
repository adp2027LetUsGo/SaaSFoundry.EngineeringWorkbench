using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.Plugins.API.Capabilities;
using SaaSFoundry.SDK.Testing.Determinism;
using Xunit;

namespace SaaSFoundry.Plugins.API.UnitTests;

public class ApiDeterminismTests
{
    [Fact]
    public void ApiCapabilities_AreDeterministic()
    {
        var capability = new HttpEndpointCapability(
            "health",
            "Health check endpoint",
            "/api/health",
            System.Net.Http.HttpMethod.Get,
            context => Task.CompletedTask
        );

        // Define a simple input state (e.g. an empty execution context)
        var inputContext = new TestExecutionContext();
        
        DeterminismVerifier.AssertDeterministic<IPluginExecutionContext, IPluginExecutionResult>(
            inputContext,
            ctx => capability.ExecuteAsync(ctx, CancellationToken.None).GetAwaiter().GetResult(),
            (res1, res2) => res1.StatusCode == res2.StatusCode
        );
    }

    private class TestExecutionContext : IPluginExecutionContext
    {
        public string ExecutionId => "test-exec";
        public IServiceProvider Services => throw new System.NotImplementedException();
        public IReadOnlyDictionary<string, object> Parameters => new System.Collections.Generic.Dictionary<string, object>();
        public string Operation => "test-op";
        public string[] Arguments => System.Array.Empty<string>();
    }
}
