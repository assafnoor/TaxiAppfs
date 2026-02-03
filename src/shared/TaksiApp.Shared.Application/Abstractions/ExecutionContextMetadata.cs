using System.Collections.Immutable;

namespace TaksiApp.Shared.Application.Abstractions;

/// <summary>
/// Strongly-typed immutable metadata container for execution context.
/// </summary>
/// <remarks>
/// <para>
/// This class provides a thread-safe, immutable way to store arbitrary metadata
/// associated with an execution context. It uses ImmutableDictionary internally
/// to ensure that metadata cannot be modified after creation.
/// </para>
/// <para>
/// <strong>IMPORTANT:</strong> This is for technical context only (debugging, correlation, diagnostics).
/// Never store business data or domain information in metadata.
/// </para>
/// <para>
/// Examples of appropriate metadata:
/// - Request IDs for correlation
/// - Client information for debugging
/// - API versions for compatibility tracking
/// - Diagnostic flags
/// </para>
/// <para>
/// Examples of inappropriate metadata (use proper domain objects instead):
/// - Business rules or decisions
/// - User preferences or settings
/// - Entity state or domain data
/// - Authorization decisions
/// </para>
/// </remarks>
public sealed class ExecutionContextMetadata
{
    private readonly ImmutableDictionary<string, object> _dictionary;

    /// <summary>
    /// Creates a new metadata instance from an optional dictionary.
    /// </summary>
    /// <param name="dictionary">Initial metadata values. If null, an empty dictionary is created.</param>
    public ExecutionContextMetadata(Dictionary<string, object>? dictionary = null)
    {
        _dictionary = (dictionary ?? new Dictionary<string, object>()).ToImmutableDictionary();
    }

    /// <summary>
    /// Creates a new metadata instance from an immutable dictionary (internal use).
    /// </summary>
    private ExecutionContextMetadata(ImmutableDictionary<string, object> dictionary)
    {
        _dictionary = dictionary;
    }

    /// <summary>
    /// Returns a read-only view of the metadata as a dictionary.
    /// </summary>
    /// <remarks>
    /// This method is provided for backward compatibility with code that expects
    /// IReadOnlyDictionary. Prefer using strongly-typed methods like GetValue and WithValue.
    /// </remarks>
    public IReadOnlyDictionary<string, object> AsDictionary() => _dictionary;

    /// <summary>
    /// Safely retrieves a value by key with type conversion.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="key">The metadata key.</param>
    /// <returns>
    /// The value if found and convertible to T; otherwise, default(T).
    /// </returns>
    /// <remarks>
    /// This method attempts to convert the stored value to the requested type.
    /// If the conversion fails, default(T) is returned (null for reference types,
    /// zero/false for value types).
    /// </remarks>
    /// <example>
    /// <code>
    /// var requestId = metadata.GetValue&lt;string&gt;("RequestId");
    /// var apiVersion = metadata.GetValue&lt;int&gt;("ApiVersion");
    /// </code>
    /// </example>
    public T? GetValue<T>(string key)
    {
        if (!_dictionary.TryGetValue(key, out var value))
            return default;

        if (value is T typedValue)
            return typedValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Returns a new metadata instance with an added or updated value.
    /// </summary>
    /// <typeparam name="T">The type of the value to store.</typeparam>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The value to store.</param>
    /// <returns>A new ExecutionContextMetadata instance with the updated value.</returns>
    /// <remarks>
    /// This method does not modify the current instance. Instead, it returns a new
    /// instance with the updated value, following immutability principles.
    /// </remarks>
    /// <example>
    /// <code>
    /// var newMetadata = metadata
    ///     .WithValue("RequestId", "req-12345")
    ///     .WithValue("ApiVersion", "v2");
    /// </code>
    /// </example>
    public ExecutionContextMetadata WithValue<T>(string key, T value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        var newDict = _dictionary.SetItem(key, value);
        return new ExecutionContextMetadata(newDict);
    }

    /// <summary>
    /// Checks if a key exists in the metadata.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

    /// <summary>
    /// Gets all metadata keys.
    /// </summary>
    public IEnumerable<string> Keys => _dictionary.Keys;

    /// <summary>
    /// Gets all metadata values.
    /// </summary>
    public IEnumerable<object> Values => _dictionary.Values;

    /// <summary>
    /// Gets the number of metadata entries.
    /// </summary>
    public int Count => _dictionary.Count;
}