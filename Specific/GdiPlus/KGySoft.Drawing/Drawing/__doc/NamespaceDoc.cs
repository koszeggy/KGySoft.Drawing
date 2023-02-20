#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NamespaceDoc.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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
    /// The <see cref="N:KGySoft.Drawing"/> namespace contains extension methods and types built around the types of the <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing" target="_blank">System.Drawing</a> namespace.
    /// Among others, provides advanced support for the <see cref="Icon"/> type such as extracting, combining and converting multi-resolution icons, including hi-resolution ones,
    /// supports saving several <see cref="Image"/> formats, including formats without built-in encoders (eg. icons and <see cref="Metafile"/>s), provides methods for pixel format conversion, quantizing, dithering, etc.
    /// </summary>
    /// <remarks>
    /// <note>Starting with version 7.0.0 the <c>KGySoft.Drawing</c> libraries are split into multiple packages.
    /// The <strong>Assembly</strong> name is indicated for all types, which indicates also the the NuGet package the type is located in.</note>
    /// <h2>The available packages of KGy SOFT Drawing Libraries:</h2>
    /// <para><list type="definition">
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
    /// <description>This package provides dedicated support for the <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Windows.Media.Imaging.WriteableBitmap" target="_blank">System.Windows.Media.Imaging.WriteableBitmap</a>
    /// and <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Windows.Media.Imaging.BitmapSource" target="_blank">System.Windows.Media.Imaging.BitmapSource</a> types.
    /// Use the <see cref="M:KGySoft.Drawing.Wpf.WriteableBitmapExtensions.GetReadWriteBitmapData(System.Windows.Media.Imaging.WriteableBitmap,System.Windows.Media.Color,System.Byte)">WriteableBitmapExtensions.GetReadWriteBitmapData</see>
    /// extension method to expose the underlying buffer of a <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Windows.Media.Imaging.WriteableBitmap" target="_blank">WriteableBitmap</a> of any pixel format
    /// as an <see cref="IReadWriteBitmapData"/> to be able to use all of the core operations and transformations for a bitmap data.</description></item>
    /// <item><term><a href="https://www.nuget.org/packages/KGySoft.Drawing.WinUI/" target="_blank">KGySoft.Drawing.WinUI</a></term>
    /// <description>Similarly to the WPF package, this one provides support for the <a href="https://docs.microsoft.com/en-us/uwp/api/microsoft.ui.xaml.media.imaging.writeablebitmap" target="_blank">Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap</a>,
    /// which is used by <a href="https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/" target="_blank">Windows App SDK</a> in WinUI applications.
    /// It also has a <see cref="M:KGySoft.Drawing.WinUI.WriteableBitmapExtensions.GetReadWriteBitmapData(Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap)">GetReadWriteBitmapData</see>
    /// extension method to obtain an <see cref="IReadWriteBitmapData"/> instance for the bitmap.
    /// This package requires targeting at least .NET 5 and Windows 10.0.17763.0 (October 2018 release, version 1809).</description></item>
    /// <item><term><a href="https://www.nuget.org/packages/KGySoft.Drawing.Uwp/" target="_blank">KGySoft.Drawing.Uwp</a></term>
    /// <description>Just like the WinUI package, this one provides support for the <a href="https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.imaging.writeablebitmap" target="_blank">Windows.UI.Xaml.Media.Imaging.WriteableBitmap</a>
    /// used by the Universal Windows Platform (UWP) platform. Its documentation is not compiled to this combined documentation due to technical reasons
    /// but it provides exactly the same functionality as the WinUI package.
    /// This package requires targeting at least Windows 10.0.16299.0 (Fall Creators Update, version 1709) so it can reference the .NET Standard 2.0 version
    /// of the <a href="https://www.nuget.org/packages/KGySoft.Drawing.Core/" target="_blank">KGySoft.Drawing.Core</a> library.</description></item>
    /// </list></para>
    /// </remarks>
    [CompilerGenerated]
    internal static class NamespaceDoc
    {
    }
}
