using DiscussionBoard.Data;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages.Forums;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly DiscussionThreadService _threadService;

    public DetailsModel(
        ApplicationDbContext context,
        DiscussionThreadService threadService)
    {
        _context = context;
        _threadService = threadService;
    }

    public Forum? CurrentForum { get; set; }

    public List<DiscussionThread> Threads { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        CurrentForum = await _context.Forums
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

        if (CurrentForum == null)
        {
            return NotFound();
        }

        Threads = await _threadService.GetForForumAsync(id);

        Threads = Threads
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

        return Page();
    }
}
