using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Practices.ServiceLocation;

using Unity.Interception.PolicyInjection.MatchingRules;

using vm.Aspects.Facilities;
using vm.Aspects.Model.Repository;
using vm.Aspects.Threading;
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
        Lazy<IRepository> _repository;

        public IRepository Repository => _repository.Value;

        public TestServiceTasks(
            Lazy<IRepository> repository)
        {
            _repository = repository;
        }

        #region ITestServiceTasks
        public async Task AddNewEntityAsync()
        {
            await Task.Run(
                () => Repository.Add(CreateEntity(_random.Next(5))));
        }
        public async Task<int> CountOfEntitiesAsync()
            => await Repository
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
            => await Repository
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
            ThrowRandomException();
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
            ThrowRandomException();
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
            ThrowRandomException(e);
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
                UpdateValue(e, v);
        }

        void UpdateValue(
            Entity e,
            Value v)
        {
            ThrowRandomException(e, v);
            v.UpdatedOn    = Facility.Clock.UtcNow;
        }

        static ReaderWriterLockSlim _sync = new ReaderWriterLockSlim();
        static DateTime _when = default(DateTime);

        static void ThrowRandomException(
            Entity e = null,
            Value v = null)
        {
            using (_sync.UpgradableReaderLock())
                if (_when == default(DateTime))
                {
                    using (_sync.WriterLock())
                        _when = DateTime.Now.AddMilliseconds(_random.Next(500));
                    return;
                }

            using (_sync.UpgradableReaderLock())
                if (_when < DateTime.Now)
                    using (_sync.WriterLock())
                    {
                        _when = DateTime.Now.AddMilliseconds(_random.Next(2000));
                        ThrowException(e, v);
                    }
        }

        static void ThrowException(
            Entity e,
            Value v)
        {
            var i = _random.Next(e!=null ? 5 : 4);

            switch (i)
            {
            case 0:
                throw new OverflowException("This is a random exception.");

            case 1:
                throw new UnauthorizedAccessException("This is a random exception.");

            case 2:
                throw new InvalidOperationException("This is a random exception.");

            case 3:
                throw new Exception("This is a random exception.");

            case 4:
                UpdateCurrentRecord(e, v);
                break;
            }
        }

        static void UpdateCurrentRecord(
            Entity e,
            Value v)
        {
            if (e == null)
                return;

            try
            {
                using (var repository = ServiceLocator.Current.GetInstance<IRepository>("transient"))
                {
                    var ent = repository
                                .Entities<Entity>()
                                .Where(x => x.Id == e.Id)
                                .FetchAlso(x => x.ValuesList)
                                .FirstOrDefault();

                    if (v == null)
                        ent.RepositoryId = Facility.GuidGenerator.NewGuid().ToString("N");
                    else
                        ent.ValuesList
                            .FirstOrDefault(y => y.RepositoryId == v.RepositoryId)
                            .RepositoryId = Facility.GuidGenerator.NewGuid().ToString("N");

                    repository.CommitChanges();
                }
            }
            catch (Exception x)
            {
                Debug.WriteLine("Exception in UpdateCurrentRecord");
                Debug.WriteLine(x.DumpString(2));
            }
        }
    }
}
