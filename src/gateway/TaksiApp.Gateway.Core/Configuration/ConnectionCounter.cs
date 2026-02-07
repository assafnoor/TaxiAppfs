namespace TaksiApp.Gateway.Core.Configuration;

/// <summary>
/// Thread-safe counter used to track active connections per destination.
/// Designed for concurrent environments such as load balancing and gateways.
/// </summary>
public sealed class ConnectionCounter
{
    private int _value;

    /// <summary>
    /// Gets the current counter value in a thread-safe manner.
    /// </summary>
    public int Value => Volatile.Read(ref _value);

    /// <summary>
    /// Atomically increments the counter by one.
    /// </summary>
    public void Increment()
    {
        Interlocked.Increment(ref _value);
    }

    /// <summary>
    /// Atomically decrements the counter by one.
    /// Ensures the counter never goes below zero.
    /// </summary>
    public void Decrement()
    {
        var newValue = Interlocked.Decrement(ref _value);

        if (newValue < 0)
        {
            // Defensive correction to avoid negative counts
            Interlocked.Exchange(ref _value, 0);
        }
    }
}
