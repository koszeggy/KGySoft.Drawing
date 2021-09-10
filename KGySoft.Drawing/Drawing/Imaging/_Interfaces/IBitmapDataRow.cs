#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataRow.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a single row of the raw data of a <see cref="Bitmap"/>. Reading or writing actual pixels is available via the derived interfaces of this type.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for details and examples.
    /// </summary>
    /// <seealso cref="IReadableBitmapDataRow"/>
    /// <seealso cref="IWritableBitmapDataRow"/>
    /// <seealso cref="IReadWriteBitmapDataRow"/>
    /// <seealso cref="IReadableBitmapData"/>
    /// <seealso cref="IWritableBitmapData"/>
    /// <seealso cref="IReadWriteBitmapData"/>
    /// <seealso cref="BitmapExtensions.GetReadableBitmapData"/>
    /// <seealso cref="BitmapExtensions.GetWritableBitmapData"/>
    /// <seealso cref="BitmapExtensions.GetReadWriteBitmapData"/>
    public interface IBitmapDataRow
    {
        #region Properties

        /// <summary>
        /// Gets the index of the current row. Can fall between zero and <see cref="IBitmapData.Height">Height</see> of the owner <see cref="IBitmapData"/> (exclusive upper bound).
        /// </summary>
        int Index { get; }

        #endregion

        #region Methods

        /// <summary>
        /// If not already in the last row (<see cref="Index"/> is less than <see cref="IBitmapData.Height">Height</see> of the owner <see cref="IBitmapData"/>),
        /// then advances the position of the current <see cref="IBitmapDataRow"/> instance so it points to the next row.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for examples.
        /// </summary>
        /// <returns><see langword="true"/>, if this <see cref="IBitmapDataRow"/> has been advanced to another row;
        /// <see langword="false"/>, if this <see cref="IBitmapDataRow"/> had already pointed to the last row before this method was called.</returns>
        bool MoveNextRow();

        #endregion
    }
}