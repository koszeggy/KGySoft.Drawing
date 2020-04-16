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
    /// Represents the raw data of a <see cref="Bitmap"/>. To obtain a readable or writable instance call the <see cref="BitmapExtensions.GetReadableBitmapData">GetReadableBitmapData</see>,
    /// <see cref="BitmapExtensions.GetWritableBitmapData">GetWritableBitmapData</see> or <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> extension methods
    /// on a <see cref="Bitmap"/> instance.
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
        /// <note>On some platforms this property can return a different <see cref="System.Drawing.Imaging.PixelFormat"/> from the <see cref="Image.PixelFormat">Image.PixelFormat</see> property of the original image.
        /// <br/>For details and further information about the possible usable <see cref="System.Drawing.Imaging.PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,System.Drawing.Imaging.PixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// </remarks>
        PixelFormat PixelFormat { get; }

        /// <summary>
        /// Gets a <see cref="Imaging.Palette"/> instance representing the colors of the owner <see cref="Bitmap"/> if it has an indexed <see cref="System.Drawing.Imaging.PixelFormat"/>,
        /// or <see langword="null"/>&#160;if the owner <see cref="Bitmap"/> is not an indexed one. For indexed bitmaps the <see cref="PixelFormat"/>
        /// property returns <see cref="System.Drawing.Imaging.PixelFormat.Format8bppIndexed"/>, <see cref="System.Drawing.Imaging.PixelFormat.Format4bppIndexed"/> or <see cref="System.Drawing.Imaging.PixelFormat.Format1bppIndexed"/>.
        /// </summary>
        Palette Palette { get; }

        /// <summary>
        /// Gets the size of a row in bytes (similar to <see cref="BitmapData.Stride">BitmapData.Stride</see> but this property always returns a positive value). Can be useful when accessing the bitmap data
        /// by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> or <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods.
        /// </summary>
        int RowSize { get; }

        #endregion
    }
}
