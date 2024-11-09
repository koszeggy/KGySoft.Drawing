﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorColor32.cs
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

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal struct BitmapDataAccessorColor32 : IBitmapDataAccessor<Color32, Color32, _>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;
        private IBitmapDataInternal bitmapData;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, _ _) => bitmapData = bitmap;
        public Color32 GetColor(int x, int y) => bitmapData.DoGetColor32(x, y);
        public void SetColor(int x, int y, Color32 color) => bitmapData.DoSetColor32(x, y, color);

        public void InitRow(IBitmapDataRowInternal row, _ _) => bitmapDataRow = row;
        public Color32 GetColor(int x) => bitmapDataRow.DoGetColor32(x);
        public void SetColor(int x, Color32 color) => bitmapDataRow.DoSetColor32(x, color);
        public Color32 GetBaseColor(int x) => bitmapDataRow.DoGetColor32(x);
        public void SetBaseColor(int x, Color32 color) => bitmapDataRow.DoSetColor32(x, color);

        #endregion
    }
}