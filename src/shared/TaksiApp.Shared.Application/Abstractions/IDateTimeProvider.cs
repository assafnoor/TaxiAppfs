// TaksiApp.Shared.Application/Abstractions/IDateTimeProvider.cs
namespace TaksiApp.Shared.Application.Abstractions;

/// <summary>
/// Abstraction for getting current time.
/// Enables time-travel testing.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Current UTC time.
    /// Always use UTC for consistency across time zones.
    /// </summary>
    DateTime UtcNow { get; }
}
