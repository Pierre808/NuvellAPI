using System.Security.Claims;
using NuvellAPI.Models.Domain;

namespace NuvellAPI.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(AppUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
    string HashToken(string token);
}