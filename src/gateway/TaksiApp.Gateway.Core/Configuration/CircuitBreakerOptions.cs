namespace TaksiApp.Gateway.Core.Configuration;

/// <summary>
/// Circuit breaker configuration
/// </summary>
public sealed class CircuitBreakerOptions
{
    /// <summary>
    /// Failure threshold percentage to open circuit
    /// </summary>
    public double FailureThreshold { get; set; } = 0.5;

    /// <summary>
    /// Minimum throughput before circuit can trip
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Duration in seconds before attempting to close circuit
    /// </summary>
    public int DurationOfBreakSeconds { get; set; } = 30;

    /// <summary>
    /// Sampling duration in seconds
    /// </summary>
    public int SamplingDurationSeconds { get; set; } = 60;
}
