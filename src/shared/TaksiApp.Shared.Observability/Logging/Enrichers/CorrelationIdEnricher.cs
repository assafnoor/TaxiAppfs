using TaksiApp.Shared.Application.Abstractions;

namespace TaksiApp.Shared.Observability.Logging.Enrichers;

/// <summary>
/// Enriches log events with correlation information from the current execution context.
/// </summary>
/// <remarks>
/// Adds correlation_id, user_id, and tenant_id (if available) to the log event properties.
/// Useful for tracing requests across services and associating logs with users or tenants.
/// </remarks>
public sealed class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly IExecutionContext _executionContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdEnricher"/> class.
    /// </summary>
    /// <param name="executionContext">The execution context providing correlation information.</param>
    public CorrelationIdEnricher(IExecutionContext executionContext)
    {
        _executionContext = executionContext;
    }

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty(
                "correlation_id",
                _executionContext.CorrelationId));

        if (_executionContext.UserId != null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty(
                    "user_id",
                    _executionContext.UserId));
        }

        if (_executionContext.TenantId != null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty(
                    "tenant_id",
                    _executionContext.TenantId));
        }
    }
}
