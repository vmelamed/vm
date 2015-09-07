using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Unity;

namespace vm.Aspects
{
    public static partial class DIContainer
    {
        #region Conditionally register a type using a registrations snapshot
        /// <summary>
        /// Registers the type <typeparamref name="T"/>, if it is not already registered.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="T"/> is already registered in the container and 
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
        ///         .RegisterTypeIfNot<Type1>(registrations);
        ///         .RegisterTypeIfNot<Type2>(registrations, new InjectionProperty("Property"));
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<T>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterTypeIfNot(registrations, typeof(T), injectionMembers);
        }

        /// <summary>
        /// Registers the type <typeparamref name="T"/> with the <paramref name="lifetimeManager"/> if it is not already registered.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="T"/> is already registered in the container and 
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
        ///         .RegisterTypeIfNot<Type1>(registrations, new PerResolveLifetimeManager())
        ///         .RegisterTypeIfNot<Type2>(registrations, new PerResolveLifetimeManager(), new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<T>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterTypeIfNot(registrations, typeof(T), lifetimeManager, injectionMembers);
        }

        /// <summary>
        /// Registers the type <typeparamref name="T"/> with resolve name <paramref name="name"/> if it is not already registered.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="name">The name of the registration.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> are <see langword="null"/>
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="T"/> is already registered in the container and 
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
        ///         .RegisterTypeIfNot<Type1>(registrations, "rest")
        ///         .RegisterTypeIfNot<Type2>(registrations, "soap", new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<T>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            string name,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterTypeIfNot(registrations, typeof(T), name, injectionMembers);
        }

        /// <summary>
        /// Registers the type <typeparamref name="T"/> with the <paramref name="lifetimeManager"/> and resolve name <paramref name="name"/> 
        /// if it is not already registered.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="name">The resolve name of the registration.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="T"/> is already registered in the container and 
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
        ///         .RegisterTypeIfNot<Type1>(registrations, "soap", new SynchronizedLifetimeManager())
        ///         .RegisterTypeIfNot<Type2>(registrations, "rest", new SynchronizedLifetimeManager(), new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<T>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            string name,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterTypeIfNot(registrations, typeof(T), name, lifetimeManager, injectionMembers);
        } 
        #endregion

        #region Conditionally register type mapping from TFrom to TTo
        /// <summary>
        /// Registers a type mapping from <typeparamref name="TFrom"/> to type <typeparamref name="TTo"/> if it is not already registered.
        /// </summary>
        /// <typeparam name="TFrom">The type to be conditionally registered from.</typeparam>
        /// <typeparam name="TTo">The type to be conditionally registered to.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="TFrom"/> is already registered in the container and 
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
        ///         .RegisterTypeIfNot<Interface1, Type1>(registrations)
        ///         .RegisterTypeIfNot<Interface2, Type2>(registrations, new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<TFrom, TTo>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterTypeIfNot(registrations, typeof(TFrom), typeof(TTo), injectionMembers);
        }

        /// <summary>
        /// Registers a type mapping from <typeparamref name="TFrom"/> to type <typeparamref name="TTo"/> if it is not already registered.
        /// </summary>
        /// <typeparam name="TFrom">The type to be conditionally registered from.</typeparam>
        /// <typeparam name="TTo">The type to be conditionally registered to.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> or <paramref name="lifetimeManager"/> 
        /// are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="TFrom"/> is already registered in the container and 
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
        ///         .RegisterTypeIfNot<Interface1, Type1>(registrations, new ContainerControlledLifetimeManager())
        ///         .RegisterTypeIfNot<Interface2, Type2>(registrations, new ContainerControlledLifetimeManager(), new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<TFrom, TTo>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterTypeIfNot(registrations, typeof(TFrom), typeof(TTo), lifetimeManager, injectionMembers);
        }

        /// <summary>
        /// Registers a type mapping from <typeparamref name="TFrom"/> to type <typeparamref name="TTo"/> with resolve name <paramref name="name"/> 
        /// if it is not already registered.
        /// </summary>
        /// <typeparam name="TFrom">The type to be conditionally registered from.</typeparam>
        /// <typeparam name="TTo">The type to be conditionally registered to.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="name">The resolve name under which to register the mapping.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="TFrom"/> is already registered in the container and 
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
        ///         .RegisterTypeIfNot<Interface1, Type1>(registrations, "abc")
        ///         .RegisterTypeIfNot<Interface2, Type2>(registrations, "xyz", new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<TFrom, TTo>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            string name,
            params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterTypeIfNot(registrations, typeof(TFrom), typeof(TTo), name, injectionMembers);
        }

        /// <summary>
        /// Registers a type mapping from <typeparamref name="TFrom"/> to type <typeparamref name="TTo"/> with resolve name <paramref name="name"/> 
        /// and a lifetime manager if it is not already registered.
        /// </summary>
        /// <typeparam name="TFrom">The type to be conditionally registered from.</typeparam>
        /// <typeparam name="TTo">The type to be conditionally registered to.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="name">The resolve name under which to register the mapping.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> or <paramref name="lifetimeManager"/> 
        /// are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="TFrom"/> is already registered in the container and 
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
        ///         .RegisterTypeIfNot<Interface1, Type1>(registrations, "abc", new TransientLifetimeManager())
        ///         .RegisterTypeIfNot<Interface2, Type2>(registrations, "xyz", new TransientLifetimeManager(), new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<TFrom, TTo>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            string name,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(injectionMembers != null, nameof(injectionMembers));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterTypeIfNot(registrations, typeof(TFrom), typeof(TTo), name, lifetimeManager, injectionMembers);
        } 
        #endregion

        #region Conditionally register instance
        /// <summary>
        /// Registers an instance of type <typeparamref name="T"/> if such registration does not exist yet.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be registered.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="instance"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="T"/> is already registered in the container and 
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
        ///         .RegisterInstanceIfNot<Type1>(registrations, instance)
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot<T>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            T instance)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(instance != null, nameof(instance));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterInstanceIfNot(registrations, typeof(T), instance);
        }

        /// <summary>
        /// Registers an instance of type <typeparamref name="T"/> with the specified lifetime manager if such registration does not exist yet.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be registered.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <param name="lifetimeManager">The lifetime manager to be associated with the instance.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="instance"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="T"/> is already registered in the container and 
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
        ///         .RegisterInstanceIfNot<Type1>(registrations, instance, new PerThreadLifetimeManager())
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot<T>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            T instance,
            LifetimeManager lifetimeManager)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(instance != null, nameof(instance));
            Contract.Requires<ArgumentNullException>(lifetimeManager != null, nameof(lifetimeManager));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterInstanceIfNot(registrations, typeof(T), instance, lifetimeManager);
        }

        /// <summary>
        /// Registers an instance of type <typeparamref name="T"/> with the specified resolve name, if such registration does not exist yet.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be registered.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="name">The resolve name of the instance.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="instance"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="T"/> is already registered in the container and 
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
        ///         .RegisterInstanceIfNot<Type1>(registrations, "abc", instance)
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot<T>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            string name,
            T instance)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(instance != null, nameof(instance));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterInstanceIfNot(registrations, typeof(T), name, instance);
        }

        /// <summary>
        /// Registers an instance of type <typeparamref name="T"/> with the specified resolve name and lifetime manager, 
        /// if such registration does not exist yet.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be registered.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="registrations">A snapshot of the registrations in the container.</param>
        /// <param name="name">The resolve name of the instance.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <param name="lifetimeManager">The lifetime manager to be associated with the instance.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="instance"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks in the snapshot of registrations if the type <typeparamref name="T"/> is already registered in the container and 
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
        ///         .RegisterInstanceIfNot<Type1>(registrations, "abc", instance, new PerThreadLifetimeManager())
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot<T>(
            this IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations,
            string name,
            T instance,
            LifetimeManager lifetimeManager)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(registrations != null, nameof(registrations));
            Contract.Requires<ArgumentNullException>(instance != null, nameof(instance));
            Contract.Requires<ArgumentNullException>(lifetimeManager != null, nameof(lifetimeManager));
            Contract.Ensures(Contract.Result<IUnityContainer>() != null);

            return container.RegisterInstanceIfNot(registrations, typeof(T), name, instance, lifetimeManager);
        } 
        #endregion
    }
}