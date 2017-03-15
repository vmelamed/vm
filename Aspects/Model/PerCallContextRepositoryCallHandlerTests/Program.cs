using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Register();
                Initialize();
                //Run();
                RunAsync().Wait();
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.DumpString());
            }
        }

        static void Register()
        {
            var container = DIContainer.Initialize();
            var registrations = container.GetRegistrationsSnapshot();

            container
                .UnsafeRegister(Facility.Registrar, registrations)
                .UnsafeRegister(TestRepository.Registrar, registrations)
                .RegisterTypeIfNot<IService, ServiceClient>(registrations, "client", new InjectionFactory(c => new ServiceClient()))
                .RegisterTypeIfNot<IServiceTasks, ServiceTasks>(registrations, "client", new InjectionFactory(c => new ServiceTasksClient()))
                ;

            var interception = container
                                    .AddNewExtension<Interception>()
                                    .Configure<Interception>()
                                    ;

            interception
                .AddPolicy(nameof(Service))
                .AddMatchingRule<TagAttributeMatchingRule>(
                        new InjectionConstructor(nameof(Service), false))

                .AddCallHandler<PerCallContextRepositoryCallHandler>()
                ;

            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                DIContainer.Root.Dump(writer);
                Debug.Print(
$@"
===============================
{writer.GetStringBuilder()}
===============================
");

                Debug.WriteLine("");
            }
        }

        static void Initialize()
        {
            using (var repository = ServiceLocator.Current.GetInstance<IRepository>("transient"))
                repository.Initialize();

            Debug.WriteLine("-------------------------------");
        }

        static async Task InitializeAsync()
        {
            using (var repository = ServiceLocator.Current.GetInstance<IRepositoryAsync>("transient"))
                await repository.InitializeAsync();
        }

        static void Run()
        {
            // initially add 100 entities
            for (var i = 0; i<100; i++)
                AddEntity();

            for (var i = 0; i<100; i++)
                UpdateEntity();
        }

        static void AddEntity()
        {
            var service = ServiceLocator.Current.GetInstance<IService>();

            service.AddNewEntity();
        }

        static void UpdateEntity()
        {
            var service = ServiceLocator.Current.GetInstance<IService>();

            service.UpdateEntities();
        }

        const int NumberOfTasks    = 100;
        const int ConcurrencyLevel = 1;

        static async Task RunAsync()
        {
            var tasks      = new List<Task>(ConcurrencyLevel);
            var n          = 0;
            var successful = 0;
            var failed     = 0;

            try
            {
                while (n < 2*NumberOfTasks || tasks.Any())
                {
                    if (n < 2*NumberOfTasks)
                        tasks.Add(GetTask(n++));

                    if (tasks.Count() == ConcurrencyLevel || n >= 2*NumberOfTasks)
                        try
                        {
                            var task = await Task.WhenAny(tasks);

                            tasks.Remove(task);
                            if (task.IsFaulted)
                                throw task.Exception;
                            successful++;
                        }
                        catch (AggregateException x)
                        {
                            Debug.WriteLine(x.DumpString());
                            failed++;
                        }
                }
            }
            catch (AggregateException x)
            {
                Debug.WriteLine(x.DumpString());
            }

            Debug.WriteLine($"Successfully executed {successful} tasks and {failed} failed");
        }

        static Task GetTask(int index)
        {
            if (index < NumberOfTasks)
                return AddEntityAsync();

            if (index < 2*NumberOfTasks)
                return UpdateEntityAsync();

            return null;
        }

        static Task AddEntityAsync()
        {
            var asyncService = ServiceLocator.Current.GetInstance<IServiceTasks>();

            return asyncService.AddNewEntityAsync();
        }

        static Task UpdateEntityAsync()
        {
            var asyncService = ServiceLocator.Current.GetInstance<IServiceTasks>();

            return asyncService.UpdateEntitiesAsync();
        }
    }
}
