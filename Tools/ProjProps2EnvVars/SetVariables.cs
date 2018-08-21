using System.Collections.Generic;

using CommandLine;

using vm.Aspects;
using vm.Aspects.Diagnostics;

namespace vm.Tools.ProjProps2EnvVars
{
    [Verb("set", HelpText = "Reads the properties of a project and sets environment variables with the same names and the corresponding values.")]
    class SetVariables
    {
#if true
        /// <summary>
        /// Initializes a new instance of the <see cref="SetVariables" /> class.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="quiet">if set to <c>true</c> the variables being set will not be displayed.</param>
        public SetVariables(
            string project,
            IEnumerable<string> properties,
            bool quiet)
        {
            Project    = project;
            Properties = properties;
            Quiet      = quiet;
        }

#endif

        /// <summary>
        /// Gets the project file name from which to read the properties.
        /// </summary>
        [Value(
            0,
            Required = true,
            HelpText = "The file name of the project.")]
        [Dump(0)]
        public string Project { get; }

        /// <summary>
        /// Gets the names of the properties to be read from the project. If empty, all properties will be read.
        /// </summary>
        [Option(
            'p',
            "properties",
            Default = new string[] { },
            Required = false,
            HelpText = "List of the project properties that should be set as environment variables. By default all project properties will be set.")]
        [Dump(1)]
        public IEnumerable<string> Properties { get; }

        /// <summary>
        /// Gets a value indicating whether to display the set variables and their values.
        /// </summary>
        [Option(
            'q',
            "quiet",
            Default = false,
            HelpText = "Controls whether to display the set the new environment variables and their values.")]
        public bool Quiet { get; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => this.DumpString();
    }
}
