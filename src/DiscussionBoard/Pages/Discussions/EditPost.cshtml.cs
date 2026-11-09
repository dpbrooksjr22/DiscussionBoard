using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages.Discussions;

[Authorize]
public class EditPostModel : PageModel
{
    private readonly PostService _postService;
    private readonly IApplicationAuthorizationService _authorizationService;

    public EditPostModel(
        PostService postService,
        IApplicationAuthorizationService authorizationService)
    {
        _postService = postService;
        _authorizationService = authorizationService;
    }



    [BindProperty]
    public DiscussionEditViewModel Input { get; set; } = new();

    public int ThreadId { get; set; }


    public int PostId { get; set; }


    public async Task<IActionResult> OnGetAsync(int threadId, int postId)
    {
        var post = await _postService.GetAsync(postId);

        if (post == null ||
            post.Id != postId ||
            post.DiscussionThreadId != threadId)
            return NotFound();

        if (!await _authorizationService.CanEditAsync(User, post))
            return Forbid();

        ThreadId = threadId;
        PostId = postId;

        Input = new DiscussionEditViewModel
        {
            Title = post.Title,
            Content = post.Content
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int threadId, int postId)
    {
        var post = await _postService.GetAsync(postId);

        if (post == null ||
            post.Id != postId ||
            post.DiscussionThreadId != threadId)
            return NotFound();

        if (!await _authorizationService.CanEditAsync(User, post))
            return Forbid();

        ThreadId = threadId;
        PostId = postId;

        if (!ModelState.IsValid)
            return Page();

        var updatedPost = new Post
        {
            Id = postId,
            Title = Input.Title.Trim(),
            Content = Input.Content.Trim()
        };
        try
        {
            bool updated = await _postService.UpdateAsync(updatedPost);

            if (!updated)
                return NotFound();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("",
                "Error updating the post.");
            return Page();
        }


        return RedirectToPage("/Discussions/Details",
            new { threadId });
    }
}
