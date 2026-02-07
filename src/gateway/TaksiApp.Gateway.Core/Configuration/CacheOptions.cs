namespace TaksiApp.Gateway.Core.Configuration;

/// <summary>
/// Cache configuration for responses
/// </summary>
public sealed class CacheOptions
{
    /// <summary>
    /// Default cache duration in seconds
    /// </summary>
    public int DefaultDurationSeconds { get; set; } = 300;

    /// <summary>
    /// Maximum cache size in MB
    /// </summary>
    public int MaxCacheSizeMB { get; set; } = 100;

    /// <summary>
    /// Enable vary by query string
    /// </summary>
    public bool VaryByQueryString { get; set; } = true;

    /// <summary>
    /// Enable vary by headers
    /// </summary>
    public bool VaryByHeaders { get; set; } = true;

    /// <summary>
    /// Headers to vary cache by
    /// </summary>
    public string[] VaryByHeaderNames { get; set; } = { "Accept-Language", "Authorization" };
}