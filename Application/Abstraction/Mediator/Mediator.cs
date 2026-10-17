using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Abstraction.Mediator;

internal sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly ConcurrentDictionary<Type, object> _handlerInvokerCache = new();

    public async Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var handlerInvoker =
            (Func<IRequest<TResponse>, IServiceProvider, CancellationToken,
                Task<TResponse>>)
            _handlerInvokerCache.GetOrAdd(requestType, CreateHandlerInvoker<TResponse>);
        
        return await ExecutePipeline(requestType, request, cancellationToken, handlerInvoker);
    }

    private async Task<TResponse> ExecutePipeline<TResponse>(
        Type requestType,
        IRequest<TResponse> request,
        CancellationToken cancellationToken,
        Func<IRequest<TResponse>,
            IServiceProvider,
            CancellationToken,
            Task<TResponse>> handlerInvoker)

    {
       
        var behaviorType = typeof(IPipelineBehavior<,>)
            .MakeGenericType(requestType, typeof(TResponse));

        var behaviors = serviceProvider
            .GetServices(behaviorType)
            .Cast<object>()
            .Reverse()
            .ToList();

        RequestHandlerDelegate<TResponse> next = () =>
            handlerInvoker(
                request,
                serviceProvider,
                cancellationToken);

        foreach (var behavior in behaviors)
        {
            var currentNext = next;

            next = () =>
            {
                var method = behavior
                                 .GetType().GetMethod("HandleAsync")
                             ?? throw new InvalidOperationException(
                                 $"HandleAsync not found on {behavior.GetType().Name}.");

                return (Task<TResponse>)method.Invoke(
                    behavior,
                    [request, cancellationToken, currentNext])!;
            };
        }

        return await next();
    }

    private Func<
            IRequest<TResponse>,
            IServiceProvider,
            CancellationToken,
            Task<TResponse>>
        CreateHandlerInvoker<TResponse>(Type requestType)
    {
        
        var responseType = typeof(TResponse);
        
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, responseType);

        var handleMethod = handlerType.GetMethod("HandleAsync")
                           ?? throw new InvalidOperationException(
                               $"HandleAsync method not found for {requestType.Name}.");

        return (request, handlerServiceProvider, cancellationToken) =>
        {
            var handler = handlerServiceProvider.GetService(handlerType)
                          ?? throw new InvalidOperationException(
                              $"No handler registered for request type {requestType.Name}.");
            
            var resultTask = (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;

            return resultTask;
        };
    }
}
