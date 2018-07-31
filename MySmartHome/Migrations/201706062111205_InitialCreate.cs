namespace MySmartHome.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DeviceLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DeviceId = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                        DogHouseHeatingOn = c.Boolean(nullable: false),
                        DogHouseTemperature = c.Decimal(nullable: false, precision: 18, scale: 2),
                        WaterOn = c.Boolean(nullable: false),
                        IsWet = c.Boolean(nullable: false),
                        Temperature = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.DeviceId, t.Created }, name: "IX_DeviceIdCreated");
            
            CreateTable(
                "dbo.Device",
                c => new
                    {
                        DeviceId = c.Guid(nullable: false),
                        Note = c.String(maxLength: 50),
                        Config = c.String(maxLength: 1024),
                        DogHouseHeatingOn = c.Boolean(nullable: false),
                        DogHouseTemperature = c.Decimal(nullable: false, precision: 18, scale: 2),
                        WaterOn = c.Boolean(nullable: false),
                        IsWet = c.Boolean(nullable: false),
                        Temperature = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Contacted = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.DeviceId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.DeviceLog", "IX_DeviceIdCreated");
            DropTable("dbo.Device");
            DropTable("dbo.DeviceLog");
        }
    }
}
