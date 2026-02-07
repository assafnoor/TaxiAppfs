namespace TaksiApp.Gateway.Core.Services;



/// <summary>
/// Maintains the current state of a circuit breaker along with the timestamp of the last state change.
/// </summary>
internal sealed class CircuitState
{
    /// <summary>
    /// Gets or sets the current state of the circuit breaker.
    /// </summary>
    public CircuitBreakerState State { get; set; } = CircuitBreakerState.Closed;

    /// <summary>
    /// Gets or sets the UTC timestamp when the state was last changed.
    /// </summary>
    public DateTime LastStateChange { get; set; } = DateTime.UtcNow;
}
