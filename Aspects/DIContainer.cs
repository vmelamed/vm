﻿using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;

namespace vm.Aspects
{
    /// <summary>
    /// Class DIContainer is a Unity DI container bootstrap class.
    /// </summary>
    /// <remarks>
    /// The class loads a unity configuration section from a passed file or the current executable's configuration file.
    /// Then it initializes the static member <see cref="Root"/> from the loaded configuration section. From this moment on
    /// the container is initialized (<c>DIContainer.Root.IsInitialized == true</c>) and can be accessed through this static property.
    /// Also the <see cref="Initialize"/> associates the Common Service Locator (CSL) with so initialized unity container.
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// ...
    /// {
    ///     DIContainer.Initialize("test.config");
    ///     
    ///     Debug.Assert(DIContainer.Root != null && DIContainer.IsInitialized);
    /// }
    /// ...
    /// ]]>
    /// </code>
    /// </example>
    public static partial class DIContainer
    {
        #region Default constants
        /// <summary>
        /// The default configuration file name - unity.config.
        /// </summary>
        public const string DefaultConfigurationFileName    = "unity.config";
        /// <summary>
        /// The default configuration section name - unity
        /// </summary>
        public const string DefaultConfigurationSectionName = "unity";
        /// <summary>
        /// The default container name - empty string.
        /// </summary>
        public const string DefaultContainerName            = "";
        #endregion

        #region Fields
        /// <summary>
        /// The root container instance.
        /// </summary>
        static IUnityContainer _root = new UnityContainer();
        /// <summary>
        /// The flag indicating if the container is initialized
        /// </summary>
        static volatile bool _isInitialized;
        #endregion

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Gets the current default container.
        /// </summary>
        public static IUnityContainer Root
        {
            get
            {
                Contract.Ensures(Contract.Result<IUnityContainer>() != null, "The root container is null.");

                return _root;
            }
        }

        /// <summary>
        /// Gets the unity configuration section with name <paramref name="configSection" /> from the specified file.
        /// </summary>
        /// <param name="configFileName">
        /// The absolute or relative path and name of the configuration file. 
        /// If the argument is <see langword="null"/>, empty or consists of whitespace characters only 
        /// the method returns <see langword="null"/>.
        /// </param>
        /// <param name="configSection">
        /// The name of the configuration section.
        /// </param>
        /// <returns>The loaded unity configuration section.</returns>
        /// <remarks>
        /// If the <paramref name="configFileName"/> is <see langword="null"/>, empty or consists of whitespace characters only 
        /// the method returns <see langword="null"/>; otherwise if <paramref name="configFileName"/> is not a rooted path (e.g. C:\...)
        /// it is pre-pended with the path of the current executable's configuration file (e.g. web.config).
        /// </remarks>
        static UnityConfigurationSection GetUnityConfigurationSection(
            string configFileName,
            string configSection)
        {
            // if specified, try loading from file (e.g. DIContainer.config)
            if (string.IsNullOrWhiteSpace(configFileName))
                return null;

            string fullConfigPathName;

            if (Path.IsPathRooted(configFileName))
                fullConfigPathName = configFileName;
            else
            {
                var data = AppDomain
                               .CurrentDomain
                               .GetData("APP_CONFIG_FILE");

                Contract.Assume(data != null);

                var path = Path.GetDirectoryName(data.ToString());

                fullConfigPathName = Path.Combine(path, configFileName);
            }

            var configuration = ConfigurationManager.OpenMappedExeConfiguration(
                                                        new ExeConfigurationFileMap { ExeConfigFilename = fullConfigPathName },
                                                        ConfigurationUserLevel.None);

            return configuration.GetSection(configSection) as UnityConfigurationSection;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "It is called by the rewritten code.")]
        [ContractAbbreviator]
        static void EnsuresContainerInitialized()
        {
            Contract.Ensures(Contract.Result<IUnityContainer>() != null, "The container was not initialized properly.");
            Contract.Ensures(IsInitialized, "The container was not initialized properly.");
            Contract.Ensures(ServiceLocator.IsLocationProviderSet, "The common service locator is not associated with any DI container.");
        }

        /// <summary>
        /// Initializes the container with the configuration in the specified file name and configuration section name.
        /// </summary>
        /// <remarks>
        /// The container is initialized from the specified unity configuration file. 
        /// If the file is not found, the method will attempt to initialize the container from the application&quot;s configuration file (app.config or web.config).
        /// </remarks>
        /// <param name="configFileName">
        /// The name of the configuration file. If not specified it defaults to <see cref="DefaultConfigurationFileName"/> - &quot;unity.config&quot;.
        /// </param>
        /// <param name="configSection">
        /// The name od the configuration file section configuring unity. If not specified it defaults to <see cref="DefaultConfigurationSectionName"/> - &quot;unity&quot;.
        /// </param>
        /// <param name="containerName">
        /// The name of the container configuration section. If not specified the default is <see cref="DefaultContainerName"/> - an empty string.
        /// </param>
        /// <param name="getConfigFileSection">
        /// Specifies a delegate that implements the strategy for finding the unity configuration, given the configuration file and section name.
        /// If not specified, defaults to the internal default behavior as implemented in the private <see cref="GetUnityConfigurationSection"/>:
        /// If the <paramref name="configFileName"/> is <see langword="null"/>, empty or consists of whitespace characters only 
        /// the method returns <see langword="null"/> and this method will attempt to find the section in the current executable configuration file (web.config or app.config derived);
        /// otherwise if <paramref name="configFileName"/> is not a rooted path (e.g. starting with C:\...) it is prepended with the path of the 
        /// current executable's configuration file (e.g. web.config) and then will attempt loading the configuration section from that path and file.
        /// </param>
        /// <returns>The created and configured container.</returns>
        /// <exception cref="System.InvalidOperationException">If the configuration section cannot be found in the configuration file.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "n/a")]
        public static IUnityContainer Initialize(
            string configFileName = DefaultConfigurationFileName,
            string configSection = DefaultConfigurationSectionName,
            string containerName = DefaultContainerName,
            Func<string, string, UnityConfigurationSection> getConfigFileSection = null)
        {
            EnsuresContainerInitialized();

            if (_isInitialized)
                return _root;

            lock (_root)
            {
                if (_isInitialized)
                    return _root;

                try
                {
                    var unityConfigSection = getConfigFileSection!=null
                                                ? getConfigFileSection(configFileName, configSection)
                                                : GetUnityConfigurationSection(configFileName, configSection);

                    // file was not specified or does not exist - try loading from web/app.config then
                    if (unityConfigSection == null)
                        unityConfigSection = ConfigurationManager.GetSection(configSection) as UnityConfigurationSection;

                    if (unityConfigSection != null)
                        if (string.IsNullOrWhiteSpace(containerName))
                            _root.LoadConfiguration(unityConfigSection);
                        else
                            _root.LoadConfiguration(unityConfigSection, containerName);

                    _isInitialized = true;

                    // initialize the CSL with Unity service location
                    if (!ServiceLocator.IsLocationProviderSet)
                        ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(_root));

                    // prepare for interception and policy injection (AOP)
                    _root
                        .AddNewExtension<Interception>()
                        .Configure<Interception>()
                        ;

                    return _root;
                }
                catch (Exception x)
                {
                    if (_root!=null)
                        _root.DebugDump();

                    var dump = x.DumpString();

                    Debug.WriteLine(dump);
                    EventLog.WriteEntry("vm.Aspects", dump, EventLogEntryType.Error);
                    throw;
                }
            }
        }

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

            lock (container)
                return container
                            .Registrations
                            .ToDictionary(
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

        /// <summary>
        /// Registers the types and instances of the <see cref="ContainerRegistrar"/> in the specified container.
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
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
        /// Registers the types and instances of the <see cref="ContainerRegistrar"/> in the specified container.
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

        /// <summary>
        /// Initializes the container (and the CSL) without reading configuration files, thus leaving it empty.
        /// Useful when all registrations will be made in code or for tests.
        /// </summary>
        /// <returns>IUnityContainer.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "n/a")]
        public static IUnityContainer InitializeEmpty()
        {
            EnsuresContainerInitialized();

            if (_isInitialized)
                return _root;

            lock (_root)
            {
                if (_isInitialized)
                    return _root;

                // initialize the CSL with Unity service locator
                if (!ServiceLocator.IsLocationProviderSet)
                    ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(_root));

                _isInitialized = true;
                return _root;
            }
        }

        /// <summary>
        /// Resets the container. Used for tests only. Only in DEBUG builds.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "n/a")]
        [Conditional("TEST")]
        public static void Reset()
        {
            Contract.Ensures(_root != null);
            Contract.Ensures(!_isInitialized);

            ServiceLocator.SetLocatorProvider(null);

            if (_root != null)
                _root.Dispose();
            _root = new UnityContainer();
            _isInitialized = false;
        }

        /// <summary>
        /// Dumps the contents of a container as a text to a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="writer">The text writer to dump the container to.</param>
        public static void Dump(
            this IUnityContainer container,
            TextWriter writer)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            var registrations = container.GetRegistrationsSnapshot();

            writer.WriteLine($"Container has {registrations.Count()} Registrations:");

            foreach (var item in registrations
                                    .Values
                                    .OrderBy(i => i.RegisteredType.Name)
                                    .ThenBy(i => i.MappedToType.Name)
                                    .ThenBy(i => i.Name))
            {
                string regType = item.RegisteredType.IsGenericType
                                    ? string.Format(
                                                "{0}<{1}>",
                                                item.RegisteredType.Name.Split('`')[0],
                                                string.Join(", ", item.RegisteredType.GenericTypeArguments.Select(ta => ta.Name)))
                                    : item.RegisteredType.Name;
                string mapTo = item.MappedToType.IsGenericType
                                        ? string.Format(
                                                "{0}<{1}>",
                                                item.MappedToType.Name.Split('`')[0],
                                                string.Join(", ", item.MappedToType.GenericTypeArguments.Select(ta => ta.Name)))
                                        : item.MappedToType.Name;
                var regName = item.Name ?? "[default]";
                var lifetime = item.LifetimeManagerType.Name;

                Contract.Assert(lifetime.Length > "LifetimeManager".Length);

                if (mapTo != regType)
                    mapTo = " -> " + mapTo;
                else
                    mapTo = string.Empty;
                lifetime = lifetime.Substring(0, lifetime.Length - "LifetimeManager".Length);

                writer.WriteLine($"+ {regType}{mapTo}  '{regName}'  {lifetime}");
            }
        }

        /// <summary>
        /// Dumps the contents of a container as a text to the VS output pane.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        [Conditional("DEBUG")]
        public static void DebugDump(
            this IUnityContainer container)
        {
            Contract.Requires<ArgumentNullException>(container != null, nameof(container));

            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                container.Dump(writer);
                Debug.Print(
$@"===============================

{writer.GetStringBuilder()}

===============================
");
            }
        }
    }
}
