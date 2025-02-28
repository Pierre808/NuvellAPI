using System.ComponentModel.DataAnnotations;

namespace NuvellAPI.Models.DTO.Workspace;

public class CreateWorkspaceDto
{
    [Required] 
    [MaxLength(50)] 
    public string? Name { get; set; } = string.Empty;
}