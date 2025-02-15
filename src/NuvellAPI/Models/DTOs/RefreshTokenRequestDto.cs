using System.ComponentModel.DataAnnotations;

namespace NuvellAPI.Models.DTOs;

public class RefreshTokenRequestDto
{
    [Required]
    public Guid UserId { get; set; }
    public required string RefreshToken { get; set; }
}