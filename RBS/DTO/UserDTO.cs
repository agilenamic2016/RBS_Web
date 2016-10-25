using RBS.Models;

namespace RBS.DTO
{
    public class UserDTO
    {
        // Main fields
        public int ID { get; set; }
        public int? RoleID { get; set; }
        public int? DepartmentID { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }

        public virtual DepartmentModel Department { get; set; }
    }
}