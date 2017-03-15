using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Repository;
using vm.Aspects.Wcf.Behaviors;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    [DIBehavior]
    [Tag(Service.PolicyName)]
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ServiceTasks : IServiceTasks
    {
        static Random _random = new Random(DateTime.UtcNow.Millisecond);
        IRepositoryAsync _repository;

        public ServiceTasks(
            IRepositoryAsync repository)
        {
            _repository = repository;
        }

        #region IServiceTasks
        public async Task AddNewEntityAsync()
        {
            Debug.WriteLine($"Method {nameof(AddNewEntityAsync)} using service with repository #{((TestRepository)_repository).Id}");

            _repository
                    .Add(CreateEntity(_random.Next(5)))
                    ;
        }
        public async Task<int> CountOfEntitiesAsync()
            => await _repository
                        .Entities<Entity>()
                        .CountAsync()
                        ;

        public async Task<ICollection<Entity>> GetEntitiesAsync(
            int skip,
            int take)
            => await _repository
                        .Entities<Entity>()
                        .FetchAlso(e => e.ValuesList)
                        .OrderBy(e => e.Id)
                        .Skip(skip)
                        .Take(take)
                        .ToListAsync()
                        ;

        public async Task UpdateEntitiesAsync()
        {
            Debug.WriteLine($"Method {nameof(UpdateEntitiesAsync)} using service with repository #{((TestRepository)_repository).Id}");

            var n = _random.Next(await CountOfEntitiesAsync() - 1);
            var e = (await GetEntitiesAsync(n, 1)).First();

            UpdateEntity(e, Math.Min(_random.Next(e.ValuesList.Count()-1), 0));
        }
        #endregion

        Entity CreateEntity(
            int numValues)
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

        void UpdateEntity(
            Entity e,
            int numValues)
        {
            e.Name = Facility.GuidGenerator.NewGuid().ToString("N");

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
            v.Name      = Facility.GuidGenerator.NewGuid().ToString("N");
            v.UpdatedOn = Facility.Clock.UtcNow;
        }
    }
}
