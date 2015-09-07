using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Unity;

namespace vm.Aspects
{
    public static partial class DIContainer
    {
        /// <summary>
        /// Registers the types and instances of the <see cref="T:ContainerRegistrar"/> in the specified container.
        /// The method is thread safe.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="registrar">The registrar.</param>
        /// <param name="isTest">if set to <see langword="true" /> forces the registrar to register its test configuration.</param>
        /// <returns>IUnityContainer - the <paramref name="container"/> if it is not <see langword="null"/>, otherwise returns <see cref="P:DIContainer.Root"/>.</returns>
        /// <remarks>
        /// This method allows for chained registrations like this:
        /// <code>
        /// <![CDATA[
        ///     DIContainer.Initialize()
        ///                .Register(Facility.Registrar)
        ///                .Register(EFRepositoryRegistrar.Default)
        ///                ...
        ///                ;
        /// ]]>
        /// </code>
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="1")]
        public static IUnityContainer Register(
            this IUnityContainer container,
            ContainerRegistrar registrar,
            bool isTest = false)
        {
            Contract.Requires<ArgumentNullException>(registrar != null, nameof(registrar));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return registrar.Register(container, isTest);
        }

        /// <summary>
        /// Registers the types and instances of the <see cref="T:ContainerRegistrar"/> in the specified container.
        /// The method is <b>not</b> thread safe and should be called from a synchronized context.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="registrar">The registrar.</param>
        /// <param name="registrations">The registrations.</param>
        /// <param name="isTest">if set to <see langword="true" /> [is test].</param>
        /// <returns>IUnityContainer.</returns>
        /// <remarks>
        /// This method allows for chained registrations like this:
        /// <code>
        /// <![CDATA[
        ///     lock (DIContainer.Initialize())
        ///     {
        ///         var registrations = DIContainer.Root.GetRegistrationsSnapshot();
        ///         
        ///         DIContainer.Root
        ///                    .Register(Facility.Registrar, registrations)
        ///                    .Register(EFRepositoryRegistrar, registrations)
        ///                    ...
        ///                    ;
        ///     }
        /// ]]>
        /// </code>
        /// </remarks>
        public static IUnityContainer UnsafeRegister(
            this IUnityContainer container, 
            ContainerRegistrar registrar, 
            IDictionary<RegistrationLookup, ContainerRegistration> registrations, 
            bool isTest = false)
        {
            Contract.Requires<ArgumentNullException>(registrar != null, nameof(registrar));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return registrar.UnsafeRegister(container, registrations, isTest);
        }
    }
}
