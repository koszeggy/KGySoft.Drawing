// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0056:Indexing can be simplified", Justification = "Cannot be used because it is not supported in every targeted platform")]
[assembly: SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Cannot be used because it is not supported in every targeted platform")]
[assembly: SuppressMessage("Style", "IDE0090:Use 'new(...)'", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "False alarm, Namespace Provider property is set to false to for folders that are not namespace providers")]
[assembly: SuppressMessage("Style", "IDE0270:Null check can be simplified (if null check)", Justification = "Decided individually. Sometimes it looks cleaner to have a separate validation block.")]
[assembly: SuppressMessage("Style", "IDE0300:Use collection expression for array'", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0305:Use collection expression for fluent", Justification = "Decided individually")]
[assembly: SuppressMessage("Style", "IDE0306:Use collection expression for new", Justification = "Decided individually. When type is not visible at the left side (e.g. field initialization), it looks cleaner if initializing with using a type name.")]
[assembly: SuppressMessage("ReSharper", "UseIndexFromEndExpression", Justification = "Cannot be used on all targeted platforms")]
[assembly: SuppressMessage("ReSharper", "ConvertToPrimaryConstructor", Justification = "Just. Don't. Do. It.")]
