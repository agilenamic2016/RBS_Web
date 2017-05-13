using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RBS.Models
{
    public class DumpUserModel
    {
        // PK
        [Key]
        public Guid RecordID { get; set; }
        public DateTime DeleteDate { get; set; }

        public int ID { get; set; }

        // FK
        public int? RoleID { get; set; }
        public int? DepartmentID { get; set; }

        // Main fields
        [Required]
        [DisplayName("Email")]
        [StringLength(50), RegularExpression(@"^[a-z0-9][-a-z0-9.!#$%&'*+-=?^_`{|}~\/]+@([-a-z0-9]+\.)+[a-z]{2,5}$", ErrorMessage = "Please enter valid email address")]
        public string Username { get; set; }

        [Required]
        [StringLength(100), RegularExpression(@"^[a-zA-Z_ \.]*$", ErrorMessage = "We think that name should not have any special characters")]
        public string Name { get; set; }

        [StringLength(128, MinimumLength = 8, ErrorMessage = "We personally think that password longer than 8 characters is more secure")]
        public string Password { get; set; }

        [StringLength(250)]
        public string TokenID { get; set; }

        [DisplayName("Account Status")]
        public bool IsActive { get; set; }

        // Tracking fields
        [DisplayName("Created By")]
        public string CreatedBy { get; set; }
        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; }
        [DisplayName("Updated By")]
        public string UpdatedBy { get; set; }
        [DisplayName("Updated Date")]
        public DateTime? UpdatedDate { get; set; }
    }
}