using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Repository;
using vm.Aspects.Threading;
using vm.Aspects.Wcf.Services;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    public sealed class ServiceInitializer : IInitializeService, IDisposable
    {
        IRepository _repository;

        public ServiceInitializer(
            [Dependency("transient")]
            IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            _repository = repository;
        }

        public bool IsInitialized { get; private set; }

        public bool Initialize(
            ServiceHost host,
            string messagingPattern,
            int maxWaitTime) => InitializeAsync(host, messagingPattern, maxWaitTime).Result;

        public async Task<bool> InitializeAsync(
            ServiceHost host,
            string messagingPattern,
            int maxWaitTime)
        {
            try
            {
                // force initialization of the exception manager
                Facility.ExceptionManager.Policies.Count();

                await TaskCombinators.RetryOnFault(

                    // the initialization function:
                    async () =>
                    {
                        using (var repository = ServiceLocator.Current.GetInstance<IRepository>("transient"))
                            await repository.InitializeAsync();

                        return IsInitialized = true;
                    },

                    // times to retry:
                    3,

                    // process exceptions:
                    async x =>
                    {
                        if (x!=null  && !x.IsTransient())
                        {
                            Facility.LogWriter
                                    .ExceptionError(x);
                            return false;
                        }

                        var delay = Task.Delay(100);

                        Facility.LogWriter
                                .ExceptionWarning(x);

                        await delay;
                        return true;
                    }
                );

                return IsInitialized;
            }
            catch (Exception x)
            {
                Exception y;

                if (Facility.ExceptionManager.HandleException(x, ExceptionPolicyProvider.LogAndRethrowPolicyName, out y)  &&  y != null)
                    throw y;

                throw;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _repository.Dispose();
        }
    }
}
