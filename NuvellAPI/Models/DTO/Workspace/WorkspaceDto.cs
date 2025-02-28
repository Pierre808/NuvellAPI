using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NuvellAPI.Models.DTO.Workspace;

public class WorkspaceDto
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}