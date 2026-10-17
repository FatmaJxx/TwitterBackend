using Domain.Entities;

namespace Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByUserIdAndTokenAsync(Guid userId, string token,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<RefreshToken>> GetAllAsync(
        CancellationToken cancellationToken = default);

    RefreshToken Add(RefreshToken refreshToken);

    RefreshToken Update(RefreshToken refreshToken);

    void Delete(RefreshToken refreshToken);
}