using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Mvc;

namespace DiscussionBoard.Pages.Admin
{
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public List<ApplicationUser> AvailableUsers { get; set; } = new();

        public UsersModel(UserManager<ApplicationUser> userManager, IApplicationAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        private readonly IApplicationAuthorizationService _authorizationService;

        public async Task OnGetAsync(string? selectedUserId)
        {
            AvailableUsers = await _userManager.Users
                .OrderBy(u => u.DisplayName)
                .ThenBy(u => u.Email)
                .ToListAsync();

            SuccessMessage = TempData["SuccessMessage"] as string;
            ErrorMessage = TempData["ErrorMessage"] as string;

            SelectedUserId = selectedUserId;

            if (!string.IsNullOrWhiteSpace(selectedUserId))
            {
                SelectedUser = await _userManager.FindByIdAsync(selectedUserId);

                if (SelectedUser != null)
                {
                    IsSelectedUserAdmin = await _userManager.IsInRoleAsync(
                        SelectedUser, ApplicationRoles.Admin);
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            AvailableUsers = await _userManager.Users
                .OrderBy(u => u.DisplayName)
                .ThenBy(u => u.Email)
                .ToListAsync();

            if (string.IsNullOrWhiteSpace(SelectedUserId))
            {
                ErrorMessage = "Please select a user.";
                return Page();
            }

            SelectedUser = await _userManager.FindByIdAsync(SelectedUserId);

            if (SelectedUser == null)
            {
                ErrorMessage = "The selected user does not exist.";
                return Page();
            }

            IsSelectedUserAdmin = await _userManager.IsInRoleAsync(
                SelectedUser,
                ApplicationRoles.Admin
            );

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateRoleAsync()
        {
            AvailableUsers = await _userManager.Users
                .OrderBy(u => u.DisplayName)
                .ThenBy(u => u.Email)
                .ToListAsync();

            if (string.IsNullOrWhiteSpace(SelectedUserId))
            {
                ErrorMessage = "Please select a valid user.";
                return Page();
            }

            if (RoleName != ApplicationRoles.Admin)
            {
                ErrorMessage = "Invalid role selected.";
                return Page();
            }

            SelectedUser = await _userManager.FindByIdAsync(SelectedUserId);

            if (SelectedUser == null)
            {
                ErrorMessage = "The selected user does not exist.";
                return Page();
            }

            IsSelectedUserAdmin = await _userManager.IsInRoleAsync(
                SelectedUser,
                ApplicationRoles.Admin
            );

            IdentityResult result;

            if (IsSelectedUserAdmin)
            {
                result = await _authorizationService.RemoveRoleAsync(
                    User,
                    SelectedUserId,
                    RoleName
                );
            }
            else
            {
                result = await _authorizationService.AddRoleAsync(
                    User,
                    SelectedUserId,
                    RoleName
                );
            }

            if (result.Succeeded)
            {
                SuccessMessage = "The user's Admin role has been updated successfully.";
            }
            else
            {
                ErrorMessage = string.Join(
                    " ",
                    result.Errors.Select(error => error.Description)
                );
            }

            IsSelectedUserAdmin = await _userManager.IsInRoleAsync(
                SelectedUser,
                ApplicationRoles.Admin
            );

            return Page();
        }

        public Task<IActionResult> OnPostAddAsync()
        {
            return ChangeRoleAsync(true);
        }

        public Task<IActionResult> OnPostRemoveAsync()
        {
            return ChangeRoleAsync(false);
        }

        private async Task<IActionResult> ChangeRoleAsync(bool add)
        {
            var result = add
                ? await _authorizationService.AddRoleAsync(
                    User, SelectedUserId ?? "", ApplicationRoles.Admin)
                : await _authorizationService.RemoveRoleAsync(
                    User, SelectedUserId ?? "", ApplicationRoles.Admin);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "The user's Admin role has been updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(
                    " ", result.Errors.Select(error => error.Description));
            }

            return RedirectToPage(new { selectedUserId = SelectedUserId });
        }

        [BindProperty]
        public string? SelectedUserId { get; set; }

        public ApplicationUser? SelectedUser { get; set; }

        public bool IsSelectedUserAdmin { get; set; }

        public string? ErrorMessage { get; set; }

        [BindProperty]
        public string? RoleName { get; set; }

        public string? SuccessMessage { get; set; }
    }
}
