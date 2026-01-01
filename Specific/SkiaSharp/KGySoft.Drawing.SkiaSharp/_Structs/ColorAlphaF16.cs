#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorAlphaF16.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Sequential, Size = 2)]
    internal readonly struct ColorAlphaF16
    {
        #region Fields

        private readonly Half a;

        #endregion

        #region Constructors

        internal ColorAlphaF16(Color32 c) => a = (Half)ColorSpaceHelper.ToFloat(c.A);
        internal ColorAlphaF16(PColor32 c) => a = (Half)ColorSpaceHelper.ToFloat(c.A);
        internal ColorAlphaF16(Color64 c) => a = (Half)ColorSpaceHelper.ToFloat(c.A);
        internal ColorAlphaF16(PColor64 c) => a = (Half)ColorSpaceHelper.ToFloat(c.A);
        internal ColorAlphaF16(ColorF c) => a = (Half)c.A;
        internal ColorAlphaF16(PColorF c) => a = (Half)c.A;

        #endregion

        #region Methods

        internal Color32 ToColor32() => Color32.FromArgb(ColorSpaceHelper.ToByte((float)a) << 24);
        internal PColor32 ToPColor32() => PColor32.FromArgb(ColorSpaceHelper.ToByte((float)a) << 24);
        internal Color64 ToColor64() => Color64.FromArgb((ulong)ColorSpaceHelper.ToUInt16((float)a) << 48);
        internal PColor64 ToPColor64() => PColor64.FromArgb((ulong)ColorSpaceHelper.ToUInt16((float)a) << 48);
        internal ColorF ToColorF() => new ColorF((float)a, 0f, 0f, 0f);
        internal PColorF ToPColorF() => new PColorF((float)a, 0f, 0f, 0f);

        #endregion
    }
}