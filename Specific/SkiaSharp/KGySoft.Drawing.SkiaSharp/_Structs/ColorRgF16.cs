#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgF16.cs
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
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    internal readonly struct ColorRgF16
    {
        #region Fields

        [FieldOffset(0)]
        private readonly Half r;
        [FieldOffset(2)]
        private readonly Half g;

        #endregion

        #region Properties

        private float R => (float)r;
        private float G => (float)g;

        #endregion

        #region Constructors

        internal ColorRgF16(Color32 c)
        {
            r = (Half)(c.R / 255f);//.ToLinear();
            g = (Half)(c.G / 255f);//.ToLinear();
        }

        #endregion

        #region Methods

        //internal Color32 ToColor32() => new Color32(r.ToNonLinear8Bit(), g.ToNonLinear8Bit(), 0);
        internal Color32 ToColor32() => new Color32(ColorSpaceHelper.ToByte(R), ColorSpaceHelper.ToByte(G), 0);

        #endregion
    }
}