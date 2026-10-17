namespace Application.DTOs.Auth;

public record TokenPairDto
{
    public required Guid UserId { get; set; }

    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }

}