#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataRowInternal.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal interface IBitmapDataRowInternal : IReadWriteBitmapDataRow
    {
        #region Methods

        Color32 DoGetColor32(int x);
        void DoSetColor32(int x, Color32 c);
        T DoReadRaw<T>(int x) where T : unmanaged;
        void DoWriteRaw<T>(int x, T data) where T : unmanaged;
        int DoGetColorIndex(int x);
        void DoSetColorIndex(int x, int colorIndex);

        #endregion
    }
}