// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0042:Deconstruct variable declaration", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "Decided individually")]
[assembly: SuppressMessage("ReSharper", "LocalizableElement", Justification = "This is just a Unit Test project")]

#if NET
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Supported also on Unix systems. See DrawingModule.")]
#endif