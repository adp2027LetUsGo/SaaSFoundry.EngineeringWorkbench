using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;
using SaaSFoundry.Plugins.Authentication.Extensions;
using Xunit;

namespace SaaSFoundry.Plugins.Authentication.UnitTests;

public class AuthenticationIntegrationTests
{
    private class TestApiKeyValidator : IApiKeyValidator
    {
        public Task<IdentityContext?> ValidateApiKeyAsync(string apiKey)
        {
            if (apiKey == "valid-key")
            {
                var claims = new Dictionary<string, string> { { "Permission", "read" } };
                return Task.FromResult<IdentityContext?>(new IdentityContext("machine-1", "Machine", claims, "tenant-1"));
            }
            return Task.FromResult<IdentityContext?>(null);
        }
    }

    [Fact]
    public async Task ApiKeyAuthentication_WithValidKey_PopulatesContexts()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IApiKeyValidator, TestApiKeyValidator>();
        services.AddSaaSFoundryAuthentication(jwt => { });
        
        var provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = provider };
        context.Request.Headers["X-Api-Key"] = "valid-key";
        
        var authService = provider.GetRequiredService<IAuthenticationService>();
        var result = await authService.AuthenticateAsync(context, "ApiKey");

        Assert.True(result.Succeeded);
        
        context.User = result.Principal!;
        
        var middleware = new ContextPopulationMiddleware(ctx => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        var identity = context.Features.Get<IdentityContext>();
        Assert.NotNull(identity);
        Assert.Equal("machine-1", identity.SubjectId);
        Assert.Equal("tenant-1", identity.TenantAssociation);

        var tenant = context.Features.Get<TenantContext>();
        Assert.NotNull(tenant);
        Assert.Equal("tenant-1", tenant.TenantId);

        var authz = context.Features.Get<AuthorizationContext>();
        Assert.NotNull(authz);
        Assert.Contains("read", authz.Permissions);
    }

    [Fact]
    public async Task ApiKeyAuthentication_WithInvalidKey_Fails()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IApiKeyValidator, TestApiKeyValidator>();
        services.AddSaaSFoundryAuthentication(jwt => { });
        
        var provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = provider };
        context.Request.Headers["X-Api-Key"] = "invalid-key";
        
        var authService = provider.GetRequiredService<IAuthenticationService>();
        var result = await authService.AuthenticateAsync(context, "ApiKey");

        Assert.False(result.Succeeded);
    }
}
