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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataRowIndexedBase : BitmapDataRowBase
    {
        #region Properties

        protected abstract uint MaxIndex { get; }

        #endregion

        #region Methods

        #region Public Methods

        public override void SetColorIndex(int x, int colorIndex)
        {
            if (colorIndex > Accessor.Palette.Count || (uint)colorIndex > MaxIndex)
                throw new ArgumentOutOfRangeException(nameof(colorIndex), PublicResources.ArgumentOutOfRange);
            base.SetColorIndex(x, colorIndex);
        }

        #endregion

        #region Internal Methods

        [SecurityCritical]
        internal override Color32 DoGetColor32(int x) => Accessor.Palette.GetColor(DoGetColorIndex(x));

        [SecurityCritical]
        internal override void DoSetColor32(int x, Color32 c) => DoSetColorIndex(x, Accessor.Palette.GetNearestColorIndex(c));

        #endregion

        #endregion
    }
}