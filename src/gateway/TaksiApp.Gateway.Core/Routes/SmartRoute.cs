using TaksiApp.Shared.Kernel.Common;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Gateway.Core.Routes;

/// <summary>
/// Represents a smart route configuration with validation
/// </summary>
public sealed class SmartRoute : ValueObject
{
    public string RouteId { get; }
    public string RoutePrefix { get; }
    public string[] Destinations { get; }
    public RoutePolicy Policy { get; }
    public int Priority { get; }
    public bool RequiresAuthentication { get; }
    public string[]? AllowedRoles { get; }

    private SmartRoute(
        string routeId,
        string routePrefix,
        string[] destinations,
        RoutePolicy policy,
        int priority,
        bool requiresAuthentication,
        string[]? allowedRoles)
    {
        RouteId = routeId;
        RoutePrefix = routePrefix;
        Destinations = destinations;
        Policy = policy;
        Priority = priority;
        RequiresAuthentication = requiresAuthentication;
        AllowedRoles = allowedRoles;
    }

    public static Result<SmartRoute> Create(
        string routeId,
        string routePrefix,
        string[] destinations,
        RoutePolicy policy,
        int priority = 0,
        bool requiresAuthentication = false,
        string[]? allowedRoles = null)
    {
        if (string.IsNullOrWhiteSpace(routeId))
            return Result.Failure<SmartRoute>(
                Error.Validation("Route.InvalidId", "Route ID is required"));

        if (string.IsNullOrWhiteSpace(routePrefix))
            return Result.Failure<SmartRoute>(
                Error.Validation("Route.InvalidPrefix", "Route prefix is required"));

        if (!routePrefix.StartsWith('/'))
            return Result.Failure<SmartRoute>(
                Error.Validation("Route.InvalidPrefix", "Route prefix must start with /"));

        if (destinations == null || destinations.Length == 0)
            return Result.Failure<SmartRoute>(
                Error.Validation("Route.NoDestinations", "At least one destination is required"));

        // Validate all destinations are valid URIs
        foreach (var dest in destinations)
        {
            if (!Uri.TryCreate(dest, UriKind.Absolute, out _))
                return Result.Failure<SmartRoute>(
                    Error.Validation("Route.InvalidDestination", $"Invalid destination URI: {dest}"));
        }

        if (priority < 0)
            return Result.Failure<SmartRoute>(
                Error.Validation("Route.InvalidPriority", "Priority must be non-negative"));

        return Result.Success(new SmartRoute(
            routeId,
            routePrefix,
            destinations,
            policy,
            priority,
            requiresAuthentication,
            allowedRoles));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return RouteId;
        yield return RoutePrefix;
    }
}
