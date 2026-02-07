using TaksiApp.Gateway.Core.Routes;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Gateway.Core.Services;

/// <summary>
/// Manages smart routes with validation and hot reload support
/// </summary>
public interface IRouteManager
{
    /// <summary>
    /// Get all registered routes
    /// </summary>
    IReadOnlyList<SmartRoute> GetRoutes();

    /// <summary>
    /// Get route by ID
    /// </summary>
    Result<SmartRoute> GetRoute(string routeId);

    /// <summary>
    /// Add or update a route
    /// </summary>
    Task<Result> UpsertRouteAsync(SmartRoute route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a route
    /// </summary>
    Task<Result> RemoveRouteAsync(string routeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reload routes from configuration
    /// </summary>
    Task<Result> ReloadRoutesAsync(CancellationToken cancellationToken = default);
}
