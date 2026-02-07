using Microsoft.AspNetCore.Mvc;
using TaksiApp.Gateway.Core.Routes;
using TaksiApp.Gateway.Core.Services;
using TaksiApp.Shared.Api.Controllers;

namespace TaksiApp.Gateway.Api.Controllers;

/// <summary>
/// API for managing gateway routes and configuration
/// </summary>
[ApiController]
[Route("api/gateway")]
public sealed class GatewayManagementController : ApiControllerBase
{
    private readonly IRouteManager _routeManager;
    private readonly IHealthMonitor _healthMonitor;
    private readonly ILogger<GatewayManagementController> _logger;

    public GatewayManagementController(
        IRouteManager routeManager,
        IHealthMonitor healthMonitor,
        ILogger<GatewayManagementController> logger)
    {
        _routeManager = routeManager ?? throw new ArgumentNullException(nameof(routeManager));
        _healthMonitor = healthMonitor ?? throw new ArgumentNullException(nameof(healthMonitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all registered routes
    /// </summary>
    [HttpGet("routes")]
    [ProducesResponseType(typeof(IEnumerable<RouteDto>), StatusCodes.Status200OK)]
    public IActionResult GetRoutes()
    {
        var routes = _routeManager.GetRoutes()
            .Select(r => new RouteDto
            {
                RouteId = r.RouteId,
                RoutePrefix = r.RoutePrefix,
                Destinations = r.Destinations,
                Priority = r.Priority,
                RequiresAuthentication = r.RequiresAuthentication,
                AllowedRoles = r.AllowedRoles,
                Policy = new RoutePolicyDto
                {
                    LoadBalancing = r.Policy.LoadBalancing.ToString(),
                    EnableRateLimiting = r.Policy.EnableRateLimiting,
                    RateLimitPermits = r.Policy.RateLimitPermits,
                    EnableCircuitBreaker = r.Policy.EnableCircuitBreaker,
                    EnableCaching = r.Policy.EnableCaching,
                    TimeoutSeconds = r.Policy.TimeoutSeconds
                }
            });

        return Ok(routes);
    }

    /// <summary>
    /// Get route by ID
    /// </summary>
    [HttpGet("routes/{routeId}")]
    [ProducesResponseType(typeof(RouteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetRoute(string routeId)
    {
        var result = _routeManager.GetRoute(routeId);

        if (result.IsFailure)
            return Problem(result.Error);

        var route = result.Value;
        return Ok(new RouteDto
        {
            RouteId = route.RouteId,
            RoutePrefix = route.RoutePrefix,
            Destinations = route.Destinations,
            Priority = route.Priority,
            RequiresAuthentication = route.RequiresAuthentication,
            AllowedRoles = route.AllowedRoles
        });
    }

    /// <summary>
    /// Add or update a route
    /// </summary>
    [HttpPut("routes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertRoute(
        [FromBody] CreateRouteRequest request,
        CancellationToken cancellationToken)
    {
        var policy = new RoutePolicy
        {
            LoadBalancing = Enum.Parse<LoadBalancingMode>(request.LoadBalancing ?? "RoundRobin"),
            EnableRateLimiting = request.EnableRateLimiting,
            RateLimitPermits = request.RateLimitPermits,
            EnableCircuitBreaker = request.EnableCircuitBreaker,
            EnableCaching = request.EnableCaching,
            TimeoutSeconds = request.TimeoutSeconds
        };

        var routeResult = SmartRoute.Create(
            request.RouteId,
            request.RoutePrefix,
            request.Destinations,
            policy,
            request.Priority,
            request.RequiresAuthentication,
            request.AllowedRoles);

        if (routeResult.IsFailure)
            return Problem(routeResult.Error);

        var result = await _routeManager.UpsertRouteAsync(routeResult.Value, cancellationToken);

        if (result.IsFailure)
            return Problem(result.Error);

        return Ok(new { message = "Route updated successfully" });
    }

    /// <summary>
    /// Delete a route
    /// </summary>
    [HttpDelete("routes/{routeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRoute(
        string routeId,
        CancellationToken cancellationToken)
    {
        var result = await _routeManager.RemoveRouteAsync(routeId, cancellationToken);

        if (result.IsFailure)
            return Problem(result.Error);

        return Ok(new { message = "Route deleted successfully" });
    }

    /// <summary>
    /// Reload all routes
    /// </summary>
    [HttpPost("routes/reload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReloadRoutes(CancellationToken cancellationToken)
    {
        var result = await _routeManager.ReloadRoutesAsync(cancellationToken);

        if (result.IsFailure)
            return Problem(result.Error);

        return Ok(new { message = "Routes reloaded successfully" });
    }

    /// <summary>
    /// Get health status of all destinations
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(IEnumerable<DestinationHealthDto>), StatusCodes.Status200OK)]
    public IActionResult GetHealthStatus()
    {
        var routes = _routeManager.GetRoutes();
        var healthStats = routes
            .SelectMany(r => r.Destinations)
            .Distinct()
            .Select(d => new DestinationHealthDto
            {
                Destination = d,
                Stats = _healthMonitor.GetStats(d)
            });

        return Ok(healthStats);
    }
}

// DTOs
public sealed class RouteDto
{
    public required string RouteId { get; init; }
    public required string RoutePrefix { get; init; }
    public required string[] Destinations { get; init; }
    public int Priority { get; init; }
    public bool RequiresAuthentication { get; init; }
    public string[]? AllowedRoles { get; init; }
    public RoutePolicyDto? Policy { get; init; }
}

public sealed class RoutePolicyDto
{
    public required string LoadBalancing { get; init; }
    public bool EnableRateLimiting { get; init; }
    public int RateLimitPermits { get; init; }
    public bool EnableCircuitBreaker { get; init; }
    public bool EnableCaching { get; init; }
    public int TimeoutSeconds { get; init; }
}

public sealed class CreateRouteRequest
{
    public required string RouteId { get; init; }
    public required string RoutePrefix { get; init; }
    public required string[] Destinations { get; init; }
    public int Priority { get; init; }
    public bool RequiresAuthentication { get; init; }
    public string[]? AllowedRoles { get; init; }
    public string? LoadBalancing { get; init; }
    public bool EnableRateLimiting { get; init; } = true;
    public int RateLimitPermits { get; init; } = 100;
    public bool EnableCircuitBreaker { get; init; } = true;
    public bool EnableCaching { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
}

public sealed class DestinationHealthDto
{
    public required string Destination { get; init; }
    public required HealthStats Stats { get; init; }
}