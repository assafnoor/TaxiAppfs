using TaksiApp.Shared.Application.Abstractions;

namespace TaksiApp.Shared.Observability.HealthChecks;

/// <summary>
/// Performs a health check on the core application services.
/// </summary>
/// <remarks>
/// Checks basic operational health of the application using <see cref="IDateTimeProvider"/>.
/// Logs any exceptions encountered during the health check and returns
/// a <see cref="HealthCheckResult"/> indicating success or failure.
/// </remarks>
public sealed class ApplicationHealthCheck : IHealthCheck
{
    private readonly ILogger<ApplicationHealthCheck> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationHealthCheck"/> class.
    /// </summary>
    /// <param name="logger">The logger to capture health check failures.</param>
    /// <param name="dateTimeProvider">Provides current UTC time for health status.</param>
    public ApplicationHealthCheck(
        ILogger<ApplicationHealthCheck> logger,
        IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var now = _dateTimeProvider.UtcNow;
            var data = new Dictionary<string, object>
            {
                ["timestamp"] = now,
                ["status"] = "operational"
            };

            return Task.FromResult(
                HealthCheckResult.Healthy(
                    "Application core services are healthy",
                    data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application health check failed");
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Application core services are unhealthy",
                    ex));
        }
    }
}
