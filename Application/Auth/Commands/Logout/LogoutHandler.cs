using Application.Abstraction.Mediator;
using Application.Abstraction.Services;


namespace Application.Auth.Commands.Logout;

internal sealed class LogoutHandler(ITokenService tokenService)
    : IRequestHandler<LogoutRequest, Unit>
{
    public async Task<Unit> HandleAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        await tokenService.LogoutAsync(request.RefreshToken, cancellationToken);

        return default;
    }
}