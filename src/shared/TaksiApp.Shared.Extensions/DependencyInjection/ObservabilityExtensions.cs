using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog.Core;
using TaksiApp.Shared.Extensions.Configuration;
using TaksiApp.Shared.Observability.HealthChecks;
using TaksiApp.Shared.Observability.Logging.Enrichers;
using TaksiApp.Shared.Observability.Metrics;
using TaksiApp.Shared.Observability.Tracing;
using MeterProvider = TaksiApp.Shared.Observability.Metrics.MeterProvider;

namespace TaksiApp.Shared.Extensions.DependencyInjection;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddSharedObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        string? serviceVersion = null,
        Action<OpenTelemetryBuilder>? configureOtel = null)
    {
        var options = configuration
            .GetSection("Observability")
            .Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        // Activity Source
        services.AddSingleton(sp =>
            ActivitySourceFactory.Create(serviceName, serviceVersion));

        // Meter
        var meterProvider = new MeterProvider(
            serviceName,
            serviceVersion);
        services.AddSingleton(meterProvider);
        services.AddSingleton(meterProvider.Meter);

        // Application Metrics
        services.AddSingleton<ApplicationMetrics>(sp =>
            new ApplicationMetrics(serviceName, serviceVersion));

        // OpenTelemetry
        var otelBuilder = services.AddOpenTelemetry();

        otelBuilder.ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion ?? "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] =
                        configuration["Environment"] ?? "Development"
                }))
            .WithTracing(tracing => tracing
                .AddSource(serviceName)
                .AddAspNetCoreInstrumentation(opts =>
                {
                    //TODO: further enrich as needed
                    opts.RecordException = true;
                    opts.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("http.request.size",
                            request.ContentLength ?? 0);
                    };
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(opts =>
                {
                    opts.SetDbStatementForText = true;
                    opts.EnrichWithIDbCommand = (activity, command) =>
                    {
                        activity.SetTag("db.statement", command.CommandText);
                    };
                })
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(options.OtlpEndpoint);
                    opts.Protocol = OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(metrics => metrics
                .AddMeter(serviceName)
                .AddRuntimeInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddPrometheusExporter()
                .AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri(options.OtlpEndpoint);
                    opts.Protocol = OtlpExportProtocol.Grpc;
                }));
        if (configureOtel != null)
        {
            configureOtel(otelBuilder);
        }
        // Health Checks
        services.AddHealthChecks()
            .AddCheck<ApplicationHealthCheck>("application");

        // Log Enrichers
        services.AddSingleton<ILogEventEnricher, TraceContextEnricher>();
        services.AddScoped<ILogEventEnricher, CorrelationIdEnricher>();

        return services;
    }
}