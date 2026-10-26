using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages.Discussions;

[Authorize]
public class EditModel : PageModel
{
    private readonly DiscussionThreadService _threadService;

    public EditModel(DiscussionThreadService threadService)
    {
        _threadService = threadService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public int ThreadId { get; set; }
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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var thread = await _threadService.GetAsync(id);

        if (thread == null)
            return NotFound();

        if (thread.ApplicationUserId !=
            User.FindFirstValue(ClaimTypes.NameIdentifier))
            return Forbid();

        ThreadId = thread.Id;
        ForumId = thread.ForumId;

        Input.Title = thread.Title;
        Input.Content = thread.Content;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var thread = await _threadService.GetAsync(id);

        if (thread == null)
            return NotFound();

        if (thread.ApplicationUserId !=
            User.FindFirstValue(ClaimTypes.NameIdentifier))
            return Forbid();

        ThreadId = thread.Id;
        ForumId = thread.ForumId;

        if (!ModelState.IsValid)
            return Page();

        thread.Title = Input.Title.Trim();
        thread.Content = Input.Content.Trim();

        try
        {
            bool updated = await _threadService.UpdateAsync(thread);

            if (!updated)
                return NotFound();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("",
                "Error updating the discussion.");
            return Page();
        }

        return RedirectToPage("/Discussions/Details",
            new { threadId = id });
    }
}
