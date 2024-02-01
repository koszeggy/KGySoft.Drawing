#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData96Rgb.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData96Rgb<T> : ManagedBitmapData1DArrayBase<T, ManagedBitmapData96Rgb<T>.Row>
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<T>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => DoReadRaw<RgbF>(x).ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
                => DoWriteRaw(x, c.A == Byte.MaxValue
                    ? new RgbF(c)
                    : new RgbF(c.ToColorF().BlendWithBackground(((ManagedBitmapData96Rgb<T>)BitmapData).backColorF, BitmapData.LinearWorkingColorSpace)));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => DoReadRaw<RgbF>(x).ToColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c)
                => DoWriteRaw(x, c.A == UInt16.MaxValue
                    ? new RgbF(c)
                    : new RgbF(c.ToColorF().BlendWithBackground(((ManagedBitmapData96Rgb<T>)BitmapData).backColorF, BitmapData.LinearWorkingColorSpace)));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => DoReadRaw<RgbF>(x).ToColor64().ToPColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => DoSetColorF(x, c.ToColorF());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => DoReadRaw<RgbF>(x).ToColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c)
                => DoWriteRaw(x, c.A >= 1f
                    ? new RgbF(c)
                    : new RgbF(c.BlendWithBackground(((ManagedBitmapData96Rgb<T>)BitmapData).backColorF, BitmapData.LinearWorkingColorSpace)));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => DoReadRaw<RgbF>(x).ToColorF().ToPColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => DoSetColorF(x, c.ToColorF());

            #endregion
        }

        #endregion

        #region Fields

        private readonly ColorF backColorF;

        #endregion

        #region Constructors

        internal ManagedBitmapData96Rgb(in BitmapDataConfig cfg)
            : base(cfg)
        {
            backColorF = BackColor.ToColorF();
        }

        internal ManagedBitmapData96Rgb(Array2D<T> buffer, in BitmapDataConfig cfg)
            : base(buffer, cfg)
        {
            backColorF = BackColor.ToColorF();
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color32 DoGetColor32(int x, int y) => GetPixelRef<RgbF>(y, x).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor32(int x, int y, Color32 c)
            => GetPixelRef<RgbF>(y, x) = c.A == Byte.MaxValue
                ? new RgbF(c)
                : new RgbF(c.ToColorF().BlendWithBackground(backColorF, LinearWorkingColorSpace));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override Color64 DoGetColor64(int x, int y) => GetPixelRef<RgbF>(y, x).ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColor64(int x, int y, Color64 c)
            => GetPixelRef<RgbF>(y, x) = c.A == UInt16.MaxValue
                ? new RgbF(c)
                : new RgbF(c.ToColorF().BlendWithBackground(backColorF, LinearWorkingColorSpace));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override PColor64 DoGetPColor64(int x, int y) => GetPixelRef<RgbF>(y, x).ToColor64().ToPColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColor64(int x, int y, PColor64 c) => DoSetColorF(x, y, c.ToColorF());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override ColorF DoGetColorF(int x, int y) => GetPixelRef<RgbF>(y, x).ToColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColorF(int x, int y, ColorF c)
            => GetPixelRef<RgbF>(y, x) = c.A >= 1f
                ? new RgbF(c)
                : new RgbF(c.BlendWithBackground(backColorF, LinearWorkingColorSpace));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override PColorF DoGetPColorF(int x, int y) => GetPixelRef<RgbF>(y, x).ToColorF().ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetPColorF(int x, int y, PColorF c) => DoSetColorF(x, y, c.ToColorF());

        #endregion
    }
}
