#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorCmyk32.cs
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

using System;
using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Wpf
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    internal readonly struct ColorCmyk32
    {
        #region Fields

        [FieldOffset(0)]
        private readonly byte c;

        [FieldOffset(1)]
        private readonly byte m;
        
        [FieldOffset(2)]
        private readonly byte y;

        [FieldOffset(3)]
        private readonly byte k;

        #endregion

        #region Constructors

        internal ColorCmyk32(Color32 color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float kF = 1f - Math.Max(r, Math.Max(g, b));
            k = (byte)(kF * 255f);
            c = (byte)((1f - r - kF) / (1f - kF) * 255f);
            m = (byte)((1f - g - kF) / (1f - kF) * 255f);
            y = (byte)((1f - b - kF) / (1f - kF) * 255f);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(
            (byte)((1f - c / 255f) * (1f - k / 255f) * 255f),
            (byte)((1f - m / 255f) * (1f - k / 255f) * 255f),
            (byte)((1f - y / 255f) * (1f - k / 255f) * 255f));

        #endregion
    }
}