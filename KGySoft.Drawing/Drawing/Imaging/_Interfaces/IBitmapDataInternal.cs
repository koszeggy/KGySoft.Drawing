#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataInternal.cs
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
    internal interface IBitmapDataInternal : IReadWriteBitmapData
    {
        #region Properties

        bool CanSetPalette { get; }

        #endregion

        #region Methods

        IBitmapDataRowInternal DoGetRow(int y);

        bool TrySetPalette(Palette palette);

        #endregion
    }
}