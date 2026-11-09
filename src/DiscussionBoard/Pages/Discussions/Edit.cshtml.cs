using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages.Discussions;

[Authorize]
public class ThreadEditModel : PageModel
{
    private readonly DiscussionThreadService _threadService;
    private readonly IApplicationAuthorizationService _authorizationService;

    public ThreadEditModel(DiscussionThreadService threadService, IApplicationAuthorizationService authorizationService)
    {
        _threadService = threadService;
        _authorizationService = authorizationService;
    }

    [BindProperty]
    public DiscussionEditViewModel Input { get; set; } = new();

    public int ThreadId { get; set; }
    public int ForumId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var thread = await _threadService.GetAsync(id);

        if (thread == null)
            return NotFound();

        if (!await _authorizationService.CanEditAsync(User, thread))
            return Forbid();

        ThreadId = thread.Id;
        ForumId = thread.ForumId;

        Input = new DiscussionEditViewModel
        {
            Title = thread.Title,
            Content = thread.Content
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var thread = await _threadService.GetAsync(id);

        if (thread == null)
            return NotFound();

        if (!await _authorizationService.CanEditAsync(User, thread))
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
