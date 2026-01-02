#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NamespaceDoc.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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

#region Suppressions

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - the references to the other packages will be resolved by the documentation builder

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// The <see cref="N:KGySoft.Drawing"/> namespace contains extension methods and types built around the types of the <a href="https://learn.microsoft.com/en-us/dotnet/api/system.drawing" target="_blank">System.Drawing</a> namespace.
    /// Among others, provides advanced support for the <see cref="Icon"/> type such as extracting, combining and converting multi-resolution icons, including hi-resolution ones,
    /// supports saving several <see cref="Image"/> formats, including formats without built-in encoders (e.g. icons and <see cref="Metafile"/>s), provides methods for pixel format conversion, quantizing, dithering, etc.
    /// </summary>
    /// <remarks>
    /// <note>Starting with version 7.0.0 the <c>KGySoft.Drawing</c> libraries are split into multiple packages.
    /// The <strong>Assembly</strong> name is indicated for all types, which indicates also the NuGet package the type is located in.</note>
    /// <h2>The available packages of KGy SOFT Drawing Libraries</h2>
    /// <para><list type="definition">
    /// <item><term><a href="https://www.nuget.org/packages/KGySoft.Drawing.Core/" target="_blank">KGySoft.Drawing.Core</a></term>
    /// <description>This package contains the technology and platform independent core functionality and covers most types in
    /// the <see cref="N:KGySoft.Drawing.Imaging"/> and <see cref="N:KGySoft.Drawing.Shapes"/> namespaces along with a few ones in the <see cref="N:KGySoft.Drawing"/> namespace.
    /// The other packages are dependent on this one.</description></item>
    /// <item><term><a href="https://www.nuget.org/packages/KGySoft.Drawing/" target="_blank">KGySoft.Drawing</a></term>
    /// <description>Most types of the <see cref="N:KGySoft.Drawing"/> namespace are located in this package. It provides support for many GDI+ types such
    /// as <see cref="Bitmap"/>, <see cref="Metafile"/>, <see cref="Icon"/>, <see cref="Graphics"/> and is dependent on
    /// the <a href="https://www.nuget.org/packages/System.Drawing.Common/" target="_blank">System.Drawing.Common</a> package, which is
    /// supported only on Windows when targeting .NET 7 or later. This package has only two public classes in the <see cref="N:KGySoft.Drawing.Imaging"/>
    /// namespace: the <see cref="ReadableBitmapDataExtensions"/> class, containing some extension methods related to the <see cref="Bitmap"/> type, and the <see cref="ReadWriteBitmapDataExtensions"/> class,
    /// which contains GDI+ specific text drawing support for the <see cref="IReadWriteBitmapData"/> type. Additionally, it defines also a single type converter in the <see cref="N:KGySoft.ComponentModel"/> namespace.</description></item>
    /// <item><term><a href="https://www.nuget.org/packages/KGySoft.Drawing.Wpf/" target="_blank">KGySoft.Drawing.Wpf</a></term>
    /// <description>This package covers the <see cref="N:KGySoft.Drawing.Wpf"/> namespace, and provides dedicated support for the <a href="https://learn.microsoft.com/en-us/dotnet/api/System.Windows.Media.Imaging.WriteableBitmap" target="_blank">System.Windows.Media.Imaging.WriteableBitmap</a>
    /// and <a href="https://learn.microsoft.com/en-us/dotnet/api/System.Windows.Media.Imaging.BitmapSource" target="_blank">System.Windows.Media.Imaging.BitmapSource</a> types.
    /// Use the <see cref="M:KGySoft.Drawing.Wpf.WriteableBitmapExtensions.GetReadWriteBitmapData(System.Windows.Media.Imaging.WriteableBitmap,System.Windows.Media.Color,System.Byte)">WriteableBitmapExtensions.GetReadWriteBitmapData</see>
    /// extension method to expose the underlying buffer of a <a href="https://learn.microsoft.com/en-us/dotnet/api/System.Windows.Media.Imaging.WriteableBitmap" target="_blank">WriteableBitmap</a> of any pixel format
    /// as an <see cref="IReadWriteBitmapData"/> instance to be able to use all the core operations and transformations for a bitmap data.
    /// Like the GDI+ package, this one also has a <see cref="T:KGySoft.Drawing.Wpf.ReadWriteBitmapDataExtensions"/> class, containing WPF-specific text drawing support for the <see cref="IReadWriteBitmapData"/> type.</description></item>
    /// <item><term><a href="https://www.nuget.org/packages/KGySoft.Drawing.WinUI/" target="_blank">KGySoft.Drawing.WinUI</a></term>
    /// <description>Similarly to the WPF package, this one provides support for the <a href="https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.media.imaging.writeablebitmap" target="_blank">Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap</a>,
    /// which is used by <a href="https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/" target="_blank">Windows App SDK</a> in WinUI applications.
    /// It also has a <see cref="M:KGySoft.Drawing.WinUI.WriteableBitmapExtensions.GetReadWriteBitmapData(Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap)">GetReadWriteBitmapData</see>
    /// extension method to obtain an <see cref="IReadWriteBitmapData"/> instance for the bitmap.
    /// This package requires targeting at least .NET 6 and Windows 10.0.17763.0 (October 2018 release, version 1809).</description></item>
    /// <item><term><a href="https://www.nuget.org/packages/KGySoft.Drawing.Uwp/" target="_blank">KGySoft.Drawing.Uwp</a></term>
    /// <description>Just like the WinUI package, this one provides support for the <a href="https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.imaging.writeablebitmap" target="_blank">Windows.UI.Xaml.Media.Imaging.WriteableBitmap</a>
    /// used by the Universal Windows Platform (UWP) platform. Its documentation is not compiled to this combined documentation due to technical reasons,
    /// but it provides exactly the same functionality as the WinUI package.
    /// This package requires targeting at least Windows 10.0.16299.0 (Fall Creators Update, version 1709) so it can reference the .NET Standard 2.0 version
    /// of the <a href="https://www.nuget.org/packages/KGySoft.Drawing.Core/" target="_blank">KGySoft.Drawing.Core</a> library.</description></item>
    /// <item><term><a href="https://www.nuget.org/packages/KGySoft.Drawing.SkiaSharp/" target="_blank">KGySoft.Drawing.SkiaSharp</a></term>
    /// <description>This package covers the <see cref="N:KGySoft.Drawing.SkiaSharp"/> namespace, and provides dedicated support for the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>,
    /// <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpixmap">SKPixmap</a>, <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skimage">SKImage</a>
    /// and <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.sksurface">SKSurface</a> types. For example,
    /// the <see cref="M:KGySoft.Drawing.SkiaSharp.SKBitmapExtensions.GetReadWriteBitmapData(SkiaSharp.SKBitmap,SkiaSharp.SKColor,System.Byte)">SKBitmapExtensions.GetReadWriteBitmapData</see>
    /// method can be used to expose the underlying buffer of an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skbitmap">SKBitmap</a>
    /// as an <see cref="IReadWriteBitmapData"/> instance to access its pixels directly regardless of its pixel format and color space and to be able to
    /// perform all operations on it that are available for an <see cref="IReadWriteBitmapData"/> instance.
    /// Like the GDI+ and WPF packages, this one also has a <see cref="T:KGySoft.Drawing.SkiaSharp.ReadWriteBitmapDataExtensions"/> class, containing SkiaSharp-specific text drawing support for the <see cref="IReadWriteBitmapData"/> type.</description></item>
    /// </list></para>
    /// </remarks>
    [CompilerGenerated]
    internal static class NamespaceDoc;
}
