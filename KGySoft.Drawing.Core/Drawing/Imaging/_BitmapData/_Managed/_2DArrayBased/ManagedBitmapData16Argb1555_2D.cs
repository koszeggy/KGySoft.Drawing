﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData16Argb1555_2D.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Needed to separate digits")]
    internal sealed class ManagedBitmapData16Argb1555_2D<T> : ManagedBitmapData2DArrayBase<T, ManagedBitmapData16Argb1555_2D<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRow2DBase<T>
        {
            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<Color16Argb1555>(x).ToColor32();

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

                DoWriteRaw(x, new Color16Argb1555(c));
            }

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData16Argb1555_2D(T[,] buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
        }

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => GetPixelRef<Color16Argb1555>(y, x).ToColor32();

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

            GetPixelRef<Color16Argb1555>(y, x) = new Color16Argb1555(c);
        }

        #endregion
    }
}
