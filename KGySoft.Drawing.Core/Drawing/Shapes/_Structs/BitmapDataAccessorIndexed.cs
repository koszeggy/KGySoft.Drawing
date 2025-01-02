#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorIndexed.cs
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
    internal struct BitmapDataAccessorIndexed : IBitmapDataAccessor<int, _>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;
        private IBitmapDataInternal bitmapData;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, _ _) => bitmapData = bitmap;
        public int GetColor(int x, int y) => bitmapData.DoGetColorIndex(x, y);
        public void SetColor(int x, int y, int colorIndex) => bitmapData.DoSetColorIndex(x, y, colorIndex);

        public void InitRow(IBitmapDataRowInternal row, _ _) => bitmapDataRow = row;
        public int GetColor(int x) => bitmapDataRow.DoGetColorIndex(x);
        public void SetColor(int x, int colorIndex) => bitmapDataRow.DoSetColorIndex(x, colorIndex);

        #endregion
    }
}