#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataRow.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a single row of an <see cref="IBitmapData"/> instance. Reading or writing actual pixels is available via the derived interfaces of this type.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for details and examples.
    /// </summary>
    /// <seealso cref="IReadableBitmapDataRow"/>
    /// <seealso cref="IWritableBitmapDataRow"/>
    /// <seealso cref="IReadWriteBitmapDataRow"/>
    /// <seealso cref="IReadableBitmapData"/>
    /// <seealso cref="IWritableBitmapData"/>
    /// <seealso cref="IReadWriteBitmapData"/>
    public interface IBitmapDataRow
    {
        #region Properties

        /// <summary>
        /// Gets the index of the current row. Can fall between zero and <see cref="IBitmapData.Height">Height</see> of the owner <see cref="IBitmapData"/> (exclusive upper bound).
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets the width of the row in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the size of the row in bytes, or zero, if this <see cref="IBitmapDataRow"/> instance does not have an actual raw buffer to access.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="IBitmapData.RowSize">IBitmapData.RowSize</see> property for details.
        /// </summary>
        int Size { get; }

        #endregion
    }
}