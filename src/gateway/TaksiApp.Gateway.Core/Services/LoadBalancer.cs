using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TaksiApp.Gateway.Core.Configuration;
using TaksiApp.Gateway.Core.Routes;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Gateway.Core.Services
{
    /// <summary>
    /// Implements multiple load balancing strategies for routing requests across destinations.
    /// Tracks active connections and integrates with a health monitor to select healthy endpoints.
    /// </summary>
    public sealed class LoadBalancer : ILoadBalancer
    {
        private readonly ILogger<LoadBalancer> _logger;
        private readonly IHealthMonitor _healthMonitor;
        private readonly ConcurrentDictionary<string, int> _roundRobinCounters = new();
        private readonly ConcurrentDictionary<string, ConnectionCounter> _activeConnections = new();
        private readonly Random _random = new();

        /// <summary>
        /// Initializes a new instance of <see cref="LoadBalancer"/>.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="healthMonitor">Health monitor for destination health checks.</param>
        public LoadBalancer(ILogger<LoadBalancer> logger, IHealthMonitor healthMonitor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _healthMonitor = healthMonitor ?? throw new ArgumentNullException(nameof(healthMonitor));
        }

        /// <summary>
        /// Selects a destination for the given route according to its load balancing policy.
        /// </summary>
        /// <param name="route">The route configuration.</param>
        /// <returns>The selected destination or failure if none available.</returns>
        public Result<string> SelectDestination(SmartRoute route)
        {
            ArgumentNullException.ThrowIfNull(route);

            var healthyDestinations = route.Destinations
                .Where(d => _healthMonitor.GetStats(d).IsHealthy)
                .ToArray();

            if (healthyDestinations.Length == 0)
            {
                _logger.LogWarning(
                    "No healthy destinations available for route {RouteId}",
                    route.RouteId);

                healthyDestinations = route.Destinations;
            }

            if (healthyDestinations.Length == 0)
            {
                return Result.Failure<string>(
                    Shared.Kernel.Results.Error.NotFound(
                        "LoadBalancer.NoDestinations",
                        $"No destinations available for route {route.RouteId}"));
            }

            var destination = route.Policy.LoadBalancing switch
            {
                LoadBalancingMode.RoundRobin => SelectRoundRobin(route.RouteId, healthyDestinations),
                LoadBalancingMode.LeastConnections => SelectLeastConnections(healthyDestinations),
                LoadBalancingMode.Random => SelectRandom(healthyDestinations),
                LoadBalancingMode.PowerOfTwoChoices => SelectPowerOfTwo(healthyDestinations),
                _ => SelectRoundRobin(route.RouteId, healthyDestinations)
            };

            var counter = _activeConnections.GetOrAdd(destination, _ => new ConnectionCounter());
            counter.Increment();

            return Result.Success(destination);
        }

        /// <summary>
        /// Records the completion of a request to a destination, decrementing its active connection count.
        /// </summary>
        /// <param name="destination">The destination endpoint.</param>
        public void RecordCompletion(string destination)
        {
            if (_activeConnections.TryGetValue(destination, out var counter))
            {
                counter.Decrement();
                if (counter.Value < 0)
                {
                    while (counter.Value < 0)
                        counter.Increment();
                }
            }
        }

        private string SelectRoundRobin(string routeId, string[] destinations)
        {
            var counter = _roundRobinCounters.AddOrUpdate(
                routeId, 0, (_, current) => (current + 1) % destinations.Length);
            return destinations[counter];
        }

        private string SelectLeastConnections(string[] destinations)
        {
            return destinations
                .OrderBy(d => _activeConnections.GetOrAdd(d, _ => new ConnectionCounter()).Value)
                .First();
        }

        private string SelectRandom(string[] destinations)
        {
            lock (_random)
            {
                return destinations[_random.Next(destinations.Length)];
            }
        }

        private string SelectPowerOfTwo(string[] destinations)
        {
            if (destinations.Length == 1)
                return destinations[0];

            int index1, index2;
            lock (_random)
            {
                index1 = _random.Next(destinations.Length);
                index2 = _random.Next(destinations.Length);
            }

            var dest1 = destinations[index1];
            var dest2 = destinations[index2];

            var connections1 = _activeConnections.GetOrAdd(dest1, _ => new ConnectionCounter()).Value;
            var connections2 = _activeConnections.GetOrAdd(dest2, _ => new ConnectionCounter()).Value;

            return connections1 <= connections2 ? dest1 : dest2;
        }
    }
}
