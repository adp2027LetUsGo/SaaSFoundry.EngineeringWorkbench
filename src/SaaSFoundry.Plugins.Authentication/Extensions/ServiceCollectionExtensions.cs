using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SaaSFoundry.Plugins.Authentication.Extensions;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddSaaSFoundryAuthentication(this IServiceCollection services, Action<JwtBearerOptions> configureJwt)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "SchemeDispatcher";
            options.DefaultChallengeScheme = "SchemeDispatcher";
        })
        .AddPolicyScheme("SchemeDispatcher", "Scheme dispatcher", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                if (context.Request.Headers.TryGetValue("X-Api-Key", out _))
                {
                    return "ApiKey";
                }
                return JwtBearerDefaults.AuthenticationScheme;
            };
        })
        .AddJwtBearer(configureJwt)
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });

        return services;
    }
}
