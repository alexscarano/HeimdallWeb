namespace HeimdallWeb.Application.Common.Interfaces;

/// <summary>
/// Interface for command handlers (CQRS Light pattern).
/// Commands represent actions that change state.
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for command handlers without a response (CQRS Light pattern).
/// Commands represent actions that change state.
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
public interface ICommandHandler<in TCommand>
{
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}