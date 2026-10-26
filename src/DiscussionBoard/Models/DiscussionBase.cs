using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.Models
{
    public abstract class DiscussionBase : EntityBase
    {
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
