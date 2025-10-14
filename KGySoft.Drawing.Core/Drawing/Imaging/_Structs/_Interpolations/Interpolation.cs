#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Interpolation.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal struct Interpolation
    {
        #region Methods

        internal static float LanczosInterpolation(float value, float radius)
        {
            if (value < 0f)
                value = -value;
            return value < radius ? value.SinC() * (value / radius).SinC() : 0f;
        }

        internal static float CubicInterpolation(float value, float bSpline, float cardinal)
        {
            if (value < 0f)
                value = -value;

            float sqr = value * value;
            return value switch
            {
                < 1f => ((12f - 9f * bSpline - 6f * cardinal) * (value * sqr)
                    + (-18f + 12f * bSpline + 6f * cardinal) * sqr
                    + (6f - 2f * bSpline)) / 6f,
                < 2f => ((-bSpline - 6f * cardinal) * (value * sqr)
                    + (6f * bSpline + 30f * cardinal) * sqr
                    + (-12f * bSpline - 48f * cardinal) * value
                    + (8f * bSpline + 24f * cardinal)) / 6f,
                _ => 0f
            };
        }

        #endregion
    }
}