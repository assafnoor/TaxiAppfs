using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TaksiApp.Shared.Application.Abstractions;

namespace TaksiApp.Gateway.Core.Services
{
    /// <summary>
    /// Implementation of a health monitor for destinations with circuit breaker pattern.
    /// Tracks success/failure rates and controls circuit state.
    /// </summary>
    public sealed class HealthMonitor : IHealthMonitor
    {
        private readonly ILogger<HealthMonitor> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ConcurrentDictionary<string, HealthStats> _stats = new();
        private readonly ConcurrentDictionary<string, CircuitState> _circuits = new();

        /// <summary>
        /// Initializes a new instance of <see cref="HealthMonitor"/>.
        /// </summary>
        /// <param name="logger">Logger for diagnostic messages.</param>
        /// <param name="dateTimeProvider">Provider for current UTC time.</param>
        /// <param name="httpClientFactory">Factory to create <see cref="HttpClient"/> instances.</param>
        public HealthMonitor(
            ILogger<HealthMonitor> logger,
            IDateTimeProvider dateTimeProvider,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <summary>
        /// Checks if a destination is healthy by performing a health request and updating circuit state.
        /// </summary>
        /// <param name="destination">The destination URL to check.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns><c>true</c> if the destination is healthy; otherwise, <c>false</c>.</returns>
        public async Task<bool> IsHealthyAsync(string destination, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(destination);

            var circuit = _circuits.GetOrAdd(destination, _ => new CircuitState());
            var now = _dateTimeProvider.UtcNow;

            // Get current state atomically
            var (currentState, lastChange) = circuit.GetStateSnapshot();

            // Check if circuit is open
            if (currentState == CircuitBreakerState.Open)
            {
                if (now - lastChange > TimeSpan.FromSeconds(30))
                {
                    circuit.SetState(CircuitBreakerState.HalfOpen, now);
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

                    var (state, _) = circuit.GetStateSnapshot();
                    if (state == CircuitBreakerState.HalfOpen)
                    {
                        circuit.SetState(CircuitBreakerState.Closed, now);
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
                    circuit.SetState(CircuitBreakerState.Open, now);
                    _logger.LogWarning("Circuit for {Destination} opened", destination);
                }

                return false;
            }
        }
        /// <summary>
        /// Records a successful health check for a given destination.
        /// </summary>
        /// <param name="destination">The destination URL.</param>
        public void RecordSuccess(string destination)
        {
            var stats = _stats.GetOrAdd(destination, _ => new HealthStats());
            stats.RecordSuccess(_dateTimeProvider.UtcNow);
        }

        /// <summary>
        /// Records a failed health check for a given destination.
        /// </summary>
        /// <param name="destination">The destination URL.</param>
        public void RecordFailure(string destination)
        {
            var stats = _stats.GetOrAdd(destination, _ => new HealthStats());
            stats.RecordFailure(_dateTimeProvider.UtcNow);
        }

        /// <summary>
        /// Gets the current health statistics for a given destination.
        /// </summary>
        /// <param name="destination">The destination URL.</param>
        /// <returns>The <see cref="HealthStats"/> for the destination.</returns>
        public HealthStats GetStats(string destination)
        {
            return _stats.GetOrAdd(destination, _ => new HealthStats());
        }
    }
}
