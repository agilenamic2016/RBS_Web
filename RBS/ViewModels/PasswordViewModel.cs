using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RBS.ViewModels
{
    public class PasswordViewModel
    {
        public string ID { get; set; }

        [Required]
        [DisplayName("Current Password")]
        public string CurrentPassword { get; set; }

        [Required]
        [DisplayName("New Password")]
        public string NewPassword { get; set; }

        [Compare("NewPassword")]
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}