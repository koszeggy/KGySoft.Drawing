﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData16Rgb565_2D.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Needed to separate digits")]
    internal sealed class ManagedBitmapData16Rgb565_2D<T> : ManagedBitmapData2DArrayBase<T, ManagedBitmapData16Rgb565_2D<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRow2DBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<Color16Rgb565>(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
                => DoWriteRaw(x, new Color16Rgb565(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BitmapData.BackColor)));

            #endregion
        }

        #endregion

        #region Constructors

        internal ManagedBitmapData16Rgb565_2D(T[,] buffer, int pixelWidth, Color32 backColor, byte alphaThreshold, Action? disposeCallback)
            : base(buffer, pixelWidth, KnownPixelFormat.Format16bppRgb565.ToInfoInternal(), backColor, alphaThreshold, disposeCallback)
        {
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetPixel(int x, int y) => GetPixelRef<Color16Rgb565>(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPixel(int x, int y, Color32 c)
            => GetPixelRef<Color16Rgb565>(y, x) = new Color16Rgb565(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BackColor));

        #endregion
    }
}