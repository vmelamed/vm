namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "HiLoIdentity._HiLoIdentityGenerator",
                c => new
                    {
                        EntitySetName = c.String(nullable: false, maxLength: 100),
                        HighValue = c.Long(nullable: false),
                        MaxLowValue = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EntitySetName);
            
            CreateTable(
                "Test.Value",
                c => new
                    {
                        Id = c.Long(nullable: false),
                        RepositoryId = c.String(maxLength: 50),
                        EntityId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        UpdatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        ConcurrencyStamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("Test.Entity", t => t.EntityId, cascadeDelete: true)
                .Index(t => t.EntityId);
            
            CreateTable(
                "Test.Entity",
                c => new
                    {
                        Id = c.Long(nullable: false),
                        RepositoryId = c.String(maxLength: 64),
                        UniqueId = c.Guid(nullable: false),
                        CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        UpdatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        ConcurrencyStamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("Test.Value", "EntityId", "Test.Entity");
            DropIndex("Test.Value", new[] { "EntityId" });
            DropTable("Test.Entity");
            DropTable("Test.Value");
            DropTable("HiLoIdentity._HiLoIdentityGenerator");
        }
    }
}
