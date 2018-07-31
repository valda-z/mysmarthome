namespace MySmartHome.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20170821210700_version_1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventList",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DeviceId = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                        EventCode = c.String(maxLength: 50),
                        EventText = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Created);
            
            CreateTable(
                "dbo.Jablotron",
                c => new
                    {
                        DeviceId = c.Guid(nullable: false),
                        Note = c.String(maxLength: 50),
                        CommandToExecute = c.String(maxLength: 100),
                        LED_A = c.Boolean(nullable: false),
                        LED_B = c.Boolean(nullable: false),
                        LED_C = c.Boolean(nullable: false),
                        LED_Warning = c.Boolean(nullable: false),
                        State = c.String(maxLength: 50),
                        Contacted = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.DeviceId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.EventList", new[] { "Created" });
            DropTable("dbo.Jablotron");
            DropTable("dbo.EventList");
        }
    }
}
