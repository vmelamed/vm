using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using vm.Aspects.Wcf;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    class Program
    {
        public static OptimisticConcurrencyStrategy OptimisticConcurrencyStrategy { get; set; }

        static void Main(string[] args)
        {
            try
            {
                var sw = new Stopwatch();

                OptimisticConcurrencyStrategy = args.Any(a => a.StartsWith("cl"))
                                                    ? OptimisticConcurrencyStrategy.ClientWins
                                                    : args.Any(a => a.StartsWith("st"))
                                                        ? OptimisticConcurrencyStrategy.StoreWins
                                                        : OptimisticConcurrencyStrategy.None;

                Register();
                StartServices();
                Initialize();

                sw.Start();

                if (args.Any(a => a.StartsWith("a")))
                    RunAsync().Wait();
                else
                    RunSync();

                sw.Stop();
                Console.WriteLine($@"Test duration: {sw.Elapsed:d\.hh\.mm\.ss\.fffffff}");
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.DumpString());
                Console.WriteLine(x.DumpString());
            }

            Console.Write("Press any key to finish...");
            Console.ReadKey(true);
        }

        static void StartServices()
        {
            var testServiceHostFactory      = new TestServiceHostFactory();
            var testServiceTasksHostFactory = new TestServiceTasksHostFactory();
            var address                     = new Uri("net.tcp://localhost:14808/");

            var testServiceHost      = testServiceHostFactory
                                            .CreateHost(address)
                                            ;
            var testServiceTasksHost = testServiceTasksHostFactory
                                            .CreateHost(address)
                                            ;

            testServiceHost.Open();
            testServiceTasksHost.Open();

            Console.WriteLine($"\nA service host for {testServiceHost.Description.ServiceType.Name} has started and is listening on:");
            foreach (var ep in testServiceHost.Description.Endpoints)
                Console.WriteLine("    {0}", ep.ListenUri.AbsoluteUri.ToString());

            Console.WriteLine($"\nA service host for {testServiceTasksHost.Description.ServiceType.Name} has started and is listening on:");
            foreach (var ep in testServiceTasksHost.Description.Endpoints)
                Console.WriteLine("    {0}", ep.ListenUri.AbsoluteUri.ToString());
        }

        static void Register()
        {
            lock (DIContainer.Root)
            {
                var registrations = DIContainer.Root.GetRegistrationsSnapshot();

                DIContainer.Root
                    .RegisterTypeIfNot<ITestService, TestServiceClient>(registrations, "client", new InjectionFactory(c => new TestServiceClient("net.tcp://localhost:14808/TestService.svc", ServiceIdentity.None, "")))
                    .RegisterTypeIfNot<ITestServiceTasks, TestServiceTasks>(registrations, "client", new InjectionFactory(c => new TestServiceTasksClient("net.tcp://localhost:14808/TestServiceTasks.svc", ServiceIdentity.None, "")))
                    ;

                DIContainer.Root
                    .Configure<Interception>()
                    .AddPolicy(nameof(TestService))
                    .AddMatchingRule<TagAttributeMatchingRule>(
                            new InjectionConstructor(nameof(TestService), false))

                    .AddCallHandler<UnitOfWorkCallHandler>()
                    ;
            }

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
            Debug.WriteLine("-------------------------------");
        }

        const int NumberOfTasks    = 100;
        const int ConcurrencyLevel = 8;

        static void RunSync()
        {
            ITestService client = null;
            var successful = 0;
            var failed     = 0;

            // initially add 100 entities
            for (var i = 0; i<NumberOfTasks; i++)
                try
                {
                    if (client == null)
                        client = ServiceLocator.Current.GetInstance<ITestService>("client");
                    client.AddNewEntity();
                    successful++;
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.DumpString());
                    client.Dispose();
                    client = null;
                    failed++;
                }

            for (var i = 0; i<NumberOfTasks; i++)
                try
                {
                    if (client == null)
                        client = ServiceLocator.Current.GetInstance<ITestService>("client");
                    client.UpdateEntities();
                    successful++;
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.DumpString());
                    client.Dispose();
                    client = null;
                    failed++;
                }

            if (client == null)
                client = ServiceLocator.Current.GetInstance<ITestService>("client");

            var counts = client.GetCounts();

            Debug.WriteLine($"Successfully made {successful}/{NumberOfTasks*2} synchronous calls. {failed}/{NumberOfTasks*2} calls failed.");
            Console.WriteLine($"Successfully made {successful}/{NumberOfTasks*2} synchronous calls. {failed}/{NumberOfTasks*2} calls failed.");
            Console.WriteLine($"The service created {counts.Entities} entities and {counts.Values} values.");
            Console.WriteLine($"There are {client.CountOfEntities()} entities and {client.CountOfValues()} values in the database.");
        }

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
                    {
                        Task task = null;

                        try
                        {
                            task = await Task.WhenAny(tasks);

                            successful++;
                        }
                        catch (AggregateException x)
                        {
                            Debug.WriteLine(x.DumpString());
                            failed++;
                        }

                        if (task != null)
                            tasks.Remove(task);
                    }
                }
            }
            catch (AggregateException x)
            {
                Debug.WriteLine(x.DumpString());
            }

            Debug.WriteLine($"Successfully made {successful}/{NumberOfTasks*2} synchronous calls. {failed}/{NumberOfTasks*2} calls failed.");
            Console.WriteLine($"Successfully made {successful}/{NumberOfTasks*2} synchronous calls. {failed}/{NumberOfTasks*2} calls failed.");

            var client = GetAsyncClient();
            var counts = client.GetCountsAsync().Result;

            Console.WriteLine($"The service created {counts.Entities} entities and {counts.Values} values.");
            Console.WriteLine($"There are {await client.CountOfEntitiesAsync()} entities and {await client.CountOfValuesAsync()} values in the database.");
        }

        static async Task GetTask(int index)
        {
            try
            {
                ITestServiceTasks client = GetAsyncClient();

                if (index < NumberOfTasks)
                    await client.AddNewEntityAsync();
                else
                    await client.UpdateEntitiesAsync();

                ReleaseAsyncClient(client);
            }
            catch (Exception)
            {
                availableClients.Dispose();
                throw;
            }
        }

        static object _sync = new object();
        static Queue<ITestServiceTasks> availableClients = new Queue<ITestServiceTasks>();

        static ITestServiceTasks GetAsyncClient()
        {
            if (availableClients.Any())
                lock (_sync)
                    if (availableClients.Any())
                        return availableClients.Dequeue();

            return ServiceLocator.Current.GetInstance<ITestServiceTasks>("client");
        }

        static void ReleaseAsyncClient(ITestServiceTasks client)
        {
            lock (_sync)
                availableClients.Enqueue(client);
        }
    }
}
