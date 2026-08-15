using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SaaSFoundry.SDK.Core.Diagnostics;
using SaaSFoundry.SDK.Core.Generators;
using SaaSFoundry.SDK.Packaging.Abstractions;
using SaaSFoundry.SDK.Packaging.Models;
using SaaSFoundry.SDK.Packaging.Results;

namespace SaaSFoundry.SDK.Packaging.Builders;

/// <summary>
/// Assembles and validates immutable engineering packages from artifact generation results.
/// Operates without filesystem access and calculates deterministic package integrity hashes.
/// </summary>
public sealed class PackageBuilder : IPackageBuilder
{
    private readonly string _pluginId;
    private readonly string _pluginVersion;
    private readonly string _generatorVersion;

    public PackageBuilder(string pluginId, string pluginVersion, string generatorVersion)
    {
        _pluginId = pluginId ?? throw new ArgumentNullException(nameof(pluginId));
        _pluginVersion = pluginVersion ?? throw new ArgumentNullException(nameof(pluginVersion));
        _generatorVersion = generatorVersion ?? throw new ArgumentNullException(nameof(generatorVersion));
    }

    /// <summary>
    /// Validates an artifact generation result against strict engineering package completeness rules.
    /// </summary>
    public IReadOnlyList<ValidationDiagnostic> Validate(ArtifactGenerationResult? result)
    {
        var diagnostics = new List<ValidationDiagnostic>();

        if (result == null)
        {
            diagnostics.Add(new ValidationDiagnostic("ERR_NULL_RESULT", "The artifact generation result cannot be null when building an engineering package.", true));
            return diagnostics;
        }

        // 1. No manifest exists
        if (result.Manifest == null)
        {
            diagnostics.Add(new ValidationDiagnostic("ERR_MISSING_MANIFEST", "No manifest exists in the generation result. An engineering package requires an authoritative artifact manifest.", true));
        }

        // 2. Artifact list is empty
        if (result.GeneratedArtifacts == null || result.GeneratedArtifacts.Count == 0)
        {
            diagnostics.Add(new ValidationDiagnostic("ERR_EMPTY_ARTIFACTS", "The artifact list is empty. An engineering package must contain at least one generated artifact.", true));
            return diagnostics;
        }

        // 3. Duplicate artifact IDs exist
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var art in result.GeneratedArtifacts)
        {
            if (string.IsNullOrWhiteSpace(art.ArtifactId))
            {
                diagnostics.Add(new ValidationDiagnostic("ERR_INVALID_ARTIFACT", "An artifact in the inventory has a null or empty ArtifactId.", true));
                continue;
            }

            if (!seenIds.Add(art.ArtifactId))
            {
                diagnostics.Add(new ValidationDiagnostic("ERR_DUPLICATE_ARTIFACT_ID", $"Duplicate artifact ID detected: '{art.ArtifactId}'. All artifacts within a package must be uniquely identified.", true, art.ArtifactId));
            }
        }

        // 4. Traceability is incomplete
        if (result.TraceabilityRecords == null || result.TraceabilityRecords.Count == 0)
        {
            diagnostics.Add(new ValidationDiagnostic("ERR_INCOMPLETE_TRACEABILITY", "Traceability records are empty or missing. Every artifact must have complete canonical traceability.", true));
        }
        else
        {
            var tracedIds = new HashSet<string>(result.TraceabilityRecords.Where(r => r.ArtifactId != null).Select(r => r.ArtifactId!), StringComparer.OrdinalIgnoreCase);
            foreach (var art in result.GeneratedArtifacts)
            {
                if (!tracedIds.Contains(art.ArtifactId))
                {
                    diagnostics.Add(new ValidationDiagnostic("ERR_INCOMPLETE_TRACEABILITY", $"Artifact '{art.ArtifactId}' is missing a corresponding traceability record.", true, art.ArtifactId));
                }
            }
        }

        // 5. Evidence is missing
        var evidenceList = result.ValidationEvidence?.Select(e => new SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence(e.PluginId, e.CapabilityId, e.Stage, e.IsSuccess, e.Message, e.Timestamp)).ToList();
        
        if (evidenceList == null || evidenceList.Count == 0)
        {
            diagnostics.Add(new ValidationDiagnostic("ERR_MISSING_EVIDENCE", "Validation evidence is empty or missing. Every generated capability and artifact requires verification evidence.", true));
        }
        else if (evidenceList.Count != result.GeneratedArtifacts.Count)
        {
            diagnostics.Add(new ValidationDiagnostic("ERR_MISSING_EVIDENCE", $"Validation evidence count ({evidenceList.Count}) does not correspond to artifact count ({result.GeneratedArtifacts.Count}). Complete evidence mapping is required.", true));
        }

        // 6. Dependency graph is invalid
        if (result.Manifest != null)
        {
            var graph = result.Manifest.DependencyGraph;
            if (graph == null || graph.Nodes == null)
            {
                diagnostics.Add(new ValidationDiagnostic("ERR_INVALID_DEPENDENCY_GRAPH", "The artifact dependency graph is missing or null in the manifest.", true));
            }
            else
            {
                var graphIds = new HashSet<string>(graph.Nodes.Select(n => n.ArtifactId), StringComparer.OrdinalIgnoreCase);
                foreach (var art in result.GeneratedArtifacts)
                {
                    if (!graphIds.Contains(art.ArtifactId))
                    {
                        diagnostics.Add(new ValidationDiagnostic("ERR_INVALID_DEPENDENCY_GRAPH", $"Artifact '{art.ArtifactId}' is omitted from the dependency graph nodes.", true, art.ArtifactId));
                    }
                }
            }
        }

        // 7. Package hash calculation validation
        try
        {
            var testHash = ComputePackageHash(result.GeneratedArtifacts, "test");
            if (string.IsNullOrWhiteSpace(testHash) || !testHash.StartsWith("SHA256:"))
            {
                diagnostics.Add(new ValidationDiagnostic("ERR_HASH_CALCULATION_FAILED", "Deterministic SHA-256 hash calculation yielded an invalid format.", true));
            }
        }
        catch (Exception ex)
        {
            diagnostics.Add(new ValidationDiagnostic("ERR_HASH_CALCULATION_FAILED", $"Package hash calculation failed with exception: {ex.Message}", true));
        }

        return diagnostics;
    }

    /// <summary>
    /// Builds an immutable EngineeringPackageDescriptor from an authoritative artifact generation result.
    /// </summary>
    public PackagePreparationResult Build(string packageId, string packageDescription, ArtifactGenerationResult result, long? timestampOverride = null)
    {
        var diagnostics = Validate(result);
        var errors = diagnostics.Where(d => d.IsError).ToList();
        
        if (errors.Count > 0)
        {
            return PackagePreparationResult.Failure(diagnostics);
        }

        var timestamp = timestampOverride ?? (result.Manifest?.GenerationTime ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        var hash = ComputePackageHash(result.GeneratedArtifacts!, packageId);
        
        var evidenceList = result.ValidationEvidence!.Select(e => new SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation.ValidationEvidence(e.PluginId, e.CapabilityId, e.Stage, e.IsSuccess, e.Message, e.Timestamp)).ToList();

        var package = new EngineeringPackageDescriptor(
            PackageId: packageId,
            PluginId: _pluginId,
            PluginVersion: _pluginVersion,
            GeneratorVersion: _generatorVersion,
            CreationTimestamp: timestamp,
            PackageDescription: packageDescription,
            Manifest: result.Manifest!,
            Artifacts: result.GeneratedArtifacts!,
            TraceabilityRecords: result.TraceabilityRecords!,
            ValidationEvidence: evidenceList,
            DependencyGraph: result.Manifest!.DependencyGraph!,
            PackageHash: hash
        );

        return PackagePreparationResult.Success(package, diagnostics.Where(d => !d.IsError).ToList());
    }

    private static string ComputePackageHash(IReadOnlyList<GeneratedArtifactDescriptor> artifacts, string packageId)
    {
        if (artifacts == null || artifacts.Count == 0)
        {
            throw new InvalidOperationException("Cannot compute package hash for empty artifact list.");
        }

        var builder = new StringBuilder();
        builder.Append("PKG:").Append(packageId).Append('|');

        // Order artifacts deterministically by ID to ensure hash invariance regardless of input array permutation
        foreach (var art in artifacts.OrderBy(a => a.ArtifactId, StringComparer.OrdinalIgnoreCase))
        {
            var hashValue = art.Hash ?? ComputeFallbackHash(art.Content);
            builder.Append(art.ArtifactId).Append('=').Append(hashValue).Append(';');
        }

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
        return "SHA256:" + Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static string ComputeFallbackHash(string content)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content ?? string.Empty));
        return "SHA256:" + Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
