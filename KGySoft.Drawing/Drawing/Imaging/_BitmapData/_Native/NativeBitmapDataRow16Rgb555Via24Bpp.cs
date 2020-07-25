#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataRow16Rgb555Via24Bpp.cs
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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class NativeBitmapDataRow16Rgb555Via24Bpp : NativeBitmapDataRowBase
    {
        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color32 DoGetColor32(int x)
            // performing the quantization to RGB555 from the actual RGB888
            => new Color16Rgb555(((Color24*)Address)[x].ToColor32()).ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor32(int x, Color32 c)
            // performing the quantization to RGB555 before setting as RGB888
            => ((Color24*)Address)[x] = new Color24(new Color16Rgb555(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor)).ToColor32());

        #endregion
    }
}