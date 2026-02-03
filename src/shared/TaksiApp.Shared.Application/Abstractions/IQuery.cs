using MediatR;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Application.Abstractions;
/// <summary>
/// Marker interface for queries (read operations).
/// Queries don't change state and return Result<T>.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
