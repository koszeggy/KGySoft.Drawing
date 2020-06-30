#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataRowIndexedBase.cs
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
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class NativeBitmapDataRowIndexedBase : NativeBitmapDataRowBase
    {
        #region Properties

        protected abstract uint MaxIndex { get; }

        #endregion

        #region Methods

        #region Public Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void SetColorIndex(int x, int colorIndex)
        {
            if (colorIndex > BitmapData.Palette.Count || (uint)colorIndex > MaxIndex)
                throw new ArgumentOutOfRangeException(nameof(colorIndex), PublicResources.ArgumentOutOfRange);
            base.SetColorIndex(x, colorIndex);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x) => BitmapData.Palette.GetColor(DoGetColorIndex(x));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, Color32 c) => DoSetColorIndex(x, BitmapData.Palette.GetNearestColorIndex(c));

        #endregion

        #endregion
    }
}