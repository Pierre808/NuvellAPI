using NuvellAPI.Models.Common;
using NuvellAPI.Models.DTO;

namespace NuvellAPI.Interfaces;

public interface IAuthService
{   
    Task<Result<string>> RegisterUserAsync(RegisterDto registerDto);
    Task<Result<UserResponseDto>> LoginUserAsync(LoginDto loginDto);
    Task<Result<UserResponseDto>> RefreshTokenAsync(UserResponseDto userResponseDto);
}