using System;

using Unity;
using Unity.Lifetime;
using Unity.Registration;

namespace vm.Aspects
{
    public static partial class DIContainer
    {
        #region Conditionally register a type
        /// <summary>
        /// Registers the type <paramref name="type"/>, if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="type">The type to be conditionally registered.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The method checks if the type <paramref name="type"/> is already registered in the container and if it is not, it will register it.
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
        ///         .RegisterTypeIfNot(typeof(Type1));
        ///         .RegisterTypeIfNot(typeof(Type2), new InjectionProperty("Property"));
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            Type type,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));

            if (!container.IsRegistered(type))
                container.RegisterType(type, injectionMembers);

            return container;
        }

        /// <summary>
        /// Registers the type <paramref name="type"/> with the <paramref name="lifetimeManager"/> if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="type">The type to be conditionally registered.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if the type <paramref name="type"/> is already registered in the container and if it is not, it will register it.
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
        ///         .RegisterTypeIfNot(typeof(Type1), new PerResolveLifetimeManager())
        ///         .RegisterTypeIfNot(typeof(Type2), new PerResolveLifetimeManager(), new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            Type type,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));

            if (!container.IsRegistered(type))
                container.RegisterType(type, lifetimeManager, injectionMembers);

            return container;

        }

        /// <summary>
        /// Registers the type <paramref name="type"/> with resolve name <paramref name="name"/> if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="type">The type to be conditionally registered.</param>
        /// <param name="name">The name of the registration.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> are <see langword="null"/>
        /// </exception>
        /// <remarks>
        /// The method checks if the type <paramref name="type"/> is already registered in the container and if it is not, it will register it.
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
        ///         .RegisterTypeIfNot(typeof(Type1), "rest")
        ///         .RegisterTypeIfNot(typeof(Type2), "soap", new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            Type type,
            string name,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));

            if (!container.IsRegistered(type, name))
                container.RegisterType(type, name, injectionMembers);

            return container;

        }

        /// <summary>
        /// Registers the type <paramref name="type"/> with the <paramref name="lifetimeManager"/> and resolve name <paramref name="name"/> 
        /// if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="type">The type to be conditionally registered.</param>
        /// <param name="name">The resolve name of the registration.</param>
        /// <param name="lifetimeManager">The lifetime manager.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if the type <paramref name="type"/> is already registered in the container and if it is not, it will register it.
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
        ///         .RegisterTypeIfNot(typeof(Type1), "soap", new SynchronizedLifetimeManager())
        ///         .RegisterTypeIfNot(typeof(Type2), "rest", new SynchronizedLifetimeManager(), new InjectionProperty("Property"))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            Type type,
            string name,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));

            if (!container.IsRegistered(type, name))
                container.RegisterType(type, name, lifetimeManager, injectionMembers);

            return container;

        }
        #endregion

        #region Conditionally register type mapping from type-1 to type-2
        /// <summary>
        /// Registers a type mapping from <paramref name="typeFrom"/> to type <paramref name="typeTo"/> if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="typeFrom">The type to be conditionally registered from.</param>
        /// <param name="typeTo">The type to be conditionally registered to.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="typeFrom"/> or <paramref name="typeTo"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if the type <paramref name="typeFrom"/> is already registered in the container and if it is not, it will register it
        /// to map to type <paramref name="typeTo"/>.
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
        ///         .RegisterTypeIfNot(typeof(Interface1), typeof(Type1))
        ///         .RegisterTypeIfNot(typeof(Interface2), typeof(Type2), new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            Type typeFrom,
            Type typeTo,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (typeTo == null)
                throw new ArgumentNullException(nameof(typeTo));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));

            if (!container.IsRegistered(typeFrom))
                container.RegisterType(typeFrom, typeTo, injectionMembers);

            return container;

        }

        /// <summary>
        /// Registers a type mapping from <paramref name="typeFrom"/> to type <paramref name="typeTo"/> if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
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
        /// The method checks if the type <paramref name="typeFrom"/> is already registered in the container and if it is not, it will register it
        /// to map to type <paramref name="typeTo"/>.
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
        ///         .RegisterTypeIfNot(typeof(Interface1), typeof(Type1), new ContainerControlledLifetimeManager())
        ///         .RegisterTypeIfNot(typeof(Interface2), typeof(Type2), new ContainerControlledLifetimeManager(), new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            Type typeFrom,
            Type typeTo,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (typeTo == null)
                throw new ArgumentNullException(nameof(typeTo));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));

            if (!container.IsRegistered(typeFrom))
                container.RegisterType(typeFrom, typeTo, lifetimeManager, injectionMembers);

            return container;

        }

        /// <summary>
        /// Registers a type mapping from <paramref name="typeFrom"/> to type <paramref name="typeTo"/> with resolve name <paramref name="name"/> 
        /// if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
        /// <param name="typeFrom">The type to be conditionally registered from.</param>
        /// <param name="typeTo">The type to be conditionally registered to.</param>
        /// <param name="name">The resolve name under which to register the mapping.</param>
        /// <param name="injectionMembers">The injection members.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="typeFrom"/> or <paramref name="typeTo"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if the type <paramref name="typeFrom"/> is already registered in the container and if it is not, it will register it
        /// to map to type <paramref name="typeTo"/>.
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
        ///         .RegisterTypeIfNot(typeof(Interface1), typeof(Type1), "abc")
        ///         .RegisterTypeIfNot(typeof(Interface2), typeof(Type2), "xyz", new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            Type typeFrom,
            Type typeTo,
            string name,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (typeTo == null)
                throw new ArgumentNullException(nameof(typeTo));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));

            if (!container.IsRegistered(typeFrom, name))
                container.RegisterType(typeFrom, typeTo, name, injectionMembers);

            return container;

        }

        /// <summary>
        /// Registers a type mapping from <paramref name="typeFrom"/> to type <paramref name="typeTo"/> with resolve name <paramref name="name"/> 
        /// and a lifetime manager if it is not already registered.
        /// </summary>
        /// <param name="container">The unity container.</param>
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
        /// The method checks if the type <paramref name="typeFrom"/> is already registered in the container and if it is not, it will register it
        /// to map to type <paramref name="typeTo"/>.
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
        ///         .RegisterTypeIfNot(typeof(Interface1), typeof(Type1), "abc", new TransientLifetimeManager())
        ///         .RegisterTypeIfNot(typeof(Interface2), typeof(Type2), "xyz", new TransientLifetimeManager(), new InjectionFactory(c => return new Type1(xyz)))
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterTypeIfNot(
            this IUnityContainer container,
            Type typeFrom,
            Type typeTo,
            string name,
            LifetimeManager lifetimeManager,
            params InjectionMember[] injectionMembers)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (typeTo == null)
                throw new ArgumentNullException(nameof(typeTo));
            if (injectionMembers == null)
                throw new ArgumentNullException(nameof(injectionMembers));

            if (!container.IsRegistered(typeFrom, name))
                container.RegisterType(typeFrom, typeTo, name, lifetimeManager, injectionMembers);

            return container;

        }
        #endregion

        #region Conditionally register instance
        /// <summary>
        /// Registers an instance of type <paramref name="type" /> if such registration does not exist yet.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="type">The type of the instance to be registered</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="instance"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if there is registered type <paramref name="type"/> already registered in the container and if it is not, it will register it.
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
        ///         .RegisterInstanceIfNot(typeof(Type1), instance)
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot(
            this IUnityContainer container,
            Type type,
            object instance)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (!container.IsRegistered(type))
                container.RegisterInstance(type, instance);

            return container;
        }

        /// <summary>
        /// Registers an instance of type <paramref name="type" /> with the specified lifetime manager if such registration does not exist yet.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="type">The type of the instance to be registered</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <param name="lifetimeManager">The lifetime manager to be associated with the instance.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="instance"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if there is registered type <paramref name="type"/> already registered in the container and if it is not, it will register it.
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
        ///         .RegisterInstanceIfNot(typeof(Type1), instance, new PerThreadLifetimeManager())
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot(
            this IUnityContainer container,
            Type type,
            object instance,
            LifetimeManager lifetimeManager)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (lifetimeManager == null)
                throw new ArgumentNullException(nameof(lifetimeManager));

            if (!container.IsRegistered(type))
                container.RegisterInstance(type, instance, lifetimeManager);

            return container;
        }

        /// <summary>
        /// Registers an instance of type <paramref name="type" /> with the specified resolve name, if such registration does not exist yet.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="type">The type of the instance to be registered</param>
        /// <param name="name">The resolve name of the instance.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="instance"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if there is registered type <paramref name="type"/> already registered in the container and if it is not, it will register it.
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
        ///         .RegisterInstanceIfNot(typeof(Type1), "abc", instance)
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot(
            this IUnityContainer container,
            Type type,
            string name,
            object instance)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (!container.IsRegistered(type, name))
                container.RegisterInstance(type, name, instance);
            return container;
        }

        /// <summary>
        /// Registers an instance of type <paramref name="type" /> with the specified resolve name and lifetime manager, 
        /// if such registration does not exist yet.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="type">The type of the instance to be registered</param>
        /// <param name="name">The resolve name of the instance.</param>
        /// <param name="instance">The instance to be registered in the container.</param>
        /// <param name="lifetimeManager">The lifetime manager to be associated with the instance.</param>
        /// <returns>The container.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> or <paramref name="type"/> or <paramref name="instance"/> or <paramref name="lifetimeManager"/> are <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The method checks if there is registered type <paramref name="type"/> already registered in the container and if it is not, it will register it.
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
        ///         .RegisterInstanceIfNot(typeof(Type1), "abc", instance, new PerThreadLifetimeManager())
        ///         ...
        ///     ;
        /// }
        /// ...
        /// ]]>
        /// </code>
        /// </example>
        public static IUnityContainer RegisterInstanceIfNot(
            this IUnityContainer container,
            Type type,
            string name,
            object instance,
            LifetimeManager lifetimeManager)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (lifetimeManager == null)
                throw new ArgumentNullException(nameof(lifetimeManager));

            if (!container.IsRegistered(type, name))
                container.RegisterInstance(type, name, instance, lifetimeManager);

            return container;
        }
        #endregion
    }
}
