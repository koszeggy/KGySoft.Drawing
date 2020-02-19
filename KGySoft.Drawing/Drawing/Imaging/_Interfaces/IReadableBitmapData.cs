#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IReadableBitmapData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
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
    /// Represents a fast read-only access to the actual data of a <see cref="Bitmap"/>. The owner <see cref="Bitmap"/> can have any <see cref="PixelFormat"/>.
    /// <br/>See the <strong>Remarks</strong> section for details.
    /// </summary>
    /// Obtain an instance by the ... extension
    /// TODO: <para>For parallel processing you can retrieve multiple rows by the indexer and process them concurrently.</para>
    /// TODO: example: Processing by coordinates
    /// TODO: example: Line by line processing by FirstRow + MoveNextRow
    /// TODO: example: Parallel processing by FirstRow + MoveNextRow
    /// <seealso cref="IWritableBitmapData"/>
    /// <seealso cref="IReadWriteBitmapData"/>
    public interface IReadableBitmapData : IBitmapData
    {
        #region Properties and Indexers

        #region Properties

        IReadableBitmapDataRow FirstRow { get; }

        #endregion

        #region Indexers

        IReadableBitmapDataRow this[int y] { get; }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Gets the color of the specified pixel.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="Color"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>If multiple pixels need to be retrieved process the bitmap line by line for better performance.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// getting the pixels by its members and then moving to the next line by the <see cref="IBitmapDataRow.MoveNextRow">MoveNextRow</see> property.</para>
        /// <note>The returned value represents a straight (non-premultiplied) color with gamma correction γ = 2.2,
        /// regardless of the underlying <see cref="PixelFormat"/>. To access the actual <see cref="PixelFormat"/>-dependent raw value
        /// obtain a row and use the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> method.</note>
        /// <note>For information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// </remarks>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        Color GetPixel(int x, int y);

        #endregion
    }
}
