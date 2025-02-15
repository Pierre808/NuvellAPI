using System.ComponentModel.DataAnnotations;

namespace NuvellAPI.Models;

public class RefreshTokenRequestDto
{
    [Required]
    public Guid UserId { get; set; }
    public required string RefreshToken { get; set; }
}