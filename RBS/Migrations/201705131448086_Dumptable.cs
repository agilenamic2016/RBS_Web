namespace RBS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Dumptable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DumpParticipantModel",
                c => new
                {
                    RecordID = c.Guid(nullable: false),
                    DeleteDate = c.DateTime(nullable: false),
                    ID = c.Int(nullable: false),
                    MeetingID = c.Int(),
                    UserID = c.Int(),
                    CreatedBy = c.String(),
                    CreatedDate = c.DateTime(),
                })
                .PrimaryKey(t => t.RecordID);

            CreateTable(
                "dbo.DumpUserModel",
                c => new
                {
                    RecordID = c.Guid(nullable: false),
                    DeleteDate = c.DateTime(nullable: false),
                    ID = c.Int(nullable: false),
                    RoleID = c.Int(),
                    DepartmentID = c.Int(),
                    Username = c.String(nullable: false, maxLength: 50),
                    Name = c.String(nullable: false, maxLength: 100),
                    Password = c.String(maxLength: 128),
                    TokenID = c.String(maxLength: 250),
                    IsActive = c.Boolean(nullable: false),
                    CreatedBy = c.String(),
                    CreatedDate = c.DateTime(),
                    UpdatedBy = c.String(),
                    UpdatedDate = c.DateTime(),
                })
                .PrimaryKey(t => t.RecordID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DumpUserModel", "RoleID", "dbo.RoleModel");
            DropForeignKey("dbo.DumpUserModel", "DepartmentID", "dbo.DepartmentModel");
            DropForeignKey("dbo.DumpParticipantModel", "UserID", "dbo.UserModel");
            DropForeignKey("dbo.DumpParticipantModel", "MeetingID", "dbo.MeetingModel");
            DropIndex("dbo.DumpUserModel", new[] { "DepartmentID" });
            DropIndex("dbo.DumpUserModel", new[] { "RoleID" });
            DropIndex("dbo.DumpParticipantModel", new[] { "UserID" });
            DropIndex("dbo.DumpParticipantModel", new[] { "MeetingID" });
            DropTable("dbo.DumpUserModel");
            DropTable("dbo.DumpParticipantModel");
        }
    }
}
