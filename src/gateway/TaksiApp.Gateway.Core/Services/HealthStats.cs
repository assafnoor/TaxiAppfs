namespace TaksiApp.Gateway.Core.Services;

/// <summary>
/// Represents health statistics for a destination, tracking request counts and success rates.
/// Provides methods to record successes and failures.
/// </summary>
public sealed class HealthStats
{
    private long _totalRequests;
    private long _successfulRequests;
    private long _failedRequests;

    /// <summary>
    /// Gets the total number of requests sent to the destination.
    /// </summary>
    public long TotalRequests => Volatile.Read(ref _totalRequests);

    /// <summary>
    /// Gets the total number of successful requests.
    /// </summary>
    public long SuccessfulRequests => Volatile.Read(ref _successfulRequests);

    /// <summary>
    /// Gets the total number of failed requests.
    /// </summary>
    public long FailedRequests => Volatile.Read(ref _failedRequests);

    /// <summary>
    /// Gets the success rate of requests. Returns 0 if no requests have been made.
    /// </summary>
    public double SuccessRate =>
        TotalRequests > 0
            ? (double)SuccessfulRequests / TotalRequests
            : 0;

    /// <summary>
    /// Gets the timestamp of the last health check.
    /// </summary>
    public DateTime LastHealthCheck { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the destination is currently considered healthy.
    /// </summary>
    public bool IsHealthy { get; private set; }

    /// <summary>
    /// Records a successful request for the destination, updating statistics and health status.
    /// </summary>
    /// <param name="now">The timestamp of the successful request.</param>
    public void RecordSuccess(DateTime now)
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Increment(ref _successfulRequests);

        LastHealthCheck = now;
        IsHealthy = true;
    }

    /// <summary>
    /// Records a failed request for the destination, updating statistics and health status.
    /// </summary>
    /// <param name="now">The timestamp of the failed request.</param>
    public void RecordFailure(DateTime now)
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Increment(ref _failedRequests);

        LastHealthCheck = now;
        IsHealthy = SuccessRate >= 0.5;
    }
}
