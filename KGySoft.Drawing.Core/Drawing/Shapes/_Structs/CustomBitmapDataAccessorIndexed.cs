#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomBitmapDataAccessorIndexed.cs
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
    internal struct CustomBitmapDataAccessorIndexed : IBitmapDataAccessor<int, _>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, _ _) => bitmapDataRow = bitmap.GetRowCached(0);

        public int GetColor(int x, int y)
        {
            if (bitmapDataRow.Index != y)
                bitmapDataRow.DoMoveToRow(y);
            return bitmapDataRow.DoGetColorIndex(x);
        }

        public void SetColor(int x, int y, int colorIndex)
        {
            if (bitmapDataRow.Index != y)
                bitmapDataRow.DoMoveToRow(y);
            bitmapDataRow.DoSetColorIndex(x, colorIndex);
        }

        public void InitRow(IBitmapDataRowInternal row, _ _) => bitmapDataRow = row;
        public int GetColor(int x) => bitmapDataRow.DoGetColorIndex(x);
        public void SetColor(int x, int colorIndex) => bitmapDataRow.DoSetColorIndex(x, colorIndex);

        #endregion
    }
}