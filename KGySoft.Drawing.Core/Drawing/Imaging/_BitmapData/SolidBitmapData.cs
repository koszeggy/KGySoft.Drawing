#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SolidBitmapData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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

            #region Public Methods
            
            public override Color32 DoGetColor32(int x) => Color;
            public override T DoReadRaw<T>(int x) => throw new NotSupportedException(PublicResources.NotSupported);
            public override void DoSetColor32(int x, Color32 c) => throw new NotSupportedException(PublicResources.NotSupported);
            public override void DoWriteRaw<T>(int x, T data) => throw new NotSupportedException(PublicResources.NotSupported);

            #endregion

            #region Protected Methods

            protected override void DoMoveToIndex()
            {
            }

            #endregion

            #endregion
        }

        #endregion

        #region Fields

        private readonly Color32 color;

        #endregion

        #region Constructors

        internal SolidBitmapData(Size size, Color32 color)
            : base(new BitmapDataConfig(size, KnownPixelFormat.Format32bppArgb.ToInfoInternal()))
        {
            this.color = color;
        }

        #endregion

        #region Methods

        protected override Color32 DoGetColor32(int x, int y) => color;
        protected override void DoSetColor32(int x, int y, Color32 c) => throw new NotSupportedException(PublicResources.NotSupported);

        protected override IBitmapDataRowInternal DoGetRow(int y) => new Row
        {
            BitmapData = this,
            Color = color,
            Index = y,
        };

        #endregion
    }
}
