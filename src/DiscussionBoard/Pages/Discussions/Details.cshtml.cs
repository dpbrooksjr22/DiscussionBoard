using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages.Discussions;

public class DetailsModel : PageModel
{
    private readonly DiscussionThreadService _threadService;
    private readonly PostService _postService;

    public DetailsModel(
        DiscussionThreadService threadService,
        PostService postService)
    {
        _threadService = threadService;
        _postService = postService;
    }

    public DiscussionThread? Thread { get; set; }

    public List<Post> Posts { get; set; } = new();

    public List<PostTreeBuilder.Node> PostTree { get; set; } = new();

    [BindProperty]
    public ReplyInputModel Input { get; set; } = new();

    public class ReplyInputModel
    {
        [Required]
        [MaxLength(10000)]
        public string Content { get; set; } = string.Empty;

        public int? ParentPostId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int threadId)
    {
        Thread = await _threadService.GetAsync(threadId);

        if (Thread == null)
        {
            return NotFound();
        }

        Posts = await _postService.GetForThreadAsync(threadId);

        PostTree = PostTreeBuilder.Build(Posts);

        return Page();
    }

    public async Task<IActionResult> OnPostReplyAsync(int threadId)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        var thread = await _threadService.GetAsync(threadId);

        if (thread == null)
        {
            return NotFound();
        }

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        if (Input.ParentPostId.HasValue)
        {
            var posts = await _postService.GetForThreadAsync(threadId);

            if (!posts.Any(p => p.Id == Input.ParentPostId.Value))
            {
                ModelState.AddModelError(
                    "Input.ParentPostId", "Invalid parent post.");
            }
        }

        if (string.IsNullOrWhiteSpace(Input.Content))
        {
            ModelState.AddModelError(
                "Input.Content", "Reply cannot be empty.");
        }

        if (!ModelState.IsValid)
        {
            return await OnGetAsync(threadId);
        }

        var post = new Post
        {
            Title = string.Empty,
            Content = Input.Content.Trim(),
            DiscussionThreadId = threadId,
            ParentPostId = Input.ParentPostId,
            ApplicationUserId = userId
        };

        try
        {
            await _postService.AddAsync(post);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                "", "Unable to save your reply.");

            return await OnGetAsync(threadId);
        }

        return RedirectToPage("/Discussions/Details",
            new { threadId });
    }
}
