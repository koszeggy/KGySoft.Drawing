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
    public delegate Color32 RowGetColor<TRow>(IBitmapData bitmapData, TRow row, int x);
    public delegate void RowSetColor<TRow>(IBitmapData bitmapData, TRow row, int x, Color32 c);
    public delegate int RowGetColorIndex<TRow>(IBitmapData bitmapData, TRow row, int x);
    public delegate void RowSetColorIndex<TRow>(IBitmapData bitmapData, TRow row, int x, int colorIndex);

    public delegate Color32 RowGetColorByRef<T>(IBitmapData bitmapData, ref T rowReference, int x);
    public delegate void RowSetColorByRef<T>(IBitmapData bitmapData, ref T rowReference, int x, Color32 c);
    public delegate int RowGetColorIndexByRef<T>(IBitmapData bitmapData, ref T rowReference, int x);
    public delegate void RowSetColorIndexByRef<T>(IBitmapData bitmapData, ref T rowReference, int x, int colorIndex);
}
