﻿using System;
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
        [DisplayName("Title of Meeting")]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Purpose { get; set; }

        [DisplayName("Booking Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? BookingDate { get; set; }

        [StringLength(4)]
        [DisplayName("Starting Time")]
        public string StartingTime { get; set; }

        [StringLength(4)]
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

        //recurrency setting
        [DisplayName("Recurrence")]
        public int RecurenceType { get; set; }
        [StringLength (10)]
        [DisplayName("Recurrence start date")]
        public string SCCStartDate { get; set; }
        [DisplayName("Recurrence end date")]
        [StringLength(10)]
        public string SCCEndDate { get; set; }
    }
}