using System;
using System.Collections.Generic;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Catalog;

namespace SaaSFoundry.Plugins.Observability.Catalog;

public static class ObservabilityPluginCatalog
{
    public static string CatalogId => "observability-catalog";
    public static string DefaultDomain => "Observability";

    public static PluginManifest GetPluginManifest()
    {
        var capabilityDefinitions = new[]
        {
            ("logging", "Structured logging architecture (OBS-003)", (string?)null, new[] { "OBS-003-Logging-Standards.md", "structured-logging-config.json" }),
            ("metrics", "Metrics aggregation and telemetry standards (OBS-004)", "logging", new[] { "OBS-004-Metrics-Standards.md", "structured-metrics-config.json" }),
            ("tracing", "Distributed tracing standards and span specifications (OBS-005)", "metrics", new[] { "OBS-005-Tracing-Standards.md", "distributed-tracing-config.json" }),
            ("healthchecks", "Operational health check probe definitions (OBS-006)", "tracing", new[] { "OBS-006-HealthChecks-Standards.md", "healthcheck-config.json" }),
            ("collector", "OpenTelemetry collector configuration and pipelines (OBS-007)", "healthchecks", new[] { "OBS-007-Collector-Standards.md", "otel-collector-config.yaml" }),
            ("configuration", "Observability runtime configuration settings (OBS-008)", "collector", new[] { "OBS-008-Configuration-Standards.md", "observability-config.json" }),
            ("dashboards", "Operational observability dashboards (OBS-009)", "configuration", new[] { "OBS-009-Dashboards-Standards.md", "default-dashboards.json" }),
            ("alerts", "Operational alerting rules and notifications (OBS-010)", "dashboards", new[] { "OBS-010-Alerts-Standards.md", "alerting-rules.json" }),
            ("documentation", "Observability engineering documentation and runbooks (OBS-011)", "alerts", new[] { "OBS-011-Documentation-Standards.md", "observability-runbook.md" }),
            ("validation", "Automated validation matrices for observability engineering (OBS-012)", "documentation", new[] { "OBS-012-Validation-Standards.md", "validation-matrix.json" })
        };

        var capabilities = new List<CapabilityManifest>(capabilityDefinitions.Length);
        foreach (var (id, desc, reqId, artifacts) in capabilityDefinitions)
        {
            var requirements = reqId != null
                ? new List<CapabilityRequirement> { new CapabilityRequirement("observability", reqId, "generate") }
                : new List<CapabilityRequirement>();

            var expectedArtifacts = new List<ArtifactDescriptor>(artifacts.Length);
            foreach (var art in artifacts)
            {
                expectedArtifacts.Add(new ArtifactDescriptor(art, $"Generated artifact for {id}"));
            }

            var validationReqs = new List<string> { $"{id}-configuration-validation", $"{id}-execution-validation" };

            capabilities.Add(new CapabilityManifest(
                CapabilityId: id,
                Operation: "generate",
                Description: desc,
                Requirements: requirements,
                ExpectedArtifacts: expectedArtifacts,
                ValidationRequirements: validationReqs
            ));
        }

        return new PluginManifest(
            PluginId: "observability",
            Name: "SaaSFoundry Observability Engineering Plugin",
            Version: new VersionDescriptor("1.0.0"),
            EngineeringDomain: DefaultDomain,
            Capabilities: capabilities,
            Dependencies: new List<PluginDependency>(),
            RequiredCanonVersion: "v1.0.0"
        );
    }

    public static EngineeringPackageManifest BuildPackageManifest(string packageId = "pkg-obs-001")
    {
        return new EngineeringPackageManifest(packageId, new List<PluginManifest> { GetPluginManifest() });
    }
}
