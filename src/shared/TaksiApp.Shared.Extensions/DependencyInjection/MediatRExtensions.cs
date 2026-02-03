using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaksiApp.Shared.Application.Behaviors;

namespace TaksiApp.Shared.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering MediatR pipeline behaviors
/// provided by the Shared Application layer.
/// </summary>
/// <remarks>
/// <para>
/// This extension registers all shared MediatR pipeline behaviors
/// in a predefined and intentional order.
/// </para>
/// <para>
/// <strong>Behavior execution order is critical.</strong>
/// MediatR executes pipeline behaviors in the order they are registered.
/// </para>
/// <para>
/// The default ordering is:
/// <list type="number">
/// <item><see cref="LoggingBehavior{TRequest, TResponse}"/></item>
/// <item><see cref="TracingBehavior{TRequest, TResponse}"/></item>
/// <item><see cref="ValidationBehavior{TRequest, TResponse}"/></item>
/// </list>
/// </para>
/// <para>
/// Changing this order may result in:
/// <list type="bullet">
/// <item>Incorrect or missing logs</item>
/// <item>Incomplete traces</item>
/// <item>Validation errors being logged incorrectly</item>
/// </list>
/// </para>
/// <para>
/// For detailed rationale, refer to <c>BEHAVIOR_ORDERING.md</c>.
/// </para>
/// </remarks>
public static class MediatRExtensions
{
    /// <summary>
    /// Registers all shared MediatR pipeline behaviors used across microservices.
    /// </summary>
    /// <param name="services">The service collection to add the behaviors to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method must be called once during application startup,
    /// typically in <c>Program.cs</c> or a dedicated composition root.
    /// </para>
    /// <para>
    /// All commands and queries are expected to return
    /// <see cref="TaksiApp.Shared.Kernel.Results.Result"/> or
    /// <see cref="TaksiApp.Shared.Kernel.Results.Result{T}"/>.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddSharedMediatRBehaviors(
        this IServiceCollection services)
    {
        // Order matters! See BEHAVIOR_ORDERING.md
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
