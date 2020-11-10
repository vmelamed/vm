using System;
using System.Runtime.InteropServices;
using System.Security;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
#if DEBUG
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = false)]
#else
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
#endif
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(
    "vm.Aspects.Diagnostics.ObjectTextDumperTests, " +
    "PublicKey=00240000048000009400000006020000002400005253413100040000010001004b151837822bdb"+
              "b18296865918eb87882f2528630f673f17668fff4c6e51b8563b1985ec7ec5e6877611068c595a"+
              "e297a32e3bf9d71a5b479a59705f0688e91f086f20fa0772135ee08f693d5310ce18761d4f71ed"+
              "cb2486ec0910dd7361decc2184708dcbd5211feb361f45b024a45b91c28d7bed158671053085b8"+
              "fae385c1")]
