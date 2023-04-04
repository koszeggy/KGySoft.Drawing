#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color16Gray.cs
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
using System.Collections.Generic;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 16-bit grayscale color.
    /// Implements <see cref="IEquatable{T}"/> because used in a <see cref="HashSet{T}"/> in <see cref="BitmapDataExtensions.GetColorCount{T}"/>
    /// </summary>
    internal readonly struct Color16Gray : IEquatable<Color16Gray>
    {
        #region Fields

        internal readonly ushort Value;

        #endregion

        #region Constructors

        internal Color16Gray(Color64 c)
        {
            Debug.Assert(c.A == UInt16.MaxValue);
            Value = c.GetBrightness();
        }

        internal Color16Gray(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            Value = ColorSpaceHelper.LinearToSrgb16Bit(c.GetBrightness());
        }

        #endregion

        #region Methods

        #region Public Methods

        public override int GetHashCode() => Value;

        public bool Equals(Color16Gray other) => Value == other.Value;

        public override bool Equals(object? obj) => obj is Color16Gray other && Equals(other);

        #endregion

        #region Internal Methods

        internal Color32 ToColor32() => Color32.FromGray((byte)(Value >> 8));

        #endregion

        #endregion
    }
}