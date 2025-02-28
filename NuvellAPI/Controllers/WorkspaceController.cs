using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvellAPI.Models.DTO.Workspace;
using NuvellAPI.Services;

namespace NuvellAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WorkspaceController (WorkspaceService workspaceService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateWorkspace(CreateWorkspaceDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(nameIdentifier) || !Guid.TryParse(nameIdentifier, out var userId))
            {
                return BadRequest("Invalid or missing user id");
            }
            var result = await workspaceService.CreateWorkspace(userId, request.Name);
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
}