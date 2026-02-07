// TaksiApp.Auth.Domain/Exceptions/DomainException.cs
namespace TaksiApp.Auth.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-level exceptions in the Authentication Service.
/// Domain exceptions represent violations of business rules and invariants.
/// </summary>
/// <remarks>
/// <para>
/// Domain exceptions should be thrown when:
/// - Business rules are violated
/// - Aggregate invariants are broken
/// - Invalid state transitions are attempted
/// - Domain operations cannot be completed due to business constraints
/// </para>
/// <para>
/// These exceptions should be caught at the application layer and translated
/// into appropriate responses (e.g., HTTP 400/422 for validation errors).
/// </para>
/// </remarks>
public class DomainException : Exception
{
    /// <summary>
    /// A unique error code identifying the type of domain error.
    /// Useful for client-side error handling and localization.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Additional context about the error, if available.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; }

    /// <summary>
    /// Creates a new domain exception with the specified error code and message.
    /// </summary>
    /// <param name="errorCode">Unique error code for this exception type</param>
    /// <param name="message">Human-readable error message</param>
    public DomainException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
    }

    /// <summary>
    /// Creates a new domain exception with the specified error code, message, and inner exception.
    /// </summary>
    /// <param name="errorCode">Unique error code for this exception type</param>
    /// <param name="message">Human-readable error message</param>
    /// <param name="innerException">The exception that caused this exception</param>
    public DomainException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
    }

    /// <summary>
    /// Creates a new domain exception with metadata for additional context.
    /// </summary>
    /// <param name="errorCode">Unique error code for this exception type</param>
    /// <param name="message">Human-readable error message</param>
    /// <param name="metadata">Additional context about the error</param>
    public DomainException(string errorCode, string message, IReadOnlyDictionary<string, object>? metadata)
        : base(message)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Metadata = metadata;
    }
}
