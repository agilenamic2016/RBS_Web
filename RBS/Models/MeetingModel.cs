using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RBS.Models
{
    public class MeetingModel
    {
        // PK
        [Key]
        public int ID { get; set; }

        // FK
        public int? RoomID { get; set; }

        // Main fields
        [Required]
        [DisplayName("Title")]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Purpose { get; set; }

        [Required]
        [DisplayName("Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? BookingDate { get; set; }

        [Required]
        [DisplayName("Starting Time")]
        public string StartingTime { get; set; }

        [Required]
        [DisplayName("Ending Time")]
        public string EndingTime { get; set; }

        // Tracking fields
        [DisplayName("Created By")]
        public string CreatedBy { get; set; }
        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; }
        [DisplayName("Updated By")]
        public string UpdatedBy { get; set; }
        [DisplayName("Updated Date")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual RoomModel Room { get; set; }
        public virtual IList<ParticipantModel> Participants { get; set; }

        //recurrency setting
        [DisplayName("Recurrence")]
        public int RecurenceType { get; set; }
        [StringLength (10)]
        [DisplayName("Recurrence Start Date")]
        public string SCCStartDate { get; set; }
        [DisplayName("Recurrence End Date")]
        [StringLength(10)]
        public string SCCEndDate { get; set; }
    }
}