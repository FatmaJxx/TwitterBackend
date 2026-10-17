using Application.DTOs.Auth;

namespace Application.Abstraction.Services;

public interface ITokenService
{
    Task<TokenPairDto> GenerateTokenPairAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<TokenPairDto> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}