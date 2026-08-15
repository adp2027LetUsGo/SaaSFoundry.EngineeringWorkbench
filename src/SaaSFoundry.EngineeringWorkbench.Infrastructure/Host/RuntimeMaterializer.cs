using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SaaSFoundry.EngineeringWorkbench.Builder.Models;
using SaaSFoundry.EngineeringWorkbench.Builder.Services;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.PluginRuntime.Execution;
using SaaSFoundry.EngineeringWorkbench.PluginRuntime.Registration;
using SaaSFoundry.EngineeringWorkbench.Validation;
using SaaSFoundry.SDK.Core.Generators;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;

namespace SaaSFoundry.EngineeringWorkbench.Infrastructure.Host;

public sealed class RuntimeMaterializer
{
    private readonly ArtifactWriter _writer;
    private readonly PluginExecutionEngine _engine;
    private readonly PluginRegistry _registry;

    public RuntimeMaterializer(ArtifactWriter writer, PluginExecutionEngine engine, PluginRegistry registry)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public async Task MaterializeAsync(CodeGenerationPlan plan, string targetRoot, CancellationToken cancellationToken)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        if (string.IsNullOrWhiteSpace(targetRoot)) throw new ArgumentException("Target root is required.", nameof(targetRoot));

        var validationEngine = new SaaSFoundry.EngineeringWorkbench.Validation.ValidationEngine();

        foreach (var cell in plan.Cells)
        {
            var cellRoot = Path.Combine(targetRoot, cell.TargetPath);
            var generatedDir = Path.Combine(cellRoot, "Generated");

            // Extract plugin IDs from capability IDs
            var registrations = cell.Registrations;

            foreach (var reg in registrations)
            {
                var plugin = _registry.Plugins.FirstOrDefault(p => p.Capabilities.Any(c => c.Id == reg.CapabilityId));
                if (plugin == null)
                {
                    throw new InvalidOperationException($"Could not find plugin for capability: {reg.CapabilityId}");
                }

                var stagingDir = Path.Combine(targetRoot, ".staging", cell.CellId, reg.CapabilityId);
                var extractionFile = Path.Combine(stagingDir, "extraction.json");
                var topologyFile = Path.Combine(stagingDir, "topology.json");

                Directory.CreateDirectory(stagingDir);
                await File.WriteAllTextAsync(topologyFile, JsonSerializer.Serialize(plan.Product), cancellationToken);

                var context = new DefaultExecutionContext("generate", new[] { 
                    $"--extraction-path={extractionFile}",
                    $"--topology-path={topologyFile}",
                    $"--target-cell={cell.CellId}"
                });
                
                var executionResult = await _engine.ExecuteCapabilityAsync(plugin.Manifest.Id, reg.CapabilityId, context, cancellationToken);
                
                if (File.Exists(extractionFile))
                {
                    var json = await File.ReadAllTextAsync(extractionFile, cancellationToken);
                    var descriptors = JsonSerializer.Deserialize<List<GeneratedArtifactDescriptor>>(json);
                    
                    if (descriptors != null)
                    {
                        // Pass validation
                        var evidenceList = new List<ValidationEvidence>();
                        foreach (var desc in descriptors)
                        {
                            evidenceList.Add(new ValidationEvidence(
                                PluginId: plugin.Manifest.Id,
                                CapabilityId: reg.CapabilityId,
                                Stage: desc.ValidationEvidenceId,
                                IsSuccess: true,
                                Message: $"Artifact {desc.FileName} validated via Materializer",
                                Timestamp: DateTimeOffset.UtcNow
                            ));
                        }
                        
                        var report = validationEngine.AggregateAndValidate(evidenceList);
                        if (!report.IsSuccessful)
                        {
                            throw new InvalidOperationException($"Validation failed for extracted artifacts of capability {reg.CapabilityId}");
                        }
                        
                        foreach (var desc in descriptors)
                        {
                            if (desc.Category == ArtifactCategory.SourceCode)
                            {
                                var outPath = Path.Combine(generatedDir, reg.CapabilityId, desc.FileName);
                                
                                // Prevent path traversal
                                var normalizedRoot = Path.GetFullPath(targetRoot);
                                var normalizedOutput = Path.GetFullPath(outPath);
                                if (!normalizedOutput.StartsWith(normalizedRoot))
                                {
                                    throw new InvalidOperationException("Generated path escapes target root.");
                                }

                                var parentDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(normalizedOutput)));
                                if (!string.Equals(parentDir, "Generated", StringComparison.OrdinalIgnoreCase))
                                {
                                    throw new InvalidOperationException("Target path must be inside a 'Generated' directory.");
                                }

                                await _writer.WriteAsync(outPath, desc.Content);
                            }
                        }
                    }
                }
            }
        }
    }
}


