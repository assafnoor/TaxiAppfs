namespace TaksiApp.Gateway.Core.Services;

internal enum CircuitBreakerState
{
    Closed,
    Open,
    HalfOpen
}