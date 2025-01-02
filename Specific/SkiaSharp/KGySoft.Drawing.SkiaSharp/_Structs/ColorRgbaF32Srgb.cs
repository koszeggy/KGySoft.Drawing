#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgbaF32Srgb.cs
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
    internal readonly struct ColorRgbaF32Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly float r;
        [FieldOffset(4)]private readonly float g;
        [FieldOffset(8)]private readonly float b;
        [FieldOffset(12)]private readonly float a;

        #endregion

        #region Constructors

        internal ColorRgbaF32Srgb(Color32 c) => this = Unsafe.As<ColorF, ColorRgbaF32Srgb>(ref Unsafe.AsRef(c.ToColorF(false)));
        internal ColorRgbaF32Srgb(Color64 c) => this = Unsafe.As<ColorF, ColorRgbaF32Srgb>(ref Unsafe.AsRef(c.ToColorF(false)));
        internal ColorRgbaF32Srgb(ColorF c) => this = Unsafe.As<ColorF, ColorRgbaF32Srgb>(ref Unsafe.AsRef(c.ToSrgb()));

        #endregion

        #region Methods

        #region Static Methods

        internal static ColorRgbaF32Srgb FromSrgb(ColorF c) => Unsafe.As<ColorF, ColorRgbaF32Srgb>(ref c);

        #endregion

        #region Instance Methods

        internal Color32 ToColor32() => Unsafe.As<ColorRgbaF32Srgb, ColorF>(ref Unsafe.AsRef(this)).ToColor32(false);
        internal Color64 ToColor64() => Unsafe.As<ColorRgbaF32Srgb, ColorF>(ref Unsafe.AsRef(this)).ToColor64(false);
        internal ColorF ToColorF() => new ColorF(a, r, g, b).ToLinear();

        #endregion

        #endregion
    }
}