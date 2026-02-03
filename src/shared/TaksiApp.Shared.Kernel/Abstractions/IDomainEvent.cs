namespace TaksiApp.Shared.Kernel.Abstractions;
// TaksiApp.Shared.Kernel/Events/IDomainEvent.cs
/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain
/// that domain experts care about.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When the event occurred in the domain (not when published).
    /// </summary>
    DateTime OccurredOnUtc { get; }
}
