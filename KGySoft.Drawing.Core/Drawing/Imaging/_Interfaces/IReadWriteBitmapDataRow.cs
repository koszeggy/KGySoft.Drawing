#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IReadWriteBitmapDataRow.cs
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

using System;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides a fast read-write access to a single row of the actual data of a <see cref="Bitmap"/>. The owner <see cref="Bitmap"/> can have any <see cref="KnownPixelFormat"/>.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for details and examples.
    /// </summary>
    /// <seealso cref="IReadableBitmapDataRow"/>
    /// <seealso cref="IWritableBitmapDataRow"/>
    /// <seealso cref="IReadWriteBitmapData"/>
    /// <seealso cref="BitmapExtensions.GetReadWriteBitmapData"/>
    public interface IReadWriteBitmapDataRow : IReadableBitmapDataRow, IWritableBitmapDataRow
    {
        #region Indexers

        /// <summary>
        /// Gets or sets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <value>A <see cref="Color32"/> instance that represents the color of the specified pixel.</value>
        /// <remarks>
        /// <para>To return a <see cref="Color"/> structure you can use also the <see cref="IReadableBitmapDataRow.GetColor">GetColor</see> method but this member has a slightly better performance.</para>
        /// <para>To set the color from a <see cref="Color"/> structure you can use also the <see cref="IWritableBitmapDataRow.SetColor">SetColor</see> method but this member has a slightly better performance.</para>
        /// <para>The color value represents a straight (non-premultiplied) color with gamma correction γ = 2.2,
        /// regardless of the underlying <see cref="KnownPixelFormat"/>. To access the actual <see cref="KnownPixelFormat"/>-dependent raw data
        /// use the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> and <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods.</para>
        /// <para>If the color to be set is not supported by owner <see cref="Bitmap"/>, then it will be quantized to a supported color value.</para>
        /// <note>For information about the possible usable <see cref="KnownPixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,KnownPixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// <note>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadWriteBitmapData"/>.</exception>
        /// <seealso cref="IReadableBitmapDataRow.GetColor"/>
        /// <seealso cref="IWritableBitmapDataRow.SetColor"/>
        /// <seealso cref="IReadableBitmapDataRow.GetColorIndex"/>
        /// <seealso cref="IWritableBitmapDataRow.SetColorIndex"/>
        /// <seealso cref="IReadableBitmapDataRow.ReadRaw{T}"/>
        /// <seealso cref="IWritableBitmapDataRow.WriteRaw{T}"/>
        new Color32 this[int x] { get; set; }

        #endregion
    }
}