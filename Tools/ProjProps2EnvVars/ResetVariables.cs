using System.Collections.Generic;

using CommandLine;

using vm.Aspects;
using vm.Aspects.Diagnostics;

namespace vm.Tools.ProjProps2EnvVars
{
    [Verb("reset", HelpText = "Reads the properties of a project and resets the environment variables with the same names.")]
    class ResetVariables
    {
#if true
        /// <summary>
        /// Initializes a new instance of the <see cref="ResetVariables" /> class.
        /// </summary>
        /// <param name="projectFileName">Name of the project file.</param>
        /// <param name="projectProperties">The project properties to clear from the environment variables.</param>
        /// <param name="quiet">if set to <c>true</c> the variables being reset will not be displayed.</param>
        public ResetVariables(
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
            HelpText = "List of the project properties mapped to environment variables that must be reset. By default any environment variable with a name of a project property will be reset.")]
        [Dump(1)]
        public IEnumerable<string> Properties { get; }

        /// <summary>
        /// Gets a value indicating whether to display the reset variables.
        /// </summary>
        [Option(
            'q',
            "quiet",
            Default = false,
            HelpText = "Controls whether to display the reset environment variables.")]
        public bool Quiet { get; }

        public override string ToString() => this.DumpString();
    }
}
