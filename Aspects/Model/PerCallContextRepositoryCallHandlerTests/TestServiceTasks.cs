using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
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
    public class TestServiceTasks : ITestServiceTasks, IHasRepository
    {
        static int _entities;
        static int _values;
        static Random _random = new Random(DateTime.UtcNow.Millisecond);
        Lazy<IRepositoryAsync> _repository;

        public IRepository Repository => _repository.Value;

        public IRepositoryAsync AsyncRepository => _repository.Value;

        public TestServiceTasks(
            Lazy<IRepositoryAsync> repository)
        {
            _repository = repository;
        }

        #region ITestServiceTasks
        public async Task AddNewEntityAsync()
        {
            await Task.Run(
                () => AsyncRepository.Add(CreateEntity(_random.Next(5))));
        }
        public async Task<int> CountOfEntitiesAsync()
            => await AsyncRepository
                        .Entities<Entity>()
                        .CountAsync()
                        ;

        public Task<int> CountOfValuesAsync()
            => Repository
                    .Values<Value>()
                    .CountAsync()
                    ;

        public async Task<ICollection<Entity>> GetEntitiesAsync(
            int skip,
            int take)
            => await AsyncRepository
                        .Entities<Entity>()
                        .FetchAlso(e => e.ValuesList)
                        .OrderBy(e => e.Id)
                        .Skip(skip)
                        .Take(take)
                        .ToListAsync()
                        ;

        public async Task UpdateEntitiesAsync()
        {
            var countOfEntities = await CountOfEntitiesAsync();

            var n = countOfEntities > 0 ? _random.Next(countOfEntities-1) : 0;
            var e = (await GetEntitiesAsync(n, 1)).First();

            UpdateEntity(e, _random.Next(Math.Max(e.ValuesList.Count()-1, 0)));
        }

        public Task<EntitiesAndValuesCountsDto> GetCountsAsync()
        {
            var counts = new EntitiesAndValuesCountsDto
            {
                Entities = _entities,
                Values   = _values,
            };

            Interlocked.Exchange(ref _entities, 0);
            Interlocked.Exchange(ref _values, 0);

            return Task.FromResult(counts);
        }
        #endregion

        Entity CreateEntity(
            int numValues)
        {
            var e = AsyncRepository.CreateEntity<Entity>();

            e.Id           = AsyncRepository.GetStoreId<Entity, long>();
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
            var v = AsyncRepository.CreateValue<Value>();

            v.Id           = AsyncRepository.GetStoreId<Value, long>();
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
