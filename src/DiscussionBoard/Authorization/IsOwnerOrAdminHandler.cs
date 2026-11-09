using System.Security.Claims;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Authorization;

namespace DiscussionBoard.Authorization
{
    public class IsOwnerOrAdminHandler : AuthorizationHandler<IsOwnerOrAdminRequirement, DiscussionBase>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IsOwnerOrAdminRequirement requirement,
            DiscussionBase resource)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Task.CompletedTask;
            }

            var currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (context.User.IsInRole(ApplicationRoles.Admin) ||
                (!string.IsNullOrEmpty(currentUserId) &&
                 currentUserId == resource.ApplicationUserId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
