#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorDithering.cs
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
    internal struct BitmapDataAccessorDithering : IBitmapDataAccessor<Color32, IDitheringSession>
    {
        #region Fields

        private IBitmapDataRowInternal bitmapDataRow;
        private IBitmapDataInternal bitmapData;
        private IDitheringSession session;

        #endregion

        #region Methods

        public void InitBitmapData(IBitmapDataInternal bitmap, IDitheringSession ditheringSession)
        {
            bitmapData = bitmap;
            session = ditheringSession;
        }

        public Color32 GetColor(int x, int y) => bitmapData.DoGetColor32(x, y);
        public void SetColor(int x, int y, Color32 color) => bitmapData.DoSetColor32(x, y, session.GetDitheredColor(color, x, y));

        public void InitRow(IBitmapDataRowInternal row, IDitheringSession ditheringSession)
        {
            bitmapDataRow = row;
            session = ditheringSession;
        }

        public Color32 GetColor(int x) => bitmapDataRow.DoGetColor32(x);
        public void SetColor(int x, Color32 color) => bitmapDataRow.DoSetColor32(x, session.GetDitheredColor(color, x, bitmapDataRow.Index));

        #endregion
    }
}