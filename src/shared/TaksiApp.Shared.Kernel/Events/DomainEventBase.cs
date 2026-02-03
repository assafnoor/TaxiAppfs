using TaksiApp.Shared.Kernel.Abstractions;

namespace TaksiApp.Shared.Kernel.Events;

// Base implementation
public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}