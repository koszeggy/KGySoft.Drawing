#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IWritableBitmapDataRow.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public interface IWritableBitmapDataRow : IBitmapDataRow
    {
        #region Indexers

        [SuppressMessage("Microsoft.Design", "CA1044: Properties should not be write only",
            Justification = "The getter counterpart is in IReadableBitmapDataRow")]
        Color32 this[int x] { set; }

        #endregion

        #region Methods

        void SetColor(int x, Color color);

        void SetColorIndex(int x, int colorIndex);

        void WriteRaw<T>(int x, T data) where T : unmanaged;

        #endregion
    }
}