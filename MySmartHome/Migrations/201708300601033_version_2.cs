namespace MySmartHome.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class version_2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Jablotron", "Note", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Jablotron", "Note", c => c.String(maxLength: 50));
        }
    }
}
