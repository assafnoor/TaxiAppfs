using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Api.Controllers;

/// <summary>
/// Base controller providing Result&lt;T&gt; to ProblemDetails conversion.
/// All API controllers should inherit from this.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Converts an Error to a ProblemDetails response with appropriate HTTP status code.
    /// Automatically includes trace context (TraceId, SpanId) for distributed tracing.
    /// </summary>
    /// <param name="error">The error to convert</param>
    /// <returns>IActionResult with ProblemDetails body</returns>
    protected IActionResult Problem(Error error)
    {
        // Error is a struct, so no null check needed
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        // Use fully qualified type name to avoid namespace conflict
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(error.Type),
            Detail = error.Message,
            Instance = HttpContext.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        // Add trace context for distributed tracing correlation
        var activity = Activity.Current;
        if (activity != null)
        {
            problemDetails.Extensions["traceId"] = activity.TraceId.ToString();
            problemDetails.Extensions["spanId"] = activity.SpanId.ToString();
        }

        // Add custom error metadata
        problemDetails.Extensions["errorCode"] = error.Code;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        // Include error metadata if present
        if (error.Metadata != null && error.Metadata.Count > 0)
        {
            problemDetails.Extensions["metadata"] = error.Metadata;
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Get human-readable title for error type.
    /// </summary>
    private static string GetTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "Validation Error",
        ErrorType.NotFound => "Resource Not Found",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        _ => "Server Error"
    };
}