using TaksiApp.Shared.Kernel.Abstractions;

namespace TaksiApp.Shared.Kernel.Events;

/// <summary>
/// Represents the base class for all domain events in the system.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="DomainEventBase"/> captures a significant change or occurrence
/// within the domain that other parts of the system might react to.
/// </para>
/// <para>
/// Inherits from <see cref="IDomainEvent"/> and provides default
/// implementation for common event properties such as <see cref="EventId"/>
/// and <see cref="OccurredOnUtc"/>.
/// </para>
/// <para>
/// Use this base class to create strongly-typed, immutable domain events
/// for use in an event-driven architecture or CQRS patterns.
/// </para>
/// </remarks>
public abstract record DomainEventBase : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this domain event.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the UTC date and time when this event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
