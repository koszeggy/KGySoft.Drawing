#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorPColor64.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
    internal struct BitmapDataAccessorPColor64 : IBitmapDataAccessor<PColor64, Color64, _>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;
        private IBitmapDataInternal bitmapData;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, _ _) => bitmapData = bitmap;
        public PColor64 GetColor(int x, int y) => bitmapData.DoGetPColor64(x, y);
        public void SetColor(int x, int y, PColor64 color) => bitmapData.DoSetPColor64(x, y, color);

        public void InitRow(IBitmapDataRowInternal row, _ _) => bitmapDataRow = row;
        public PColor64 GetColor(int x) => bitmapDataRow.DoGetPColor64(x);
        public void SetColor(int x, PColor64 color) => bitmapDataRow.DoSetPColor64(x, color);
        public Color64 GetBaseColor(int x) => bitmapDataRow.DoGetColor64(x);
        public void SetBaseColor(int x, Color64 color) => bitmapDataRow.DoSetColor64(x, color);

        #endregion
    }
}