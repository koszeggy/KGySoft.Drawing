#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataRowMovable.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a single row of an <see cref="IBitmapData"/> instance that allows setting its position to any row. Reading or writing actual pixels is available via the derived interfaces of this type.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for details and examples.
    /// </summary>
    public interface IBitmapDataRowMovable : IBitmapDataRow
    {
        #region Methods

        /// <summary>
        /// If not already in the last row (<see cref="IBitmapDataRow.Index"/> is less than <see cref="IBitmapData.Height">Height</see> of the owner <see cref="IBitmapData"/>),
        /// then advances the position of the current <see cref="IBitmapDataRowMovable"/> instance so it points to the next row.
        /// <br/>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.
        /// </summary>
        /// <returns><see langword="true"/>, if this <see cref="IBitmapDataRow"/> has been advanced to another row;
        /// <see langword="false"/>, if this <see cref="IBitmapDataRow"/> had already pointed to the last row before this method was called.</returns>
        bool MoveNextRow();

        /// <summary>
        /// Moves the current <see cref="IBitmapDataRowMovable"/> to the specified row of the underlying <see cref="IBitmapData"/>.
        /// </summary>
        /// <param name="y">The row index to set.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="y"/> is negative or greater than or equal to <see cref="IBitmapData.Height">Height</see> of the owner <see cref="IBitmapData"/>.</exception>
        void MoveToRow(int y);

        #endregion
    }
}
