#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IReadWriteBitmapData.cs
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
    /// Provides a fast read-write access to the actual data of a <see cref="Bitmap"/>. The owner <see cref="Bitmap"/> can have any <see cref="PixelFormat"/>.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for details and examples.
    /// </summary>
    /// <seealso cref="IReadableBitmapData"/>
    /// <seealso cref="IWritableBitmapData"/>
    /// <seealso cref="BitmapExtensions.GetReadWriteBitmapData"/>
    public interface IReadWriteBitmapData : IReadableBitmapData, IWritableBitmapData
    {
        #region Properties and Indexers

        #region Properties

        /// <summary>
        /// Gets an <see cref="IReadWriteBitmapDataRow"/> instance representing the first row of the current <see cref="IReadWriteBitmapData"/>.
        /// Subsequent rows can be accessed by calling the <see cref="IBitmapDataRow.MoveNextRow">MoveNextRow</see> method on the returned instance
        /// while it returns <see langword="true"/>. Alternatively, you can use the <see cref="this">indexer</see> to obtain any row.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for examples.
        /// </summary>
        new IReadWriteBitmapDataRow FirstRow { get; }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets an <see cref="IReadWriteBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IReadWriteBitmapData"/>.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for examples.
        /// </summary>
        /// <param name="y">The y.</param>
        /// <returns>An <see cref="IReadWriteBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IReadWriteBitmapData"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        new IReadWriteBitmapDataRow this[int y] { get; }

        #endregion

        #endregion
    }
}
