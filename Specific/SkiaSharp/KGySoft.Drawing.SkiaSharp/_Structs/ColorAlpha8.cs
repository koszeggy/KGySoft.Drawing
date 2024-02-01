#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorAlpha8.cs
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

using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal readonly struct ColorAlpha8
    {
        #region Fields

        private readonly byte a;

        #endregion

        #region Constructors

        internal ColorAlpha8(Color32 c) => a = c.A;
        internal ColorAlpha8(PColor32 c) => a = c.A;
        internal ColorAlpha8(Color64 c) => a = ColorSpaceHelper.ToByte(c.A);
        internal ColorAlpha8(PColor64 c) => a = ColorSpaceHelper.ToByte(c.A);
        internal ColorAlpha8(ColorF c) => a = ColorSpaceHelper.ToByte(c.A);
        internal ColorAlpha8(PColorF c) => a = ColorSpaceHelper.ToByte(c.A);

        #endregion

        #region Methods

        internal Color32 ToColor32() => Color32.FromArgb(a << 24);
        internal PColor32 ToPColor32() => PColor32.FromArgb(a << 24);
        internal Color64 ToColor64() => Color64.FromArgb((ulong)ColorSpaceHelper.ToUInt16(a) << 48);
        internal PColor64 ToPColor64() => PColor64.FromArgb((ulong)ColorSpaceHelper.ToUInt16(a) << 48);
        internal ColorF ToColorF() => new ColorF(ColorSpaceHelper.ToFloat(a), 0f, 0f, 0f);
        internal PColorF ToPColorF() => new PColorF(ColorSpaceHelper.ToFloat(a), 0f, 0f, 0f);

        #endregion
    }
}