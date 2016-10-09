namespace RBS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ConfigModel",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Key = c.String(),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.LogModel",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Type = c.String(),
                        Page = c.String(),
                        Action = c.String(),
                        Source = c.String(),
                        Data = c.String(),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.MeetingModel",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RoomID = c.Int(),
                        Title = c.String(nullable: false, maxLength: 100),
                        Purpose = c.String(maxLength: 500),
                        BookingDate = c.DateTime(),
                        StartingTime = c.String(maxLength: 4),
                        EndingTime = c.String(maxLength: 4),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.RoomModel", t => t.RoomID)
                .Index(t => t.RoomID);
            
            CreateTable(
                "dbo.RoomModel",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        PhotoFilePath = c.String(),
                        PhotoFileName = c.String(maxLength: 100),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.ParticipantModel",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MeetingID = c.Int(),
                        UserID = c.Int(),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.MeetingModel", t => t.MeetingID)
                .ForeignKey("dbo.UserModel", t => t.UserID)
                .Index(t => t.MeetingID)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.UserModel",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        RoleID = c.Int(),
                        Username = c.String(nullable: false, maxLength: 50),
                        Password = c.String(maxLength: 128),
                        TokenID = c.String(maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.RoleModel", t => t.RoleID)
                .Index(t => t.RoleID);
            
            CreateTable(
                "dbo.RoleModel",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.SessionModel",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        SessionKey = c.String(),
                        CreatedBy = c.String(),
                        CreatedDate = c.DateTime(),
                        UpdatedBy = c.String(),
                        UpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.UserModel", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SessionModel", "UserID", "dbo.UserModel");
            DropForeignKey("dbo.ParticipantModel", "UserID", "dbo.UserModel");
            DropForeignKey("dbo.UserModel", "RoleID", "dbo.RoleModel");
            DropForeignKey("dbo.ParticipantModel", "MeetingID", "dbo.MeetingModel");
            DropForeignKey("dbo.MeetingModel", "RoomID", "dbo.RoomModel");
            DropIndex("dbo.SessionModel", new[] { "UserID" });
            DropIndex("dbo.UserModel", new[] { "RoleID" });
            DropIndex("dbo.ParticipantModel", new[] { "UserID" });
            DropIndex("dbo.ParticipantModel", new[] { "MeetingID" });
            DropIndex("dbo.MeetingModel", new[] { "RoomID" });
            DropTable("dbo.SessionModel");
            DropTable("dbo.RoleModel");
            DropTable("dbo.UserModel");
            DropTable("dbo.ParticipantModel");
            DropTable("dbo.RoomModel");
            DropTable("dbo.MeetingModel");
            DropTable("dbo.LogModel");
            DropTable("dbo.ConfigModel");
        }
    }
}
