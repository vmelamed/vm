using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using CommandLine;

using vm.Aspects;

namespace vm.Tools.ProjProps2EnvVars
{
    class Program
    {
        static int Main(string[] args)
        {
            var result = 1;
            var parser = new Parser(
                                with =>
                                {
                                    with.HelpWriter                = Console.Out;
                                    with.CaseSensitive             = false;
                                    with.CaseInsensitiveEnumValues = true;
                                });
            result = parser
                        .ParseArguments<SetVariables, ResetVariables>(args)
                        .MapResult(
                            (SetVariables s) => SetEnvironmentVariables(s),
                            (ResetVariables r) => ResetEnvironmentVariables(r),
                            e => 1);

            return result;
        }

        static int Display(
            IEnumerable<Error> errors)
        {
#if DEBUG
            foreach (var e in errors)
                Console.WriteLine(e.DumpString());
#endif
            return 1;
        }

        static int SetEnvironmentVariables(
            SetVariables p)
        {
            foreach (var property in GetProjectProperties(p.Project, p.Properties))
            {
                Environment.SetEnvironmentVariable(property.Key, property.Value, EnvironmentVariableTarget.User);
                if (!p.Quiet)
                    Console.WriteLine($"set {property.Key}={property.Value}");
            }

            if (!p.Quiet)
                Finish();

            return 0;
        }

        static int ResetEnvironmentVariables(
            ResetVariables p)
        {
            foreach (var property in GetProjectProperties(p.Project, p.Properties))
            {
                Environment.SetEnvironmentVariable(property.Key, string.Empty, EnvironmentVariableTarget.User);
                if (!p.Quiet)
                    Console.WriteLine($"set {property.Key}=");
            }

            if (!p.Quiet)
                Finish();

            return 0;
        }

        static void Finish()
        {
            Console.Write("Press any key to finish...");
            Console.ReadKey(true);
            Console.WriteLine();
        }

        static IEnumerable<KeyValuePair<string, string>> GetProjectProperties(
            string projectFileName,
            IEnumerable<string> propertyNames)
        {
            using (var reader = new StreamReader(projectFileName))
                return XElement
                            .Load(reader)
                            .Elements("PropertyGroup")
                            .SelectMany(e => e.Descendants())
                            .Where(p => !propertyNames.Any() ||
                                        propertyNames.Contains(p.Name.LocalName))
                            .ToDictionary(
                                e => e.Name.LocalName,
                                e => e.Value.ToString());
        }
    }
}
