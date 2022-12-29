#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgbaF32.cs
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

using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal readonly struct ColorRgbaF32
    {
        #region Fields

        [FieldOffset(0)]
        private readonly float r;
        [FieldOffset(4)]
        private readonly float g;
        [FieldOffset(8)]
        private readonly float b;
        [FieldOffset(12)]
        private readonly float a;

        #endregion

        #region Constructors

        #region Internal Constructors

        internal ColorRgbaF32(Color32 c)
        {
            r = (c.R / 255f);//.ToLinear();
            g = (c.G / 255f);//.ToLinear();
            b = (c.B / 255f);//.ToLinear();
            a = c.A / 255f;
        }

        #endregion

        #region Private Constructors

        private ColorRgbaF32(float a, float r, float g, float b)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }

        #endregion

        #endregion

        #region Methods

        //internal Color32 ToColor32() => new Color32(a.To8Bit(), r.ToNonLinear8Bit(), g.ToNonLinear8Bit(), b.ToNonLinear8Bit());
        internal Color32 ToColor32() => new Color32(a.To8Bit(), r.To8Bit(), g.To8Bit(), b.To8Bit());

        internal ColorRgbaF32 ToPremultiplied() => a switch
        {
            <= 0f => default,
            < 1f => new ColorRgbaF32(a, a * r, a * g, a * b),
            _ => this
        };

        internal ColorRgbaF32 ToStraight() => a switch
        {
            <= 0f => default,
            < 1f => new ColorRgbaF32(a, r / a, g / a, b / a),
            _ => this
        };

        #endregion
    }
}