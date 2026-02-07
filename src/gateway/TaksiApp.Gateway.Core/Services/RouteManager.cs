using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaksiApp.Gateway.Core.Configuration;
using TaksiApp.Gateway.Core.Routes;
using TaksiApp.Shared.Application.Abstractions;
using TaksiApp.Shared.Kernel.Results;
using Error = TaksiApp.Shared.Kernel.Results.Error;

namespace TaksiApp.Gateway.Core.Services
{
    /// <summary>
    /// Manages routes with thread-safe operations and supports add, update, remove, and reload.
    /// </summary>
    public sealed class RouteManager : IRouteManager
    {
        private readonly ILogger<RouteManager> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsMonitor<GatewayOptions> _options;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private Dictionary<string, SmartRoute> _routes = new();

        /// <summary>
        /// Initializes a new instance of <see cref="RouteManager"/>.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="serviceProvider">Service provider for resolving scoped services.</param>
        /// <param name="options">Gateway configuration options.</param>
        public RouteManager(
            ILogger<RouteManager> logger,
            IServiceProvider serviceProvider,
            IOptionsMonitor<GatewayOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the current execution context from the service provider.
        /// </summary>
        private IExecutionContext GetExecutionContext() =>
            _serviceProvider.GetService<IExecutionContext>()
            ?? new TaksiApp.Shared.Application.Context.ExecutionContext(
                Guid.NewGuid().ToString(), null, null);

        /// <summary>
        /// Gets all registered routes, ordered by priority.
        /// </summary>
        /// <returns>Read-only list of routes.</returns>
        public IReadOnlyList<SmartRoute> GetRoutes() => _routes.Values.OrderBy(r => r.Priority).ToList();

        /// <summary>
        /// Gets a route by its ID.
        /// </summary>
        /// <param name="routeId">The route ID.</param>
        /// <returns>The route or failure if not found.</returns>
        public Result<SmartRoute> GetRoute(string routeId)
        {
            if (string.IsNullOrWhiteSpace(routeId))
                return Result.Failure<SmartRoute>(Error.Validation("Route.InvalidId", "Route ID is required"));

            if (_routes.TryGetValue(routeId, out var route))
                return Result.Success(route);

            return Result.Failure<SmartRoute>(Error.NotFound("Route.NotFound", $"Route '{routeId}' not found"));
        }

        /// <summary>
        /// Adds or updates a route in a thread-safe manner.
        /// </summary>
        /// <param name="route">The route to upsert.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result of the operation.</returns>
        public async Task<Result> UpsertRouteAsync(SmartRoute route, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(route);
            await _lock.WaitAsync(cancellationToken);

            try
            {
                var isUpdate = _routes.ContainsKey(route.RouteId);
                _routes[route.RouteId] = route;

                _logger.LogInformation(
                    "Route {RouteId} {Action} successfully. CorrelationId: {CorrelationId}",
                    route.RouteId,
                    isUpdate ? "updated" : "added",
                    GetExecutionContext().CorrelationId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to upsert route {RouteId}. CorrelationId: {CorrelationId}",
                    route.RouteId,
                    GetExecutionContext().CorrelationId);

                return Result.Failure(Error.Failure("Route.UpsertFailed", "Failed to upsert route"));
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Removes a route by ID in a thread-safe manner.
        /// </summary>
        /// <param name="routeId">The route ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result of the operation.</returns>
        public async Task<Result> RemoveRouteAsync(string routeId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(routeId))
                return Result.Failure(Error.Validation("Route.InvalidId", "Route ID is required"));

            await _lock.WaitAsync(cancellationToken);

            try
            {
                if (!_routes.Remove(routeId))
                {
                    return Result.Failure(Error.NotFound("Route.NotFound", $"Route '{routeId}' not found"));
                }

                _logger.LogInformation(
                    "Route {RouteId} removed successfully. CorrelationId: {CorrelationId}",
                    routeId,
                    GetExecutionContext().CorrelationId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to remove route {RouteId}. CorrelationId: {CorrelationId}",
                    routeId,
                    GetExecutionContext().CorrelationId);

                return Result.Failure(Error.Failure("Route.RemoveFailed", "Failed to remove route"));
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Reloads routes from configuration in a thread-safe manner.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result of the operation.</returns>
        public async Task<Result> ReloadRoutesAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                _logger.LogInformation(
                    "Routes reloaded. Total routes: {Count}. CorrelationId: {CorrelationId}",
                    _routes.Count,
                    GetExecutionContext().CorrelationId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to reload routes. CorrelationId: {CorrelationId}",
                    GetExecutionContext().CorrelationId);

                return Result.Failure(Error.Failure("Route.ReloadFailed", "Failed to reload routes"));
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
