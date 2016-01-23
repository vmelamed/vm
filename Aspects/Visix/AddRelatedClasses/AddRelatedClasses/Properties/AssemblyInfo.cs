using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;

[assembly: AssemblyCompany("vm")]
[assembly: AssemblyProduct("vm.Aspects")]
[assembly: AssemblyCopyright("Copyright © vm 2013-2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = false)]
#else
[assembly: AssemblyConfiguration("Release")]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
#endif

// The types in this project are not CLS compliant, 
// so instead of including AssemblyInfo.global.cs we copy it here
[assembly: CLSCompliant(false)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("")]
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AddRelatedClasses")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyVersion("0.0.5")]
[assembly: AssemblyFileVersion("0.0.5")]
[assembly: AssemblyInformationalVersion("0.0.5")]
