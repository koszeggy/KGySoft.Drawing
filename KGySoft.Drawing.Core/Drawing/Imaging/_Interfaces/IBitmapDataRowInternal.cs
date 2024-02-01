#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataRowInternal.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal interface IBitmapDataRowInternal : IReadWriteBitmapDataRowMovable
    {
        #region Properties
        
        IBitmapData BitmapData { get; }

        #endregion

        #region Methods

        Color32 DoGetColor32(int x);
        PColor32 DoGetPColor32(int x);
        void DoSetColor32(int x, Color32 c);
        void DoSetPColor32(int x, PColor32 c);
        Color64 DoGetColor64(int x);
        PColor64 DoGetPColor64(int x);
        void DoSetColor64(int x, Color64 c);
        void DoSetPColor64(int x, PColor64 c);
        ColorF DoGetColorF(int x);
        PColorF DoGetPColorF(int x);
        void DoSetColorF(int x, ColorF c);
        void DoSetPColorF(int x, PColorF c);
        T DoReadRaw<T>(int x) where T : unmanaged;
        void DoWriteRaw<T>(int x, T data) where T : unmanaged;
        int DoGetColorIndex(int x);
        void DoSetColorIndex(int x, int colorIndex);
        void DoMoveToRow(int y);

        #endregion
    }
}