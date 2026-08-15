
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SaaSFoundry.Transport.Generated;

public class MtlsAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MtlsAuthorizationMiddleware> _logger;
    private static readonly string[] AllowedCallers = new[] { "spiffe://saasfoundry/vibestock/ingestor.cell", "spiffe://saasfoundry/vibestock/bridge.cell" };

    public MtlsAuthorizationMiddleware(RequestDelegate next, ILogger<MtlsAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Verify connection is mTLS
        var cert = context.Connection.ClientCertificate;
        if (cert == null)
        {
            _logger.LogWarning("mTLS authorization failed: No client certificate.");
            context.Response.StatusCode = 401;
            return;
        }

        // 2. Validate SPIFFE URI SAN
        var sanExtensions = cert.Extensions["2.5.29.17"];
        if (sanExtensions == null)
        {
            _logger.LogWarning("mTLS authorization failed: No Subject Alternative Name extension.");
            context.Response.StatusCode = 401;
            return;
        }

        var spiffeUri = ExtractSpiffeUri(sanExtensions.Format(false));
        if (string.IsNullOrEmpty(spiffeUri))
        {
            _logger.LogWarning("mTLS authorization failed: No SPIFFE URI SAN.");
            context.Response.StatusCode = 401;
            return;
        }

        // 3. Topology Authorization
        if (!AllowedCallers.Contains(spiffeUri, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning($"mTLS authorization failed: Caller {spiffeUri} is not authorized by the communication graph.");
            context.Response.StatusCode = 403;
            return;
        }

        await _next(context);
    }

    private string? ExtractSpiffeUri(string sanFormatted)
    {
        var parts = sanFormatted.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            if (p.StartsWith("URI=", StringComparison.OrdinalIgnoreCase) || p.StartsWith("URL=", StringComparison.OrdinalIgnoreCase))
            {
                var val = p.Substring(4);
                if (val.StartsWith("spiffe://", StringComparison.OrdinalIgnoreCase)) return val;
            }
        }
        return null;
    }
}