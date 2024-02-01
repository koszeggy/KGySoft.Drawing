#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GrayF.cs
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
using System.Collections.Generic;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 32-bit grayscale color.
    /// </summary>
    internal readonly struct GrayF : IEquatable<GrayF>
    {
        #region Fields

        internal readonly float Value;

        #endregion

        #region Constructors

        #region Internal Constructors
        
        internal GrayF(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            Value = ColorSpaceHelper.SrgbToLinear(c.GetBrightness());
        }

        internal GrayF(Color64 c)
        {
            Debug.Assert(c.A == UInt16.MaxValue);
            Value = ColorSpaceHelper.SrgbToLinear(c.GetBrightness());
        }

        internal GrayF(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            Value = c.GetBrightness();
        }

        #endregion

        #region Private Constructors

        private GrayF(float value) => Value = value;

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        public override int GetHashCode() => Value.GetHashCode();

        public bool Equals(GrayF other) => Value.Equals(other.Value);

        public override bool Equals(object? obj) => obj is GrayF other && Equals(other);

        #endregion

        #region Internal Methods

        internal GrayF Clip() => new GrayF(Value.ClipF());
        internal Color32 ToColor32() => Color32.FromGray(ColorSpaceHelper.LinearToSrgb8Bit(Value));
        internal Color64 ToColor64() => Color64.FromGray(ColorSpaceHelper.LinearToSrgb16Bit(Value));
        internal ColorF ToColorF() => ColorF.FromGray(Value);

        #endregion

        #endregion
    }
}