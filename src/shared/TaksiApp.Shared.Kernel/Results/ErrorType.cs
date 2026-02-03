namespace TaksiApp.Shared.Kernel.Results;

/// <summary>
/// Represents the category of an error.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ErrorType"/> is used to classify errors in a consistent way
/// across the application and infrastructure layers.
/// </para>
/// <para>
/// Error types are commonly mapped to HTTP status codes
/// at the API boundary (e.g., Validation → 400, Unauthorized → 401).
/// </para>
/// </remarks>
public enum ErrorType
{
    /// <summary>
    /// Indicates that the error was caused by invalid input or validation failure.
    /// </summary>
    Validation,

    /// <summary>
    /// Indicates that a requested resource could not be found.
    /// </summary>
    NotFound,

    /// <summary>
    /// Indicates a conflict with the current state of the system.
    /// </summary>
    Conflict,

    /// <summary>
    /// Indicates an unexpected technical failure.
    /// </summary>
    Failure,

    /// <summary>
    /// Indicates that authentication is required or has failed.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Indicates that the caller is authenticated but lacks sufficient permissions.
    /// </summary>
    Forbidden
}
