namespace TaksiApp.Gateway.Core.Services;

/// <summary>
/// Represents the possible states of a circuit breaker.
/// </summary>
internal enum CircuitBreakerState
{
    /// <summary>
    /// Circuit is closed: requests flow normally.
    /// </summary>
    Closed,

    /// <summary>
    /// Circuit is open: requests are blocked to prevent further failures.
    /// </summary>
    Open,

    /// <summary>
    /// Circuit is half-open: a limited number of requests are allowed to test recovery.
    /// </summary>
    HalfOpen
}