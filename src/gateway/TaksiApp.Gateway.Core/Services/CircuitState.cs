namespace TaksiApp.Gateway.Core.Services;

/// <summary>
/// Maintains the current state of a circuit breaker along with the timestamp of the last state change.
/// Thread-safe implementation using locks for state transitions.
/// </summary>
internal sealed class CircuitState
{
    private readonly object _lock = new();
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private DateTime _lastStateChange = DateTime.UtcNow;

    /// <summary>
    /// Gets the current state of the circuit breaker (thread-safe).
    /// </summary>
    public CircuitBreakerState State
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
        set
        {
            lock (_lock)
            {
                _state = value;
                _lastStateChange = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Gets the UTC timestamp when the state was last changed (thread-safe).
    /// </summary>
    public DateTime LastStateChange
    {
        get
        {
            lock (_lock)
            {
                return _lastStateChange;
            }
        }
    }

    /// <summary>
    /// Atomically updates state and timestamp together.
    /// </summary>
    public void SetState(CircuitBreakerState newState, DateTime timestamp)
    {
        lock (_lock)
        {
            _state = newState;
            _lastStateChange = timestamp;
        }
    }

    /// <summary>
    /// Gets both state and timestamp atomically.
    /// </summary>
    public (CircuitBreakerState State, DateTime LastChange) GetStateSnapshot()
    {
        lock (_lock)
        {
            return (_state, _lastStateChange);
        }
    }
}