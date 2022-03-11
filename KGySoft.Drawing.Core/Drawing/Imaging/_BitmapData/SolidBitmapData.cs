#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SolidBitmapData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a read-only bitmap data of a single color.
    /// As a public instance should be exposed as an <see cref="IReadableBitmapData"/>.
    /// </summary>
    internal sealed class SolidBitmapData : BitmapDataBase
    {
        #region Row class

        private sealed class Row : BitmapDataRowBase
        {
            #region Fields

            internal Color32 Color;

            #endregion

            #region Methods

            public override Color32 DoGetColor32(int x) => Color;

            public override T DoReadRaw<T>(int x) => throw new InvalidOperationException();
            public override void DoSetColor32(int x, Color32 c) => throw new InvalidOperationException();
            public override void DoWriteRaw<T>(int x, T data) => throw new InvalidOperationException();

            #endregion
        }

        #endregion

        #region Fields

        private readonly Color32 color;

        private IBitmapDataRowInternal? lastRow;

        #endregion

        #region Constructors

        internal SolidBitmapData(Size size, Color32 color)
            : base(size, KnownPixelFormat.Format32bppArgb.ToInfo())
        {
            this.color = color;
        }

        #endregion

        #region Methods

        public override IBitmapDataRowInternal DoGetRow(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row. This is only needed because Index is mutable.
            IBitmapDataRowInternal? result = lastRow;
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
