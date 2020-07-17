#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SolidBitmapData.cs
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
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a read-only bitmap data of a single color.
    /// As a public instance should be exposed as an <see cref="IReadableBitmapData"/>.
    /// </summary>
    internal class SolidBitmapData : BitmapDataBase
    {
        #region Nested classes

        #region Row class

        private sealed class Row : BitmapDataRowBase
        {
            #region Fields

            internal Color32 Color;

            #endregion

            #region Methods

            public override Color32 DoGetColor32(int x) => Color;

            public override T DoReadRaw<T>(int x) => throw new InvalidOperationException();
            public override int DoGetColorIndex(int x) => throw new InvalidOperationException();
            public override void DoSetColor32(int x, Color32 c) => throw new InvalidOperationException();
            public override void DoWriteRaw<T>(int x, T data) => throw new InvalidOperationException();
            public override void DoSetColorIndex(int x, int colorIndex) => throw new InvalidOperationException();

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly Color32 color;

        private IBitmapDataRowInternal lastRow;

        #endregion

        #region Properties

        public override int Height { get; }
        public override int Width { get; }
        public override PixelFormat PixelFormat => PixelFormat.Format32bppArgb;
        public override int RowSize => 0;

        #endregion

        #region Constructors

        internal SolidBitmapData(Size size, Color32 color)
        {
            Debug.Assert(size.Width > 0 && size.Height > 0);
            Width = size.Width;
            Height = size.Height;
            this.color = color;
        }

        #endregion

        #region Methods

        public override IBitmapDataRowInternal GetRow(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row. This is only needed because Index is mutable.
            IBitmapDataRowInternal result = lastRow;
            if (result?.Index == y)
                return result;

            // Otherwise, we create and cache the result.
            return lastRow = new Row
            {
                BitmapData = this,
                Color = color,
                Index = y,
            };
        }

        #endregion
    }
}
