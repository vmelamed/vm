using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    [Tag(nameof(Service))]
    public class Service : IService, IServiceTasks
    {
        static Random _random = new Random(DateTime.UtcNow.Millisecond);
        IRepositoryAsync _repository;

        public Service(IRepositoryAsync repository)
        {
            _repository = repository;
        }

        #region IService
        public void AddNewEntity()
            => _repository
                    .Add(CreateEntity())
                    ;

        public int CountOfEntities()
            => _repository
                    .Entities<Entity>()
                    .Count()
                    ;

        public ICollection<Entity> GetEntities(int skip, int take)
            => _repository
                    .Entities<Entity>()
                    .FetchAlso(e => e.ValuesList)
                    .Skip(skip)
                    .Take(take)
                    .ToList()
                    ;

        public void UpdateEntities()
        {
            var n = CountOfEntities();
            var n1 = _random.Next(n-1);
            var n2 = _random.Next(n-1);
            var skip = Math.Min(n1, n2);
            var take = Math.Max(n1, n2) - skip;

            foreach (var e in GetEntities(skip, take))
                UpdateEntity(e, _random.Next(n-1));
        }
        #endregion

        #region IServiceAsync
        public async Task AddNewEntityAsync()
            => _repository
                    .Add(CreateEntity())
                    ;

        public async Task<int> CountOfEntitiesAsync()
            => await _repository
                        .Entities<Entity>()
                        .CountAsync()
                        ;

        public async Task<ICollection<Entity>> GetEntitiesAsync(int skip, int take)
            => await _repository
                        .Entities<Entity>()
                        .FetchAlso(e => e.ValuesList)
                        .Skip(skip)
                        .Take(take)
                        .ToListAsync()
                        ;

        public async Task UpdateEntitiesAsync()
        {
            var n = await CountOfEntitiesAsync();
            var n1 = _random.Next(n-1);
            var n2 = _random.Next(n-1);
            var skip = Math.Min(n1, n2);
            var take = Math.Max(n1, n2) - skip;

            foreach (var e in await GetEntitiesAsync(skip, take))
                UpdateEntity(e, _random.Next(n-1));
        }
        #endregion

        Entity CreateEntity(int numValues = 3)
        {
            var e = _repository.CreateEntity<Entity>();

            e.Id        = _repository.GetStoreId<Entity, long>();
            e.UniqueId  = Facility.GuidGenerator.NewGuid();
            e.Name      = Facility.GuidGenerator.NewGuid().ToString("N");
            e.CreatedOn =
            e.UpdatedOn = Facility.Clock.UtcNow;

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
            var v = _repository.CreateValue<Value>();

            v.Id        = _repository.GetStoreId<Value, long>();
            v.Name      = Facility.GuidGenerator.NewGuid().ToString("N");
            v.CreatedOn =
            v.UpdatedOn = Facility.Clock.UtcNow;

            return v;
        }

        void UpdateEntity(Entity e, int numValues = 2)
        {
            e.Name = Facility.GuidGenerator.NewGuid().ToString("N");

            for (var i = 0; i<numValues; i++)
            {
                var v = CreateValue();

                e.ValuesList.Add(v);
                v.Entity = e;
            }

            e.UpdatedOn = Facility.Clock.UtcNow;
        }

        void UpdateValues(Entity e, int skip, int take)
        {
            foreach (var v in e.ValuesList.Skip(skip).Take(take).ToList())
                UpdateValue(v);
        }

        void UpdateValue(Value v)
        {
            v.Name      = Facility.GuidGenerator.NewGuid().ToString("N");
            v.UpdatedOn = Facility.Clock.UtcNow;
        }
    }
}
