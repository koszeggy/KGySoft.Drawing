#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgbaF16Srgb.cs
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
using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ColorRgbaF16Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly Half r;
        [FieldOffset(2)]private readonly Half g;
        [FieldOffset(4)]private readonly Half b;
        [FieldOffset(6)]private readonly Half a;

        #endregion

        #region Properties

        private float A => (float)a;
        private float R => (float)r;
        private float G => (float)g;
        private float B => (float)b;

        #endregion

        #region Constructors
        
        #region Internal Constructors

        internal ColorRgbaF16Srgb(Color32 c)
        {
            ColorF srgbF = c.ToColorF(false);
            r = (Half)srgbF.R;
            g = (Half)srgbF.G;
            b = (Half)srgbF.B;
            a = (Half)srgbF.A;
        }

        internal ColorRgbaF16Srgb(Color64 c)
        {
            ColorF srgbF = c.ToColorF(false);
            r = (Half)srgbF.R;
            g = (Half)srgbF.G;
            b = (Half)srgbF.B;
            a = (Half)srgbF.A;
        }

        internal ColorRgbaF16Srgb(ColorF c)
        {
            ColorF srgbF = c.ToSrgb();
            r = (Half)srgbF.R;
            g = (Half)srgbF.G;
            b = (Half)srgbF.B;
            a = (Half)srgbF.A;
        }

        #endregion

        #region Private Constructors

        private ColorRgbaF16Srgb(Half a, Half r, Half g, Half b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        internal static ColorRgbaF16Srgb FromSrgb(ColorF c) => new ColorRgbaF16Srgb((Half)c.A, (Half)c.R, (Half)c.G, (Half)c.B);

        #endregion

        #region Instance Methods

        internal Color32 ToColor32() => new ColorF(A, R, G, B).ToColor32(false);
        internal Color64 ToColor64() => new ColorF(A, R, G, B).ToColor64(false);

        internal ColorF ToColorF() =>
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            ColorF.FromRgba(ColorSpaceHelper.SrgbToLinearVectorRgba(new ColorF(A, R, G, B).ToRgba()));
#else
            new ColorF(A,
                ColorSpaceHelper.SrgbToLinear(R),
                ColorSpaceHelper.SrgbToLinear(G),
                ColorSpaceHelper.SrgbToLinear(B));
#endif

        #endregion

        #endregion
    }
}