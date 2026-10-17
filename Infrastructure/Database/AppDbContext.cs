using Application.Abstraction.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<IdempotencyRecord> IdempotencyRecords =>
        Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequestHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(x => x.ResponseJson)
                .IsRequired();

            entity.HasIndex(x => x.ExpiresAt);
        });
    }
}