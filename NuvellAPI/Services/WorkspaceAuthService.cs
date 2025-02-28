using Microsoft.AspNetCore.Identity;
using NuvellAPI.Data;
using NuvellAPI.Models.Common;
using NuvellAPI.Models.Domain;

namespace NuvellAPI.Services;

public class WorkspaceAuthService (
    UserManager<AppUser> userManager,
    AppDbContext context)
{
    public async Task<Result<bool>> VerifyWorkspaceAccess(Guid userId, Guid workspaceId)
    {
        var verifyExistingUserResult = await VerifyExistingUser(userId);
        if (!verifyExistingUserResult.Success)
        {
            return Result<bool>.Failure(verifyExistingUserResult.ErrorMessage!);
        }
        
        var workspace = await context.Workspaces.FindAsync(workspaceId);
        if (workspace == null)
        {
            return Result<bool>.Failure("Workspace not found");
        }
        
        //verify if workspace is owned by user
        if (workspace.UserId != userId)
        {
            return Result<bool>.Failure("User is not authorized to access this workspace");
        }
        
        return Result<bool>.SuccessResult(true);
    }

    /// <summary>
    /// Verifies if the user exists
    /// </summary>
    /// <param name="userId">Guid of the user to verify</param>
    /// <returns></returns>
    public async Task<Result<AppUser>> VerifyExistingUser(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return Result<AppUser>.Failure("User not found");
        }
        return Result<AppUser>.SuccessResult(user);
    }
}