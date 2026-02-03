using TaksiApp.Shared.Application.Abstractions;

namespace TaksiApp.Shared.Application.Context;

/// <summary>
/// Immutable execution context for a request or background job.
/// </summary>
public sealed class ExecutionContext : IExecutionContext
{
    private readonly ExecutionContextMetadata _metadata;

    public string CorrelationId { get; }
    public string? UserId { get; }
    public string? TenantId { get; }

    /// <summary>
    /// Strongly-typed metadata access.
    /// </summary>
    public ExecutionContextMetadata Metadata => _metadata;

    /// <summary>
    /// Dictionary-based metadata for backward compatibility.
    /// </summary>
    IReadOnlyDictionary<string, object> IExecutionContext.Metadata => _metadata.AsDictionary();

    public ExecutionContext(
        string correlationId,
        string? userId = null,
        string? tenantId = null,
        Dictionary<string, object>? metadata = null)
    {
        CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
        UserId = userId;
        TenantId = tenantId;
        _metadata = new ExecutionContextMetadata(metadata);
    }

    /// <summary>
    /// Returns a new ExecutionContext with updated metadata.
    /// </summary>
    public ExecutionContext WithMetadata(ExecutionContextMetadata newMetadata)
    {
        return new ExecutionContext(
            correlationId: this.CorrelationId,
            userId: this.UserId,
            tenantId: this.TenantId,
            metadata: newMetadata.AsDictionary() as Dictionary<string, object>);
    }
    public void EnsureTenantAccess(string? resourceTenantId)
    {
        // System context (no tenant) can access anything
        if (TenantId == null) return;

        // Null resource tenant = shared resource
        if (resourceTenantId == null) return;

        // Cross-tenant access denied
        if (resourceTenantId != TenantId)
        {
            throw new UnauthorizedAccessException(
                $"Cross-tenant access violation. User tenant: {TenantId}, Resource tenant: {resourceTenantId}");
        }
    }
}
