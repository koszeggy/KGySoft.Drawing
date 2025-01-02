#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomBitmapDataAccessorPColor32.cs
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
    internal struct CustomBitmapDataAccessorPColor32 : IBitmapDataAccessor<PColor32, Color32, _>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, _ _) => bitmapDataRow = bitmap.GetRowCached(0);

        public PColor32 GetColor(int x, int y)
        {
            if (bitmapDataRow.Index != y)
                bitmapDataRow.DoMoveToRow(y);
            return bitmapDataRow.DoGetPColor32(x);
        }

        public void SetColor(int x, int y, PColor32 color)
        {
            if (bitmapDataRow.Index != y)
                bitmapDataRow.DoMoveToRow(y);
            bitmapDataRow.DoSetPColor32(x, color);
        }

        public void InitRow(IBitmapDataRowInternal row, _ _) => bitmapDataRow = row;
        public PColor32 GetColor(int x) => bitmapDataRow.DoGetPColor32(x);
        public void SetColor(int x, PColor32 color) => bitmapDataRow.DoSetPColor32(x, color);
        public Color32 GetBaseColor(int x) => bitmapDataRow.DoGetColor32(x);
        public void SetBaseColor(int x, Color32 color) => bitmapDataRow.DoSetColor32(x, color);

        #endregion
    }
}