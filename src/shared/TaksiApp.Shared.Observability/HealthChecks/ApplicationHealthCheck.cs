
using TaksiApp.Shared.Application.Abstractions;

namespace TaksiApp.Shared.Observability.HealthChecks;
// TaksiApp.Shared.Observability/HealthChecks/ApplicationHealthCheck.cs
public sealed class ApplicationHealthCheck : IHealthCheck
{
    private readonly ILogger<ApplicationHealthCheck> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ApplicationHealthCheck(
        ILogger<ApplicationHealthCheck> logger,
        IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

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