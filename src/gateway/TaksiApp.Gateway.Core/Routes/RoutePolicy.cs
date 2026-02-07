namespace TaksiApp.Gateway.Core.Routes;

/// <summary>
/// Configuration for route behavior and policies such as load balancing,
/// caching, circuit breaking, and rate limiting.
/// </summary>
public sealed class RoutePolicy
{
    /// <summary>
    /// Gets or sets the load balancing strategy for this route.
    /// </summary>
    public LoadBalancingMode LoadBalancing { get; set; } = LoadBalancingMode.RoundRobin;

    /// <summary>
    /// Gets or sets a value indicating whether rate limiting is enabled.
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of permitted requests per rate limit window.
    /// </summary>
    public int RateLimitPermits { get; set; } = 100;

    /// <summary>
    /// Gets or sets the duration of the rate limit window in seconds.
    /// </summary>
    public int RateLimitWindowSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets a value indicating whether circuit breaker is enabled.
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether caching is enabled for this route.
    /// </summary>
    public bool EnableCaching { get; set; } = false;

    /// <summary>
    /// Gets or sets the cache duration in seconds if caching is enabled.
    /// </summary>
    public int CacheDurationSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of retries on failed requests.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
