using MediatR;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Application.Abstractions;

/// <summary>
/// Defines a handler for processing query requests following the CQRS pattern.
/// </summary>
/// <remarks>
/// <para>
/// A query handler is responsible for handling read-only operations and must not
/// perform any state changes. It processes a query request and returns a
/// <see cref="Result{TResponse}"/> containing the requested data or an error.
/// </para>
/// <para>
/// This abstraction integrates with MediatR and enforces a consistent result
/// envelope across the application layer.
/// </para>
/// </remarks>
/// <typeparam name="TQuery">
/// The type of the query being handled. Must implement <see cref="IQuery{TResponse}"/>.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of the response returned when the query is successfully processed.
/// </typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
