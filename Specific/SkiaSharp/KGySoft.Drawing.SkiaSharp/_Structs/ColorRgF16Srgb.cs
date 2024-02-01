#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgF16Srgb.cs
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
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    internal readonly struct ColorRgF16Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly Half r;
        [FieldOffset(2)]private readonly Half g;

        #endregion

        #region Properties

        private float R => (float)r;
        private float G => (float)g;

        #endregion

        #region Constructors

        #region Internal Constructors

        internal ColorRgF16Srgb(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            r = (Half)ColorSpaceHelper.ToFloat(c.R);
            g = (Half)ColorSpaceHelper.ToFloat(c.G);
        }

        internal ColorRgF16Srgb(Color64 c)
        {
            Debug.Assert(c.A == UInt16.MaxValue);
            r = (Half)ColorSpaceHelper.ToFloat(c.R);
            g = (Half)ColorSpaceHelper.ToFloat(c.G);
        }

        internal ColorRgF16Srgb(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            r = (Half)ColorSpaceHelper.LinearToSrgb(c.R);
            g = (Half)ColorSpaceHelper.LinearToSrgb(c.G);
        }

        #endregion

        #region Private Constructors

        private ColorRgF16Srgb(Half r, Half g)
        {
            this.r = r;
            this.g = g;
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        internal static ColorRgF16Srgb FromSrgb(ColorF c) => new ColorRgF16Srgb((Half)c.R, (Half)c.G);

        #endregion

        #region Instance Methods

        internal Color32 ToColor32() => new Color32(ColorSpaceHelper.ToByte(R), ColorSpaceHelper.ToByte(G), 0);
        internal Color64 ToColor64() => new Color64(ColorSpaceHelper.ToUInt16(R), ColorSpaceHelper.ToUInt16(G), 0);
        internal ColorF ToColorF() => new ColorF(ColorSpaceHelper.SrgbToLinear(R), ColorSpaceHelper.SrgbToLinear(G), 0);

        #endregion

        #endregion
    }
}