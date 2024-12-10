﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomBitmapDataAccessorPColorF.cs
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
    internal struct CustomBitmapDataAccessorPColorF : IBitmapDataAccessor<PColorF, ColorF, _>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, _ _) => bitmapDataRow = bitmap.GetRowCached(0);

        public PColorF GetColor(int x, int y)
        {
            if (bitmapDataRow.Index != y)
                bitmapDataRow.DoMoveToRow(y);
            return bitmapDataRow.DoGetPColorF(x);
        }

        public void SetColor(int x, int y, PColorF color)
        {
            if (bitmapDataRow.Index != y)
                bitmapDataRow.DoMoveToRow(y);
            bitmapDataRow.DoSetPColorF(x, color);
        }

        public void InitRow(IBitmapDataRowInternal row, _ _) => bitmapDataRow = row;
        public PColorF GetColor(int x) => bitmapDataRow.DoGetPColorF(x);
        public void SetColor(int x, PColorF color) => bitmapDataRow.DoSetPColorF(x, color);
        public ColorF GetBaseColor(int x) => bitmapDataRow.DoGetColorF(x);
        public void SetBaseColor(int x, ColorF color) => bitmapDataRow.DoSetColorF(x, color);

        #endregion
    }
}