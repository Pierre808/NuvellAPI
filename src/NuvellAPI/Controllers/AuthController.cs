using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvellAPI.Entities;
using NuvellAPI.Models;
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
        public async Task<ActionResult<string>> Login(UserDto request)
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
            
            var token = await authService.LoginAsync(request);
            if (token == null)
            {
                return BadRequest(new { message = "Username or password is incorrect." });
            }
            return Ok(token);
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
