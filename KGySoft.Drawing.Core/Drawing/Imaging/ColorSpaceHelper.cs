using System;

namespace KGySoft.Drawing.Imaging
{
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

        public static float ToFloat(byte value) => (float)value / Byte.MaxValue;

        public static byte ToByte(float value)
        {
            value = value * Byte.MaxValue + 0.5f;
            return value < Byte.MinValue ? Byte.MinValue
                : value > Byte.MaxValue ? Byte.MaxValue
                : (byte)value; // including NaN, which will be 0
        }

        public static float SrgbToLinear(byte value) => Color32ToLinearCache.LookupTable[value];

        public static float SrgbToLinear(float value) => value switch
        {
            // formula is taken from here: https://en.wikipedia.org/wiki/SRGB
            <= 0f => 0f,
            <= 0.04045f => value / 12.92f,
            < 1f => MathF.Pow((value + 0.055f) / 1.055f, 2.4f),
            >= 1f => 1f,
            _ => 0 // NaN
        };

        internal static float LinearToSrgb(float value) => value switch
        {
            // formula is taken from here: https://en.wikipedia.org/wiki/SRGB
            <= 0f => 0,
            <= 0.0031308f => value * 12.92f,
            < 1f => (1.055f * MathF.Pow(value, 1f / 2.4f)) - 0.055f,
            >= 1f => 255,
            _ => 0 // NaN
        };

        internal static byte LinearToSrgb8Bit(float value) => value switch
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
