namespace MySmartHome.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class version_3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Device", "Config", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Device", "Config", c => c.String(maxLength: 1024));
        }
    }
}
