using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using TaksiApp.Shared.Api.Constants;

/// <summary>
/// Middleware that extracts or generates a correlation ID for the current request
/// and creates an execution context.
/// </summary>
/// <remarks>
/// This middleware:
/// 1. Extracts correlation ID from X-Correlation-Id header or generates a new one
/// 2. Optionally extracts user and tenant identifiers using provided extractors
/// 3. Creates an execution context and stores it in HttpContext.Items
/// 4. Adds correlation ID to response headers
/// 5. Enriches the current Activity with correlation, user, and tenant tags
/// 
/// The middleware is designed to be configured per-service with custom extraction logic,
/// as different services may use different authentication schemes and claim structures.
/// </remarks>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Func<HttpContext, string?>? _userIdExtractor;
    private readonly Func<HttpContext, string?>? _tenantIdExtractor;

    /// <summary>
    /// Initializes a new instance of the correlation ID middleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="userIdExtractor">
    /// Optional function to extract user ID from HttpContext.
    /// If not provided, no user ID will be extracted.
    /// </param>
    /// <param name="tenantIdExtractor">
    /// Optional function to extract tenant ID from HttpContext.
    /// If not provided, no tenant ID will be extracted.
    /// </param>
    public CorrelationIdMiddleware(
        RequestDelegate next,
        Func<HttpContext, string?>? userIdExtractor = null,
        Func<HttpContext, string?>? tenantIdExtractor = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _userIdExtractor = userIdExtractor;
        _tenantIdExtractor = tenantIdExtractor;
    }

    /// <summary>
    /// Processes the HTTP request.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Extract or generate correlation ID
        var correlationId = context.Request.Headers[HttpContextConstants.CorrelationIdHeaderName]
            .FirstOrDefault() ?? Guid.NewGuid().ToString();

        // Extract user and tenant identifiers using provided extractors
        var userId = _userIdExtractor?.Invoke(context);
        var tenantId = _tenantIdExtractor?.Invoke(context);

        // Create execution context
        var executionContext = new ExecutionContext(
            correlationId,
            userId,
            tenantId);

        // Store in HttpContext.Items for scoped retrieval
        context.Items[HttpContextConstants.ExecutionContextKey] = executionContext;

        // Add correlation ID to response headers
        if (!context.Response.HasStarted)
        {
            context.Response.Headers.TryAdd(
               HttpContextConstants.CorrelationIdHeaderName,
                correlationId);
        }

        // Enrich current Activity for distributed tracing
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("correlation_id", correlationId);

            if (userId != null)
            {
                activity.SetTag("user_id", userId);
            }

            if (tenantId != null)
            {
                activity.SetTag("tenant_id", tenantId);
            }
        }

        await _next(context);
    }
}