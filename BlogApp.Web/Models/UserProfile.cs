using BlogApp.Web.Models.Entities;

namespace BlogApp.Web.Models
{
    public class UserProfile
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public ICollection<Post> Posts{ get; set; }
        public int PostCount { get; set; }
    }
}
