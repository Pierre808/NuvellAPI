using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvellAPI.Interfaces;
using NuvellAPI.Models.DTO;

namespace NuvellAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController (IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await authService.RegisterUserAsync(request);
            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
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

            var result = await authService.LoginUserAsync(request);
            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await authService.RefreshTokenAsync(request);
            if (!result.Success)
            {
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
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