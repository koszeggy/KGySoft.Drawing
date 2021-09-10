#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataRowInternal.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal interface IBitmapDataRowInternal : IReadWriteBitmapDataRow
    {
        #region Methods

        Color32 DoGetColor32(int x);
        Color32 DoGetColor32Premultiplied(int x);
        void DoSetColor32(int x, Color32 c);
        void DoSetColor32Premultiplied(int x, Color32 c);
        T DoReadRaw<T>(int x) where T : unmanaged;
        void DoWriteRaw<T>(int x, T data) where T : unmanaged;
        int DoGetColorIndex(int x);
        void DoSetColorIndex(int x, int colorIndex);

        #endregion
    }
}