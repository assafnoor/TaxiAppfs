using TaksiApp.Shared.Api.Constants;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace TaksiApp.Gateway.Api.Transforms;

/// <summary>
/// YARP transform to propagate correlation ID to backend services
/// </summary>
public sealed class CorrelationIdTransform : RequestTransform
{
    public override ValueTask ApplyAsync(RequestTransformContext context)
    {
        var httpContext = context.HttpContext;

        // Get or generate correlation ID
        var correlationId = httpContext.Request.Headers[HttpContextConstants.CorrelationIdHeaderName]
            .FirstOrDefault() ?? Guid.NewGuid().ToString();

        // Add to outgoing request
        context.ProxyRequest.Headers.Remove(HttpContextConstants.CorrelationIdHeaderName);
        context.ProxyRequest.Headers.Add(HttpContextConstants.CorrelationIdHeaderName, correlationId);

        // Also add to response
        if (!httpContext.Response.HasStarted)
        {
            httpContext.Response.Headers.TryAdd(
                HttpContextConstants.CorrelationIdHeaderName,
                correlationId);
        }

        return default;
    }
}

/// <summary>
/// Extension methods for adding correlation ID transform
/// </summary>
public static class CorrelationIdTransformExtensions
{
    private static readonly CorrelationIdTransform _transform = new();

    public static TransformBuilderContext AddCorrelationId(this TransformBuilderContext context)
    {
        context.AddRequestTransform(ctx => _transform.ApplyAsync(ctx));
        return context;
    }
}