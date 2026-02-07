namespace TaksiApp.Gateway.Core.Configuration;

/// <summary>
/// Rate limiting configuration per route
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>
    /// Maximum requests allowed per window
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Time window in seconds
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Queue limit for requests waiting
    /// </summary>
    public int QueueLimit { get; set; } = 10;
}
