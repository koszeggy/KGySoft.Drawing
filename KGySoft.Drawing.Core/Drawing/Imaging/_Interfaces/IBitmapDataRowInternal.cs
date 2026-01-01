#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataRowInternal.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

using System.Security;

namespace KGySoft.Drawing.Imaging
{
    internal interface IBitmapDataRowInternal : IReadWriteBitmapDataRowMovable
    {
        #region Properties
        
        IBitmapDataInternal BitmapData { get; }

        #endregion

        #region Methods

        [SecurityCritical]Color32 DoGetColor32(int x);
        [SecurityCritical]PColor32 DoGetPColor32(int x);
        [SecurityCritical]void DoSetColor32(int x, Color32 c);
        [SecurityCritical]void DoSetPColor32(int x, PColor32 c);
        [SecurityCritical]Color64 DoGetColor64(int x);
        [SecurityCritical]PColor64 DoGetPColor64(int x);
        [SecurityCritical]void DoSetColor64(int x, Color64 c);
        [SecurityCritical]void DoSetPColor64(int x, PColor64 c);
        [SecurityCritical]ColorF DoGetColorF(int x);
        [SecurityCritical]PColorF DoGetPColorF(int x);
        [SecurityCritical]void DoSetColorF(int x, ColorF c);
        [SecurityCritical]void DoSetPColorF(int x, PColorF c);
        [SecurityCritical]T DoReadRaw<T>(int x) where T : unmanaged;
        [SecurityCritical]void DoWriteRaw<T>(int x, T data) where T : unmanaged;
        [SecurityCritical]int DoGetColorIndex(int x);
        [SecurityCritical]void DoSetColorIndex(int x, int colorIndex);
        void DoMoveToRow(int y);

        #endregion
    }
}