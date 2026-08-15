using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Transport;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.Core.UnitTests.Contracts.Transport;

public class InterCellMetadataKeysTests
{
    [Fact]
    public void MetadataKeys_AreCorrect()
    {
        Assert.Equal("x-tenant-id", InterCellMetadataKeys.TenantId);
        Assert.Equal("x-identity-subject-id", InterCellMetadataKeys.IdentitySubjectId);
        Assert.Equal("x-identity-type", InterCellMetadataKeys.IdentityType);
        Assert.Equal("x-identity-tenant-association", InterCellMetadataKeys.IdentityTenantAssociation);
        Assert.Equal("x-idempotency-key", InterCellMetadataKeys.IdempotencyKey);
    }
}
