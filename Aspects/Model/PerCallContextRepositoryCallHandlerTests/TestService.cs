using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Repository;
using vm.Aspects.Wcf.Behaviors;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    [DIBehavior]
    [Tag(TestService.PolicyName)]
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public partial class TestService : ITestService, IHasRepository
    {
        static Random _random = new Random(DateTime.UtcNow.Millisecond);
        Lazy<IRepository> _repository;

        IRepository Repository => _repository.Value;

        public TestService(
            Lazy<IRepository> repository)
        {
            _repository = repository;
        }

        #region IHasRepository
        IRepository IHasRepository.Repository => Repository;

        public IRepositoryAsync AsyncRepository => null;
        #endregion

        #region IService
        public void AddNewEntity()
            => Repository
                    .Add(CreateEntity(_random.Next(5)))
                    ;

        public int CountOfEntities()
            => Repository
                    .Entities<Entity>()
                    .Count()
                    ;

        public ICollection<Entity> GetEntities(
            int skip,
            int take)
            => Repository
                    .Entities<Entity>()
                    .FetchAlso(e => e.ValuesList)
                    .OrderBy(e => e.Id)
                    .Skip(skip)
                    .Take(take)
                    .ToList()
                    ;

        public void UpdateEntities()
        {
            var n = _random.Next(CountOfEntities()-1);
            var e = GetEntities(n, 1).First();

            UpdateEntity(e, _random.Next(5));
        }
        #endregion

        Entity CreateEntity(
            int numValues)
        {
            var e = Repository.CreateEntity<Entity>();

            e.Id           = Repository.GetStoreId<Entity, long>();
            e.UniqueId     = Facility.GuidGenerator.NewGuid();
            e.RepositoryId = ((TestRepository)Repository).Id.ToString("N");
            e.CreatedOn    =
            e.UpdatedOn    = Facility.Clock.UtcNow;

            for (var i = 0; i<numValues; i++)
            {
                var v = CreateValue();

                e.ValuesList.Add(v);
                v.Entity = e;
            }

            return e;
        }

        Value CreateValue()
        {
            var v = Repository.CreateValue<Value>();

            v.Id           = Repository.GetStoreId<Value, long>();
            v.RepositoryId = ((TestRepository)Repository).Id.ToString("N");
            v.CreatedOn    =
            v.UpdatedOn    = Facility.Clock.UtcNow;

            return v;
        }

        void UpdateEntity(
            Entity e,
            int numValues)
        {
            e.RepositoryId = Facility.GuidGenerator.NewGuid().ToString("N");

            UpdateValues(e, 0, e.ValuesList.Count()-1);
            for (var i = 0; i<numValues; i++)
            {
                var v = CreateValue();

                e.ValuesList.Add(v);
                v.Entity = e;
            }

            e.UpdatedOn = Facility.Clock.UtcNow;
        }

        void UpdateValues(
            Entity e,
            int skip,
            int take)
        {
            foreach (var v in e.ValuesList
                               .OrderBy(v => v.Id)
                               .Skip(skip)
                               .Take(take)
                               .ToList())
                UpdateValue(v);
        }

        void UpdateValue(
            Value v)
        {
            v.RepositoryId = ((TestRepository)Repository).Id.ToString("N");
            v.UpdatedOn    = Facility.Clock.UtcNow;
        }
    }
}
