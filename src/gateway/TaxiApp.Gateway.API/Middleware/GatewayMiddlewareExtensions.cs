namespace TaksiApp.Gateway.Api.Middleware;

public static class GatewayMiddlewareExtensions
{
    /// <summary>
    /// Adds request timeout middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseRequestTimeout(this IApplicationBuilder app)
        => app.UseMiddleware<RequestTimeoutMiddleware>();

    /// <summary>
    /// Adds gateway metrics middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseGatewayMetrics(this IApplicationBuilder app)
        => app.UseMiddleware<GatewayMetricsMiddleware>();
}
