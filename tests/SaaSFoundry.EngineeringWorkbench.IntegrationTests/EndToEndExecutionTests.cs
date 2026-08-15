using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Infrastructure.Host;
using SaaSFoundry.EngineeringWorkbench.Validation;
using SaaSFoundry.EngineeringWorkbench.Packaging;

namespace SaaSFoundry.EngineeringWorkbench.IntegrationTests;

public class EndToEndExecutionTests
{
    [Fact]
    public async Task Validate_Complete_Execution_Flow()
    {
        // 1. Workbench Setup
        var services = new EmptyServiceProvider();
        var host = new WorkbenchHost(services); // Internally calls PluginCompositionRoot.Compose()
        
        using var cts = new CancellationTokenSource();
        await host.InitializeAsync(cts.Token);
        
        var context = new DefaultExecutionContext("generate", Array.Empty<string>());
        
        // 2. Execution Pipeline
        var execResult = await host.ExecuteCapabilityAsync("observability", "logging", context, cts.Token);
        
        Assert.NotNull(execResult);
        Assert.Equal(0, execResult.Result.StatusCode);
        
        // 3. Validation Flow
        var validationEngine = new ValidationEngine();
        var report = validationEngine.AggregateAndValidate(execResult.Evidence);
        
        Assert.True(report.IsSuccessful);
        Assert.Equal(4, report.Evidence.Count);
        
        // 4. Packaging Flow
        var packagingEngine = new PackagingEngine();
        var package = packagingEngine.CreatePackage("pkg-obs-001", report, execResult.Artifacts);
        
        Assert.Equal("pkg-obs-001", package.PackageId);
        Assert.Equal(4, package.ArtifactPaths.Count);
        
        var exporter = new FolderPackageExporter();
        await packagingEngine.ExportPackageAsync(package, exporter, "/tmp/packages");
        
        await host.ShutdownAsync(cts.Token);
    }
    
    private class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
