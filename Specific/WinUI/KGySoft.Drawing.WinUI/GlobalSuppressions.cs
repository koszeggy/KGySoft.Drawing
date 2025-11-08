// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility",
    Justification = "False alarm, the TargetFramework and .nuspec enforce that consumer applications and libraries have the required support.")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "False alarm, Namespace Provider property is set to false to for folders that are not namespace providers")]
