//------------------------------------------------------------------------------
// <copyright file="RootCommand.cs" company="vm">
//     Copyright (c) vm.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using vm.Aspects.Visix.AddRelatedClasses.Properties;
using VSLangProj;

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
        /// <value>The DTE2.</value>
        internal static DTE2 Dte { get { return Package.GetGlobalService(typeof(DTE)) as DTE2; } }

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

        static void AddSubMenuItem(
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

        void MessageBox(
            string title,
            string message)
        {
            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "EnvDTE.EditPoint.Insert(System.String)", Justification = "partial is a C# keyword.")]
        void AddMetadataType(object sender, EventArgs e)
        {
            var cached = new CachedItems(".Metadata.cs");

            //
            // TODO: does it make sense to have metadata type on an interface?
            //

            if (!cached.HasClass)
            {
                MessageBox(Resources.AddMetadataType, Resources.CannotGenerateMetadataType);
                return;
            }

            var project = cached.SourceProjectItem.ContainingProject;

            // edit the source file:
            // 1. add reference to the System.ComponentModel.DataAnnotations.dll
            var vsProject = project.Object as VSProject;

            if (!vsProject.References.OfType<Reference>().Any(r => r.Name == "System.ComponentModel.DataAnnotations"))
                vsProject.References.Add("System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            // 2. add the using System.ComponentModel.DataAnnotation
            if (!cached.SourceCodeModel.CodeElements.OfType<CodeImport>().Any(i => i.Namespace == "System.ComponentModel.DataAnnotations"))
                cached.SourceCodeModel.AddImport("System.ComponentModel.DataAnnotations");

            // 3. add a MetadataAttribute to the source class
            if (!cached.SourceClass.Attributes.OfType<CodeAttribute2>().Any(a => a.Name == "MetadataType"))
                cached.SourceClass.AddAttribute("MetadataType", string.Format(CultureInfo.InvariantCulture, "typeof({0}Metadata)", cached.SourceClass.Name));

            if (cached.SourceClass.ClassKind != vsCMClassKind.vsCMClassKindPartialClass)
            {
                // make the source class partial - required by classes with metadata types:
                cached.SourceProjectItem.Open(EnvDTE.Constants.vsViewKindPrimary);

                var point = cached.SourceClass
                                  .GetStartPoint(vsCMPart.vsCMPartHeader)
                                  .CreateEditPoint();

                point.FindPattern("class ", 0, ref point);
                point.CharLeft("class ".Length);
                point.Insert("partial ");
            }

            // fire-up the T4 engine and generate the text
            var t4 = ServiceProvider.GetService(typeof(STextTemplating)) as ITextTemplating;
            if (t4 == null)
                return;

            var t4SessionHost = t4 as ITextTemplatingSessionHost;

            t4SessionHost.Session = t4SessionHost.CreateSession();
            t4SessionHost.Session["sourcePathName"] = cached.SourcePathName;
            t4SessionHost.Session["targetPathName"] = cached.TargetPathName;

            var generatedText = t4.ProcessTemplate("Templates\\ClassMetadata.tt", File.ReadAllText("Templates\\ClassMetadata.tt"));

            // create the new file with the generated text and add it to the project:
            File.WriteAllText(cached.TargetPathName, generatedText);

            project.ProjectItems.AddFromFile(cached.TargetPathName);
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
            var cached = new CachedItems(".Tasks.cs");

            if (!cached.HasInterface)
            {
                MessageBox(Resources.AddAsyncInterface, Resources.CannotGenerateAsyncInterface);
                return;
            }

            // fire-up the T4 engine and generate the text
            var t4 = ServiceProvider.GetService(typeof(STextTemplating)) as ITextTemplating;
            if (t4 == null)
                return;

            var t4SessionHost = t4 as ITextTemplatingSessionHost;

            t4SessionHost.Session = t4SessionHost.CreateSession();
            t4SessionHost.Session["sourcePathName"] = cached.SourcePathName;
            t4SessionHost.Session["targetPathName"] = cached.TargetPathName;

            var generatedText = t4.ProcessTemplate("Templates\\InterfaceTasks.tt", File.ReadAllText("Templates\\InterfaceTasks.tt"));

            // create the new file with the generated text and add it to the project:
            File.WriteAllText(cached.TargetPathName, generatedText);

            cached.SourceProjectItem
                  .ContainingProject
                  .ProjectItems
                  .AddFromFile(cached.TargetPathName);
        }

        void AddClient(object sender, EventArgs e)
        {
            var cached = new CachedItems("");

            if (!cached.HasInterface)
            {
                MessageBox(Resources.AddClient, Resources.CannotGenerateClient);
                return;
            }

            string baseName;

            if (cached.SourceInterface.Name.StartsWith("I", StringComparison.Ordinal))
                baseName = cached.SourceInterface.Name.Substring(1);
            else
                baseName = cached.SourceInterface.Name;

            string targetPathName = Path.Combine(
                                        Path.GetDirectoryName(cached.SourcePathName),
                                        baseName + ".Client.cs");

            // fire-up the T4 engine and generate the text
            var t4 = ServiceProvider.GetService(typeof(STextTemplating)) as ITextTemplating;
            if (t4 == null)
                return;

            var t4SessionHost = t4 as ITextTemplatingSessionHost;

            t4SessionHost.Session = t4SessionHost.CreateSession();
            t4SessionHost.Session["baseName"]       = baseName;
            t4SessionHost.Session["sourcePathName"] = cached.SourcePathName;
            t4SessionHost.Session["targetPathName"] = targetPathName;

            var generatedText = t4.ProcessTemplate("Templates\\LightClient.tt", File.ReadAllText("Templates\\LightClient.tt"));

            // create the new file with the generated text and add it to the project:
            File.WriteAllText(cached.TargetPathName, generatedText);

            cached.SourceProjectItem
                  .ContainingProject
                  .ProjectItems
                  .AddFromFile(cached.TargetPathName);
        }
    }
}
