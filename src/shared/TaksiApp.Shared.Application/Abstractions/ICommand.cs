using MediatR;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Application.Abstractions;
/// <summary>
/// Marker interface for commands (write operations).
/// Commands change system state and return Result.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Command that returns a typed result.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}