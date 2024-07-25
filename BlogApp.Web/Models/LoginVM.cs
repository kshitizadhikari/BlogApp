using System.ComponentModel.DataAnnotations;

namespace BlogApp.Web.Models
{
    public class LoginVM
    {
        [Required]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
