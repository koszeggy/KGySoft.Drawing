#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IReadableBitmapData.cs
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
    /// Represents a readable <see cref="IBitmapData"/> instance.
    /// To create an instance use the <see cref="BitmapDataFactory"/> class or the <see cref="BitmapExtensions.GetReadableBitmapData">BitmapExtensions.GetReadableBitmapData</see> extension method.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for details and examples.
    /// </summary>
    /// <seealso cref="IWritableBitmapData"/>
    /// <seealso cref="IReadWriteBitmapData"/>
    /// <seealso cref="BitmapExtensions.GetReadableBitmapData"/>
    public interface IReadableBitmapData : IBitmapData
    {
        #region Properties and Indexers

        #region Properties

        /// <summary>
        /// Gets an <see cref="IReadableBitmapDataRow"/> instance representing the first row of the current <see cref="IReadableBitmapData"/>.
        /// Subsequent rows can be accessed by calling the <see cref="IBitmapDataRow.MoveNextRow">MoveNextRow</see> method on the returned instance
        /// while it returns <see langword="true"/>. Alternatively, you can use the <see cref="this">indexer</see> to obtain any row.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for examples.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadWriteBitmapData"/> has already been disposed.</exception>
        IReadableBitmapDataRow FirstRow { get; }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets an <see cref="IReadableBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IReadableBitmapData"/>.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for examples.
        /// </summary>
        /// <param name="y">The y-coordinate of the row to obtain.</param>
        /// <returns>An <see cref="IReadableBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IReadableBitmapData"/>.</returns>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadWriteBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        IReadableBitmapDataRow this[int y] { get; }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Gets the color of the pixel at the specified coordinates.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="Color"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>If multiple pixels need to be retrieved process the bitmap line by line for better performance.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// getting the pixels by the <see cref="IReadableBitmapDataRow"/> members and then moving to the next line by the <see cref="IBitmapDataRow.MoveNextRow">MoveNextRow</see> property.</para>
        /// <para>The returned value represents a straight (non-premultiplied) color with gamma correction γ = 2.2,
        /// regardless of the underlying <see cref="KnownPixelFormat"/>. To access the actual <see cref="KnownPixelFormat"/>-dependent raw value
        /// obtain a row and use the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> method.</para>
        /// <note>For information about the possible usable <see cref="KnownPixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,KnownPixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// <note>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for examples.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadWriteBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        Color GetPixel(int x, int y);

        #endregion
    }
}
