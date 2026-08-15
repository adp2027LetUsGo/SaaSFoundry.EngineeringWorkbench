namespace SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport;

public static class InterCellMetadataKeys
{
    public const string TenantId = "x-tenant-id";
    public const string IdentitySubjectId = "x-identity-subject-id";
    public const string IdentityType = "x-identity-type";
    public const string IdentityTenantAssociation = "x-identity-tenant-association";
    public const string IdempotencyKey = "x-idempotency-key";
}
