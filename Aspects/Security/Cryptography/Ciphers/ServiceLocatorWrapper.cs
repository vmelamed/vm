using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Practices.ServiceLocation;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The DefaultServiceLocator wraps the <see cref="Microsoft.Practices.ServiceLocation.ServiceLocator"/>.
    /// The idea is that if the users have IoC container which has implementation of <see cref="Microsoft.Practices.ServiceLocation.IServiceLocator"/>
    /// (as most do - see https://commonservicelocator.codeplex.com/) they could put their implementation of some required services in the container 
    /// of their choice, otherwise in either case the dumper will resolve the services from this class.
    /// </summary>
    internal class ServiceLocatorWrapper : IServiceLocator
    {
        static readonly Lazy<ServiceLocatorWrapper> _default = new Lazy<ServiceLocatorWrapper>(() => new ServiceLocatorWrapper());

        public static ServiceLocatorWrapper Default
        {
            get { return _default.Value; }
        }

        #region IServiceLocator Members
        public IEnumerable<TService> GetAllInstances<TService>()
        {
            Contract.Ensures(Contract.Result<IEnumerable<TService>>() != null);

            try
            {
                // try the CSL (user's or local)
                return Current.GetAllInstances<TService>();
            }
            catch (ActivationException)
            {
                // This is the case when there is user's CSL but it doesn't register TService.
                // if not found - search directly in the internal service resolver.
                return ServiceResolver.Current.GetAllInstances<TService>();
            }
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);

            try
            {
                // try the CSL (user's or local)
                return Current.GetAllInstances(serviceType);
            }
            catch (ActivationException)
            {
                // This is the case when there is user's CSL but it doesn't register serviceType.
                // if not found - search directly in the internal service resolver.
                return ServiceResolver.Current.GetAllInstances(serviceType);
            }
        }

        public TService GetInstance<TService>(string key)
        {
            try
            {
                // try the CSL (user's or local)
                return Current.GetInstance<TService>(key);
            }
            catch (ActivationException)
            {
                // This is the case when there is user's CSL but it doesn't register TService.
                // if not found - search directly in the internal service resolver.
                return ServiceResolver.Current.GetInstance<TService>(key);
            }
        }

        public TService GetInstance<TService>()
        {
            try
            {
                // try the CSL (user's or local)
                return Current.GetInstance<TService>();
            }
            catch (ActivationException)
            {
                // This is the case when there is user's CSL but it doesn't register TService.
                // if not found - search directly in the internal service resolver.
                return ServiceResolver.Current.GetInstance<TService>();
            }
        }

        public object GetInstance(Type serviceType, string key)
        {
            Contract.Ensures(Contract.Result<object>() != null);

            try
            {
                // try the CSL (user's or local)
                return Current.GetInstance(serviceType, key);
            }
            catch (ActivationException)
            {
                // This is the case when there is user's CSL but it doesn't register serviceType.
                // if not found - search directly in the internal service resolver.
                return ServiceResolver.Current.GetInstance(serviceType, key);
            }
        }

        public object GetInstance(Type serviceType)
        {
            Contract.Ensures(Contract.Result<object>() != null);

            try
            {
                // try the CSL (user's or local)
                return Current.GetInstance(serviceType);
            }
            catch (ActivationException)
            {
                // This is the case when there is user's CSL but it doesn't register serviceType.
                // if not found - search directly in the internal service resolver.
                return ServiceResolver.Current.GetInstance(serviceType);
            }
        }
        #endregion

        #region IServiceProvider Members
        /// <summary>
        /// Gets the service object of the specified type. Not used internally
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type <paramref name="serviceType" />.
        /// </returns>
        public object GetService(Type serviceType)
        {
            Contract.Ensures(Contract.Result<object>() != null);

            try
            {
                // try the CSL (user's or local)
                return Current.GetService(serviceType);
            }
            catch (ActivationException)
            {
                // This is the case when there is user's CSL but it doesn't register TService.
                // if not found - search directly in the internal service resolver.
                return ServiceResolver.Current.GetService(serviceType);
            }
        }
        #endregion

        static IServiceLocator _current;

        public static IServiceLocator Current
        {
            get
            {
                Contract.Ensures(Contract.Result<IServiceLocator>() != null);

                if (_current == null)
                    try
                    {
                        _current = ServiceLocator.Current;
                    }
                    catch (InvalidOperationException)
                    {
                        _current = ServiceResolver.Current;
                    }

                return _current;
            }
        }

        /// <summary>
        /// Resets the cached service locator. Used in unit tests only.
        /// </summary>
        public static void Reset()
        {
            _current = null;
            if (ServiceLocator.IsLocationProviderSet)
                ServiceLocator.SetLocatorProvider(null);
        }
    }
}
