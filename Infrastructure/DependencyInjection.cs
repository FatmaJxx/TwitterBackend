using System.Text;
using Application.Abstraction.Data;
using Application.Abstraction.Services;
using Infrastructure.Configuration;
using Domain.Interfaces;
using Infrastructure.Database;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddAuthenticationInternal(configuration);

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddHostedService<IdempotencyCleanupService>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register the interface using the same AppDbContext instance.
        services.AddScoped<IAppDbContext>(provider =>
            provider.GetRequiredService<AppDbContext>());
        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetSection("Jwt")
        );

        var jwtSettings = configuration.GetSection("Jwt")
                              .Get<JwtSettings>()
                          ?? throw new InvalidOperationException(
                              "JWT settings are missing or misconfigured in appsettings.json.");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.SecretKey
                        )
                    ),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        return services;
    }
}