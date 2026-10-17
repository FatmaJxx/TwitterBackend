using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Index(nameof(Email), IsUnique = true)]
public class User : IBaseEntity
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }
}