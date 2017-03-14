using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Register();
            Initialize();
            Run();
        }

        static void Register()
        {
            var container = DIContainer.Initialize();
            var registrations = container.GetRegistrationsSnapshot();

            container
                .Register(Facility.Registrar)
                .Register(Repository.Registrar)
                .RegisterTypeIfNot<IService, Service>(
                        new Interceptor<InterfaceInterceptor>(),
                        new InterceptionBehavior<PolicyInjectionBehavior>())
                .RegisterTypeIfNot<IServiceTasks, Service>(
                        new Interceptor<InterfaceInterceptor>(),
                        new InterceptionBehavior<PolicyInjectionBehavior>())
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
            var repository = ServiceLocator.Current.GetInstance<IRepository>();

            repository.Initialize();
        }

        static async Task InitializeAsync()
        {
            var repository = ServiceLocator.Current.GetInstance<IRepositoryAsync>();

            await repository.InitializeAsync();
        }

        static void Run()
        {
            // initially add 100 entities
            for (var i = 0; i<100; i++)
            {
                var service = ServiceLocator.Current.GetInstance<IService>();

                service.AddNewEntity();
            }

            for (var i = 0; i<100; i++)
            {
                var service = ServiceLocator.Current.GetInstance<IService>();

                service.UpdateEntities();
            }
        }

        static async Task RunAsync()
        {
        }
    }
}
