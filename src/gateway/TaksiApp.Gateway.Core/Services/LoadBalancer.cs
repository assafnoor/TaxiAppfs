using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TaksiApp.Gateway.Core.Routes;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Gateway.Core.Services;

/// <summary>
/// Implementation supporting multiple load balancing strategies
/// </summary>
public sealed class LoadBalancer : ILoadBalancer
{
    private readonly ILogger<LoadBalancer> _logger;
    private readonly IHealthMonitor _healthMonitor;
    private readonly ConcurrentDictionary<string, int> _roundRobinCounters = new();
    private readonly ConcurrentDictionary<string, int> _activeConnections = new();
    private readonly Random _random = new();

    public LoadBalancer(
        ILogger<LoadBalancer> logger,
        IHealthMonitor healthMonitor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _healthMonitor = healthMonitor ?? throw new ArgumentNullException(nameof(healthMonitor));
    }

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

            // Fallback to all destinations if none are healthy
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

        Interlocked.Increment(ref _activeConnections.GetOrAdd(destination, 0));

        return Result.Success(destination);
    }

    public void RecordCompletion(string destination)
    {
        if (_activeConnections.TryGetValue(destination, out var count))
        {
            Interlocked.Decrement(ref count);
            if (count < 0)
            {
                _activeConnections[destination] = 0;
            }
        }
    }

    private string SelectRoundRobin(string routeId, string[] destinations)
    {
        var counter = _roundRobinCounters.AddOrUpdate(
            routeId,
            0,
            (_, current) => (current + 1) % destinations.Length);

        return destinations[counter];
    }

    private string SelectLeastConnections(string[] destinations)
    {
        return destinations
            .OrderBy(d => _activeConnections.GetOrAdd(d, 0))
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

        var connections1 = _activeConnections.GetOrAdd(dest1, 0);
        var connections2 = _activeConnections.GetOrAdd(dest2, 0);

        return connections1 <= connections2 ? dest1 : dest2;
    }
}