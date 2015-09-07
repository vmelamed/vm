using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Practices.Unity;

namespace vm.Aspects
{
    public static partial class DIContainer
    {
        #region GetRegistrationSnapshot
        /// <summary>
        /// Gets a snapshot of the registrations in the specified container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>
        /// Instance of <see cref="T:IDictionary{RegistrationLookup, ContainerRegistration}"/> - a snapshot of the current registrations in the container.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method is useful only inside of a critical sections guarded with <c>lock (DIContainer.Root)</c> - 
        /// then it is safe to use the registration family of methods RegisterTypeIfNot or RegisterInstanceIfNot which take a second (or third) 
        /// parameter of type <see cref="T:IDictionary{RegistrationLookup, ContainerRegistration}"/>. Using the snapshot improves the
        /// performance issues documented in http://philipm.at/2011/0819/ when used for a series of registrations.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot(registrations, typeof(Interface1), typeof(Type1))
        ///         .RegisterTypeIfNot(registrations, typeof(Interface2), typeof(Type2))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IDictionary<RegistrationLookup, ContainerRegistration> GetRegistrationsSnapshot(
            this IUnityContainer container)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Ensures(Contract.Result<IDictionary<RegistrationLookup, ContainerRegistration>>() != null);

            return container.Registrations.ToDictionary(
                        cr => new RegistrationLookup(cr.RegisteredType, cr.Name),
                        cr => cr);
        }

        /// <summary>
        /// Gets a snapshot of the registrations in the specified container.
        /// Simply a short cut for <c>GetRegistrationDictionary(DIContainer.Root)</c>
        /// </summary>
        public static IDictionary<RegistrationLookup, ContainerRegistration> GetRegistrationsSnapshot()
        {
            Contract.Ensures(Contract.Result<IDictionary<RegistrationLookup, ContainerRegistration>>() != null);

            return GetRegistrationsSnapshot(DIContainer.Root);
        }
        #endregion

        #region Conditionally register a type using a registrations snapshot
        /// <summary>
        /// Registers the type <paramref name="type"/>, if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="type">The type to be conditionally registered.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="type"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot(registrations, typeof(Type1));
        ///         .RegisterTypeIfNot(registrations, typeof(Type2), new InjectionProperty("Property"));
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type type,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(type)))
                container.RegisterType(type, injectionMembers);
            return container;
        }

        /// <summary>
        /// Registers the type <paramref name="type"/> with the <paramref name="lifetimeManager"/> if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="type">The type to be conditionally registered.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="type"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot(registrations, typeof(Type1), new PerResolveLifetimeManager())
        ///         .RegisterTypeIfNot(registrations, typeof(Type2), new PerResolveLifetimeManager(), new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type type,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(type)))
                container.RegisterType(type, lifetimeManager, injectionMembers);
            return container;
        }

        /// <summary>
        /// Registers the type <paramref name="type"/> with resolve name <paramref name="name"/> if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="type">The type to be conditionally registered.</param>
        /// <param name="name">The name of the registration.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> are <see langword="null"/>
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="type"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot(registrations, typeof(Type1), "rest")
        ///         .RegisterTypeIfNot(registrations, typeof(Type2), "soap", new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type type,
            string name,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(type, name)))
                container.RegisterType(type, name, injectionMembers);
            return container;
        }

        /// <summary>
        /// Registers the type <paramref name="type"/> with the <paramref name="lifetimeManager"/> and resolve name <paramref name="name"/> 
        /// if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="type">The type to be conditionally registered.</param>
        /// <param name="name">The resolve name of the registration.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="type"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot(registrations, typeof(Type1), "soap", new SynchronizedLifetimeManager())
        ///         .RegisterTypeIfNot(registrations, typeof(Type2), "rest", new SynchronizedLifetimeManager(), new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type type,
            string name,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(type, name)))
                container.RegisterType(type, name, lifetimeManager, injectionMembers);

            return container;
        }
        #endregion

        #region Conditionally register type mapping from type-1 to type-2
        /// <summary>
        /// Registers a type mapping from <paramref name="typeFrom"/> to type <paramref name="typeTo"/> if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="typeFrom">The type to be conditionally registered from.</param>
        /// <param name="typeTo">The type to be conditionally registered to.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="typeFrom"/> or <paramref name="typeTo"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="typeFrom"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot(registrations, typeof(Interface1), typeof(Type1))
        ///         .RegisterTypeIfNot(registrations, typeof(Interface2), typeof(Type2), new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type typeFrom,
            Type typeTo,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(typeFrom != null, nameof(typeFrom));
            Contract.Requires<ArgumentNullException>(typeTo != null, nameof(typeTo));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(typeFrom)))
                container.RegisterType(typeFrom, typeTo, injectionMembers);

            return container;
        }

        /// <summary>
        /// Registers a type mapping from <paramref name="typeFrom"/> to type <paramref name="typeTo"/> if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="typeFrom">The type to be conditionally registered from.</param>
        /// <param name="typeTo">The type to be conditionally registered to.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="typeFrom"/> or <paramref name="typeTo"/> or <paramref name="lifetimeManager"/> 
        /// are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="typeFrom"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot(registrations, typeof(Interface1), typeof(Type1), new ContainerControlledLifetimeManager())
        ///         .RegisterTypeIfNot(registrations, typeof(Interface2), typeof(Type2), new ContainerControlledLifetimeManager(), new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type typeFrom,
            Type typeTo,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(typeFrom != null, nameof(typeFrom));
            Contract.Requires<ArgumentNullException>(typeTo != null, nameof(typeTo));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(typeFrom)))
                container.RegisterType(typeFrom, typeTo, lifetimeManager, injectionMembers);

            return container;
        }

        /// <summary>
        /// Registers a type mapping from <paramref name="typeFrom"/> to type <paramref name="typeTo"/> with resolve name <paramref name="name"/> 
        /// if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="typeFrom">The type to be conditionally registered from.</param>
        /// <param name="typeTo">The type to be conditionally registered to.</param>
        /// <param name="name">The resolve name under which to register the mapping.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="typeFrom"/> or <paramref name="typeTo"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="typeFrom"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot(registrations, typeof(Interface1), typeof(Type1), "abc")
        ///         .RegisterTypeIfNot(registrations, typeof(Interface2), typeof(Type2), "xyz", new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type typeFrom,
            Type typeTo,
            string name,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(typeFrom != null, nameof(typeFrom));
            Contract.Requires<ArgumentNullException>(typeTo != null, nameof(typeTo));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(typeFrom, name)))
                container.RegisterType(typeFrom, typeTo, name, injectionMembers);

            return container;
        }

        /// <summary>
        /// Registers a type mapping from <paramref name="typeFrom"/> to type <paramref name="typeTo"/> with resolve name <paramref name="name"/> 
        /// and a lifetime manager if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="typeFrom">The type to be conditionally registered from.</param>
        /// <param name="typeTo">The type to be conditionally registered to.</param>
        /// <param name="name">The resolve name under which to register the mapping.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="typeFrom"/> or <paramref name="typeTo"/> or <paramref name="lifetimeManager"/> 
        /// are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="typeFrom"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot(registrations, typeof(Interface1), typeof(Type1), "abc", new TransientLifetimeManager())
        ///         .RegisterTypeIfNot(registrations, typeof(Interface2), typeof(Type2), "xyz", new TransientLifetimeManager(), new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type typeFrom,
            Type typeTo,
            string name,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(typeFrom != null, nameof(typeFrom));
            Contract.Requires<ArgumentNullException>(typeTo != null, nameof(typeTo));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(typeFrom, name)))
                container.RegisterType(typeFrom, typeTo, name, lifetimeManager, injectionMembers);

            return container;
        }
        #endregion

        #region Conditionally register instance
        /// <summary>
        /// Registers an instance of type <paramref name="type" /> if such registration does not exist yet.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="type">The type of the instance to be registered</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="instance"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="type"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterInstanceIfNot(registrations, typeof(Type1), instance)
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type type,
            object instance)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(type)))
                container.RegisterInstance(type, instance);

            return container;
        }

        /// <summary>
        /// Registers an instance of type <paramref name="type" /> with the specified lifetime manager if such registration does not exist yet.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="type">The type of the instance to be registered</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <param name="lifetimeManager">The lifetime manager to be associated with the instance.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="instance"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="type"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterInstanceIfNot(registrations, typeof(Type1), instance, new PerThreadLifetimeManager())
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type type,
            object instance,
            LifetimeManager lifetimeManager)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));
            Contract.Requires<ArgumentNullException>(lifetimeManager != null, nameof(lifetimeManager));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(type)))
                container.RegisterInstance(type, instance, lifetimeManager);
            return container;
        }

        /// <summary>
        /// Registers an instance of type <paramref name="type" /> with the specified resolve name, if such registration does not exist yet.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="type">The type of the instance to be registered</param>
        /// <param name="name">The resolve name of the instance.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="instance"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="type"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterInstanceIfNot(registrations, typeof(Type1), "abc", instance)
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type type,
            string name,
            object instance)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(type, name)))
                container.RegisterInstance(type, name, instance);

            return container;
        }

        /// <summary>
        /// Registers an instance of type <paramref name="type" /> with the specified resolve name and lifetime manager, 
        /// if such registration does not exist yet.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="type">The type of the instance to be registered</param>
        /// <param name="name">The resolve name of the instance.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <param name="lifetimeManager">The lifetime manager to be associated with the instance.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="instance"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <paramref name="type"/> is already registered in the container and 
        /// if it is not, it will register it. This allows a container to be configured both from configuration file and from code, where 
        /// the registrations from the configuration file take precedence over code configuration. The registrations snapshot improves the performance
        /// of the check described in http://philipm.at/2011/0819/.
        /// </para><para>
        /// The method should be used from within the scope of a lock statement. See the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     var registrations = DIContainer.Root.GetRegistrationSnapshot();
        /// 
        ///     DIContainer.Root
        ///         .RegisterInstanceIfNot(registrations, typeof(Type1), "abc", instance, new PerThreadLifetimeManager())
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            Type type,
            string name,
            object instance,
            LifetimeManager lifetimeManager)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));
            Contract.Requires<ArgumentNullException>(lifetimeManager != null, nameof(lifetimeManager));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            if (!registrations.ContainsKey(new RegistrationLookup(type, name)))
                container.RegisterInstance(type, name, instance, lifetimeManager);

            return container;
        }
        #endregion
    }
}
