using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace TaksiApp.Gateway.Api.Transforms;

/// <summary>
/// YARP transform to forward authentication information
/// </summary>
public sealed class AuthenticationTransform : RequestTransform
{
    public override ValueTask ApplyAsync(RequestTransformContext context)
    {
        var httpContext = context.HttpContext;

        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            // Forward user ID
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? httpContext.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                context.ProxyRequest.Headers.Remove("X-User-Id");
                context.ProxyRequest.Headers.Add("X-User-Id", userId);
            }

            // Forward tenant ID
            var tenantId = httpContext.User.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
            {
                context.ProxyRequest.Headers.Remove("X-Tenant-Id");
                context.ProxyRequest.Headers.Add("X-Tenant-Id", tenantId);
            }

            // Forward user roles
            var roles = httpContext.User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray();

            if (roles.Length > 0)
            {
                context.ProxyRequest.Headers.Remove("X-User-Roles");
                context.ProxyRequest.Headers.Add("X-User-Roles", string.Join(",", roles));
            }

            // Forward user email
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                context.ProxyRequest.Headers.Remove("X-User-Email");
                context.ProxyRequest.Headers.Add("X-User-Email", email);
            }
        }

        return default;
    }
}

/// <summary>
/// Extension methods for adding authentication transform
/// </summary>
public static class AuthenticationTransformExtensions
{
    private static readonly AuthenticationTransform _transform = new();

    public static TransformBuilderContext AddAuthentication(this TransformBuilderContext context)
    {
        context.AddRequestTransform(ctx => _transform.ApplyAsync(ctx));
        return context;
    }
}