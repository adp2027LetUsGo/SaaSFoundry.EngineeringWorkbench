using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Governance;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Plugins;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Validation;
using SaaSFoundry.Plugins.Observability.Traceability;
using SaaSFoundry.SDK.Plugins.Abstractions;
using SaaSFoundry.SDK.Packaging.Models;
            using SaaSFoundry.SDK.Core.Generators;
            using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.Plugins.Observability.Certification;

public sealed class ObservabilityCertificationEngine
{
    private static readonly HashSet<string> ExpectedCapabilities = new(StringComparer.OrdinalIgnoreCase)
    {
        "configuration", "tracing", "logging", "metrics", "healthchecks",
        "collector", "dashboards", "alerts", "documentation", "validation"
    };

    public CertificationResult Evaluate(IEngineeringPlugin plugin, SaaSFoundry.SDK.Packaging.Models.EngineeringPackageDescriptor package)
    {
        if (plugin == null) throw new ArgumentNullException(nameof(plugin));
        if (package == null) throw new ArgumentNullException(nameof(package));

        var metadataProvider = plugin as IPluginMetadataProvider;
        var diagnostics = new List<ValidationDiagnostic>();
        var failedRules = new List<string>();
        const int totalRules = 16;

        void CheckRule(string ruleId, bool condition, string errorMessage)
        {
            if (!condition)
            {
                failedRules.Add(ruleId);
                diagnostics.Add(new ValidationDiagnostic(
                    Code: ruleId,
                    Message: errorMessage,
                    IsError: true,
                    ArtifactId: package.PackageId
                ));
            }
        }

        // Pillar 1: Plugin Identity
        CheckRule("RULE_ID_001_PLUGIN_ID_EXISTS", 
            metadataProvider != null && !string.IsNullOrWhiteSpace(metadataProvider.Identity.PluginId), 
            "Plugin must provide a valid non-empty PluginId in its Identity.");

        CheckRule("RULE_ID_002_VERSION_EXISTS", 
            metadataProvider != null && !string.IsNullOrWhiteSpace(metadataProvider.Identity.Version), 
            "Plugin must provide a valid non-empty Version in its Identity.");

        CheckRule("RULE_ID_003_FINGERPRINT_EXISTS", 
            metadataProvider != null && !string.IsNullOrWhiteSpace(metadataProvider.Identity.Fingerprint), 
            "Plugin must provide a valid non-empty Fingerprint in its Identity.");

        // Pillar 2: Capability Coverage
        var capabilities = plugin.Capabilities.ToList();
        CheckRule("RULE_CAP_001_ALL_TEN_CAPABILITIES_REGISTERED",
            capabilities.Count == 10 && capabilities.All(c => ExpectedCapabilities.Contains(c.Id)),
            "Plugin must explicitly register all 10 canonical Observability capabilities.");

        CheckRule("RULE_CAP_002_CAPABILITY_INTERFACES",
            capabilities.All(c => c is ITraceablePluginCapability && c is IGovernedPluginCapability),
            "All registered capabilities must implement both ITraceablePluginCapability and IGovernedPluginCapability.");

        // Pillar 3: Traceability
        var artifacts = package.Artifacts;
        CheckRule("RULE_TRC_001_CANON_REF",
            artifacts.All(a => !string.IsNullOrWhiteSpace(a.CanonReference)),
            "Every artifact must specify a valid canonical architectural reference (e.g. OBS-001).");

        CheckRule("RULE_TRC_002_IMPL_REF",
            artifacts.All(a => !string.IsNullOrWhiteSpace(a.ImplementationReference)),
            "Every artifact must specify a valid canonical implementation reference (e.g. OBS-101).");

        CheckRule("RULE_TRC_003_CAPABILITY_OWNER",
            artifacts.All(a => !string.IsNullOrWhiteSpace(a.CapabilityId)),
            "Every artifact must designate a clear capability owner ID.");

        CheckRule("RULE_TRC_004_VALIDATION_EVIDENCE",
            artifacts.All(a => !string.IsNullOrWhiteSpace(a.ValidationEvidenceId)),
            "Every artifact must link to a deterministic validation evidence identifier.");

        CheckRule("RULE_TRC_005_PACKAGE_TRACEABILITY_COMPLETE",
            package.TraceabilityRecords.Count == artifacts.Count && package.ValidationEvidence.Count == artifacts.Count,
            "Package must contain a 1-to-1 mapping of Traceability Records and Validation Evidence for all artifacts.");

        // Pillar 4: Governance
        var governed = capabilities.OfType<IGovernedPluginCapability>().ToList();
        CheckRule("RULE_GOV_001_RISK_LEVEL",
            governed.All(c => c.GovernanceMetadata.Risk != RiskLevel.None),
            "Every governed capability must explicitly declare a risk level higher than None.");

        CheckRule("RULE_GOV_002_PERMISSIONS",
            governed.All(c => c.GovernanceMetadata.RequiredPermissions != null && c.GovernanceMetadata.RequiredPermissions.Any()),
            "Every governed capability must explicitly declare required execution permissions.");

        CheckRule("RULE_GOV_003_VALIDATION_REQUIREMENTS",
            governed.All(c => c.GovernanceMetadata.ValidationRequirements != null && c.GovernanceMetadata.ValidationRequirements.Any()),
            "Every governed capability must explicitly declare governance validation requirements.");

        // Pillar 5: Package Integrity
        CheckRule("RULE_PKG_001_MANIFEST_EXISTS",
            package.Manifest != null,
            "Package must possess an immutable Engineering Artifact Manifest.");

        CheckRule("RULE_PKG_002_DEPENDENCY_GRAPH_VALID",
            package.DependencyGraph != null && package.DependencyGraph.Nodes.Count == artifacts.Count,
            "Package dependency graph must be completely articulated for all inventory nodes.");

        CheckRule("RULE_PKG_003_SHA256_HASH_VALID",
            !string.IsNullOrWhiteSpace(package.PackageHash) && package.PackageHash.StartsWith("SHA256:", StringComparison.Ordinal),
            "Package must possess a valid SHA-256 cryptographic integrity hash.");

        double complianceScore = Math.Round(((totalRules - failedRules.Count) / (double)totalRules) * 100.0, 2);
        bool isCertified = failedRules.Count == 0;

        return new CertificationResult(isCertified, diagnostics, complianceScore, failedRules);
    }

    public CertifiedPluginDescriptor Certify(IEngineeringPlugin plugin, SaaSFoundry.SDK.Packaging.Models.EngineeringPackageDescriptor package, long certificationTimestamp = 0)
    {
        var evaluation = Evaluate(plugin, package);
        if (!evaluation.IsCertified)
        {
            throw new InvalidOperationException($"Plugin certification failed with compliance score {evaluation.ComplianceScore}% and {evaluation.FailedRules.Count} rule violations: {string.Join(", ", evaluation.FailedRules)}.");
        }

        var provider = (IPluginMetadataProvider)plugin;
        long timestamp = certificationTimestamp > 0 ? certificationTimestamp : package.CreationTimestamp;
        
        string payload = $"{provider.Identity.PluginId}:{provider.Identity.Version}:{provider.Identity.Fingerprint}:{package.PackageHash}:{timestamp}:{provider.Metadata.Capabilities.Count}:{package.Artifacts.Count}";
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(payload));
        string certHash = "SHA256:" + BitConverter.ToString(hashBytes).Replace("-", "");

        return new CertifiedPluginDescriptor(
            PluginIdentity: provider.Identity,
            PluginMetadata: provider.Metadata,
            CapabilityCount: provider.Metadata.Capabilities.Count,
            ArtifactCount: package.Artifacts.Count,
            TraceabilityCoverage: 100.0,
            GovernanceCoverage: 100.0,
            ValidationStatus: "CERTIFIED_COMPLIANT_V1",
            CertificationTimestamp: timestamp,
            CertificationHash: certHash
        );
    }

    public GoldenReferencePackage GenerateGoldenReferencePackage(IEngineeringPlugin plugin, SaaSFoundry.SDK.Packaging.Models.EngineeringPackageDescriptor package, long certificationTimestamp = 0)
    {
        var certification = Certify(plugin, package, certificationTimestamp);
        var provider = (IPluginMetadataProvider)plugin;
        var governanceMetadata = plugin.Capabilities
            .Cast<IGovernedPluginCapability>()
            .Select(c => c.GovernanceMetadata)
            .ToList();

        return new GoldenReferencePackage(
            PackageId: package.PackageId,
            PackageHash: package.PackageHash,
            PluginMetadata: provider.Metadata,
            GovernanceMetadata: governanceMetadata,
            ArtifactInventory: package.Artifacts,
            DependencyGraph: package.DependencyGraph,
            ValidationEvidence: package.ValidationEvidence,
            Certification: certification
        );
    }
}
