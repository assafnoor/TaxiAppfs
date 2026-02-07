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

/// <summary>
/// Provides extension methods for registering shared observability components
/// such as logging, tracing, metrics, and health checks.
/// </summary>
/// <remarks>
/// This extension configures a complete observability stack based on OpenTelemetry,
/// including:
/// <list type="bullet">
/// <item><description>Distributed tracing using <see cref="ActivitySource"/></description></item>
/// <item><description>Application and infrastructure metrics</description></item>
/// <item><description>Health checks for core application services</description></item>
/// <item><description>Log enrichment with correlation and tracing context</description></item>
/// </list>
/// <para>
/// Intended to be used by all services to ensure consistent observability configuration
/// across the platform.
/// </para>
/// </remarks>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Registers shared observability services including OpenTelemetry tracing,
    /// metrics, health checks, and log enrichers.
    /// </summary>
    /// <param name="services">
    /// The dependency injection service collection.
    /// </param>
    /// <param name="configuration">
    /// The application configuration used to read observability settings.
    /// </param>
    /// <param name="serviceName">
    /// The logical name of the service. This value is used as the OpenTelemetry
    /// service name, meter name, and activity source name.
    /// </param>
    /// <param name="serviceVersion">
    /// Optional version of the service. Defaults to <c>"1.0.0"</c> if not provided.
    /// This value is attached to telemetry resources for version-aware tracing
    /// and metrics.
    /// </param>
    /// <param name="configureOtel">
    /// Optional callback allowing callers to further customize the
    /// <see cref="OpenTelemetryBuilder"/> (e.g., adding exporters, processors,
    /// or custom instrumentation).
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance, allowing
    /// fluent configuration.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is designed to be called once during application startup
    /// (typically in <c>Program.cs</c>).
    /// </para>
    /// <para>
    /// The following components are registered:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="ActivitySource"/> for distributed tracing
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="Meter"/> and <see cref="Metrics.MeterProvider"/> for metrics creation
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="ApplicationMetrics"/> for generic application-level metrics
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// OpenTelemetry tracing and metrics pipelines with OTLP and Prometheus exporters
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Health checks for core application services
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Log enrichers for correlation and trace context
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// Most components are registered as singletons, as they are thread-safe
    /// and intended to be shared across the application lifetime.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Services.AddSharedObservability(
    ///     builder.Configuration,
    ///     serviceName: "TaksiApp.Orders",
    ///     serviceVersion: "1.2.0");
    /// </code>
    /// </example>
    public static IServiceCollection AddSharedObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        string? serviceVersion = null,
        Action<OpenTelemetryBuilder>? configureOtel = null)
    {
        // method body unchanged

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