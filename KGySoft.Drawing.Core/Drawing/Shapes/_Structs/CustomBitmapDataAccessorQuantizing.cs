﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomBitmapDataAccessorQuantizing.cs
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
    internal struct CustomBitmapDataAccessorQuantizing : IBitmapDataAccessor<Color32, IQuantizingSession>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;
        private IQuantizingSession session;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, IQuantizingSession quantizingSession)
        {
            bitmapDataRow = bitmap.GetRowCached(0);
            session = quantizingSession;
        }

        [SecurityCritical]
        public Color32 GetColor(int x, int y)
        {
            if (bitmapDataRow.Index != y)
                bitmapDataRow.DoMoveToRow(y);
            return bitmapDataRow.DoGetColor32(x);
        }

        [SecurityCritical]
        public void SetColor(int x, int y, Color32 color)
        {
            if (bitmapDataRow.Index != y)
                bitmapDataRow.DoMoveToRow(y);
            bitmapDataRow.DoSetColor32(x, session.GetQuantizedColor(color));
        }

        public void InitRow(IBitmapDataRowInternal row, IQuantizingSession quantizingSession)
        {
            bitmapDataRow = row;
            session = quantizingSession;
        }

        [SecurityCritical]public Color32 GetColor(int x) => bitmapDataRow.DoGetColor32(x);
        [SecurityCritical]public void SetColor(int x, Color32 color) => bitmapDataRow.DoSetColor32(x, session.GetQuantizedColor(color));

        #endregion
    }
}