using System;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;
using SaaSFoundry.SDK.Plugins.Authoring;
using Xunit;

namespace SaaSFoundry.SDK.Plugins.UnitTests;

public class PluginBuilderTests
{
    private class DummyCapability : IPluginCapability
    {
        public DummyCapability(string id) => Id = id;

        public string Id { get; }
        public string Description => "Test";
        public System.Collections.Generic.IReadOnlyList<string> SupportedOperations => Array.Empty<string>();

        public System.Threading.Tasks.Task ValidateConfigurationAsync(System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task ValidateInputAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task<IPluginExecutionResult> ExecuteAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken) => throw new NotImplementedException();
        public System.Threading.Tasks.Task GenerateArtifactsAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task ValidateOutputAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken) => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<ValidationEvidence>> ProduceValidationEvidenceAsync(IPluginExecutionContext context, System.Threading.CancellationToken cancellationToken) => throw new NotImplementedException();
        public System.Collections.Generic.IReadOnlyList<string> ReportGeneratedFiles() => Array.Empty<string>();
    }

    [Fact]
    public void Build_WithValidIdentity_ReturnsDeterministicPlugin()
    {
        var builder = new PluginBuilder()
            .WithIdentity("test-plugin", "1.0.0", "Author", "SHA256:123")
            .WithManifest("Test Plugin", "Test Description", "v1.0")
            .AddCapability(new DummyCapability("cap1"));

        var plugin = builder.Build();

        Assert.Equal("test-plugin", plugin.Identity.PluginId);
        Assert.Equal("1.0.0", plugin.Identity.Version);
        Assert.Equal("Author", plugin.Identity.Author);
        Assert.Equal("SHA256:123", plugin.Identity.Fingerprint);
        
        Assert.Equal("Test Plugin", plugin.Manifest.Name);
        Assert.Equal("Test Description", plugin.Manifest.Description);
        
        Assert.Single(plugin.Capabilities);
        Assert.Equal("cap1", plugin.Capabilities.First().Id);
    }

    [Fact]
    public void AddCapability_WithDuplicateId_ThrowsInvalidOperationException()
    {
        var builder = new PluginBuilder()
            .WithIdentity("test-plugin", "1.0.0", "Author", "SHA256:123")
            .WithManifest("Test Plugin", "Test Description", "v1.0")
            .AddCapability(new DummyCapability("cap1"));

        var ex = Assert.Throws<InvalidOperationException>(() => builder.AddCapability(new DummyCapability("cap1")));
        Assert.Contains("Duplicate capability ID detected", ex.Message);
    }

    [Fact]
    public void Build_WithoutCapabilities_ThrowsInvalidOperationException()
    {
        var builder = new PluginBuilder()
            .WithIdentity("test-plugin", "1.0.0", "Author", "SHA256:123")
            .WithManifest("Test Plugin", "Test Description", "v1.0");

        var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Contains("At least one capability must be registered", ex.Message);
    }
}
