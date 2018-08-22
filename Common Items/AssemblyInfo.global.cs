using System;
using System.Resources;
using System.Runtime.InteropServices;

#if NETFRAMEWORK
using System.Reflection;
using System.Security;

[assembly: AssemblyProduct("vm.Aspects")]
[assembly: AssemblyCompany("vm")]
[assembly: AssemblyCopyright("Copyright © vm 2013-2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = false)]
#else
[assembly: AssemblyConfiguration("Release")]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
#endif
#endif

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguage("en-US")]
