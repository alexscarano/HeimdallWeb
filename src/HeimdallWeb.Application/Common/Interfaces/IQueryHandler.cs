namespace HeimdallWeb.Application.Common.Interfaces;

/// <summary>
/// Interface for query handlers (CQRS Light pattern).
/// Queries represent read operations that don't change state.
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken = default);
}
