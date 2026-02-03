using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;

namespace TaksiApp.Shared.Api.Factories;

/// <summary>
/// Factory for creating ProblemDetails responses with trace context.
/// Allows configurable ProblemDetails Type URLs.
/// </summary>
public sealed class SharedProblemDetailsFactory : ProblemDetailsFactory
{
    private readonly Func<int, string>? _typeUrlFactory;

    public SharedProblemDetailsFactory(Func<int, string>? typeUrlFactory = null)
    {
        _typeUrlFactory = typeUrlFactory;
    }

    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        statusCode ??= StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type
                ?? _typeUrlFactory?.Invoke(statusCode.Value)
                ?? $"https://httpstatuses.com/{statusCode}",
            Detail = detail,
            Instance = instance ?? httpContext.Request.Path
        };

        ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode.Value);

        return problemDetails;
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelStateDictionary,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        statusCode ??= StatusCodes.Status400BadRequest;

        var problemDetails = new ValidationProblemDetails(modelStateDictionary)
        {
            Status = statusCode,
            Title = title ?? "One or more validation errors occurred.",
            Type = type
                ?? _typeUrlFactory?.Invoke(statusCode.Value)
                ?? $"https://httpstatuses.com/{statusCode}",
            Detail = detail,
            Instance = instance ?? httpContext.Request.Path
        };

        ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode.Value);

        return problemDetails;
    }

    private static void ApplyProblemDetailsDefaults(
        HttpContext httpContext,
        ProblemDetails problemDetails,
        int statusCode)
    {
        problemDetails.Status ??= statusCode;

        var activity = Activity.Current;
        if (activity != null)
        {
            problemDetails.Extensions["traceId"] = activity.TraceId.ToString();
            problemDetails.Extensions["spanId"] = activity.SpanId.ToString();
        }
        else
        {
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        }

        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
    }
}
