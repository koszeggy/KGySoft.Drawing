﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorColorF.cs
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
    internal struct BitmapDataAccessorColorF : IBitmapDataAccessor<ColorF, ColorF, _>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;
        private IBitmapDataInternal bitmapData;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, _ _) => bitmapData = bitmap;
        [SecurityCritical]public ColorF GetColor(int x, int y) => bitmapData.DoGetColorF(x, y);
        [SecurityCritical]public void SetColor(int x, int y, ColorF color) => bitmapData.DoSetColorF(x, y, color);

        public void InitRow(IBitmapDataRowInternal row, _ _) => bitmapDataRow = row;
        [SecurityCritical]public ColorF GetColor(int x) => bitmapDataRow.DoGetColorF(x);
        [SecurityCritical]public void SetColor(int x, ColorF color) => bitmapDataRow.DoSetColorF(x, color);
        [SecurityCritical]public ColorF GetBaseColor(int x) => bitmapDataRow.DoGetColorF(x);
        [SecurityCritical]public void SetBaseColor(int x, ColorF color) => bitmapDataRow.DoSetColorF(x, color);

        #endregion
    }
}