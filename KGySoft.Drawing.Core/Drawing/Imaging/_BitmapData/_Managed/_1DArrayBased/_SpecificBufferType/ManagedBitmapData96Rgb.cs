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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData96Rgb : ManagedBitmapData1DArrayBase<RgbF, ManagedBitmapData96Rgb.Row>
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowBase<RgbF>
        {
            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => Row[x].ToColor32();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c)
                => Row[x] = c.A == Byte.MaxValue
                    ? new RgbF(c)
                    : new RgbF(c.ToColorF().BlendWithBackground(((ManagedBitmapData96Rgb)BitmapData).backColorF, BitmapData.LinearWorkingColorSpace));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => Row[x].ToColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c)
                => Row[x] = c.A == UInt16.MaxValue
                    ? new RgbF(c)
                    : new RgbF(c.ToColorF().BlendWithBackground(((ManagedBitmapData96Rgb)BitmapData).backColorF, BitmapData.LinearWorkingColorSpace));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => Row[x].ToColor64().ToPColor64();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => DoSetColorF(x, c.ToColorF());

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => Row[x].ToColorF();

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c)
                => Row[x] = c.A >= 1f
                    ? new RgbF(c)
                    : new RgbF(c.BlendWithBackground(((ManagedBitmapData96Rgb)BitmapData).backColorF, BitmapData.LinearWorkingColorSpace));

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => Row[x].ToColorF().ToPColorF();

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

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => Buffer[y, x].ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, int y, Color32 c)
            => Buffer[y, x] = c.A == Byte.MaxValue
                ? new RgbF(c)
                : new RgbF(c.ToColorF().BlendWithBackground(backColorF, LinearWorkingColorSpace));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color64 DoGetColor64(int x, int y) => Buffer[y, x].ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor64(int x, int y, Color64 c)
            => Buffer[y, x] = c.A == UInt16.MaxValue
                ? new RgbF(c)
                : new RgbF(c.ToColorF().BlendWithBackground(backColorF, LinearWorkingColorSpace));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColor64 DoGetPColor64(int x, int y) => Buffer[y, x].ToColor64().ToPColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColor64(int x, int y, PColor64 c) => DoSetColorF(x, y, c.ToColorF());

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override ColorF DoGetColorF(int x, int y) => Buffer[y, x].ToColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorF(int x, int y, ColorF c)
            => Buffer[y, x] = c.A >= 1f
                ? new RgbF(c)
                : new RgbF(c.BlendWithBackground(backColorF, LinearWorkingColorSpace));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColorF DoGetPColorF(int x, int y) => Buffer[y, x].ToColorF().ToPColorF();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColorF(int x, int y, PColorF c) => DoSetColorF(x, y, c.ToColorF());

        #endregion
    }
}
