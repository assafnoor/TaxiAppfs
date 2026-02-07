namespace TaksiApp.Gateway.Core.Services;

internal sealed class CircuitState
{
    public CircuitBreakerState State { get; set; } = CircuitBreakerState.Closed;
    public DateTime LastStateChange { get; set; } = DateTime.UtcNow;
}
