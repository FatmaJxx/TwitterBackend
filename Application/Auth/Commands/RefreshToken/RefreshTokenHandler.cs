using Application.Abstraction.Mediator;
using Application.Abstraction.Services;
using Application.DTOs.Auth;

namespace Application.Auth.Commands.RefreshToken;

internal sealed class RefreshTokenHandler(ITokenService tokenService)
    : IRequestHandler<RefreshTokenRequest, TokenPairDto>
{
    public async Task<TokenPairDto> HandleAsync(RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenPair = await tokenService.RefreshAsync(request.RefreshToken, cancellationToken);

        return tokenPair;
    }
}