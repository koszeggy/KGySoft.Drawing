﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData16Argb1555.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData16Argb1555<T> : ManagedBitmapDataCastArray2DBase<T, Color16Argb1555, ManagedBitmapData16Argb1555<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataCastArrayRowBase<T, Color16Argb1555>
        {
            #region Methods
            
            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => GetPixelRef(x).ToColor32();

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
            {
                if (c.A != Byte.MaxValue)
                {
                    c = c.A >= BitmapData.AlphaThreshold ? c.BlendWithBackground(BitmapData.BackColor, BitmapData.LinearWorkingColorSpace)
                        : c.A < 128 ? c
                        : default;
                }

                GetPixelRef(x) = new Color16Argb1555(c);
            }

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData16Argb1555(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        internal ManagedBitmapData16Argb1555(CastArray2D<T, Color16Argb1555> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => GetPixelRef(y, x).ToColor32();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, int y, Color32 c)
        {
            if (c.A != Byte.MaxValue)
            {
                c = c.A >= AlphaThreshold ? c.BlendWithBackground(BackColor, LinearWorkingColorSpace)
                    : c.A < 128 ? c
                    : default;
            }

            GetPixelRef(y, x) = new Color16Argb1555(c);
        }

        #endregion
    }
}
