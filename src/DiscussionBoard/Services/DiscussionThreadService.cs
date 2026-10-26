using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DiscussionBoard.Services;

public class DiscussionThreadService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DiscussionThreadService> _logger;

    public DiscussionThreadService(
        ApplicationDbContext context,
        ILogger<DiscussionThreadService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public DiscussionThreadService(ApplicationDbContext context)
        : this(context, NullLogger<DiscussionThreadService>.Instance)
    {
    }

    public async Task<DiscussionThread?> GetAsync(int id)
    {
        return await _context.DiscussionThreads
            .Include(t => t.ApplicationUser)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<List<DiscussionThread>> GetForForumAsync(int forumId)
    {
        return await _context.DiscussionThreads
            .Include(t => t.ApplicationUser)
            .Where(t => t.ForumId == forumId && !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<DiscussionThread> AddAsync(DiscussionThread thread)
    {
        try
        {
            _context.DiscussionThreads.Add(thread);
            await _context.SaveChangesAsync();

            return thread;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error adding discussion thread.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(DiscussionThread thread)
    {
        try
        {
            var existingThread = await _context.DiscussionThreads
                .FirstOrDefaultAsync(t => t.Id == thread.Id && !t.IsDeleted);

            if (existingThread == null)
            {
                return false;
            }

            existingThread.Title = thread.Title;
            existingThread.Content = thread.Content;

            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error updating discussion thread.");
            throw;
        }
    }
}
