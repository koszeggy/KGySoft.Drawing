#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Gray16.cs
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
using System.Collections.Generic;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 16-bit grayscale color.
    /// Implements <see cref="IEquatable{T}"/> because used in a <see cref="HashSet{T}"/> in <see cref="BitmapDataExtensions.GetColorCount{T}"/>
    /// </summary>
    internal readonly struct Gray16 : IEquatable<Gray16>
    {
        #region Fields

        internal readonly ushort Value;

        #endregion

        #region Constructors

        internal Gray16(Color64 c)
        {
            Debug.Assert(c.A == UInt16.MaxValue);
            Value = c.GetBrightness();
        }

        internal Gray16(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            Value = ColorSpaceHelper.LinearToSrgb16Bit(c.GetBrightness());
        }

        #endregion

        #region Methods

        #region Public Methods

        public override int GetHashCode() => Value;

        public bool Equals(Gray16 other) => Value == other.Value;

        public override bool Equals(object? obj) => obj is Gray16 other && Equals(other);

        #endregion

        #region Internal Methods

        internal Color32 ToColor32() => Color32.FromGray(ColorSpaceHelper.ToByte(Value));
        internal Color64 ToColor64() => Color64.FromGray(Value);

        #endregion

        #endregion
    }
}