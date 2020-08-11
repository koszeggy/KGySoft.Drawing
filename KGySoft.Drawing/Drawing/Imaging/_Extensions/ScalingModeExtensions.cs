#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ScalingModeExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class ScalingModeExtensions
    {
        #region Methods

        #region Internal Methods

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "False alarm, Enum.ToString is not affected by culture")]
        internal static (float, Func<float, float>) GetInterpolation(this ScalingMode scalingMode)
        {
            switch (scalingMode)
            {
                case ScalingMode.NearestNeighbor:
                    return (1f, x => x);
                case ScalingMode.Box:
                    return (0.5f, BoxInterpolation);
                case ScalingMode.Bilinear:
                    return (1f, BilinearInterpolation);
                case ScalingMode.Bicubic:
                    return (2f, BicubicInterpolation);
                case ScalingMode.Lanczos2:
                    return (2f, Lanczos2Interpolation);
                case ScalingMode.Lanczos3:
                    return (3f, Lanczos3Interpolation);
                case ScalingMode.Spline:
                    return (2f, SplineInterpolation);
                case ScalingMode.CatmullRom:
                    return (2f, CatmullRomInterpolation);
                case ScalingMode.MitchellNetravali:
                    return (2f, MitchellNetravaliInterpolation);
                case ScalingMode.Robidoux:
                    return (2f, RobidouxInterpolation);
                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected scaling mode: {scalingMode}"));
            }
        }

        #endregion

        #region Private Methods

        private static float BoxInterpolation(float value) => value > -0.5f && value <= 0.5f ? 1f : 0f;

        private static float BilinearInterpolation(float value)
        {
            if (value < 0f)
                value = -value;
            return value < 1f ? 1f - value : 0f;
        }

        private static float BicubicInterpolation(float value)
        {
            if (value < 0f)
                value = -value;
            return value <= 1f ? (1.5f * value - 2.5f) * value * value + 1f
                : value < 2f ? ((-0.5F * value + 2.5F) * value - 4f) * value + 2f
                : 0f;
        }

        private static float LanczosInterpolation(float value, float radius)
        {
            if (value < 0f)
                value = -value;
            return value < radius ? SinC(value) * SinC(value / radius) : 0f;
        }

        private static float Lanczos2Interpolation(float value) => LanczosInterpolation(value, 2f);

        private static float Lanczos3Interpolation(float value) => LanczosInterpolation(value, 3f);

        private static float CubicInterpolation(float value, float bspline, float cardinal)
        {
            if (value < 0f)
                value = -value;

            float sqr = value * value;

            if (value < 1f)
            {
                value = (12f - 9f * bspline - 6f * cardinal) * (value * sqr)
                    + (-18f + 12f * bspline + 6f * cardinal) * sqr
                    + (6f - 2f * bspline);
                return value / 6f;
            }

            if (value < 2f)
            {
                value = (-bspline - 6f * cardinal) * (value * sqr)
                    + (6f * bspline + 30f * cardinal) * sqr
                    + (-12f * bspline - 48f * cardinal) * value
                    + (8f * bspline + 24f * cardinal);
                return value / 6f;
            }

            return 0f;
        }

        private static float SplineInterpolation(float value) => CubicInterpolation(value, 1f, 0f);

        private static float CatmullRomInterpolation(float value) => CubicInterpolation(value, 0f, 0.5f);

        private static float MitchellNetravaliInterpolation(float value) => CubicInterpolation(value, 0.33333333f, 0.33333333f);

        private static float RobidouxInterpolation(float value) => CubicInterpolation(value, 0.37821576f, 0.310892122f);

        /// <summary>
        /// Gets the sine cardinal of <paramref name="x"/>, which is Sin(PI * x) / (PI * x)
        /// </summary>
        private static float SinC(float x)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            double d = x * Math.PI;
            return (float)(Math.Sin(d) / d);
#else
            x *= MathF.PI;
            return MathF.Sin(x) / x;
#endif

        }

        #endregion

        #endregion
    }
}
