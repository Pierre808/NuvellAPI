namespace NuvellAPI.Entities;

public class User
{
    public Guid Id { get; init; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}