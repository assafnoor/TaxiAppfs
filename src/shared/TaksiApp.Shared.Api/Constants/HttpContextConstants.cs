namespace TaksiApp.Shared.Api.Constants;

/// <summary>
/// Constants used for HTTP context items and headers across the shared library.
/// </summary>
/// <remarks>
/// Centralizing these constants prevents typos and makes refactoring easier.
/// All middleware and handlers should reference these constants instead of
/// defining their own private constants.
/// </remarks>
public static class HttpContextConstants
{
    /// <summary>
    /// The HTTP header name for correlation ID.
    /// </summary>
    /// <remarks>
    /// This header is used to track requests across service boundaries.
    /// Standard convention is "X-Correlation-Id" but some organizations
    /// may prefer "traceparent" or other W3C Trace Context headers.
    /// </remarks>
    public const string CorrelationIdHeaderName = "X-Correlation-Id";

    /// <summary>
    /// The HttpContext.Items key for storing execution context.
    /// </summary>
    /// <remarks>
    /// The execution context is stored in HttpContext.Items by CorrelationIdMiddleware
    /// and retrieved by ExecutionContextExtensions and other components.
    /// </remarks>
    public const string ExecutionContextKey = "ExecutionContext";
}