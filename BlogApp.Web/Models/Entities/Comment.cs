using System.ComponentModel.DataAnnotations;

namespace BlogApp.Web.Models.Entities
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
