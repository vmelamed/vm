using System.Collections.Generic;
using System.Threading.Tasks;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    public interface IServiceTasks
    {
        Task AddNewEntityAsync();

        Task UpdateEntitiesAsync();

        Task<int> CountOfEntitiesAsync();

        Task<ICollection<Entity>> GetEntitiesAsync(int skip, int take);
    }
}
