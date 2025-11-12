#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IReadableBitmapDataRowMovable.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a single row of an <see cref="IReadableBitmapData"/> instance that allows setting its position to any row.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://koszeggy.github.io/docs/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for details and examples.
    /// </summary>
    public interface IReadableBitmapDataRowMovable : IReadableBitmapDataRow, IBitmapDataRowMovable
    {
    }
}