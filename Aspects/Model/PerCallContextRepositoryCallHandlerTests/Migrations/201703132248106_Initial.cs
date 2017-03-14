namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests.Migrations
{
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
                "dbo.Entity",
                c => new
                {
                    Id = c.Long(nullable: false),
                    UniqueId = c.Guid(nullable: false),
                    Name = c.String(maxLength: 64),
                    CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    UpdatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Value",
                c => new
                {
                    Id = c.Long(nullable: false),
                    Name = c.String(maxLength: 50),
                    EntityId = c.Long(nullable: false),
                    CreatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    UpdatedOn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entity", t => t.EntityId, cascadeDelete: true)
                .Index(t => t.EntityId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.Value", "EntityId", "dbo.Entity");
            DropIndex("dbo.Value", new[] { "EntityId" });
            DropTable("dbo.Value");
            DropTable("dbo.Entity");
            DropTable("HiLoIdentity._HiLoIdentityGenerator");
        }
    }
}
