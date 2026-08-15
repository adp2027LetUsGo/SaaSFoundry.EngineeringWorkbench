using System.Collections.Generic;
using SaaSFoundry.Plugins.Observability.Traceability;
            using SaaSFoundry.SDK.Core.Generators;
            using SaaSFoundry.SDK.Core.Diagnostics;

namespace SaaSFoundry.Plugins.Observability.Certification;

public sealed record CertificationResult(
    bool IsCertified,
    IReadOnlyList<ValidationDiagnostic> Diagnostics,
    double ComplianceScore,
    IReadOnlyList<string> FailedRules
);
