using Application.Abstraction.Mediator;
using Application.DTOs.Auth;

namespace Application.Auth.Commands.RefreshToken;

public record RefreshTokenRequest(string RefreshToken)
    : IRequest<TokenPairDto>, ITransactionalRequest;