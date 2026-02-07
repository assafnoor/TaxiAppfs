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
    /// <summary>
    /// Atomically decrements the counter by one.
    /// Ensures the counter never goes below zero using Compare-And-Swap (CAS).
    /// </summary>
    public void Decrement()
    {
        int current, newValue;
        do
        {
            current = Volatile.Read(ref _value);

            // Don't decrement if already at zero
            if (current <= 0)
            {
                // Defensively set to zero if somehow negative
                if (current < 0)
                    Interlocked.Exchange(ref _value, 0);
                return;
            }

            newValue = current - 1;

        } while (Interlocked.CompareExchange(ref _value, newValue, current) != current);
    }
}
