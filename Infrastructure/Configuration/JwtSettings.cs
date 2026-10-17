namespace Infrastructure.Configuration;

public record JwtSettings
{
    public required string SecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required TimeSpan AccessTokenExpiration { get; set; }
    public required TimeSpan RefreshTokenExpiration { get; set; }
}