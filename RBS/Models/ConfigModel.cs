using System.ComponentModel.DataAnnotations;

namespace RBS.Models
{
    public class ConfigModel
    {
        // PK
        [Key]
        public int ID { get; set; }

        // Main fields
        public string Key { get; set; }
        public string Value { get; set; }
    }
}