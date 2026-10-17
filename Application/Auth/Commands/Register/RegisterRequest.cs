using Application.Abstraction.Mediator;
using Application.DTOs.Auth;

namespace Application.Auth.Commands.Register;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    DateOnly DateOfBirth
) : IRequest<TokenPairDto>, ITransactionalRequest,
    IIdempotentRequest
{
    public Guid IdempotencyKey { get; set; }
}