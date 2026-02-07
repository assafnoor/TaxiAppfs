using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading.RateLimiting;
using TaksiApp.Gateway.Api.Middleware;
using TaksiApp.Gateway.Api.Transforms;
using TaksiApp.Gateway.Core.Configuration;
using TaksiApp.Gateway.Core.Services;
using TaksiApp.Shared.Api.Factories;
using TaksiApp.Shared.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Transforms;
using LoadBalancer = TaksiApp.Gateway.Core.Services.LoadBalancer;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// SERILOG CONFIGURATION
// ============================================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TaksiApp.Gateway")
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/gateway-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// ============================================================================
// SHARED SERVICES
// ============================================================================
builder.Services.AddSharedApplication(builder.Configuration);
builder.Services.AddSharedInfrastructure();
builder.Services.AddSharedObservability(
    builder.Configuration,
    "TaksiApp.Gateway",
    "1.0.0");

// ============================================================================
// GATEWAY CONFIGURATION
// ============================================================================
builder.Services.Configure<GatewayOptions>(
    builder.Configuration.GetSection("Gateway"));
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection("Gateway:RateLimit"));
builder.Services.Configure<CircuitBreakerOptions>(
    builder.Configuration.GetSection("Gateway:CircuitBreaker"));
builder.Services.Configure<CacheOptions>(
    builder.Configuration.GetSection("Gateway:Cache"));

// ============================================================================
// GATEWAY SERVICES
// ============================================================================
builder.Services.AddSingleton<IRouteManager, RouteManager>();
builder.Services.AddSingleton<IHealthMonitor, HealthMonitor>();
builder.Services.AddSingleton<ILoadBalancer, LoadBalancer>();

// ============================================================================
// HTTP CLIENT FOR HEALTH CHECKS
// ============================================================================
builder.Services.AddHttpClient("HealthCheck")
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(5);
    });

// ============================================================================
// CONTROLLERS & API
// ============================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "TaksiApp Smart Gateway API",
        Version = "v1",
        Description = "Smart API Gateway with load balancing, health monitoring, and observability"
    });
});

// Custom ProblemDetails factory
builder.Services.AddSingleton<Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory, SharedProblemDetailsFactory>();

// ============================================================================
// RATE LIMITING
// ============================================================================
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: correlationId,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                error = "Too Many Requests",
                message = "Rate limit exceeded. Please try again later.",
                retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? (double?)retryAfter.TotalSeconds
                    : null
            },
            cancellationToken: cancellationToken);
    };
});

// ============================================================================
// YARP REVERSE PROXY
// ============================================================================
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilder =>
    {
        // Add correlation ID to all requests
        transformBuilder.AddCorrelationId();

        // Add authentication headers
        transformBuilder.AddAuthentication();

        // Add request/response logging
        transformBuilder.AddRequestTransform(async context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();

            logger.LogInformation(
                "Gateway proxying {Method} {Path} to {Destination}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                context.DestinationPrefix);

            await ValueTask.CompletedTask;
        });
    });

// ============================================================================
// CORS
// ============================================================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("X-Correlation-Id");
    });
});

var app = builder.Build();

// ============================================================================
// MIDDLEWARE PIPELINE
// ============================================================================

// Swagger (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

// Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent);
        diagnosticContext.Set("CorrelationId",
            httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault());
    };
});

// CORS
app.UseCors();

// Custom middleware من الـ Shared
app.UseCorrelationId(
    userIdExtractor: ctx => ctx.User?.Identity?.Name,
    tenantIdExtractor: ctx => ctx.User?.FindFirst("tenant_id")?.Value);

// Gateway-specific middleware
app.UseRequestTimeout();
app.UseGatewayMetrics();

// Rate limiting
app.UseRateLimiter();

// Health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready");

// Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint();

// Management API
app.MapControllers();

// YARP Reverse Proxy
app.MapReverseProxy(proxyPipeline =>
{
    // Add custom middleware to proxy pipeline if needed
    proxyPipeline.Use(async (context, next) =>
    {
        var loadBalancer = context.RequestServices.GetRequiredService<ILoadBalancer>();
        var healthMonitor = context.RequestServices.GetRequiredService<IHealthMonitor>();

        try
        {
            await next();

            // Record successful completion
            var destination = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
            if (destination != null)
            {
                loadBalancer.RecordCompletion(destination);
                healthMonitor.RecordSuccess(destination);
            }
        }
        catch
        {
            var destination = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
            if (destination != null)
            {
                loadBalancer.RecordCompletion(destination);
                healthMonitor.RecordFailure(destination);
            }
            throw;
        }
    });
});

// ============================================================================
// STARTUP & SHUTDOWN
// ============================================================================
try
{
    Log.Information("Starting TaksiApp Smart Gateway...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway startup failed");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}