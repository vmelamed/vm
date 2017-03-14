using System.Data.Entity.Migrations;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Repository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled        = true;
            AutomaticMigrationDataLossAllowed = false;
            ContextKey                        = "PerCallContextRepositoryCallHandlerTest";
            MigrationsDirectory               = @"Migrations";
        }

        protected override void Seed(Repository context)
        {
        }
    }
}
