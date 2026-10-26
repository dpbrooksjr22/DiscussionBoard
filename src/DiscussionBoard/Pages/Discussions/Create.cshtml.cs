using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages.Discussions;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly DiscussionThreadService _threadService;

    public CreateModel(
        ApplicationDbContext context,
        DiscussionThreadService threadService)
    {
        _context = context;
        _threadService = threadService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public int ForumId { get; set; }

    public class InputModel
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(10000)]
        public string Content { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int forumId)
    {
        if (!await _context.Forums.AnyAsync(
                f => f.Id == forumId && !f.IsDeleted))
        {
            return NotFound();
        }

        ForumId = forumId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int forumId)
    {
        ForumId = forumId;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!await _context.Forums.AnyAsync(
                f => f.Id == forumId && !f.IsDeleted))
        {
            return NotFound();
        }

        string? userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var thread = new DiscussionThread
        {
            Title = Input.Title.Trim(),
            Content = Input.Content.Trim(),
            ForumId = forumId,
            ApplicationUserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await _threadService.AddAsync(thread);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                "",
                "Error saving discussion thread.");

            return Page();
        }

        return RedirectToPage(
            "/Discussions/Details",
            new { threadId = thread.Id });
    }
}
