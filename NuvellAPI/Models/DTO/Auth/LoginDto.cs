using System.ComponentModel.DataAnnotations;

namespace NuvellAPI.Models.DTO;

public class LoginDto
{
    [Required]
    [MaxLength(50)]
    [EmailAddress]
    public string? Email { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string? Password { get; set; } = string.Empty;
}