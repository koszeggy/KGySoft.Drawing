#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color24.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct Color24 : IEquatable<Color24>
    {
        #region Fields

        [FieldOffset(0)]
        private readonly byte b;

        [FieldOffset(1)]
        private readonly byte g;

        [FieldOffset(2)]
        private readonly byte r;

        #endregion

        #region Constructors

        internal Color24(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            r = c.R;
            g = c.G;
            b = c.B;
        }

        #endregion

        #region Methods

        #region Public Methods

        public override int GetHashCode() => ToColor32().GetHashCode();
        public bool Equals(Color24 other) => r == other.r && g == other.g && b == other.b;
        public override bool Equals(object? obj) => obj is Color24 other && Equals(other);

        #endregion

        #region Internal Methods

        internal Color32 ToColor32() => new Color32(r, g, b);

        #endregion

        #endregion
    }
}