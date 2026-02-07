using TaksiApp.Gateway.Core.Routes;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Gateway.Core.Services;

/// <summary>
/// Handles load balancing across multiple destinations
/// </summary>
public interface ILoadBalancer
{
    /// <summary>
    /// Select the best destination for a route
    /// </summary>
    Result<string> SelectDestination(SmartRoute route);

    /// <summary>
    /// Record request completion for load balancing metrics
    /// </summary>
    void RecordCompletion(string destination);
}
