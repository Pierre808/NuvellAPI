using System.ComponentModel.DataAnnotations;

namespace NuvellAPI.Models.Domain;

public class UserRefreshTokens
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    [MaxLength(256)]
    public required string RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
    public bool IsActive { get; set; }
}