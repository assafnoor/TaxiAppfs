namespace TaksiApp.Gateway.Core.Services;

/// <summary>
/// Health statistics for a destination
/// </summary>
public sealed class HealthStats
{
    private long _totalRequests;
    private long _successfulRequests;
    private long _failedRequests;

    public long TotalRequests => Volatile.Read(ref _totalRequests);
    public long SuccessfulRequests => Volatile.Read(ref _successfulRequests);
    public long FailedRequests => Volatile.Read(ref _failedRequests);

    public double SuccessRate =>
        TotalRequests > 0
            ? (double)SuccessfulRequests / TotalRequests
            : 0;

    public DateTime LastHealthCheck { get; private set; }
    public bool IsHealthy { get; private set; }

    public void RecordSuccess(DateTime now)
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Increment(ref _successfulRequests);

        LastHealthCheck = now;
        IsHealthy = true;
    }

    public void RecordFailure(DateTime now)
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Increment(ref _failedRequests);

        LastHealthCheck = now;
        IsHealthy = SuccessRate >= 0.5;
    }
}
