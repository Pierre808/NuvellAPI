using NuvellAPI.Entities;
using NuvellAPI.Models;

namespace NuvellAPI.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(UserDto request);
    Task<string?> LoginAsync(UserDto request);
}