#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Delegates.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    public delegate TColor RowGetColor<TRow, TColor>(IBitmapData bitmapData, TRow row, int x) where TColor : unmanaged;
    public delegate void RowSetColor<TRow, TColor>(IBitmapData bitmapData, TRow row, int x, TColor color) where TColor : unmanaged;

    public delegate TColor RowGetColorByRef<T, TColor>(IBitmapData bitmapData, ref T rowReference, int x) where TColor : unmanaged;
    public delegate void RowSetColorByRef<T, TColor>(IBitmapData bitmapData, ref T rowReference, int x, TColor color) where TColor : unmanaged;
}
