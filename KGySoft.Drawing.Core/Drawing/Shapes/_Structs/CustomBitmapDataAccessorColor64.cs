#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomBitmapDataAccessorColor64.cs
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

using System.Security;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal struct CustomBitmapDataAccessorColor64 : IBitmapDataAccessor<Color64, Color64, _>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, _ _) => bitmapDataRow = bitmap.GetRowCached(0);

        [SecurityCritical]
        public Color64 GetColor(int x, int y)
        {
            if (bitmapDataRow.Index != y)
                bitmapDataRow.DoMoveToRow(y);
            return bitmapDataRow.DoGetColor64(x);
        }

        [SecurityCritical]
        public void SetColor(int x, int y, Color64 color)
        {
            if (bitmapDataRow.Index != y)
                bitmapDataRow.DoMoveToRow(y);
            bitmapDataRow.DoSetColor64(x, color);
        }

        public void InitRow(IBitmapDataRowInternal row, _ _) => bitmapDataRow = row;
        [SecurityCritical]public Color64 GetColor(int x) => bitmapDataRow.DoGetColor64(x);
        [SecurityCritical]public void SetColor(int x, Color64 color) => bitmapDataRow.DoSetColor64(x, color);
        [SecurityCritical]public Color64 GetBaseColor(int x) => bitmapDataRow.DoGetColor64(x);
        [SecurityCritical]public void SetBaseColor(int x, Color64 color) => bitmapDataRow.DoSetColor64(x, color);

        #endregion
    }
}