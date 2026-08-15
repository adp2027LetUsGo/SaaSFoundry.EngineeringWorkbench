using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;

namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.BackgroundJobs;

public record JobExecutionContext(
    IdentityContext Identity,
    TenantContext Tenant,
    AuthorizationContext Authorization
);
