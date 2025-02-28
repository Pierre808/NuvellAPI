using System.Text.Json.Serialization;

namespace NuvellAPI.Models.DTO;

public class UserResponseDto
{   
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
}