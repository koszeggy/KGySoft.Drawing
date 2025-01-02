﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgbaF32Srgb.cs
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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

#region Suppressions

#if NET8_0_OR_GREATER
#pragma warning disable CS9193 // Argument should be a variable because it is passed to a 'ref readonly' parameter - false alarm
#pragma warning disable CS9195 // Argument should be passed with the 'in' keyword - false alarm
#endif

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal readonly struct ColorPrgbaF32Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly float r;
        [FieldOffset(4)]private readonly float g;
        [FieldOffset(8)]private readonly float b;
        [FieldOffset(12)]private readonly float a;

        #endregion

        #region Constructors

        internal ColorPrgbaF32Srgb(PColor32 c) => this = Unsafe.As<PColorF, ColorPrgbaF32Srgb>(ref Unsafe.AsRef(c.ToPColorF(false)));
        internal ColorPrgbaF32Srgb(PColor64 c) => this = Unsafe.As<PColorF, ColorPrgbaF32Srgb>(ref Unsafe.AsRef(c.ToPColorF(false)));

        /// <summary>
        /// Note that this ctor is from ColorF and not PColorF because the color space change must be performed with straight colors
        /// so if the parameter was PColorF, then for getting/setting ColorF instances an unnecessary extra conversion would be performed.
        /// </summary>
        internal ColorPrgbaF32Srgb(ColorF c)
        {
            PColorF srgbF = ColorF.FromRgba(ColorSpaceHelper.LinearToSrgbVectorRgba(c.ToRgba())).ToPremultiplied();
            this = Unsafe.As<PColorF, ColorPrgbaF32Srgb>(ref srgbF);
        }

        #endregion

        #region Methods

        internal PColor32 ToPColor32() => Unsafe.As<ColorPrgbaF32Srgb, PColorF>(ref Unsafe.AsRef(this)).ToPColor32(false);
        internal PColor64 ToPColor64() => Unsafe.As<ColorPrgbaF32Srgb, PColorF>(ref Unsafe.AsRef(this)).ToPColor64(false);

        /// <summary>
        /// Note that this method converts to ColorF instead of PColorF.
        /// It's to spare an unnecessary back-and-forth conversion if ColorF is requested.
        /// </summary>
        internal ColorF ToColorF()
            => ColorF.FromRgba(ColorSpaceHelper.SrgbToLinearVectorRgba(Unsafe.As<ColorPrgbaF32Srgb, PColorF>(ref Unsafe.AsRef(this))
            .ToStraight()
            .ToRgba()));

        #endregion
    }
}