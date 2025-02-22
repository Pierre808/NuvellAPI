using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuvellAPI.Data;
using NuvellAPI.Interfaces;
using NuvellAPI.Migrations;
using NuvellAPI.Models.Domain;
using NuvellAPI.Models.DTO;

namespace NuvellAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _context;

    public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appUser = new AppUser
            {
                Email = request.Email,
                UserName = request.Email
            };
            
            var createdUser = await _userManager.CreateAsync(appUser, request.Password!);
            if (!createdUser.Succeeded)
            {
                return BadRequest(createdUser.Errors);
            }
            
            var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            return Ok(appUser.Email);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == request.Email!.ToUpper());
            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, true);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid email or password");
            }

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var hashedRefreshToken = _tokenService.HashToken(refreshToken);

            var refreshTokenDb = await _context.UserRefreshTokens.FirstOrDefaultAsync(x => x.UserId == user.Id);
            // if refresh-token is null for this user create a new one
            if (refreshTokenDb == null)
            {
                _context.UserRefreshTokens.Add(new UserRefreshTokens
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

            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                Email = user.Email!,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("token/refresh")]
    public async Task<IActionResult> Refresh(UserResponseDto request)
    {
        try
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (email == null)
            {
                return BadRequest("Invalid refresh token");
            }
            
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("Invalid refresh token");
            }
            
            var hashedRefreshToken = _tokenService.HashToken(request.RefreshToken);
            
            var refreshTokenDb = await _context.UserRefreshTokens.FirstOrDefaultAsync(x => x.UserId == user.Id 
                && x.RefreshToken == hashedRefreshToken && x.IsActive == true);
            if (refreshTokenDb == null || 
                refreshTokenDb.RefreshToken != hashedRefreshToken ||
                refreshTokenDb.UserId != user.Id ||
                refreshTokenDb.Expiration < DateTime.UtcNow
                || refreshTokenDb.IsActive == false)
            {
                return BadRequest("Invalid refresh token");
            }
            
            _context.UserRefreshTokens.Remove(refreshTokenDb);
            
            var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            _context.UserRefreshTokens.Add(new UserRefreshTokens
            {
                UserId = user.Id,
                RefreshToken = _tokenService.HashToken(newRefreshToken),
                Expiration = DateTime.UtcNow.AddDays(7),
                IsActive = true,
            });
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                Email = user.Email!,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("authenticated")]
    public async Task<IActionResult> AuthenticatedOnlyEndpoint()
    {
        return await Task.FromResult(Ok("You are authenticated."));
    }
}