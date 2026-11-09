using System.Security.Claims;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DiscussionBoard.Authorization
{
    public class ApplicationAuthorizationService : IApplicationAuthorizationService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationAuthorizationService(UserManager<ApplicationUser> userManager, IAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        private readonly IAuthorizationService _authorizationService;

        public async Task<bool> CanEditAsync(ClaimsPrincipal principal, DiscussionBase resource)
        {
            var result = await _authorizationService.AuthorizeAsync(
                principal,
                resource,
                ApplicationAuthorizationPolicies.IsOwnerOrAdmin
            );

            return result.Succeeded;
        }

        public async Task<IdentityResult> AddRoleAsync(ClaimsPrincipal actingUser, string targetUserId, string roleName)
        {
            if (actingUser.Identity?.IsAuthenticated != true ||
                !actingUser.IsInRole(ApplicationRoles.Admin))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "Unauthorized",
                    Description = "Only administrators can assign roles."
                });
            }

            var currentUser = await _userManager.GetUserAsync(actingUser);

            if (currentUser == null ||
                !await _userManager.IsInRoleAsync(currentUser, ApplicationRoles.Admin))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "Unauthorized",
                    Description = "Only administrators can assign roles."
                });
            }

            if (roleName != ApplicationRoles.Admin)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidRole",
                    Description = "Only the Admin role can be assigned."
                });
            }
            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidUser",
                    Description = "A valid user ID is required."
                });
            }

            var targetUser = await _userManager.FindByIdAsync(targetUserId);

            if (targetUser == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "Selected user does not exist."
                });
            }
            return await _userManager.AddToRoleAsync(targetUser, ApplicationRoles.Admin);
        }

        public async Task<IdentityResult> RemoveRoleAsync(ClaimsPrincipal actingUser, string targetUserId, string roleName)
        {
            if (actingUser.Identity?.IsAuthenticated != true ||
                !actingUser.IsInRole(ApplicationRoles.Admin))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "Unauthorized",
                    Description = "Only administrators can remove roles."
                });
            }
            var currentUser = await _userManager.GetUserAsync(actingUser);

            if (currentUser == null ||
                !await _userManager.IsInRoleAsync(currentUser, ApplicationRoles.Admin))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "Unauthorized",
                    Description = "Only administrators can remove roles."
                });
            }

            if (roleName != ApplicationRoles.Admin)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidRole",
                    Description = "Only the Admin role can be removed."
                });
            }

            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidUser",
                    Description = "A valid user ID is required."
                });
            }

            var targetUser = await _userManager.FindByIdAsync(targetUserId);

            if (targetUser == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "Selected user does not exist."
                });
            }

            var actingUserId = _userManager.GetUserId(actingUser);

            if (actingUserId == targetUserId)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "SelfRemoval",
                    Description = "You cannot remove your own Admin role."
                });
            }

            if (!await _userManager.IsInRoleAsync(targetUser, ApplicationRoles.Admin))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotInRole",
                    Description = "The selected user is not an administrator."
                });
            }

            var admins = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);

            if (admins.Count <= 1)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "LastAdmin",
                    Description = "Cannot remove the last remaining administrator."
                });
            }

            return await _userManager.RemoveFromRoleAsync(targetUser, ApplicationRoles.Admin);
        }

    }
}
