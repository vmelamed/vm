using System;
using Unity;

namespace vm.Aspects
{
    public static partial class DIContainer
    {
        #region Conditionally register a type
        /// <summary>
        /// Registers the type <typeparamref name="T"/>, if it is not already registered.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks if the type <typeparamref name="T"/> is already registered in the container and if it is not, it will register it.
        /// This allows a container to be configured both from configuration file and from code, where the registrations from the 
        /// configuration file take precedence over code configuration.
        /// </para><para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot<Type1>();
        ///         .RegisterTypeIfNot<Type2>(new InjectionProperty("Property"));
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<T>(
            this IUnityContainer container,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));
            
            return container.RegisterTypeIfNot(typeof(T), injectionMembers);
        }

        /// <summary>
        /// Registers the type <typeparamref name="T"/> with the <paramref name="lifetimeManager"/> if it is not already registered.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if the type <typeparamref name="T"/> is already registered in the container and if it is not, it will register it.
        /// This allows a container to be configured both from configuration file or other source and from code, where the registrations from the 
        /// configuration file have precedence over code configuration.
        /// <para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot<Type1>(new PerResolveLifetimeManager())
        ///         .RegisterTypeIfNot<Type2>(new PerResolveLifetimeManager(), new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<T>(
            this IUnityContainer container,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));
            
            return container.RegisterTypeIfNot(typeof(T), lifetimeManager, injectionMembers);
        }

        /// <summary>
        /// Registers the type <typeparamref name="T"/> with resolve name <paramref name="name"/> if it is not already registered.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="name">The name of the registration.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> are <see langword="null"/>
        /// </exception>
        /// <remarks>
        /// The method checks if the type <typeparamref name="T"/> is already registered in the container and if it is not, it will register it.
        /// This allows a container to be configured both from configuration file or other source and from code, where the registrations from the 
        /// configuration file have precedence over code configuration.
        /// <para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot<Type1>("rest")
        ///         .RegisterTypeIfNot<Type2>("soap", new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<T>(
            this IUnityContainer container,
            string name,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));
            
            return container.RegisterTypeIfNot(typeof(T), name, injectionMembers);
        }

        /// <summary>
        /// Registers the type <typeparamref name="T"/> with the <paramref name="lifetimeManager"/> and resolve name <paramref name="name"/> 
        /// if it is not already registered.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="name">The resolve name of the registration.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if the type <typeparamref name="T"/> is already registered in the container and if it is not, it will register it.
        /// This allows a container to be configured both from configuration file or other source and from code, where the registrations from the 
        /// configuration file have precedence over code configuration.
        /// <para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot<Type1>("soap", new SynchronizedLifetimeManager())
        ///         .RegisterTypeIfNot<Type2>("rest", new SynchronizedLifetimeManager(), new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<T>(
            this IUnityContainer container,
            string name,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));
            
            return container.RegisterTypeIfNot(typeof(T), name, lifetimeManager, injectionMembers);
        } 
        #endregion

        #region Conditionally register type mapping from TFrom to TTo
        /// <summary>
        /// Registers a type mapping from <typeparamref name="TFrom"/> to type <typeparamref name="TTo"/> if it is not already registered.
        /// </summary>
        /// <typeparam name="TFrom">The type to be conditionally registered from.</typeparam>
        /// <typeparam name="TTo">The type to be conditionally registered to.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if the type <typeparamref name="TFrom"/> is already registered in the container and if it is not, it will register it
        /// to map to type <typeparamref name="TTo"/>.
        /// This allows a container to be configured both from configuration file or other source and from code, where the registrations from the 
        /// configuration file have precedence over code configuration.
        /// <para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot<Interface1, Type1>()
        ///         .RegisterTypeIfNot<Interface2, Type2>(new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<TFrom, TTo>(
            this IUnityContainer container,
            params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));
            
            return container.RegisterTypeIfNot(typeof(TFrom), typeof(TTo), injectionMembers);
        }

        /// <summary>
        /// Registers a type mapping from <typeparamref name="TFrom"/> to type <typeparamref name="TTo"/> if it is not already registered.
        /// </summary>
        /// <typeparam name="TFrom">The type to be conditionally registered from.</typeparam>
        /// <typeparam name="TTo">The type to be conditionally registered to.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> or <paramref name="lifetimeManager"/> 
        /// are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if the type <typeparamref name="TFrom"/> is already registered in the container and if it is not, it will register it
        /// to map to type <typeparamref name="TTo"/>.
        /// This allows a container to be configured both from configuration file or other source and from code, where the registrations from the 
        /// configuration file have precedence over code configuration.
        /// <para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot<Interface1, Type1>(new ContainerControlledLifetimeManager())
        ///         .RegisterTypeIfNot<Interface2, Type2>(new ContainerControlledLifetimeManager(), new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<TFrom, TTo>(
            this IUnityContainer container,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));
            
            return container.RegisterTypeIfNot(typeof(TFrom), typeof(TTo), lifetimeManager, injectionMembers);
        }

        /// <summary>
        /// Registers a type mapping from <typeparamref name="TFrom"/> to type <typeparamref name="TTo"/> with resolve name <paramref name="name"/> 
        /// if it is not already registered.
        /// </summary>
        /// <typeparam name="TFrom">The type to be conditionally registered from.</typeparam>
        /// <typeparam name="TTo">The type to be conditionally registered to.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="name">The resolve name under which to register the mapping.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if the type <typeparamref name="TFrom"/> is already registered in the container and if it is not, it will register it
        /// to map to type <typeparamref name="TTo"/>.
        /// This allows a container to be configured both from configuration file or other source and from code, where the registrations from the 
        /// configuration file have precedence over code configuration.
        /// <para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot<Interface1, Type1>("abc")
        ///         .RegisterTypeIfNot<Interface2, Type2>("xyz", new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<TFrom, TTo>(
            this IUnityContainer container,
            string name,
            params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));
            
            return container.RegisterTypeIfNot(typeof(TFrom), typeof(TTo), name, injectionMembers);
        }

        /// <summary>
        /// Registers a type mapping from <typeparamref name="TFrom"/> to type <typeparamref name="TTo"/> with resolve name <paramref name="name"/> 
        /// and a lifetime manager if it is not already registered.
        /// </summary>
        /// <typeparam name="TFrom">The type to be conditionally registered from.</typeparam>
        /// <typeparam name="TTo">The type to be conditionally registered to.</typeparam>
        /// <param name="container">The unity container.</param>
        /// <param name="name">The resolve name under which to register the mapping.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="TFrom"/> or <typeparamref name="TTo"/> or <paramref name="lifetimeManager"/> 
        /// are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if the type <typeparamref name="TFrom"/> is already registered in the container and if it is not, it will register it
        /// to map to type <typeparamref name="TTo"/>.
        /// This allows a container to be configured both from configuration file or other source and from code, where the registrations from the 
        /// configuration file have precedence over code configuration.
        /// <para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterTypeIfNot<Interface1, Type1>("abc", new TransientLifetimeManager())
        ///         .RegisterTypeIfNot<Interface2, Type2>("xyz", new TransientLifetimeManager(), new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot<TFrom, TTo>(
            this IUnityContainer container,
            string name,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));
            
            return container.RegisterTypeIfNot(typeof(TFrom), typeof(TTo), name, lifetimeManager, injectionMembers);
        } 
        #endregion

        #region Conditionally register instance
        /// <summary>
        /// Registers an instance of type <typeparamref name="T"/> if such registration does not exist yet.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="instance"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if there is registered type <typeparamref name="T"/> already registered in the container and if it is not, it will register it.
        /// This allows a container to be configured both from configuration file or other source and from code, where the registrations from the 
        /// configuration file have precedence over code configuration.
        /// <para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterInstanceIfNot<Type1>(instance)
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot<T>(
            this IUnityContainer container,
            T instance)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            
            return container.RegisterInstanceIfNot(typeof(T), instance);
        }

        /// <summary>
        /// Registers an instance of type <typeparamref name="T"/> with the specified lifetime manager if such registration does not exist yet.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <param name="lifetimeManager">The lifetime manager to be associated with the instance.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="instance"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if there is registered type <typeparamref name="T"/> already registered in the container and if it is not, it will register it.
        /// This allows a container to be configured both from configuration file or other source and from code, where the registrations from the 
        /// configuration file have precedence over code configuration.
        /// <para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterInstanceIfNot<Type1>(instance, new PerThreadLifetimeManager())
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot<T>(
            this IUnityContainer container,
            T instance,
            LifetimeManager lifetimeManager)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (lifetimeManager == null)
                throw new ArgumentNullException(nameof(lifetimeManager));
            
            return container.RegisterInstanceIfNot(typeof(T), instance, lifetimeManager);
        }

        /// <summary>
        /// Registers an instance of type <typeparamref name="T"/> with the specified resolve name, if such registration does not exist yet.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="name">The resolve name of the instance.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="instance"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if there is registered type <typeparamref name="T"/> already registered in the container and if it is not, it will register it.
        /// This allows a container to be configured both from configuration file or other source and from code, where the registrations from the 
        /// configuration file have precedence over code configuration.
        /// <para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterInstanceIfNot<Type1>("abc", instance)
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot<T>(
            this IUnityContainer container,
            string name,
            T instance)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            
            return container.RegisterInstanceIfNot(typeof(T), name, instance);
        }

        /// <summary>
        /// Registers an instance of type <typeparamref name="T"/> with the specified resolve name and lifetime manager, 
        /// if such registration does not exist yet.
        /// </summary>
        /// <typeparam name="T">The type to be conditionally registered.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="name">The resolve name of the instance.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <param name="lifetimeManager">The lifetime manager to be associated with the instance.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <typeparamref name="T"/> or <paramref name="instance"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if there is registered type <typeparamref name="T"/> already registered in the container and if it is not, it will register it.
        /// This allows a container to be configured both from configuration file or other source and from code, where the registrations from the 
        /// configuration file have precedence over code configuration.
        /// <para>
        /// Note that the method uses the extension method <see cref="T:Unity.UnityContainerExtensions.IsRegistered"/>. 
        /// There are well documented performance and multi-threaded issues with this method, e.g. http://philipm.at/2011/0819/.
        /// However if used only during the container's configuration at application initialization time and 
        /// within the context of a synchronization lock, these should be acceptable.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// ...
        /// lock (DIContainer.Root)
        /// {
        ///     DIContainer.Root
        ///         .RegisterInstanceIfNot<Type1>("abc", instance, new PerThreadLifetimeManager())
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot<T>(
            this IUnityContainer container,
            string name,
            T instance,
            LifetimeManager lifetimeManager)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (lifetimeManager == null)
                throw new ArgumentNullException(nameof(lifetimeManager));
            
            return container.RegisterInstanceIfNot(typeof(T), name, instance, lifetimeManager);
        } 
        #endregion
    }
}