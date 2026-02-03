

using TaksiApp.Shared.Application.Abstractions;

namespace TaksiApp.Shared.Observability.Logging.Enrichers;
// TaksiApp.Shared.Observability/Logging/Enrichers/CorrelationIdEnricher.cs
public sealed class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly IExecutionContext _executionContext;

    public CorrelationIdEnricher(IExecutionContext executionContext)
    {
        _executionContext = executionContext;
    }

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