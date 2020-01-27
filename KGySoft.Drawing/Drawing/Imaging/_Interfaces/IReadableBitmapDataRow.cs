#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IReadableBitmapDataRow.cs
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

#region Usings

using System;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public interface IReadableBitmapDataRow : IBitmapDataRow
    {
        #region Indexers

        Color32 this[int x] { get; }

        #endregion

        #region Methods

        Color GetColor(int x);

        int GetColorIndex(int x);

        T ReadRaw<T>(int x) where T : unmanaged;

        #endregion
    }
}