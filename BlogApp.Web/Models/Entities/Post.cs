namespace BlogApp.Web.Models.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } =DateTime.Now;
        public DateTime UpdatedAt { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    }
}
