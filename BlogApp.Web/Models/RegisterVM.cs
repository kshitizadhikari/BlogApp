using System.ComponentModel.DataAnnotations;

namespace BlogApp.Web.Models
{
    public class RegisterVM
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "UserName")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm Password field must match Password field")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword{ get; set; }
    }
}
