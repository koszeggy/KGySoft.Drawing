#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataInternal.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

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

        #endregion
    }
}