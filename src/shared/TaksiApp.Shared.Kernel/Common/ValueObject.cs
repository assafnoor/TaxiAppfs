// TaksiApp.Shared.Kernel/Common/ValueObject.cs
namespace TaksiApp.Shared.Kernel.Common;

/// <summary>
/// Base class for value objects in DDD.
/// Value objects:
/// 1. Have no identity (compared by value, not ID)
/// 2. Are immutable
/// 3. Are self-validating
/// 4. Are side-effect free
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Override to return all components that determine equality.
    /// ORDER MATTERS: ["Street", "City"] != ["City", "Street"]
    /// </summary>
    /// <example>
    /// <code>
    /// protected override IEnumerable&lt;object?&gt; GetEqualityComponents()
    /// {
    ///     yield return Street;
    ///     yield return City;
    ///     yield return ZipCode;
    /// }
    /// </code>
    /// </example>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (GetType() != obj.GetType())
            return false;

        var valueObject = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(valueObject.GetEqualityComponents());
    }

    public bool Equals(ValueObject? other) => Equals((object?)other);

    public override int GetHashCode()
    {
        // Use HashCode.Combine for better distribution and collision resistance
        // XOR can cause collisions (e.g., a ^ b ^ c == c ^ b ^ a)
        var components = GetEqualityComponents().ToArray();
        if (components.Length == 0)
            return 0;

        // For single component, use its hash code directly
        if (components.Length == 1)
            return components[0]?.GetHashCode() ?? 0;

        // For multiple components, use HashCode.Combine (available in .NET Core 2.1+)
        var hash = new HashCode();
        foreach (var component in components)
        {
            hash.Add(component);
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !(left == right);

    /// <summary>
    /// Create a shallow copy of this value object using MemberwiseClone.
    /// 
    /// WARNING: This method performs a SHALLOW COPY only.
    /// - Value types and strings are copied by value (safe)
    /// - Reference types are copied by reference (shared between original and copy)
    /// 
    /// IMPORTANT: Since value objects should be immutable and contain only value types/immutable types,
    /// this method should work correctly for typical value object implementations.
    /// However, if your value object contains mutable reference types, this copy will share those references.
    /// 
    /// RECOMMENDATION: For value objects with complex structures, consider implementing
    /// a custom copy method that performs deep cloning if needed.
    /// 
    /// This method is useful for creating modified versions of immutable value objects
    /// in a functional programming style.
    /// </summary>
    /// <typeparam name="T">Type of value object (must be the same type or derived)</typeparam>
    /// <returns>Shallow copy of this value object</returns>
    /// <example>
    /// var newAddress = address.Copy&lt;Address&gt;();
    /// // Modify newAddress properties if needed (though value objects should be immutable)
    /// </example>
    protected T Copy<T>() where T : ValueObject => (T)MemberwiseClone();
}