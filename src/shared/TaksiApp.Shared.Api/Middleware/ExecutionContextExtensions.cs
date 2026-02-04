using Microsoft.AspNetCore.Http;
using TaksiApp.Shared.Api.Constants;
using TaksiApp.Shared.Application.Abstractions;

namespace TaksiApp.Shared.Api.Middleware;

/// <summary>
/// Extension methods for retrieving execution context from HTTP context.
/// </summary>
public static class ExecutionContextExtensions
{

    /// <summary>
    /// Retrieves the execution context for the current HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>
    /// The execution context if found; otherwise, a new anonymous execution context
    /// with a generated correlation ID.
    /// </returns>
    /// <remarks>
    /// This method should be called after CorrelationIdMiddleware has executed.
    /// If called before middleware runs, it returns a fallback anonymous context.
    /// </remarks>
    public static IExecutionContext GetExecutionContext(this HttpContext context)
    {
        if (context.Items.TryGetValue(HttpContextConstants.ExecutionContextKey, out var value) &&
            value is IExecutionContext executionContext)
        {
            return executionContext;
        }

        // Fallback: create anonymous execution context
        return new Application.Context.ExecutionContext(
            Guid.NewGuid().ToString(),
            userId: null,
            tenantId: null);
    }
}