using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RBS.Models
{
    public class DumpParticipantModel
    {
        // PK
        [Key]
        public Guid RecordID { get; set; }
        public DateTime DeleteDate { get; set; }

        public int ID { get; set; }

        // FK
        public int? MeetingID { get; set; }
        public int? UserID { get; set; }

        // Tracking fields
        [DisplayName("Created By")]
        public string CreatedBy { get; set; }
        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; }
    }
}