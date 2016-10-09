using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RBS.Models
{
    public class ParticipantModel
    {
        // PK
        [Key]
        public int ID { get; set; }

        // FK
        public int? MeetingID { get; set; }
        public int? UserID { get; set; }

        // Tracking fields
        [DisplayName("Created By")]
        public string CreatedBy { get; set; }
        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; }

        // Navigation properties
        public virtual MeetingModel Meeting { get; set; }
        public virtual UserModel User { get; set; }

    }
}