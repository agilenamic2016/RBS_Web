using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RBS.ViewModels
{
    public class UserViewModel
    {
        [Required]
        [DisplayName("Email")]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(128)]
        public string Password { get; set; }

        public UserViewModel()
        {
            Username = String.Empty;
            Password = String.Empty;
        }
    }
}