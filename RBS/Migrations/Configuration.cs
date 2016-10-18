namespace RBS.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Models;

    internal sealed class Configuration : DbMigrationsConfiguration<RBS.DAL.RBSContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(DAL.RBSContext context)
        {
            //  This method will be called after migrating to the latest version.
            var Configs = new List<ConfigModel>
            {
                new ConfigModel { Key = "SessionKeyValidityInSecond", Value = "1800" },
                new ConfigModel { Key = "PageSize", Value = "20" },
                new ConfigModel { Key = "AllowedExtension", Value = ".gif, .jpg, .jpeg, .png" },
                new ConfigModel { Key = "DebuggingMode", Value = "off" },
            };

            Configs.ForEach(s => context.Configs.Add(s));
            context.SaveChanges();

            var Roles = new List<RoleModel>
            {
                new RoleModel { Name = "Admin", CreatedBy = "System", CreatedDate = System.DateTime.Now },
                new RoleModel { Name = "User", CreatedBy = "System", CreatedDate = System.DateTime.Now }
            };

            Roles.ForEach(s => context.Roles.Add(s));
            context.SaveChanges();

            var Departments = new List<DepartmentModel>
            {
                new DepartmentModel { Name = "Project Manager", CreatedBy = "System", CreatedDate = System.DateTime.Now }
            };

            Departments.ForEach(s => context.Departments.Add(s));
            context.SaveChanges();

            var Users = new List<UserModel>
            {
                new UserModel { Username="agilenamic@gmail.com", Name="Vendor", Password="2C0E7487D1744E1413664C424FFB2884BDCCFFAD7CDB2C1BDCA35EA8EB046EA421B0B7203D7553B8873F3BC95BE05189D3D3F4846C7B045F2A816688CAE3E127", RoleID = Roles.FirstOrDefault(s => s.Name =="Admin").ID,
                DepartmentID=1, IsActive = true, CreatedBy = "System", CreatedDate = System.DateTime.Now },
            };

            Users.ForEach(s => context.Users.Add(s));
            context.SaveChanges();
        }
    }
}
