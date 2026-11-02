using DiscussionBoard.Data;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services;

public class UserProfileService : IUserProfileService
{
    private readonly ApplicationDbContext _context;

    public UserProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsDisplayNameAvailableAsync(
        string displayName, string? excludeUserId = null)
    {
        string trimmedName = displayName.Trim() ?? "";

        if (trimmedName.Length < 2 || trimmedName.Length > 60)
        {
            return false;
        }

        string normalizedName = trimmedName.ToLower();

        bool nameExists = await _context.Users.AnyAsync(user =>
            user.Id != excludeUserId &&
            user.DisplayName != null &&
            user.DisplayName.ToLower() == normalizedName);

        return !nameExists;
    }
}
