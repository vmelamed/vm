using System.Data.Entity.Migrations;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<TestRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled        = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey                        = "PerCallContextRepositoryCallHandlerTest";
            MigrationsDirectory               = @"Migrations";
        }

        protected override void Seed(TestRepository context)
        {
        }
    }
}
