using Application.Abstraction.Mediator;
using FluentValidation;

namespace Application.Common.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
      
        if (!validators.Any())
        {
            return await next();
        }

        var validationResult = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(request,
                cancellationToken)));

        var errors = validationResult
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToList();

        if (errors.Count != 0)
        {
            throw new ValidationException(errors);
        }
        
        return await next();
    }
}