using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TaksiApp.Shared.Application.Abstractions;

namespace TaksiApp.Gateway.Core.Services;

/// <summary>
/// Implementation of health monitor with circuit breaker pattern
/// </summary>
public sealed class HealthMonitor : IHealthMonitor
{
    private readonly ILogger<HealthMonitor> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConcurrentDictionary<string, HealthStats> _stats = new();
    private readonly ConcurrentDictionary<string, CircuitState> _circuits = new();

    public HealthMonitor(
        ILogger<HealthMonitor> logger,
        IDateTimeProvider dateTimeProvider,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<bool> IsHealthyAsync(string destination, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);

        var circuit = _circuits.GetOrAdd(destination, _ => new CircuitState());
        var now = _dateTimeProvider.UtcNow;

        // Check if circuit is open
        if (circuit.State == CircuitBreakerState.Open)
        {
            // Try to half-open after timeout
            if (now - circuit.LastStateChange > TimeSpan.FromSeconds(30))
            {
                circuit.State = CircuitBreakerState.HalfOpen;
                _logger.LogInformation("Circuit for {Destination} moved to HalfOpen", destination);
            }
            else
            {
                return false;
            }
        }

        try
        {
            var client = _httpClientFactory.CreateClient("HealthCheck");
            var healthUrl = $"{destination.TrimEnd('/')}/health";

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await client.GetAsync(healthUrl, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                RecordSuccess(destination);

                if (circuit.State == CircuitBreakerState.HalfOpen)
                {
                    circuit.State = CircuitBreakerState.Closed;
                    circuit.LastStateChange = now;
                    _logger.LogInformation("Circuit for {Destination} closed", destination);
                }

                return true;
            }

            RecordFailure(destination);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for {Destination}", destination);
            RecordFailure(destination);

            var stats = GetStats(destination);
            if (stats.SuccessRate < 0.5 && stats.TotalRequests >= 10)
            {
                circuit.State = CircuitBreakerState.Open;
                circuit.LastStateChange = now;
                _logger.LogWarning("Circuit for {Destination} opened", destination);
            }

            return false;
        }
    }

    public void RecordSuccess(string destination)
    {
        var stats = _stats.GetOrAdd(destination, _ => new HealthStats());
        stats.RecordSuccess(_dateTimeProvider.UtcNow);
    }

    public void RecordFailure(string destination)
    {
        var stats = _stats.GetOrAdd(destination, _ => new HealthStats());
        stats.RecordFailure(_dateTimeProvider.UtcNow);
    }

    public HealthStats GetStats(string destination)
    {
        return _stats.GetOrAdd(destination, _ => new HealthStats());
    }

}
