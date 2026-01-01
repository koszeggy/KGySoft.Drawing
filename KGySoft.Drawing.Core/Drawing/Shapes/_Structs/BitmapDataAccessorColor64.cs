#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorPColor32.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
    internal struct BitmapDataAccessorColor64 : IBitmapDataAccessor<Color64, Color64, _>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;
        private IBitmapDataInternal bitmapData;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, _ _) => bitmapData = bitmap;
        [SecurityCritical]public Color64 GetColor(int x, int y) => bitmapData.DoGetColor64(x, y);
        [SecurityCritical]public void SetColor(int x, int y, Color64 color) => bitmapData.DoSetColor64(x, y, color);

        public void InitRow(IBitmapDataRowInternal row, _ _) => bitmapDataRow = row;
        [SecurityCritical]public Color64 GetColor(int x) => bitmapDataRow.DoGetColor64(x);
        [SecurityCritical]public void SetColor(int x, Color64 color) => bitmapDataRow.DoSetColor64(x, color);
        [SecurityCritical]public Color64 GetBaseColor(int x) => bitmapDataRow.DoGetColor64(x);
        [SecurityCritical]public void SetBaseColor(int x, Color64 color) => bitmapDataRow.DoSetColor64(x, color);

        #endregion
    }
}