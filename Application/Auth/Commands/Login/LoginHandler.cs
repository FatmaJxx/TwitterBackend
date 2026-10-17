using Application.Abstraction.Mediator;
using Application.Abstraction.Services;
using Application.DTOs.Auth;
using Domain.Interfaces;

namespace Application.Auth.Commands.Login;

internal sealed class LoginHandler(
    IUserRepository userRepository,
    ITokenService tokenService
) : IRequestHandler<LoginRequest, TokenPairDto>
{
    public async Task<TokenPairDto> HandleAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
                   ?? throw new UnauthorizedAccessException("Invalid email or password.");

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var tokens = await tokenService.GenerateTokenPairAsync(user.Id, cancellationToken);

        return tokens;
    }
}