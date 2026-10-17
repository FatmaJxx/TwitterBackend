using Application.Abstraction.Mediator;
using Application.DTOs.Auth;

namespace Application.Auth.Commands.Login;

public record LoginRequest(string Email, string Password)
    : IRequest<TokenPairDto>, ITransactionalRequest,
        IIdempotentRequest
{
    public Guid IdempotencyKey { get; set; }
}