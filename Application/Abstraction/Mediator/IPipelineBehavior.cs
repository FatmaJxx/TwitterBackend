namespace Application.Abstraction.Mediator;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next);
}