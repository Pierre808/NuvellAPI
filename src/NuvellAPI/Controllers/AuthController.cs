using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvellAPI.Models.Domain;
using NuvellAPI.Models.DTOs;
using NuvellAPI.Services;

namespace NuvellAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                // Return a 400 Bad Request with validation errors
                return BadRequest(new
                {
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }
            
            var user = await authService.RegisterAsync(request);
            if (user == null)
            {
                return BadRequest(new { message = "Email adress is already in use." });
            }
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserDto request)
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                // Return a 400 Bad Request with validation errors
                return BadRequest(new
                {
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }
            
            var response = await authService.LoginAsync(request);
            if (response == null)
            {
                return BadRequest(new { message = "Username or password is incorrect." });
            }
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                // Return a 400 Bad Request with validation errors
                return BadRequest(new
                {
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }
            
            var result = await authService.RefreshTokensAsync(request);
            if (result == null || result.AccessToken == null || result.RefreshToken == null)
            {
                return Unauthorized(new { message = "Refresh token is invalid." });
            }
            return Ok(result);
        }
        
        [Authorize]
        [HttpGet("authenticated")]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are authenticated.");
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are an admin.");
        }
    }
}
