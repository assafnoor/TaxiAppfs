using TaksiApp.Shared.Kernel.Common;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Gateway.Core.Routes
{
    /// <summary>
    /// Represents a fully configured route with destinations, policy, and access control.
    /// </summary>
    public sealed class SmartRoute : ValueObject
    {
        /// <summary>
        /// Gets the unique identifier of the route.
        /// </summary>
        public string RouteId { get; }

        /// <summary>
        /// Gets the route prefix (must start with '/').
        /// </summary>
        public string RoutePrefix { get; }

        /// <summary>
        /// Gets the list of destination URLs for this route.
        /// </summary>
        public string[] Destinations { get; }

        /// <summary>
        /// Gets the policy configuration for this route.
        /// </summary>
        public RoutePolicy Policy { get; }

        /// <summary>
        /// Gets the priority of the route, used for ordering in case of conflicts.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Gets a value indicating whether authentication is required for this route.
        /// </summary>
        public bool RequiresAuthentication { get; }

        /// <summary>
        /// Gets the list of allowed roles for this route if authentication is required.
        /// </summary>
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

        /// <summary>
        /// Factory method to create a validated <see cref="SmartRoute"/> instance.
        /// </summary>
        /// <param name="routeId">Unique route identifier.</param>
        /// <param name="routePrefix">Route prefix (must start with '/').</param>
        /// <param name="destinations">Array of absolute destination URLs.</param>
        /// <param name="policy">Policy configuration for this route.</param>
        /// <param name="priority">Optional route priority, default is 0.</param>
        /// <param name="requiresAuthentication">Optional flag indicating if auth is required, default is false.</param>
        /// <param name="allowedRoles">Optional allowed roles array if auth is required.</param>
        /// <returns>A <see cref="Result{SmartRoute}"/> containing the created route or validation errors.</returns>
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

        /// <inheritdoc/>
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return RouteId;
            yield return RoutePrefix;
        }
    }
}
