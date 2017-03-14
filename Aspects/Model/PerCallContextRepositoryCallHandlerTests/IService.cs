using System.Collections.Generic;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    public interface IService
    {
        void AddNewEntity();

        void UpdateEntities();

        int CountOfEntities();

        ICollection<Entity> GetEntities(int skip, int take);
    }
}
