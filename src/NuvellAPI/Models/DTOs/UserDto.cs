using System.ComponentModel.DataAnnotations;

namespace NuvellAPI.Models.DTOs;

public class UserDto
{
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}