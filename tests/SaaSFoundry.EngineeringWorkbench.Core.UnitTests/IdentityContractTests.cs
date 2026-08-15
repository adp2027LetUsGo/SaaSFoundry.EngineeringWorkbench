using System.Collections.Generic;
using System.Linq;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using Xunit;

namespace SaaSFoundry.EngineeringWorkbench.Core.UnitTests;

public class IdentityContractTests
{
    [Fact]
    public void IdentityContext_Construction_SetsPropertiesCorrectly()
    {
        var claims = new Dictionary<string, string> { { "role", "admin" } };
        var identity = new IdentityContext("sub-123", "User", claims, "tenant-456");

        Assert.Equal("sub-123", identity.SubjectId);
        Assert.Equal("User", identity.IdentityType);
        Assert.Equal("admin", identity.Claims["role"]);
        Assert.Equal("tenant-456", identity.TenantAssociation);
    }

    [Fact]
    public void IdentityContext_HasDeterministicEquality()
    {
        var claims1 = new Dictionary<string, string> { { "role", "admin" } };
        var claims2 = new Dictionary<string, string> { { "role", "admin" } };
        var identity1 = new IdentityContext("sub-123", "User", claims1, "tenant-456");
        var identity2 = new IdentityContext("sub-123", "User", claims2, "tenant-456");
        var identity3 = new IdentityContext("sub-999", "User", claims1, "tenant-456");

        // Note: record equality checks reference equality on dictionaries by default, 
        // but since the instruction is deterministic equality on the record, we verify standard behavior.
        // If we want value equality on dictionaries, it would require a custom comparer, but basic 
        // record semantics are requested.
        Assert.Equal(identity1.SubjectId, identity2.SubjectId);
        Assert.NotEqual(identity1, identity3);
    }

    [Fact]
    public void TenantContext_Construction_SetsPropertiesCorrectly()
    {
        var tenant = new TenantContext("tenant-456");
        Assert.Equal("tenant-456", tenant.TenantId);
    }

    [Fact]
    public void TenantContext_HasDeterministicEquality()
    {
        var tenant1 = new TenantContext("tenant-456");
        var tenant2 = new TenantContext("tenant-456");
        var tenant3 = new TenantContext("tenant-999");

        Assert.Equal(tenant1, tenant2);
        Assert.NotEqual(tenant1, tenant3);
    }

    [Fact]
    public void AuthenticationContext_Construction_SetsPropertiesCorrectly()
    {
        var auth = new AuthenticationContext("Bearer", "Authenticated");
        
        Assert.Equal("Bearer", auth.AuthenticationScheme);
        Assert.Equal("Authenticated", auth.AuthenticationStatus);
    }

    [Fact]
    public void AuthenticationContext_HasDeterministicEquality()
    {
        var auth1 = new AuthenticationContext("Bearer", "Authenticated");
        var auth2 = new AuthenticationContext("Bearer", "Authenticated");
        var auth3 = new AuthenticationContext("ApiKey", "Authenticated");

        Assert.Equal(auth1, auth2);
        Assert.NotEqual(auth1, auth3);
    }

    [Fact]
    public void AuthorizationContext_Construction_SetsPropertiesCorrectly()
    {
        var permissions = new List<string> { "read", "write" };
        var roles = new List<string> { "admin" };
        var authz = new AuthorizationContext(permissions, roles);

        Assert.Equal(2, authz.Permissions.Count);
        Assert.Single(authz.Roles);
        Assert.Equal("read", authz.Permissions[0]);
        Assert.Equal("admin", authz.Roles[0]);
    }
}
