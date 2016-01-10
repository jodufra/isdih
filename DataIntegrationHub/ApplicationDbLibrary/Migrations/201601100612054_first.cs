namespace ApplicationDbLibrary.Entities.Context
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class first : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.records",
                c => new
                    {
                        IdRecord = c.Int(nullable: false, identity: true),
                        NodeId = c.Int(nullable: false),
                        Channel = c.String(nullable: false, unicode: false),
                        Value = c.Single(nullable: false),
                        DateCreated = c.DateTime(nullable: false, precision: 0),
                    })
                .PrimaryKey(t => t.IdRecord);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.records");
        }
    }
}
