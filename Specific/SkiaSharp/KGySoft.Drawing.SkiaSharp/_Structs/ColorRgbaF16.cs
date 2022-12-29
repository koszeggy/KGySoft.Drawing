#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgbaF16.cs
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
using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ColorRgbaF16
    {
        #region Fields

        [FieldOffset(0)]
        private readonly Half r;
        [FieldOffset(2)]
        private readonly Half g;
        [FieldOffset(4)]
        private readonly Half b;
        [FieldOffset(6)]
        private readonly Half a;

        #endregion

        #region Properties

        private float A => (float)a;
        private float R => (float)r;
        private float G => (float)g;
        private float B => (float)b;

        #endregion

        #region Constructors

        #region Internal Constructors

        internal ColorRgbaF16(Color32 c)
        {
            r = (Half)(c.R / 255f);//.ToLinear();
            g = (Half)(c.G / 255f);//.ToLinear();
            b = (Half)(c.B / 255f);//.ToLinear();
            a = (Half)(c.A / 255f);
        }

        #endregion

        #region Private Constructors

        private ColorRgbaF16(float a, float r, float g, float b)
        {
            this.a = (Half)a;
            this.r = (Half)r;
            this.g = (Half)g;
            this.b = (Half)b;
        }

        #endregion

        #endregion

        #region Methods

        //internal Color32 ToColor32() => new Color32(a.To8Bit(), r.ToNonLinear8Bit(), g.ToNonLinear8Bit(), b.ToNonLinear8Bit());
        internal Color32 ToColor32() => new Color32(A.To8Bit(), R.To8Bit(), G.To8Bit(), B.To8Bit());

        internal ColorRgbaF16 ToPremultiplied()
        {
            float a = A;
            return a switch
            {
                <= 0f => default,
                < 1f => new ColorRgbaF16(a, a * R, a * G, a * B),
                _ => this
            };
        }

        internal ColorRgbaF16 ToStraight()
        {
            float a = A;
            return a switch
            {
                <= 0f => default,
                < 1f => new ColorRgbaF16(a, R / a, G / a, B / a),
                _ => this
            };
        }

        #endregion
    }
}