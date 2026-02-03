// TaksiApp.Shared.Kernel/Abstractions/IAggregateRoot.cs
namespace TaksiApp.Shared.Kernel.Abstractions;

/// <summary>
/// Marker interface for aggregate roots that can raise domain events.
/// Used by infrastructure to collect and publish events.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Domain events raised by this aggregate.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clear all domain events after publishing.
    /// Called by infrastructure only.
    /// </summary>
    void ClearDomainEvents();
}