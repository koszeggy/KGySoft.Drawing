#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorQuantizing.cs
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
    internal struct BitmapDataAccessorQuantizing : IBitmapDataAccessor<Color32, IQuantizingSession>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;
        private IBitmapDataInternal bitmapData;
        private IQuantizingSession session;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, IQuantizingSession quantizingSession)
        {
            bitmapData = bitmap;
            session = quantizingSession;
        }

        public Color32 GetColor(int x, int y) => bitmapData.DoGetColor32(x, y);
        public void SetColor(int x, int y, Color32 color) => bitmapData.DoSetColor32(x, y, session.GetQuantizedColor(color));

        public void InitRow(IBitmapDataRowInternal row, IQuantizingSession quantizingSession)
        {
            bitmapDataRow = row;
            session = quantizingSession;
        }

        public Color32 GetColor(int x) => bitmapDataRow.DoGetColor32(x);
        public void SetColor(int x, Color32 color) => bitmapDataRow.DoSetColor32(x, session.GetQuantizedColor(color));

        #endregion
    }
}