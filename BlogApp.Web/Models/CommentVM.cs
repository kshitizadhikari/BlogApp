using BlogApp.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Web.Models
{
    public class CommentVM
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Content { get; set; }
        public int PostId { get; set; }

    }
}
