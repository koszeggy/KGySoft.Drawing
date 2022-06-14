#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NamespaceDoc.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// The <c>KGySoft.Drawing</c> namespace contains extension methods and types built around the types of the <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing" target="_blank">System.Drawing</a> namespace.
    /// Among others, provides advanced support for the <see cref="Icon"/> type such as extracting, combining and converting multi-resolution icons, including hi-resolution ones,
    /// supports saving several <see cref="Image"/> formats, including formats without built-in encoders (eg. icons and <see cref="Metafile"/>s), provides methods for pixel format conversion, quantizing, dithering, etc.
    /// <br/>See the <strong>Remarks</strong> section for details.
    /// </summary>
    /// <remarks>
    /// <note>Starting with version 7.0.0 the <c>KGySoft.Drawing</c> libraries have been split into multiple packages.
    /// If a type is not found at compile time you can check this documentation where the <strong>Assembly</strong> indicates the name of the assembly
    /// (and the NuGet package) in which the type is located.
    /// <list type="definition">
    /// <item><term><a href="https://www.nuget.org/packages/KGySoft.Drawing.Core/" target="_blank">KGySoft.Drawing.Core</a></term>
    /// <description>This package contains the technology and platform independent core functionality and covers most of the types in
    /// the <see cref="N:KGySoft.Drawing.Imaging"/> namespace along with a few ones in the <see cref="N:KGySoft.Drawing"/> namespace.
    /// The other packages are dependent on this one.</description></item>
    /// <item><term><a href="https://www.nuget.org/packages/KGySoft.Drawing/" target="_blank">KGySoft.Drawing</a></term>
    /// <description>Most types of the <see cref="N:KGySoft.Drawing"/> namespace are located in this package. It provides support for many GDI+ types such
    /// as <see cref="Bitmap"/>, <see cref="Metafile"/>, <see cref="Icon"/>, <see cref="Graphics"/> and is dependent on
    /// the <a href="https://www.nuget.org/packages/System.Drawing.Common/" target="_blank">System.Drawing.Common</a> package, which is
    /// supported only on Windows when targeting .NET 7 or later. This package has only one single public class in the <see cref="N:KGySoft.Drawing.Imaging"/>
    /// namespace, <see cref="ReadableBitmapDataExtensions"/>, which contains the extension methods that had to be removed from the <see cref="BitmapDataExtensions"/>
    /// class because they use the <see cref="Bitmap"/> type. Additionally, it defines also a single type converter in the <see cref="N:KGySoft.ComponentModel"/> namespace.</description></item>
    /// <item><term><a href="https://www.nuget.org/packages/KGySoft.Drawing.Wpf/" target="_blank">KGySoft.Drawing.Wpf</a></term>
    /// <description>This package provides direct support for the <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Windows.Media.Imaging.WriteableBitmap" target="_blank">System.Windows.Media.Imaging.WriteableBitmap</a>
    /// type and covers the <see cref="N:KGySoft.Drawing.Wpf"/> namespace.</description></item>
    /// </list></note>
    /// </remarks>
    [CompilerGenerated]
    internal static class NamespaceDoc
    {
    }
}
