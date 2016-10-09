using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RBS.Models
{
    public class LogModel
    {
        // PK
        [Key]
        public int ID { get; set; }

        // Main fields
        public string Type { get; set; }    // INFO, ERROR
        public string Page { get; set; }
        public string Action { get; set; }    // LOGIN, LOGOUT, CREATE, READ, UPDATE, DELETE
        public string Source { get; set; }    // Username
        public string Data { get; set; }

        // Tracking fields
        [DisplayName("Created By")]
        public string CreatedBy { get; set; }
        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; }
    }
}