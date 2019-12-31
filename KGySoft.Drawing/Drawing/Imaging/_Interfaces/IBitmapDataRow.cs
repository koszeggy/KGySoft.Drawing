#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataRow.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
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
    public interface IBitmapDataRow
    {
        #region Properties and Indexers

        #region Properties

        IntPtr Address { get; }

        int Index { get; }

        #endregion

        #region Indexers

        Color32 this[int x] { get; set; }

        #endregion

        #endregion

        #region Methods

        Color GetColor(int x);

        void SetColor(int x, Color color);

        int GetColorIndex(int x);

        void SetColorIndex(int x, int colorIndex);

        T ReadRaw<T>(int x) where T : unmanaged;

        void WriteRaw<T>(int x, T data) where T : unmanaged;

        bool MoveNextRow();

        #endregion
    }
}