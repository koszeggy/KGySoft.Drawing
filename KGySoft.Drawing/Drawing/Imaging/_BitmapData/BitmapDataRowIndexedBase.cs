#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRowIndexedBase.cs
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
    internal abstract class BitmapDataRowIndexedBase : BitmapDataRowBase
    {
        #region Fields

        internal Palette Palette;

        #endregion

        #region Properties

        protected abstract uint MaxIndex { get; }

        #endregion

        #region Methods

        #region Public Methods

        public override Color GetColor(int x) => Palette.GetColor(GetColorIndex(x));

        public override void SetColorIndex(int x, int colorIndex)
        {
            if (colorIndex > Palette.Length || (uint)colorIndex > MaxIndex)
                throw new ArgumentOutOfRangeException(nameof(colorIndex), PublicResources.ArgumentOutOfRange);
            base.SetColorIndex(x, colorIndex);
        }

        #endregion

        #region Internal Methods

        internal override Color32 DoGetColor32(int x) => Palette.GetColor32(DoGetColorIndex(x));

        internal override void DoSetColor32(int x, Color32 c) => DoSetColorIndex(x, Palette.GetColorIndex(c));

        #endregion

        #endregion
    }
}