// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("ReSharper", "LocalizableElement", Justification = "This is just a Unit Test project")]
[assembly: SuppressMessage("Style", "IDE0300:Use collection expression for array'", Justification = "Decided individually")]
#if NET
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "False alarm, WPF is supported on Windows only.")]
#endif