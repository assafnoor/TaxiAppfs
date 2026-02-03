using Microsoft.AspNetCore.Http;
using TaksiApp.Shared.Api.Constants;

namespace TaksiApp.Shared.Api.Middleware;

/// <summary>
/// HTTP message handler that propagates correlation ID to outgoing HTTP requests.
/// </summary>
/// <remarks>
/// This handler extracts the correlation ID from the current execution context
/// and adds it to the X-Correlation-Id header of outgoing HTTP requests.
/// This enables end-to-end request tracking across service boundaries.
/// 
/// If no execution context is available (e.g., background jobs), a new correlation ID is generated.
/// </remarks>
public sealed class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the correlation ID delegating handler.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// Accessor for the current HTTP context to retrieve execution context.
    /// </param>
    public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// Sends an HTTP request with correlation ID propagation.
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Try to get correlation ID from execution context
        var executionContext = _httpContextAccessor.HttpContext?.GetExecutionContext();
        var correlationId = executionContext?.CorrelationId ?? Guid.NewGuid().ToString();

        // Add correlation ID header if not already present
        if (!request.Headers.Contains(HttpContextConstants.CorrelationIdHeaderName))
        {
            request.Headers.Add(HttpContextConstants.CorrelationIdHeaderName, correlationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}