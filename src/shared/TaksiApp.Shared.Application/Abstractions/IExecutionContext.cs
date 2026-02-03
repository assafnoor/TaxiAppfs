namespace TaksiApp.Shared.Application.Abstractions;
// TaksiApp.Shared.Application/Abstractions/IExecutionContext.cs
/// <summary>
/// Provides context about the current execution environment.
/// Injected as scoped to maintain per-request state.
/// </summary>
public interface IExecutionContext
{
    /// <summary>
    /// Correlation ID for distributed tracing.
    /// Should flow across service boundaries.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// User ID of the authenticated user, if any.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Tenant ID for multi-tenant systems.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Additional contextual metadata.
    /// For backward compatibility, returns dictionary interface.
    /// Use ExecutionContext.Metadata property for strongly-typed access.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Validates that current execution context can access a resource belonging to specified tenant.
    /// Throws UnauthorizedAccessException if cross-tenant access attempted.
    /// </summary>
    void EnsureTenantAccess(string? resourceTenantId);
}