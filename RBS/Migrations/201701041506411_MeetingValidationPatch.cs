namespace RBS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MeetingValidationPatch : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.MeetingModel", "BookingDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.MeetingModel", "StartingTime", c => c.String(nullable: false));
            AlterColumn("dbo.MeetingModel", "EndingTime", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.MeetingModel", "EndingTime", c => c.String());
            AlterColumn("dbo.MeetingModel", "StartingTime", c => c.String());
            AlterColumn("dbo.MeetingModel", "BookingDate", c => c.DateTime());
        }
    }
}
