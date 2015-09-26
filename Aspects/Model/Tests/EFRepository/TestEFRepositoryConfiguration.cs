using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace vm.Aspects.Model.EFRepository.Tests
{
    public class TestEFRepositoryConfiguration : DbConfiguration
    {
        public TestEFRepositoryConfiguration()
        {
            SetProviderServices(
                SqlProviderServices.ProviderInvariantName,
                SqlProviderServices.Instance);
        }
    }
}
