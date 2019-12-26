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

using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public interface IBitmapDataRow
    {
        #region Indexers

        Color this[int x] { get; set; }

        #endregion

        #region Methods

        Color32 GetPixelColor32(int x);

        Color64 GetPixelColor64(int x);

        int GetPixelColorIndex(int x);

        Color32 SetPixelColor(int x, Color32 color);

        Color64 SetPixelColor(int x, Color64 color);

        void SetPixelColorIndex(int x, int colorIndex);

        bool MoveNextRow();

        #endregion
    }
}