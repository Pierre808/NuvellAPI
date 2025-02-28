using Microsoft.AspNetCore.Authorization;
using NuvellAPI.Data;
using NuvellAPI.Models.Common;
using NuvellAPI.Models.Domain;
using NuvellAPI.Models.DTO.Workspace;

namespace NuvellAPI.Services;

public class WorkspaceService (
    WorkspaceAuthService workspaceAuthService,
    AppDbContext context)
{
    public async Task<Result<WorkspaceDto>> CreateWorkspace(Guid userId, string? workspaceName)
    {
        
        var verifyExistingUserResult = await workspaceAuthService.VerifyExistingUser(userId);
        if (!verifyExistingUserResult.Success)
        {
            return Result<WorkspaceDto>.Failure(verifyExistingUserResult.ErrorMessage!);
        }

        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            return Result<WorkspaceDto>.Failure("Name is required");
        }
        
        var workspace = new Workspace
        {
            Name = workspaceName,
            UserId = userId,
            User = verifyExistingUserResult.Data!,
        };
        context.Workspaces.Add(workspace);
        await context.SaveChangesAsync();

        var result = new WorkspaceDto
        {
            Id = workspace.Id,
            Name = workspace.Name
        };
        
        return Result<WorkspaceDto>.SuccessResult(result);
    }
}