using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.InMemory
{
    public sealed partial class ListObjectsRepository
    {
        /// <summary>
        /// The objects repository registrar instance.
        /// </summary>
        static readonly ListObjectsRepositoryRegistrar _registrar = new ListObjectsRepositoryRegistrar();

        /// <summary>
        /// Gets the facilities registrar instance.
        /// </summary>
        public static ContainerRegistrar Registrar
        {
            get
            {

                return _registrar;
            }
        }

        /// <summary>
        /// Class ObjectRepositoryRegistrar. Registers the object repository related types.
        /// </summary>
        internal class ListObjectsRepositoryRegistrar : ContainerRegistrar
        {
            /// <summary>
            /// Does the actual work of the registration.
            /// The method is called from a synchronized context, i.e. does not need to be thread safe.
            /// </summary>
            /// <param name="container">The container where to register the defaults.</param>
            /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
            /// <exception cref="System.NotImplementedException"></exception>
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                container
                    .RegisterInstanceIfNot<IOrmSpecifics>(registrations, new ObjectsRepositorySpecifics())
                    .RegisterTypeIfNot<IRepository, ListObjectsRepository>(registrations)
                    ;
            }
        }
    }
}
