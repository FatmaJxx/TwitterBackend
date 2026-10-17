using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Abstraction.Services;
using Infrastructure.Configuration;
using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

internal sealed class TokenService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<JwtSettings> options)
    : ITokenService

{
    private readonly JwtSettings _jwtSettings = options.Value;

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (!await ValidateRefreshTokenAsync(refreshToken))
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var userId = GetUserIdFromToken(refreshToken);

        var tokenHash = HashToken(refreshToken);

        var storedToken = await refreshTokenRepository.GetByUserIdAndTokenAsync(userId, tokenHash, cancellationToken) ??
                          throw new UnauthorizedAccessException(
                              "Invalid refresh token.");

        refreshTokenRepository.Delete(storedToken);
    }

    public async Task<TokenPairDto> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // Step 1: Validate that the provided token is a refresh token
        if (!await ValidateRefreshTokenAsync(refreshToken))
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        // Step 2: Extract the userId from the token
        var userId = GetUserIdFromToken(refreshToken);

        // Step 3: Retrieve the user from the database
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
                   ?? throw new UnauthorizedAccessException("User doesn't exist.");

        // Step 4: Hash the refresh token for secure comparison
        var hashed = HashToken(refreshToken);

        // Step 5: Verify that the hashed token exists in the DB and belongs to the user
        var storedToken = await refreshTokenRepository.GetByUserIdAndTokenAsync(user.Id, hashed, cancellationToken) ??
                          throw new UnauthorizedAccessException(
                              "Refresh token not recognized (maybe used or expired?).");

        // Step 6: Delete the old refresh token
        refreshTokenRepository.Delete(storedToken);

        // Step 7: Generate new session tokens
        return await GenerateTokenPairAsync(user.Id, cancellationToken);
    }

    private static Guid GetUserIdFromToken(string refreshToken)
    {
        var handler = new JsonWebTokenHandler();

        var token = handler.ReadJsonWebToken(refreshToken);

        var userId = token.Claims
            .First(x => x.Type == JwtRegisteredClaimNames.Sub)
            .Value;

        return Guid.Parse(userId);
    }

    private async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
    {
        var handler = new JsonWebTokenHandler();


        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)
            ),

            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };

        var result = await handler.ValidateTokenAsync(refreshToken, validationParameters);

        if (!result.IsValid)
        {
            return false;
        }

        var type = result.ClaimsIdentity.FindFirst("type")?.Value;

        if (type != "RefreshToken")
        {
            return false;
        }

        var userIdValue = result.ClaimsIdentity
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(userIdValue, out _);
    }

    public async Task<TokenPairDto> GenerateTokenPairAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var existingToken = await refreshTokenRepository.GetByUserIdAsync(userId, cancellationToken);

        if (existingToken != null)
        {
            refreshTokenRepository.Delete(existingToken);
        }

        var accessToken = GenerateAccessToken(userId);
        var refreshToken = GenerateRefreshToken(userId);

        var refreshEntity = new RefreshToken
        {
            UserId = userId,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.Add(
                _jwtSettings.RefreshTokenExpiration)
        };

        refreshTokenRepository.Add(refreshEntity);

        return new TokenPairDto
        {
            UserId = userId,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private string GenerateToken(Guid userId, string type, TimeSpan expiry)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                [
                    new Claim(
                        JwtRegisteredClaimNames.Sub,
                        userId.ToString()
                    ),
                    new Claim("type", type),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
                ]
            ),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.Add(expiry)
        };


        var handler = new JsonWebTokenHandler();

        string token = handler.CreateToken(tokenDescriptor);

        return token;
    }

    private string GenerateAccessToken(Guid userId)
    {
        return GenerateToken(userId, "AccessToken", _jwtSettings.AccessTokenExpiration);
    }

    private string GenerateRefreshToken(Guid userId)
    {
        return GenerateToken(userId, "RefreshToken", _jwtSettings.RefreshTokenExpiration);
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(token)
        );

        return Convert.ToBase64String(hash);
    }
}