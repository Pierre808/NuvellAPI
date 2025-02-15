using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuvellAPI.Data;
using NuvellAPI.Models;
using NuvellAPI.Models.Domain;
using NuvellAPI.Models.DTOs;

namespace NuvellAPI.Services;

/// <summary>
/// Provides methods for user registration, login, and JWT token creation.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="configuration">The configuration settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Token"]!));
    }
    
    /// <summary>
    /// Registers a new user with the specified credentials.
    /// </summary>
    /// <param name="request">The DTO containing the user's registration information.</param>
    /// <returns>A <see cref="User"/> object if registration was successful; otherwise, null.</returns>
    public async Task<User?> RegisterAsync(UserDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return null; // e-mail already registered
        }
        
        var user = new User();
        var hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, request.Password);
            
        user.Email = request.Email;
        user.PasswordHash = hashedPassword;
            
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return user;
    }

    /// <summary>
    /// Logs in a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The DTO containing the user's login information.</param>
    /// <returns>A JWT token as a string if login was successful; otherwise, null.</returns>
    public async Task<TokenResponseDto?> LoginAsync(UserDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return null; //user does not exist
        }
        
        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
            return null; //wrong password
        }

        var response = new TokenResponseDto
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSafeRefreshTokenAsync(user),
        };
        return response;
    }

    public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
    {
        var user = await ValidateRefreshTokenAsync(request);
        if(user is null) return null;

        return new TokenResponseDto
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSafeRefreshTokenAsync(user),
        };
    }

    /// <summary>
    /// Creates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to create the token.</param>
    /// <returns>A JWT token as a string.</returns>
    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            //new Claim(ClaimTypes.Role, user.Role),
        };
            
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("Jwt:Issuer"),
            audience: _configuration.GetValue<string>("Jwt:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );
            
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSafeRefreshTokenAsync(User user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(1);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    private async Task<User?> ValidateRefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
        if (user == null || user.RefreshToken == null || user.RefreshTokenExpiry == null
            || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return null;
        }
        
        return user;
    }
}