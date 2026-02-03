// TaksiApp.Shared.Kernel/Common/Entity.cs
namespace TaksiApp.Shared.Kernel.Common;

/// <summary>
/// Base class for all entities in the domain.
/// Entities have identity and are compared by their ID.
/// </summary>
/// <typeparam name="TId">Type of the entity's identifier (Guid, int, string, etc.)</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
{
    /// <summary>
    /// Unique identifier for this entity.
    /// Once set, cannot be changed (immutable).
    /// </summary>
    public TId Id { get; protected init; } = default!;

    /// <summary>
    /// Parameterless constructor for ORM frameworks.
    /// </summary>
    protected Entity() { }

    /// <summary>
    /// Constructor with ID validation.
    /// </summary>
    protected Entity(TId id)
    {
        // Handle both reference types (null check) and value types (default check)
        if (id is null || EqualityComparer<TId>.Default.Equals(id, default(TId)))
            throw new ArgumentException("Entity ID cannot be null or default value", nameof(id));

        Id = id;
    }

    /// <summary>
    /// Entities are equal if they have the same ID and are the same type.
    /// Handles EF Core proxy types correctly.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        // Same reference = same entity
        if (ReferenceEquals(this, other))
            return true;

        // Different types = not equal (even with same ID)
        if (GetUnproxiedType(this) != GetUnproxiedType(other))
            return false;

        // Transient entities (no ID yet) are never equal
        // Use EqualityComparer for consistent comparison across all types
        if (EqualityComparer<TId>.Default.Equals(Id, default(TId)) ||
            EqualityComparer<TId>.Default.Equals(other.Id, default(TId)))
            return false;

        // Compare by ID
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public bool Equals(Entity<TId>? other) => Equals((object?)other);

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(Entity<TId>? a, Entity<TId>? b) => !(a == b);

    public override int GetHashCode() =>
        HashCode.Combine(GetUnproxiedType(this).ToString(), Id);

    /// <summary>
    /// Handle EF Core dynamic proxies (Castle.Proxies).
    /// Returns the actual entity type, not the proxy type.
    /// </summary>
    private static Type GetUnproxiedType(object obj)
    {
        var type = obj.GetType();

        // EF Core proxies have "Castle.Proxies" in their name
        if (type.ToString().Contains("Castle.Proxies", StringComparison.Ordinal))
            return type.BaseType!;

        return type;
    }

    public override string ToString() => $"{GetType().Name} [Id={Id}]";
}