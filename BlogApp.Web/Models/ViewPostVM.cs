using BlogApp.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApp.Web.Models
{
    public class ViewPostVM
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public AppUser Author { get; set; }

    }
}
