using System;

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// A helper class containing low-level conversion methods for <see cref="byte">byte</see> and <see cref="float">float</see> types
    /// to convert color components using the sRGB and linear color spaces.
    /// </summary>
    public static class ColorSpaceHelper
    {
        #region Color32Cache class

        private static class Color32ToLinearCache
        {
            #region Fields

            internal static readonly float[] LookupTable = InitLookupTable();

            #endregion

            #region Methods

            private static float[] InitLookupTable()
            {
                var result = new float[1 << 8];
                for (int i = 0; i <= Byte.MaxValue; i++)
                    result[i] = SrgbToLinear(i / (float)Byte.MaxValue);

                return result;
            }

            #endregion
        }

        #endregion

        /// <summary>
        /// Converts a <see cref="byte">byte</see> to a floating-point value between 0 and 1 without changing the color space.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A floating-point value between 0 and 1.</returns>
        public static float ToFloat(byte value) => (float)value / Byte.MaxValue;

        /// <summary>
        /// Converts a floating-point value ranging from 0 to 1 to a <see cref="byte">byte</see> without changing the color space.
        /// Out-of-range values are allowed in which case the result will be clipped
        /// to <see cref="Byte.MinValue">Byte.MinValue</see> or <see cref="Byte.MaxValue">Byte.MaxValue</see>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static byte ToByte(float value)
        {
            // Not using Math.Clamp because that does not convert NaN
            value = value * Byte.MaxValue + 0.5f;
            return value < Byte.MinValue ? Byte.MinValue
                : value > Byte.MaxValue ? Byte.MaxValue
                : (byte)value; // including NaN, which will be 0
        }

        /// <summary>
        /// Converts a <see cref="byte">byte</see> value representing an sRGB color component to a floating-point value between 0 and 1
        /// representing an RGB color component in the linear color space.
        /// </summary>
        /// <param name="value">The <see cref="byte">byte</see> value to convert.</param>
        /// <returns>A floating-point value between 0 and 1 representing an RGB color component in the linear color space.</returns>
        public static float SrgbToLinear(byte value) => Color32ToLinearCache.LookupTable[value];

        /// <summary>
        /// Converts a floating-point value representing an sRGB color component to a value representing an RGB color component in the linear color space.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A floating-point value between 0 and 1 representing an RGB color component in the linear color space.</returns>
        public static float SrgbToLinear(float value) => value switch
        {
            // formula is taken from here: https://en.wikipedia.org/wiki/SRGB
            <= 0f => 0f,
            <= 0.04045f => value / 12.92f,
            < 1f => MathF.Pow((value + 0.055f) / 1.055f, 2.4f),
            >= 1f => 1f,
            _ => 0 // NaN
        };

        /// <summary>
        /// Converts a floating-point value representing a color component in the linear color space
        /// to a value representing an sRGB color component.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A floating-point value between 0 and 1 representing an sRGB color component.</returns>
        public static float LinearToSrgb(float value) => value switch
        {
            // formula is taken from here: https://en.wikipedia.org/wiki/SRGB
            <= 0f => 0,
            <= 0.0031308f => value * 12.92f,
            < 1f => (1.055f * MathF.Pow(value, 1f / 2.4f)) - 0.055f,
            >= 1f => 255,
            _ => 0 // NaN
        };

        /// <summary>
        /// Converts a floating-point value representing a color component in the linear color space
        /// to a <see cref="byte">byte</see> value representing an sRGB color component.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A <see cref="byte">byte</see> value representing an sRGB color component.</returns>
        public static byte LinearToSrgb8Bit(float value) => value switch
        {
            // formula is taken from here: https://en.wikipedia.org/wiki/SRGB
            <= 0f => 0,
            <= 0.0031308f => (byte)((Byte.MaxValue * value * 12.92f) + 0.5f),
            < 1f => (byte)((Byte.MaxValue * ((1.055f * MathF.Pow(value, 1f / 2.4f)) - 0.055f)) + 0.5f),
            >= 1f => 255,
            _ => 0 // NaN
        };

    }
}
