#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataInternal.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal interface IBitmapDataInternal : IReadWriteBitmapData
    {
        #region Properties

        bool CanSetPalette { get; }
        bool IsCustomPixelFormat { get; }

        #endregion

        #region Methods

        bool TrySetPalette(Palette? palette);
        IBitmapDataRowInternal GetRowUncached(int y);

        /// <summary>
        /// Should be called from internal row access if the row is not exposed for public usage.
        /// If called repeatedly by the same thread, the same row instance is returned with adjusted row index.
        /// Works only if the result row is used in a scope that is never accessible to multiple threads.
        /// </summary>
        IBitmapDataRowInternal GetRowCached(int y);
        IBitmapDataRowInternal GetRowCached(int y, IBitmapDataRowInternal usedRow);

        [SecurityCritical]Color32 DoGetColor32(int x, int y);
        [SecurityCritical]void DoSetColor32(int x, int y, Color32 c);
        [SecurityCritical]PColor32 DoGetPColor32(int x, int y);
        [SecurityCritical]void DoSetPColor32(int x, int y, PColor32 c);
        [SecurityCritical]Color64 DoGetColor64(int x, int y);
        [SecurityCritical]void DoSetColor64(int x, int y, Color64 c);
        [SecurityCritical]PColor64 DoGetPColor64(int x, int y);
        [SecurityCritical]void DoSetPColor64(int x, int y, PColor64 c);
        [SecurityCritical]ColorF DoGetColorF(int x, int y);
        [SecurityCritical]void DoSetColorF(int x, int y, ColorF c);
        [SecurityCritical]PColorF DoGetPColorF(int x, int y);
        [SecurityCritical]void DoSetPColorF(int x, int y, PColorF c);
        [SecurityCritical]T DoReadRaw<T>(int x, int y) where T : unmanaged;
        [SecurityCritical]void DoWriteRaw<T>(int x, int y, T data) where T : unmanaged;
        [SecurityCritical]int DoGetColorIndex(int x, int y);
        [SecurityCritical]void DoSetColorIndex(int x, int y, int colorIndex);

        #endregion
    }
}