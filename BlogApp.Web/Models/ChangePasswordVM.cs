using BlogApp.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Web.Models
{
    public class ChangePasswordVM
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name="Current Password")]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "This field must match the NewPassword field")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }


    }
}
