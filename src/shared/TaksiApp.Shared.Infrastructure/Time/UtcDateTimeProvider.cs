using TaksiApp.Shared.Application.Abstractions;

namespace TaksiApp.Shared.Infrastructure.Time;

public sealed class UtcDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
