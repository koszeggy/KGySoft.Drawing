﻿using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("KGySoft.Drawing.Core")]
[assembly: AssemblyDescription("KGy SOFT Drawing Core Libraries")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("KGy SOFT")]
[assembly: AssemblyProduct("KGy SOFT Drawing")]
[assembly: AssemblyCopyright("Copyright © KGy SOFT. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7a04b1af-395d-4219-8828-a195bdea66f2")]

[assembly: CLSCompliant(true)]

[assembly: AssemblyVersion("8.2.0.0")]
[assembly: AssemblyFileVersion("8.2.0.0")]
[assembly: AssemblyInformationalVersion("8.2.0")]

[assembly: NeutralResourcesLanguage("en")]
[assembly: InternalsVisibleTo("KGySoft.Drawing.Core.UnitTest, PublicKey=00240000048000009400000006020000002400005253413100040000010001003928BADFAA8C02789566AB7AC64A59DCDE30B798589A68EF92CBB04C9DED3FCBFE41F644D424DCF82F8A13F9148D45EE15785450318388E01AA8C4CF645E81C772E39DCA0D14B33CF48167B70F5C34A0E7B763141ED3AFDDAD0373D9FCD2E153E78D201C5C4EB61DBBD586EC6291EABFBE11879865C3776088605FA8820387C2")]
[assembly: InternalsVisibleTo("KGySoft.Drawing.Core.PerformanceTest, PublicKey=00240000048000009400000006020000002400005253413100040000010001003928BADFAA8C02789566AB7AC64A59DCDE30B798589A68EF92CBB04C9DED3FCBFE41F644D424DCF82F8A13F9148D45EE15785450318388E01AA8C4CF645E81C772E39DCA0D14B33CF48167B70F5C34A0E7B763141ED3AFDDAD0373D9FCD2E153E78D201C5C4EB61DBBD586EC6291EABFBE11879865C3776088605FA8820387C2")]
[assembly: AllowPartiallyTrustedCallers]

#if !NET35
[assembly: SecurityRules(SecurityRuleSet.Level1, SkipVerificationInFullTrust = true)]
#endif
