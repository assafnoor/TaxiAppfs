using System.Collections.ObjectModel;

namespace TaksiApp.Shared.Kernel.Results;

/// <summary>
/// Represents an error in the Result pattern.
/// </summary>
/// <remarks>
/// <para>
/// Errors are immutable and serialization-safe. They provide structured information
/// about failures in a type-safe way, avoiding exception-driven control flow.
/// </para>
/// <para>
/// <strong>Design principle:</strong> Errors should represent business rule violations
/// or technical failures, not exceptions. Use Result&lt;T&gt; to return errors as
/// first-class values instead of throwing exceptions.
/// </para>
/// <para>
/// Metadata is intentionally limited to string values for safe serialization across
/// service boundaries. If you need complex error context, consider creating specific
/// error types instead of relying on metadata.
/// </para>
/// </remarks>
public readonly struct Error : IEquatable<Error>
{
    /// <summary>
    /// Gets the error code (should be unique and machine-readable).
    /// </summary>
    /// <remarks>
    /// Error codes should follow a consistent naming convention:
    /// - Domain.Operation.Reason (e.g., "Order.Create.InvalidQuantity")
    /// - Category.Reason (e.g., "Validation.Required", "NotFound.Customer")
    /// 
    /// Avoid generic codes like "Error", "Failed", "Invalid".
    /// </remarks>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    /// <remarks>
    /// Messages should be clear and actionable. Avoid technical jargon
    /// unless the audience is developers.
    /// </remarks>
    public string Message { get; }

    /// <summary>
    /// Gets the error type (determines HTTP status code mapping in APIs).
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Gets optional metadata providing additional error context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Metadata is limited to string values for safe serialization.
    /// Use metadata for debugging information, not for business logic decisions.
    /// </para>
    /// <para>
    /// Examples of good metadata:
    /// - Field names for validation errors
    /// - Entity IDs for not found errors
    /// - Attempted values for validation errors
    /// </para>
    /// <para>
    /// Examples of bad metadata (use specific error types instead):
    /// - Complex objects
    /// - Business rules
    /// - Large data structures
    /// </para>
    /// </remarks>
    public IReadOnlyDictionary<string, string>? Metadata { get; }

    private Error(
        string code,
        string message,
        ErrorType type,
        Dictionary<string, string>? metadata = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Type = type;
        Metadata = metadata != null
            ? new ReadOnlyDictionary<string, string>(metadata)
            : null;
    }

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    /// <param name="code">Error code (e.g., "Validation.Required").</param>
    /// <param name="message">Human-readable error message.</param>
    /// <param name="metadata">Optional metadata (e.g., field names, attempted values).</param>
    /// <returns>A validation error.</returns>
    /// <remarks>
    /// Validation errors typically map to HTTP 400 Bad Request.
    /// Use for client input validation failures.
    /// </remarks>
    public static Error Validation(
        string code,
        string message,
        Dictionary<string, string>? metadata = null) =>
        new(code, message, ErrorType.Validation, metadata);

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    /// <param name="code">Error code (e.g., "NotFound.Customer").</param>
    /// <param name="message">Human-readable error message.</param>
    /// <returns>A not found error.</returns>
    /// <remarks>
    /// Not found errors typically map to HTTP 404 Not Found.
    /// Use when a requested resource doesn't exist.
    /// </remarks>
    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    /// <param name="code">Error code (e.g., "Conflict.DuplicateEmail").</param>
    /// <param name="message">Human-readable error message.</param>
    /// <returns>A conflict error.</returns>
    /// <remarks>
    /// Conflict errors typically map to HTTP 409 Conflict.
    /// Use for business rule violations or duplicate resource errors.
    /// </remarks>
    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    /// <summary>
    /// Creates a general failure error.
    /// </summary>
    /// <param name="code">Error code (e.g., "Failure.DatabaseTimeout").</param>
    /// <param name="message">Human-readable error message.</param>
    /// <returns>A failure error.</returns>
    /// <remarks>
    /// Failure errors typically map to HTTP 500 Internal Server Error.
    /// Use for unexpected technical failures.
    /// </remarks>
    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    /// <param name="code">Error code (e.g., "Unauthorized.InvalidToken").</param>
    /// <param name="message">Human-readable error message.</param>
    /// <returns>An unauthorized error.</returns>
    /// <remarks>
    /// Unauthorized errors typically map to HTTP 401 Unauthorized.
    /// Use when authentication is required but missing or invalid.
    /// </remarks>
    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    /// <param name="code">Error code (e.g., "Forbidden.InsufficientPermissions").</param>
    /// <param name="message">Human-readable error message.</param>
    /// <returns>A forbidden error.</returns>
    /// <remarks>
    /// Forbidden errors typically map to HTTP 403 Forbidden.
    /// Use when user is authenticated but lacks required permissions.
    /// </remarks>
    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    /// <summary>
    /// Represents the absence of an error (success state).
    /// </summary>
    /// <remarks>
    /// This is rarely needed in practice. Prefer using Result.Success() instead.
    /// </remarks>
    public static Error None => new("Error.None", "No error", ErrorType.Failure);

    /// <summary>
    /// Represents a null value error (use for guard clauses).
    /// </summary>
    public static Error NullValue =>
        new("Error.NullValue", "A required value was null", ErrorType.Validation);

    /// <summary>
    /// Determines whether two errors are equal based on code and type.
    /// </summary>
    /// <remarks>
    /// Errors are considered equal if they have the same code and type,
    /// regardless of message or metadata. This allows for error comparison
    /// in tests and business logic.
    /// </remarks>
    public bool Equals(Error other) =>
        Code == other.Code && Type == other.Type;

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is Error error && Equals(error);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Code, Type);

    /// <summary>
    /// Determines whether two errors are equal.
    /// </summary>
    public static bool operator ==(Error left, Error right) => left.Equals(right);

    /// <summary>
    /// Determines whether two errors are not equal.
    /// </summary>
    public static bool operator !=(Error left, Error right) => !(left == right);

    /// <summary>
    /// Returns a string representation of the error.
    /// </summary>
    /// <returns>A formatted string containing error type, code, and message.</returns>
    public override string ToString() => $"[{Type}] {Code}: {Message}";
}