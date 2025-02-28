using System.ComponentModel.DataAnnotations;

namespace NuvellAPI.Models.Domain;

public class Workspace
{
    public Guid Id { get; set; }
    [MaxLength(50)]
    public required string Name { get; set; }
    public required Guid UserId { get; set; }
    public required AppUser User { get; set; }
}