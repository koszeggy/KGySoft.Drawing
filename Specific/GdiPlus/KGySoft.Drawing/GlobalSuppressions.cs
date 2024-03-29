// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly",
    Justification = "AssemblyInformationalVersion reflects the nuget versioning convention.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "~N:KGySoft.ComponentModel",
    Justification = "Along with the referenced CoreLibraries it contains more types.")]
[assembly: SuppressMessage("Style", "IDE0042:Deconstruct variable declaration", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0056:Indexing can be simplified", Justification = "Cannot be used because it is not supported in every targeted platform")]
[assembly: SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Cannot be used because it is not supported in every targeted platform")]
[assembly: SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "Decided individually")]

#if NET && !NET7_0_OR_GREATER
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Supported also on Unix systems. See DrawingModule.")]
#endif