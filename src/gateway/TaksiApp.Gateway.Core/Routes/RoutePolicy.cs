namespace TaksiApp.Gateway.Core.Routes;

/// <summary>
/// Route policy configuration
/// </summary>
public sealed class RoutePolicy
{
    public LoadBalancingMode LoadBalancing { get; set; } = LoadBalancingMode.RoundRobin;
    public bool EnableRateLimiting { get; set; } = true;
    public int RateLimitPermits { get; set; } = 100;
    public int RateLimitWindowSeconds { get; set; } = 60;
    public bool EnableCircuitBreaker { get; set; } = true;
    public bool EnableCaching { get; set; } = false;
    public int CacheDurationSeconds { get; set; } = 300;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
}
