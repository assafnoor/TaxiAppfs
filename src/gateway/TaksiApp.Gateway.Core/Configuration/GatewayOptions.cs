using System.ComponentModel.DataAnnotations;

namespace TaksiApp.Gateway.Core.Configuration;

/// <summary>
/// Configuration options for the Smart Gateway
/// </summary>
public sealed class GatewayOptions
{
    /// <summary>
    /// Enable rate limiting
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Enable circuit breaker
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;

    /// <summary>
    /// Enable request/response caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Enable load balancing
    /// </summary>
    public bool EnableLoadBalancing { get; set; } = true;

    /// <summary>
    /// Enable authentication forwarding
    /// </summary>
    public bool EnableAuthenticationForwarding { get; set; } = true;

    /// <summary>
    /// Default timeout for requests in seconds
    /// </summary>
    [Range(1, 300)]
    public int DefaultTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum concurrent requests per route
    /// </summary>

    [Range(1, 10000)]
    public int MaxConcurrentRequests { get; set; } = 100;
}
