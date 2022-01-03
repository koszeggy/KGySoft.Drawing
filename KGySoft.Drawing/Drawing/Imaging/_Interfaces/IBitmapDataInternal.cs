#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataInternal.cs
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
    internal interface IBitmapDataInternal : IReadWriteBitmapData
    {
        #region Properties

        bool CanSetPalette { get; }
        bool IsCustomPixelFormat { get; }

        #endregion

        #region Methods

        IBitmapDataRowInternal DoGetRow(int y);

        bool TrySetPalette(Palette? palette);

        #endregion
    }
}