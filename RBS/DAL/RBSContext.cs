using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using RBS.Models;

namespace RBS.DAL
{
    public class RBSContext : DbContext
    {
        public RBSContext() : base("RBSContext") { }

        public DbSet<ConfigModel> Configs { get; set; }
        public DbSet<LogModel> Logs { get; set; }
        public DbSet<MeetingModel> Meetings { get; set; }
        public DbSet<ParticipantModel> Participants { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<DepartmentModel> Departments { get; set; }
        public DbSet<RoomModel> Rooms { get; set; }
        public DbSet<SessionModel> Sessions { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<DumpParticipantModel> DumpParticipant { get; set; }
        public DbSet<DumpUserModel> DumpUser { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}