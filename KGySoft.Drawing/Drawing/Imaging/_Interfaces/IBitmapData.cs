#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents the raw data of a bitmap. To create a managed instance use the <see cref="BitmapDataFactory"/> class.
    /// To obtain a readable or writable instance for a native <see cref="Bitmap"/> instance call the <see cref="BitmapExtensions.GetReadableBitmapData">GetReadableBitmapData</see>,
    /// <see cref="BitmapExtensions.GetWritableBitmapData">GetWritableBitmapData</see> or <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> extension methods.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for details and examples.
    /// </summary>
    /// <seealso cref="IReadableBitmapData"/>
    /// <seealso cref="IWritableBitmapData"/>
    /// <seealso cref="IReadWriteBitmapData"/>
    /// <seealso cref="BitmapExtensions.GetReadableBitmapData"/>
    /// <seealso cref="BitmapExtensions.GetWritableBitmapData"/>
    /// <seealso cref="BitmapExtensions.GetReadWriteBitmapData"/>
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
        /// Gets the <see cref="PixelFormat"/> of the current <see cref="IBitmapData"/> instance.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property determines how the raw underlying values should be interpreted if the pixels
        /// are accessed by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> or <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see>
        /// methods. Otherwise, in most cases using the members of the interfaces derived from the <see cref="IBitmapData"/> and <see cref="IBitmapDataRow"/> interfaces
        /// work seamlessly.</para>
        /// <para>If this property returns an indexed format (<see cref="System.Drawing.Imaging.PixelFormat.Format8bppIndexed"/>, <see cref="System.Drawing.Imaging.PixelFormat.Format4bppIndexed"/> or <see cref="System.Drawing.Imaging.PixelFormat.Format1bppIndexed"/>),
        /// then the <see cref="Palette"/> property returns a non-<see langword="null"/>&#160;value.</para>
        /// <note>If this <see cref="IBitmapData"/> instance represents a native <see cref="Bitmap"/>, then on some platforms this property can return a different <see cref="System.Drawing.Imaging.PixelFormat"/>
        /// from the <see cref="Image.PixelFormat">Image.PixelFormat</see> property of the original image.
        /// <br/>For details and further information about the possible usable <see cref="System.Drawing.Imaging.PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,System.Drawing.Imaging.PixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// </remarks>
        PixelFormat PixelFormat { get; }

        /// <summary>
        /// Gets a <see cref="Imaging.Palette"/> instance representing the colors of the owner <see cref="Bitmap"/> if it has an indexed <see cref="System.Drawing.Imaging.PixelFormat"/>,
        /// or <see langword="null"/>&#160;if the owner <see cref="Bitmap"/> is not an indexed one. For indexed bitmaps the <see cref="PixelFormat"/>
        /// property returns <see cref="System.Drawing.Imaging.PixelFormat.Format8bppIndexed"/>, <see cref="System.Drawing.Imaging.PixelFormat.Format4bppIndexed"/> or <see cref="System.Drawing.Imaging.PixelFormat.Format1bppIndexed"/>.
        /// </summary>
        Palette? Palette { get; }

        /// <summary>
        /// Gets the size of a row in bytes, or zero, if this <see cref="IBitmapData"/> instance does not have an actual raw buffer to access.
        /// Otherwise, <see cref="RowSize"/> is similar to <see cref="BitmapData.Stride">BitmapData.Stride</see> but this property never returns a negative value.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <remarks>
        /// <para>This property can be useful when accessing the bitmap data by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> or <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods.</para>
        /// <para>As <see cref="IBitmapData"/> can represent also a managed bitmap data, row size is not guaranteed to be a multiple of 4.</para>
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
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> extension method for details and examples.
        /// </summary>
        Color32 BackColor { get; }

        /// <summary>
        /// If this <see cref="IBitmapData"/> represents a bitmap with single bit alpha or with a palette that has a transparent color,
        /// then gets a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the pixels to be set will never be transparent.
        /// </summary>
        byte AlphaThreshold { get; }

        #endregion
    }
}
