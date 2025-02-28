using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuvellAPI.Data;
using NuvellAPI.Models.Common;
using NuvellAPI.Models.Domain;
using NuvellAPI.Models.DTO;

namespace NuvellAPI.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    TokenService tokenService,
    AppDbContext context)
{
    public async Task<Result<string>> RegisterUserAsync(RegisterDto registerDto)
    {
        if (await userManager.FindByEmailAsync(registerDto.Email!) != null)
            return Result<string>.Failure("Email already exists");
        
        var appUser = new AppUser
        {
            Email = registerDto.Email,
            UserName = registerDto.Email
        };
        
        var result = await userManager.CreateAsync(appUser, registerDto.Password!);
        if (!result.Succeeded)
        {
            return Result<string>.Failure(string.Join(", ", result.Errors));    
        }
        
        var roleResult = await userManager.AddToRoleAsync(appUser, "User");
        if (!roleResult.Succeeded)
            return Result<string>.Failure(string.Join(", ", roleResult.Errors.Select(e => e.Description)));

        return Result<string>.SuccessResult(registerDto.Email!);
    }

    public async Task<Result<UserResponseDto>> LoginUserAsync(LoginDto loginDto)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == loginDto.Email!.ToUpper());
        if (user == null)
        {
            return Result<UserResponseDto>.Failure("Invalid email or password");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password!, true);
        if (!result.Succeeded)
        {
            return Result<UserResponseDto>.Failure("Invalid email or password");
        }

        var accessToken = await tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        var hashedRefreshToken = tokenService.HashToken(refreshToken);

        var refreshTokenDb = await context.UserRefreshTokens.FirstOrDefaultAsync(x => x.UserId == user.Id);
        // if refresh-token is null for this user create a new one
        if (refreshTokenDb == null)
        {
            context.UserRefreshTokens.Add(new UserRefreshTokens
            {
                UserId = user.Id,
                RefreshToken = hashedRefreshToken,
                Expiration = DateTime.UtcNow.AddDays(7),
                IsActive = true,
            });
        }
        // else update refresh-token for this user
        else
        {
            refreshTokenDb.RefreshToken = hashedRefreshToken;
            refreshTokenDb.Expiration = DateTime.UtcNow.AddDays(7);
            refreshTokenDb.IsActive = true;
        }

        await context.SaveChangesAsync();

        var response = new UserResponseDto
        {
            Email = user.Email!,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
        
        return Result<UserResponseDto>.SuccessResult(response);
    }

    public async Task<Result<UserResponseDto>> RefreshTokenAsync(UserResponseDto userResponseDto)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(userResponseDto.AccessToken);
        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (email == null)
        {
            return Result<UserResponseDto>.Failure("Invalid refresh token");
        }

        if (email != userResponseDto.Email)
        {
            return Result<UserResponseDto>.Failure("Invalid refresh token");
        }
            
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result<UserResponseDto>.Failure("Invalid refresh token");
        }
            
        var hashedRefreshToken = tokenService.HashToken(userResponseDto.RefreshToken);
            
        var refreshTokenDb = await context.UserRefreshTokens.FirstOrDefaultAsync(x => x.UserId == user.Id 
            && x.RefreshToken == hashedRefreshToken && x.IsActive == true);
        if (refreshTokenDb == null || 
            refreshTokenDb.RefreshToken != hashedRefreshToken ||
            refreshTokenDb.UserId != user.Id ||
            refreshTokenDb.Expiration < DateTime.UtcNow
            || refreshTokenDb.IsActive == false)
        {
            return Result<UserResponseDto>.Failure("Invalid refresh token");
        }
            
        context.UserRefreshTokens.Remove(refreshTokenDb);
            
        var newAccessToken = await tokenService.GenerateAccessTokenAsync(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        context.UserRefreshTokens.Add(new UserRefreshTokens
        {
            UserId = user.Id,
            RefreshToken = tokenService.HashToken(newRefreshToken),
            Expiration = DateTime.UtcNow.AddDays(7),
            IsActive = true,
        });
        await context.SaveChangesAsync();

        var response = new UserResponseDto
        {
            Email = user.Email!,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
        return Result<UserResponseDto>.SuccessResult(response);
    }
}