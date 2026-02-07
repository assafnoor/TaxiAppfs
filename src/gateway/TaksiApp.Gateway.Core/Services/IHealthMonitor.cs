namespace TaksiApp.Gateway.Core.Services;

/// <summary>
/// Monitors health of backend services
/// </summary>
public interface IHealthMonitor
{
    /// <summary>
    /// Check if a destination is healthy
    /// </summary>
    Task<bool> IsHealthyAsync(string destination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a successful request
    /// </summary>
    void RecordSuccess(string destination);

    /// <summary>
    /// Record a failed request
    /// </summary>
    void RecordFailure(string destination);

    /// <summary>
    /// Get health statistics for a destination
    /// </summary>
    HealthStats GetStats(string destination);
}
