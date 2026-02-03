using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaksiApp.Shared.Api.Middleware;

namespace TaksiApp.Shared.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering shared middleware components.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds correlation ID middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="userIdExtractor">
    /// Optional function to extract user ID from HttpContext.
    /// Default implementation extracts from HttpContext.User.Identity.Name.
    /// </param>
    /// <param name="tenantIdExtractor">
    /// Optional function to extract tenant ID from HttpContext.
    /// Default implementation extracts from "tenant_id" claim.
    /// </param>
    /// <returns>The application builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// The default extractors assume:
    /// - User ID: HttpContext.User.Identity.Name
    /// - Tenant ID: "tenant_id" claim in the user principal
    /// </para>
    /// <para>
    /// Override these for custom authentication schemes:
    /// <code>
    /// app.UseCorrelationId(
    ///     userIdExtractor: ctx => ctx.User.FindFirst("sub")?.Value,
    ///     tenantIdExtractor: ctx => ctx.User.FindFirst("organization_id")?.Value
    /// );
    /// </code>
    /// </para>
    /// </remarks>
    public static IApplicationBuilder UseCorrelationId(
        this IApplicationBuilder app,
        Func<HttpContext, string?>? userIdExtractor = null,
        Func<HttpContext, string?>? tenantIdExtractor = null)
    {
        // Use default extractors if not provided
        userIdExtractor ??= ctx => ctx.User?.Identity?.Name;
        tenantIdExtractor ??= ctx => ctx.User?.FindFirst("tenant_id")?.Value;

        return app.UseMiddleware<CorrelationIdMiddleware>(
            userIdExtractor,
            tenantIdExtractor);
    }

    /// <summary>
    /// Adds correlation ID propagation handler to an HTTP client builder.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <returns>The HTTP client builder for chaining.</returns>
    /// <remarks>
    /// This handler ensures that correlation IDs flow from incoming requests
    /// to outgoing HTTP calls, enabling distributed tracing.
    /// 
    /// The handler requires IHttpContextAccessor to be registered in DI.
    /// This is automatically handled by AddSharedApplicationContracts().
    /// </remarks>
    public static IHttpClientBuilder AddCorrelationIdHandler(
        this IHttpClientBuilder builder)
    {
        // Register handler as singleton (it's stateless and thread-safe)
        builder.Services.TryAddSingleton<CorrelationIdDelegatingHandler>();

        return builder.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
    }
}