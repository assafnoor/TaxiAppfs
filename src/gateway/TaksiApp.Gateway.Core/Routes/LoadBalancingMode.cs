namespace TaksiApp.Gateway.Core.Routes;

/// <summary>
/// Load balancing strategies
/// </summary>
public enum LoadBalancingMode
{
    RoundRobin,
    LeastConnections,
    Random,
    WeightedRoundRobin,
    PowerOfTwoChoices
}