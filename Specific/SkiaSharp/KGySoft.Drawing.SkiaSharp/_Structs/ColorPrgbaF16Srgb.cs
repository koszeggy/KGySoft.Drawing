#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgbaF32Srgb.cs
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
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ColorPrgbaF16Srgb
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

        internal ColorPrgbaF16Srgb(PColor32 c)
        {
            PColorF srgbF = c.ToPColorF(false);
            r = (Half)srgbF.R;
            g = (Half)srgbF.G;
            b = (Half)srgbF.B;
            a = (Half)srgbF.A;
        }

        internal ColorPrgbaF16Srgb(PColor64 c)
        {
            PColorF srgbF = c.ToPColorF(false);
            r = (Half)srgbF.R;
            g = (Half)srgbF.G;
            b = (Half)srgbF.B;
            a = (Half)srgbF.A;
        }

        /// <summary>
        /// Note that this ctor is from ColorF and not PColorF because the color space change must be performed with straight colors
        /// so if the parameter was PColorF, then for getting/setting ColorF instances an unnecessary extra conversion would be performed.
        /// </summary>
        internal ColorPrgbaF16Srgb(ColorF c)
        {
            PColorF srgbF = c.ToSrgb().ToPremultiplied();
            r = (Half)srgbF.R;
            g = (Half)srgbF.G;
            b = (Half)srgbF.B;
            a = (Half)srgbF.A;
        }

        #endregion

        #region Methods

        internal PColor32 ToPColor32() => new PColorF(A, R, G, B).ToPColor32(false);
        internal PColor64 ToPColor64() => new PColorF(A, R, G, B).ToPColor64(false);

        /// <summary>
        /// Note that this method converts to ColorF instead of PColorF.
        /// It's to spare an unnecessary back-and-forth conversion if ColorF is requested.
        /// </summary>
        internal ColorF ToColorF()
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return ColorF.FromRgba(ColorSpaceHelper.SrgbToLinearVectorRgba(new PColorF(A, R, G, B).ToStraight().ToRgba()));
#else
            ColorF srgbF = new PColorF(A, R, G, B).ToStraight();
            return new ColorF(srgbF.A,
                ColorSpaceHelper.SrgbToLinear(srgbF.R),
                ColorSpaceHelper.SrgbToLinear(srgbF.G),
                ColorSpaceHelper.SrgbToLinear(srgbF.B));
#endif
        }

        #endregion
    }
}