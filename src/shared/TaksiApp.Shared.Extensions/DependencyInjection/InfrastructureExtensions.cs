using Microsoft.Extensions.DependencyInjection;
using TaksiApp.Shared.Application.Abstractions;
using TaksiApp.Shared.Infrastructure.Time;

namespace TaksiApp.Shared.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering shared infrastructure services.
/// </summary>
/// <remarks>
/// <para>
/// This class registers infrastructure implementations of application contracts.
/// It maintains proper layer separation by keeping infrastructure concerns
/// separate from application contracts.
/// </para>
/// <para>
/// Currently registers:
/// - IDateTimeProvider → UtcDateTimeProvider
/// </para>
/// <para>
/// Future infrastructure services should be added here (e.g., file storage,
/// email providers, SMS services) as long as they remain generic and reusable
/// across multiple domains.
/// </para>
/// </remarks>
public static class InfrastructureExtensions
{
    /// <summary>
    /// Registers shared infrastructure services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Call this method after AddSharedApplicationContracts() to ensure
    /// application contracts are registered first.
    /// </para>
    /// <para>
    /// <strong>Thread safety:</strong> All registered services are designed to be
    /// thread-safe when registered as singleton (like IDateTimeProvider).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Program.cs or Startup.cs
    /// services
    ///     .AddSharedApplicationContracts(configuration)
    ///     .AddSharedInfrastructure() // Infrastructure services
    ///     .AddSharedObservability(configuration, "MyService");
    /// </code>
    /// </example>
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Date/time provider - singleton as it's stateless
        services.AddSingleton<IDateTimeProvider, UtcDateTimeProvider>();

        // Future infrastructure services can be added here:
        // services.AddSingleton<IFileStorage, LocalFileStorage>();
        // services.AddTransient<IEmailSender, SmtpEmailSender>();

        return services;
    }
}