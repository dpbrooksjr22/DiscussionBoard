
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using DiscussionBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages;

[Authorize]
public class MyProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserProfileService _profileService;

    public MyProfileModel(
        UserManager<ApplicationUser> userManager,
        IUserProfileService profileService)
    {
        _userManager = userManager;
        _profileService = profileService;
    }

    [BindProperty]
    public UserProfile Input { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        Input = new UserProfile
        {
            DisplayName = user.DisplayName ?? "",
            AvatarId = AvatarCatalog.IsValid(user.AvatarId)
                ? user.AvatarId!
                : AvatarCatalog.DefaultId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        Input.DisplayName = (Input.DisplayName ?? "").Trim();

        if (Input.DisplayName.Length < 2 ||
            Input.DisplayName.Length > 60)
        {
            ModelState.AddModelError(
                "Profile.DisplayName",
                "Display name must be between 2 and 60 characters.");
        }
        else
        {
            bool available = await _profileService
                .IsDisplayNameAvailableAsync(Input.DisplayName, user.Id);

            if (!available)
            {
                ModelState.AddModelError(
                    "Profile.DisplayName",
                    "This display name is already taken.");
            }
        }

        if (!AvatarCatalog.IsValid(Input.AvatarId))
        {
            ModelState.AddModelError(
                "Profile.AvatarId",
                "Please select a valid avatar.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        user.DisplayName = Input.DisplayName;
        user.AvatarId = Input.AvatarId;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty, error.Description);
            }

            return Page();
        }

        SuccessMessage = "Your profile was updated successfully!";

        return RedirectToPage();
    }
}
