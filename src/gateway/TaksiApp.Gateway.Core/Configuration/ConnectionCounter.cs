namespace TaksiApp.Gateway.Core.Configuration;

public sealed class ConnectionCounter
{
    private int _value;

    public int Value => Volatile.Read(ref _value);

    public void Increment()
        => Interlocked.Increment(ref _value);

    public void Decrement()
        => Interlocked.Decrement(ref _value);
}
