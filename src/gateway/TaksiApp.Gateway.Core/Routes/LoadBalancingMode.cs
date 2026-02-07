namespace TaksiApp.Gateway.Core.Routes;

/// <summary>
/// Defines the supported load balancing strategies.
/// </summary>
public enum LoadBalancingMode
{
    /// <summary>
    /// Distribute requests in round-robin order.
    /// </summary>
    RoundRobin,

    /// <summary>
    /// Select the destination with the least number of active connections.
    /// </summary>
    LeastConnections,

    /// <summary>
    /// Select a random destination.
    /// </summary>
    Random,

    /// <summary>
    /// Round-robin weighted by configured weights (not implemented yet).
    /// </summary>
    WeightedRoundRobin,

    /// <summary>
    /// Select two random destinations and choose the one with fewer connections.
    /// </summary>
    PowerOfTwoChoices
}