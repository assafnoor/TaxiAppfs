using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaksiApp.Shared.Api.Middleware;
using TaksiApp.Shared.Application.Abstractions;

namespace TaksiApp.Shared.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering Application layer contracts and services.
/// </summary>
/// <remarks>
/// <para>
/// This extension class registers only application-layer abstractions and their
/// implementations that don't depend on infrastructure concerns.
/// </para>
/// <para>
/// <strong>IMPORTANT:</strong> This does NOT register IDateTimeProvider.
/// Infrastructure implementations should be registered separately using
/// AddSharedInfrastructure() to maintain proper layer separation.
/// </para>
/// </remarks>
public static class ApplicationExtensions
{
    /// <summary>
    /// Registers core application layer services and contracts.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method registers:
    /// - IHttpContextAccessor (required for execution context)
    /// - IExecutionContext (scoped, extracted from HTTP context)
    /// </para>
    /// <para>
    /// Call this method before AddMediatR or other application service registrations.
    /// </para>
    /// <para>
    /// <strong>Layer separation:</strong> This method only registers application-layer
    /// contracts. Infrastructure implementations (like IDateTimeProvider) should be
    /// registered using AddSharedInfrastructure().
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Program.cs or Startup.cs
    /// services
    ///     .AddSharedApplication(configuration)
    ///     .AddSharedInfrastructure() // Separate call for infrastructure
    ///     .AddSharedObservability(configuration, "MyService");
    /// </code>
    /// </example>
    public static IServiceCollection AddSharedApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Required for accessing HTTP context in middleware and handlers
        services.AddHttpContextAccessor();

        // Register execution context as scoped - extracted from HTTP context
        // Falls back to anonymous context if no HTTP context is available (e.g., background jobs)
        services.AddScoped<IExecutionContext>(sp =>
        {
            var httpContextAccessor = sp.GetService<IHttpContextAccessor>();

            // Try to get execution context from current HTTP request
            if (httpContextAccessor?.HttpContext != null)
            {
                var context = httpContextAccessor.HttpContext.GetExecutionContext();
                if (context != null)
                    return context;
            }

            // Fallback: create anonymous execution context
            // This happens in background jobs or when middleware hasn't run yet
            return new Application.Context.ExecutionContext(
                correlationId: Guid.NewGuid().ToString(),
                userId: null,
                tenantId: null);
        });

        return services;
    }
}