using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// The DefaultServiceLocator wraps the <see cref="Microsoft.Practices.ServiceLocation.ServiceLocator"/>.
    /// The idea is that if the users have IoC container which has implementation of <see cref="Microsoft.Practices.ServiceLocation.IServiceLocator"/>
    /// (as most do - see https://commonservicelocator.codeplex.com/) they could put their implementation of some required services in the container 
    /// of their choice, otherwise in either case the dumper will resolve the services from this class.
    /// </summary>
    class DefaultServiceLocator : IServiceLocator
    {
        #region IServiceLocator Members

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return GetService().GetAllInstances<TService>();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return GetService().GetAllInstances(serviceType);
        }

        public TService GetInstance<TService>(string key)
        {
            return GetService().GetInstance<TService>(key);
        }

        public TService GetInstance<TService>()
        {
            return GetService().GetInstance<TService>();
        }

        public object GetInstance(Type serviceType, string key)
        {
            return GetService().GetInstance(serviceType, key);
        }

        public object GetInstance(Type serviceType)
        {
            return GetService().GetInstance(serviceType);
        }

        #endregion

        #region IServiceProvider Members
        /// <summary>
        /// Gets the service object of the specified type. Not used internally
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type <paramref name="serviceType" />.</returns>
        public object GetService(Type serviceType)
        {
            if (serviceType != typeof(IServiceLocator))
                return null;

            if (ServiceLocator.Current != null)
                return ServiceLocator.Current;
            else
                return ServiceResolver.Default;
        }

        #endregion

        public IServiceLocator GetService()
        {
            return (IServiceLocator)GetService(typeof(IServiceLocator));
        }
    }
}
