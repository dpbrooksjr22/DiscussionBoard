using Microsoft.AspNetCore.Identity;

namespace DiscussionBoard.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? AvatarId { get; set; }

        public ICollection<DiscussionThread> DiscussionThreads { get; set; }
            = new List<DiscussionThread>();

        public ICollection<Post> Posts { get; set; }
            = new List<Post>();
    }
}
