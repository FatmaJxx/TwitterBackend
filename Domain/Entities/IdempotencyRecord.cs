namespace Domain.Entities;

public sealed class IdempotencyRecord : IBaseEntity
{ 
    public Guid Id { get; set; }
    public required string RequestHash { get; set; }
    public required string ResponseJson { get; set; }
    public DateTime ExpiresAt { get; set; }
}