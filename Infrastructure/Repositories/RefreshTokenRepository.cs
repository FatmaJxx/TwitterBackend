using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class RefreshTokenRepository(AppDbContext dbContext)
    : Repository<RefreshToken>(dbContext), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByUserIdAndTokenAsync(Guid userId, string token,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == token && x.UserId == userId, cancellationToken);
    }

    public async Task<RefreshToken?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }
}