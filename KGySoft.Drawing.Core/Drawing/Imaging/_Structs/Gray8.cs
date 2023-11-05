#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Gray8.cs
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
    /// Represents a 8-bit grayscale color.
    /// Implements <see cref="IEquatable{T}"/> because used in a <see cref="HashSet{T}"/> in <see cref="BitmapDataExtensions.GetColorCount{T}"/>
    /// </summary>
    internal readonly struct Gray8 : IEquatable<Gray8>
    {
        #region Fields

        internal readonly byte Value;

        #endregion

        #region Constructors

        internal Gray8(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            Value = c.GetBrightness();
        }

        internal Gray8(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            Value = ColorSpaceHelper.LinearToSrgb8Bit(c.GetBrightness());
        }

        #endregion

        #region Methods

        #region Public Methods

        public override int GetHashCode() => Value;

        public bool Equals(Gray8 other) => Value == other.Value;

        public override bool Equals(object? obj) => obj is Gray8 other && Equals(other);

        #endregion

        #region Internal Methods

        internal Color32 ToColor32() => Color32.FromGray(Value);

        #endregion

        #endregion
    }
}