using System.ComponentModel.DataAnnotations;

namespace NuvellAPI.Models.DTO;

public class RegisterDto
{
    [Required]
    [MaxLength(50)]
    [EmailAddress]
    public string? Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    [MaxLength(50)]
    public string? Password { get; set; } = string.Empty;
}