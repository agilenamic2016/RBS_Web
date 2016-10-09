using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RBS.Models
{
    public class RoomModel
    {
        // PK
        [Key]
        public int ID { get; set; }

        // Main fields
        [Required]
        [DisplayName("Meeting Room")]
        [StringLength(100)]
        public string Name { get; set; }
        public string PhotoFilePath { get; set; }
        [StringLength(100)]
        public string PhotoFileName { get; set; }

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