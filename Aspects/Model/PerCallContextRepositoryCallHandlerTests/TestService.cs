using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
using System.Threading;
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
        static int _entities;
        static int _values;
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

        public int CountOfValues()
            => Repository
                    .Values<Value>()
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

        public EntitiesAndValuesCountsDto GetCounts()
        {
            var counts = new EntitiesAndValuesCountsDto
            {
                Entities = _entities,
                Values   = _values,
            };

            Interlocked.Exchange(ref _entities, 0);
            Interlocked.Exchange(ref _values, 0);

            return counts;
        }
        #endregion

        Entity CreateEntity(
            int numValues)
        {
            var e = Repository.CreateEntity<Entity>();

            e.Id           = Repository.GetStoreId<Entity, long>();
            e.UniqueId     = Facility.GuidGenerator.NewGuid();
            e.CreatedOn    =
            e.UpdatedOn    = Facility.Clock.UtcNow;

            for (var i = 0; i<numValues; i++)
            {
                var v = CreateValue();

                e.ValuesList.Add(v);
                v.Entity = e;
            }
            Interlocked.Increment(ref _entities);

            return e;
        }

        Value CreateValue()
        {
            var v = Repository.CreateValue<Value>();

            v.Id           = Repository.GetStoreId<Value, long>();
            v.CreatedOn    =
            v.UpdatedOn    = Facility.Clock.UtcNow;
            Interlocked.Increment(ref _values);

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
            v.UpdatedOn    = Facility.Clock.UtcNow;
        }
    }
}
