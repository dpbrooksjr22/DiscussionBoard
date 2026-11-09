using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services;

public class PostService
{
    private readonly ApplicationDbContext _context;

    public PostService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Post>> GetForThreadAsync(int threadId)
    {
        return await _context.Posts
            .Include(p => p.ApplicationUser)
            .Where(p => p.DiscussionThreadId == threadId && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<Post> AddAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return post;
    }

    public async Task<Post?> GetAsync(int id)
    {
        return await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<bool> UpdateAsync(Post post)
    {
        var existingPost = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == post.Id && !p.IsDeleted);

        if (existingPost == null)
            return false;

        existingPost.Title = post.Title;
        existingPost.Content = post.Content;

        await _context.SaveChangesAsync();
        return true;
    }
}
