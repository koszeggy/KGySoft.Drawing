#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapData.cs
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

using System;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents the raw data of a bitmap. To create a managed instance use the <see cref="BitmapDataFactory"/> class.
    /// To obtain a readable or writable instance for platform specific bitmaps you can either use the specific <c>GetReadableBitmapData</c>, <c>GetWritableBitmapData</c>
    /// or <c>GetReadWriteBitmapData</c> extension methods when applicable (see the <strong>Remarks</strong> section of the <see cref="N:KGySoft.Drawing"/> namespace for a list
    /// about the supported platforms). Otherwise, you can use the members of the <see cref="BitmapDataFactory"/> class to create a bitmap data for
    /// any managed or unmanaged preallocated buffer of any bitmap implementation.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
    /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for all sources.
    /// </summary>
    /// <seealso cref="IReadableBitmapData"/>
    /// <seealso cref="IWritableBitmapData"/>
    /// <seealso cref="IReadWriteBitmapData"/>
    public interface IBitmapData : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the height of the current <see cref="IBitmapData"/> instance in pixels.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the width of the current <see cref="IBitmapData"/> instance in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the size of the current <see cref="IBitmapData"/> instance in pixels.
        /// </summary>
#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0
        Size Size { get; }
#else
        Size Size => new Size(Width, Height);
#endif

        /// <summary>
        /// Gets a <see cref="PixelFormatInfo"/> of the current <see cref="IBitmapData"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property determines how the raw underlying values should be interpreted if the pixels
        /// are accessed by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> or <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see>
        /// methods. Otherwise, in most cases using the members of the interfaces derived from the <see cref="IBitmapData"/> and <see cref="IBitmapDataRow"/> interfaces
        /// work seamlessly.</para>
        /// <para>If this property returns an indexed format (see <see cref="PixelFormatInfo.Indexed"/>),
        /// then the <see cref="Palette"/> property returns a non-<see langword="null"/> value.</para>
        /// </remarks>
        PixelFormatInfo PixelFormat { get; }

        /// <summary>
        /// Gets a <see cref="Imaging.Palette"/> instance representing the colors used in this <see cref="IBitmapData"/> if <see cref="PixelFormat"/> represents an indexed format.
        /// For indexed bitmaps <see cref="PixelFormatInfo.Indexed"/> property of <see cref="PixelFormat"/> returns <see langword="true"/>.
        /// </summary>
        Palette? Palette { get; }

        /// <summary>
        /// Gets the size of a row in bytes, or zero, if this <see cref="IBitmapData"/> instance does not have an actual raw buffer to access.
        /// </summary>
        /// <remarks>
        /// <para>This property can be useful when accessing the bitmap data by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> or <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods.</para>
        /// <para>As <see cref="IBitmapData"/> can represent any custom bitmap data, row size is not guaranteed to be a multiple of 4.</para>
        /// <note>
        /// <para>This property can return 0 if the current <see cref="IBitmapData"/> instance represents a bitmap data without actual raw data or represents a clipped
        /// region where the left edge of the clipping has an offset compared to the original bitmap data.</para>
        /// <para>Even if this property returns a nonzero value, it is possible that raw access does not cover the few last columns.
        /// This may occur in case of indexed <see cref="PixelFormat"/>s if the bitmap data is clipped and the right edge of the clipping does not fall at byte boundary.</para>
        /// </note>
        /// </remarks>
        int RowSize { get; }

        /// <summary>
        /// When accessing pixels of indexed bitmaps, or setting pixels of bitmaps without alpha support, gets the color of the background.
        /// For example, when setting color values with alpha, which are considered opaque, they will be blended with this color before setting the pixel.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> extension method for details and examples.
        /// </summary>
        Color32 BackColor { get; }

        /// <summary>
        /// If this <see cref="IBitmapData"/> represents a bitmap with single bit alpha or with a palette that has a transparent color,
        /// then gets a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the pixels to be set will never be transparent.
        /// </summary>
        byte AlphaThreshold { get; }

        /// <summary>
        /// Gets a hint indicating the preferred blending mode of this <see cref="IBitmapData"/> instance.
        /// Some operations, such as setting pixels, drawing another bitmap data into this instance and performing other operations
        /// consider the value of this property. Operations that use an <see cref="IQuantizer"/> instance may override the value of this property.
        /// <br/>Default value if not implemented: <see cref="Imaging.BlendingMode.Default"/>. (Only in .NET Core 3.0/.NET Standard 2.1 and above. In earlier targeted frameworks this member must be implemented.)
        /// </summary>
#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0
        BlendingMode BlendingMode { get; }
#else
        BlendingMode BlendingMode => BlendingMode.Default;
#endif

        /// <summary>
        /// Gets whether this <see cref="IBitmapData"/> instance is disposed.
        /// </summary>
        bool IsDisposed { get; }

        #endregion
    }
}
