using NuvellAPI.Models;
using NuvellAPI.Models.Domain;
using NuvellAPI.Models.DTOs;

namespace NuvellAPI.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(UserDto request);
    Task<TokenResponseDto?> LoginAsync(UserDto request);
    Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
}