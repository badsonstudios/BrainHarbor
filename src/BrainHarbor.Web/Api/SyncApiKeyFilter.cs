using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BrainHarbor.Web.Api;

/// <summary>
/// API-key auth for /api/sync/* (architecture.md §4): a single long random
/// key in a header, HTTPS only, 401 without it. The key lives in user-secrets
/// locally and App Service config in prod.
/// </summary>
public sealed class SyncApiKeyFilter(IConfiguration configuration, ILogger<SyncApiKeyFilter> logger)
    : IEndpointFilter
{
    public const string HeaderName = "X-BrainHarbor-Key";

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var configured = configuration["SYNC_API_KEY"] ?? configuration["Sync:ApiKey"];

        // Fail CLOSED: an unconfigured key must never mean "open write surface".
        if (string.IsNullOrWhiteSpace(configured))
        {
            logger.LogError(
                "SYNC_API_KEY is not configured — refusing all sync requests. " +
                "Set it via user-secrets (dev) or App Service configuration (prod).");
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        var provided = context.HttpContext.Request.Headers[HeaderName].ToString();
        if (string.IsNullOrEmpty(provided) || !FixedTimeEquals(provided, configured))
        {
            logger.LogWarning("Rejected sync request from {RemoteIp} — bad or missing API key.",
                context.HttpContext.Connection.RemoteIpAddress);
            return Results.Unauthorized();
        }

        return await next(context);
    }

    /// <summary>Length-independent constant-time comparison (no timing oracle
    /// on the key, and no early-out on length either).</summary>
    private static bool FixedTimeEquals(string a, string b)
    {
        var hashA = SHA256.HashData(Encoding.UTF8.GetBytes(a));
        var hashB = SHA256.HashData(Encoding.UTF8.GetBytes(b));
        return CryptographicOperations.FixedTimeEquals(hashA, hashB);
    }
}
