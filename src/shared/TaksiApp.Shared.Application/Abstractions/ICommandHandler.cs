using MediatR;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Application.Abstractions;

/// <summary>
/// Defines a handler for processing command requests that do not return a response,
/// following the CQRS pattern.
/// </summary>
/// <remarks>
/// <para>
/// A command handler represents an operation that changes system state.
/// It executes a command and returns a <see cref="Result"/> indicating
/// whether the operation succeeded or failed.
/// </para>
/// <para>
/// Commands handled by this interface must not return data. For commands that
/// produce a response, use <see cref="ICommandHandler{TCommand, TResponse}"/>.
/// </para>
/// </remarks>
/// <typeparam name="TCommand">
/// The type of the command being handled. Must implement <see cref="ICommand"/>.
/// </typeparam>
public interface ICommandHandler<in TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Defines a handler for processing command requests that return a response,
/// following the CQRS pattern.
/// </summary>
/// <remarks>
/// <para>
/// A command handler represents an operation that changes system state and
/// returns a response wrapped in a <see cref="Result{TResponse}"/>.
/// </para>
/// <para>
/// This interface is intended for commands that need to return data such as
/// identifiers, summaries, or operation results.
/// </para>
/// </remarks>
/// <typeparam name="TCommand">
/// The type of the command being handled. Must implement <see cref="ICommand{TResponse}"/>.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of the response returned when the command is successfully processed.
/// </typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
