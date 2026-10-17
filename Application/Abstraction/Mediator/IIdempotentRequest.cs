namespace Application.Abstraction.Mediator;

public interface IIdempotentRequest
{
    Guid IdempotencyKey { get; }
}