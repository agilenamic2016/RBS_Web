using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RBS.DTO
{
    public class MeetingDTO
    {
        // Main fields
        public int ID { get; set; }
        public int? RoomID { get; set; }
        public string Title { get; set; }
        public string Purpose { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? BookingDate { get; set; }
        public string StartingTime { get; set; }
        public string EndingTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int RecurenceType { get; set; }
        public string SCCStartDate { get; set; }
        public string SCCEndDate { get; set; }

    }
}