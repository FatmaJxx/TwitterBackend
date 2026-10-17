using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class UserRepository(AppDbContext dbContext) : Repository<User>(dbContext), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken
            );
    }
}