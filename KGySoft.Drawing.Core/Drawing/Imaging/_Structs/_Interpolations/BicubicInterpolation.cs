#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BicubicInterpolation.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal struct BicubicInterpolation : IFixedSizeInterpolation
    {
        #region Properties

        public float Radius => 2f;

        #endregion

        #region Methods

        public float GetValue(float value)
        {
            if (value < 0f)
                value = -value;
            return value <= 1f ? (1.5f * value - 2.5f) * value * value + 1f
                : value < 2f ? ((-0.5F * value + 2.5F) * value - 4f) * value + 2f
                : 0f;
        }

        #endregion
    }
}