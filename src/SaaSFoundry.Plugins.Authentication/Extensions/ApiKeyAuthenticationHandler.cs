using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaaSFoundry.EngineeringWorkbench.Core.Contracts.Identity;

namespace SaaSFoundry.Plugins.Authentication.Extensions;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = "X-Api-Key";
}

public interface IApiKeyValidator
{
    Task<IdentityContext?> ValidateApiKeyAsync(string apiKey);
}

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyValidator _apiKeyValidator;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyValidator apiKeyValidator)
        : base(options, logger, encoder)
    {
        _apiKeyValidator = apiKeyValidator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out var extractedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var identityContext = await _apiKeyValidator.ValidateApiKeyAsync(extractedApiKey.ToString());
        if (identityContext == null)
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, identityContext.SubjectId),
            new Claim("IdentityType", identityContext.IdentityType),
            new Claim("TenantId", identityContext.TenantAssociation)
        };

        foreach (var claim in identityContext.Claims)
        {
            claims.Add(new Claim(claim.Key, claim.Value));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
