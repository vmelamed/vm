//------------------------------------------------------------------------------
// <copyright file="RootCommand.cs" company="vm">
//     Copyright (c) vm.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;

namespace vm.Aspects.Visix.AddRelatedClasses
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RootCommand
    {
        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("66cb4b6e-0919-4cfa-96ec-cb36b93c6234");

        /// <summary>
        /// The command identifier of the 'Add Related Type' menu command in the context menu of the solution explorer.
        /// Used for dynamic visibility only.
        /// </summary>
        public const int cmdIdAddRelatedType = 0x1101;

        /// <summary>
        /// The command identifier of the 'Add Related Type' menu command in the context menu of the solution explorer.
        /// Used for dynamic visibility only.
        /// </summary>
        public const int cmdIdAddRelatedTypeSubMenu = 0x1102;

        // class related commands:
        /// <summary>
        /// The command identifier of the 'Add Metadata Type' menu command
        /// </summary>
        public const int cmdIdAddMetadataType = 0x1201;
        /// <summary>
        /// The command identifier of the 'Add DTO' menu command
        /// </summary>
        public const int cmdIdAddDto = 0x1202;

        // interface related commands:
        /// <summary>
        /// The command identifier of the 'Add Contract' menu command
        /// </summary>
        public const int cmdIdAddContractClass = 0x1211;
        /// <summary>
        /// The command identifier of the 'Add Async Interface' menu command
        /// </summary>
        public const int cmdIdAddAsyncInterface = 0x1212;
        /// <summary>
        /// The command identifier of the 'Add Client' menu command
        /// </summary>
        public const int cmdIdAddClient = 0x1213;
        /// <summary>
        /// The command identifier of the 'Add Async Client' menu command
        /// </summary>
        public const int cmdIdAddAsyncClient = 0x1214;

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "It is called in Initialize(Package) below")]
        public static RootCommand Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new RootCommand(package);
        }

        /// <summary>
        /// Gets the DTE.
        /// </summary>
        /// <value>The DTE.</value>
        internal static DTE Dte { get { return Package.GetGlobalService(typeof(DTE)) as DTE; } }
        /// <summary>
        /// Gets the DTE2.
        /// </summary>
        /// <value>The DTE2.</value>
        internal static DTE2 Dte2 { get { return Package.GetGlobalService(typeof(DTE)) as DTE2; } }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RootCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        RootCommand(Package package)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            ServiceProvider = package;

            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (commandService == null)
                return;

            AddSubMenuItem(commandService, cmdIdAddMetadataType, AddMetadataType);
            AddSubMenuItem(commandService, cmdIdAddDto, AddDto);
            AddSubMenuItem(commandService, cmdIdAddContractClass, AddContractClass);
            AddSubMenuItem(commandService, cmdIdAddAsyncInterface, AddAsyncInterface);
            AddSubMenuItem(commandService, cmdIdAddClient, AddClient);
        }

        void AddSubMenuItem(
            OleMenuCommandService commandService,
            int cmdId,
            EventHandler handler,
            EventHandler beforeQuery = null)
        {
            var command = new OleMenuCommand(handler, new CommandID(CommandSet, cmdId));

            if (beforeQuery != null)
                command.BeforeQueryStatus += beforeQuery;
            commandService.AddCommand(command);
        }

        void AddMetadataType(object sender, EventArgs e)
        {
            throw new NotImplementedException("'Add Metadata Type' is not implemented yet.");
        }

        void AddDto(object sender, EventArgs e)
        {
            throw new NotImplementedException("'Add DTO' is not implemented yet.");
        }

        void AddContractClass(object sender, EventArgs e)
        {
            throw new NotImplementedException("'Add Contracts Class' is not implemented yet.");
        }

        void AddAsyncInterface(object sender, EventArgs e)
        {
            throw new NotImplementedException("'Add Async Interface' is not implemented yet.");
        }

        void AddClient(object sender, EventArgs e)
        {
            throw new NotImplementedException("'Add Client' is not implemented yet.");
        }
    }
}
