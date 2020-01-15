#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OrderedDitherer.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    public class OrderedDitherer : IDitherer
    {
#pragma warning disable CA1814 // arrays in this class are better as matrices than jagged arrays as they really should be rectangular

        #region Nested classes

        #region OrderedDitheringSession class

        private sealed class OrderedDitheringSession : IDitheringSession
        {
            #region Fields

            private readonly IQuantizingSession quantizer;
            private readonly OrderedDitherer ditherer;
            private readonly float strength;

            #endregion

            #region Properties

            public bool IsSequential => false;

            #endregion

            #region Constructors

            internal OrderedDitheringSession(IQuantizingSession quantizer, OrderedDitherer ditherer)
            {
                this.quantizer = quantizer ?? throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);
                this.ditherer = ditherer;
                if (ditherer.strength > 0f)
                {
                    strength = ditherer.strength;
                    return;
                }

                // Auto strength is calculated by color count. The correct value actually depends on the
                // used quantizer and the image. In general case (with not so perfect colors) the lower is better.
                int colorCount = quantizer.Palette?.Length ?? 0; // or 32768, 2^24 but we don't know exactly
                strength = colorCount == 0 ? 1 / 32f
                    : colorCount == 2 ? 1f
                    : 1 / (Math.Min(colorCount + 1, 16) / 2f);
            }

            #endregion

            #region Methods

            public Color32 GetDitheredColor(Color32 origColor, int x, int y)
            {
                static byte ToByteSafe(int value)
                    => value < Byte.MinValue ? Byte.MinValue
                        : value > Byte.MaxValue ? Byte.MaxValue
                        : (byte)value;

                Color32 c;

                // handling alpha
                if (origColor.A != Byte.MaxValue)
                {
                    // the color can be considered fully transparent
                    if (origColor.A < quantizer.AlphaThreshold)
                    {
                        // and even the quantizer returns a transparent color
                        c = quantizer.GetQuantizedColor(origColor);
                        if (c.A == 0)
                            return c;
                    }

                    // the color will not be transparent in the end: blending
                    c = origColor.BlendWithBackground(quantizer.BackColor);
                }
                else
                    c = origColor;

                // applying the matrix and strength adjustments
                int offset = ditherer.premultipliedMatrix[y % ditherer.matrixHeight, x % ditherer.matrixWidth];
                if (strength < 1)
                    offset = (int)(offset * strength);

                c = new Color32(ToByteSafe(c.R + offset), ToByteSafe(c.G + offset), ToByteSafe(c.B + offset));

                // getting the quantized value of the dithered result
                return quantizer.GetQuantizedColor(c);
            }

            public void Dispose()
            {
            }

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        #region Static Fields
        // ReSharper disable InconsistentNaming - x in names are meant to be lowercase

        private static readonly byte[,] bayer2x2 =
        {
            { 0, 2 },
            { 3, 1 },
        };

        private static readonly byte[,] bayer3x3 =
        {
            { 0, 7, 3 },
            { 6, 5, 2 },
            { 4, 1, 8 },
        };

        private static readonly byte[,] bayer4x4 =
        {
            { 0, 8, 2, 10 },
            { 12, 4, 14, 6 },
            { 3, 11, 1, 9 },
            { 15, 7, 13, 5 },
        };

        private static readonly byte[,] bayer8x8 =
        {
            { 0, 48, 12, 60, 3, 51, 15, 63 },
            { 32, 16, 44, 28, 35, 19, 47, 31 },
            { 8, 56, 4, 52, 11, 59, 7, 55 },
            { 40, 24, 36, 20, 43, 27, 39, 23 },
            { 2, 50, 14, 62, 1, 49, 13, 61 },
            { 34, 18, 46, 30, 33, 17, 45, 29 },
            { 10, 58, 6, 54, 9, 57, 5, 53 },
            { 42, 26, 38, 22, 41, 25, 37, 21 }
        };

        private static readonly byte[,] halftone5 =
        {
            {  0,  2,  4,  2,  1 },
            {  2,  5,  6,  5,  2 },
            {  4,  6,  7,  6,  4 },
            {  2,  5,  6,  5,  2 },
            {  1,  2,  4,  2,  1 },
        };


        private static readonly byte[,] halftone7 =
        {
            {  0,  2,  4,  5,  4,  2,   1},
            {  2,  3,  6,  7,  6,  3,   2},
            {  4,  6,  8,  9,  8,  6,   4},
            {  5,  7,  9, 10,  9,  7,   5},
            {  4,  6,  8,  9,  8,  6,   4},
            {  2,  3,  6,  7,  6,  3,   2},
            {  1,  2,  4,  5,  4,  2,   1},
        };

        // ReSharper restore InconsistentNaming
        #endregion

        #region Instance Fields

        private readonly sbyte[,] premultipliedMatrix;
        private readonly int matrixWidth;
        private readonly int matrixHeight;
        private readonly float strength;

        #endregion

        #endregion

        #region Constructors

        private OrderedDitherer(byte[,] matrix, float strength)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix), PublicResources.ArgumentNull);
            if (matrix.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(matrix));
            if (Single.IsNaN(strength) || strength < 0f || strength > 1f)
                throw new ArgumentOutOfRangeException(nameof(strength), PublicResources.ArgumentMustBeBetween(0, 1));

            this.strength = strength;
            matrixWidth = matrix.GetUpperBound(1) + 1;
            matrixHeight = matrix.GetUpperBound(0) + 1;
            int shades = 0;

            // extensions cannot be used on matrices (unsafe would work though)
            foreach (byte b in matrix)
            {
                if (b > shades)
                    shades = b;
            }

            // adding two levels for total black and white
            shades += 2;

            // Elements in premultiplied matrix are between -128..127
            premultipliedMatrix = new sbyte[matrixHeight, matrixWidth];
            for (int y = 0; y < matrixHeight; y++)
            {
                for (int x = 0; x < matrixWidth; x++)
                    // +1 for separating total black from the first pattern, -128 for balancing brightness level
                    premultipliedMatrix[y, x] = (sbyte)((matrix[y, x] + 1) * 255 / shades - 128);
            }
        }

        #endregion

        #region Methods

        #region Static Methods

        /// <summary>
        /// Gets an <see cref="OrderedDitherer"/> using the standard Bayer 2x2 matrix and the specified <paramref name="strength"/>.
        /// </summary>
        /// <param name="strength">The strength of the dithering effect between 0 and 1.
        /// Specify <c>0</c> to use an auto value for each dithering session based on the used quantizer. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <returns>An <see cref="OrderedDitherer"/> instance using the standard Bayer 2x2 matrix and the specified <paramref name="strength"/>.</returns>
        public static OrderedDitherer Bayer2x2(float strength = 0f) => new OrderedDitherer(bayer2x2, strength);
        public static OrderedDitherer Bayer3x3(float strength = 0f) => new OrderedDitherer(bayer3x3, strength);
        public static OrderedDitherer Bayer4x4(float strength = 0f) => new OrderedDitherer(bayer4x4, strength);
        public static OrderedDitherer Bayer8x8(float strength = 0f) => new OrderedDitherer(bayer8x8, strength);

        /// <summary>
        /// Gets an <see cref="OrderedDitherer"/> with the specified <paramref name="strength"/>
        /// using a 5x5 halftone pattern matrix with 8 different patterns.
        /// </summary>
        /// <param name="strength">The strength of the dithering effect between 0 and 1.
        /// Specify <c>0</c> to use an auto value for each dithering session based on the used quantizer. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <returns>An <see cref="OrderedDitherer"/> instance using a 5x5 halftone matrix of 8 patterns and the specified <paramref name="strength"/>.</returns>
        public static OrderedDitherer Halftone5(float strength = 0f) => new OrderedDitherer(halftone5, strength);
        public static OrderedDitherer Halftone7(float strength = 0f) => new OrderedDitherer(halftone7, strength);

        /// <summary>
        /// Gets an <see cref="OrderedDitherer"/> using the specified <paramref name="matrix"/> and <paramref name="strength"/>.
        /// </summary>
        /// <param name="matrix">A matrix to be used as the coefficients of the dithering. Ideally contains every value from 0
        /// up to the total size of the matrix (excluding upper bound) so the average brightness of the image will not change.</param>
        /// <param name="strength">The strength of the dithering effect between 0 and 1.
        /// Specify <c>0</c> to use an auto value for each dithering session based on the used quantizer. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <returns>An <see cref="OrderedDitherer"/> instance using the specified <paramref name="matrix"/> and <paramref name="strength"/>.</returns>
        public static OrderedDitherer FromCustomMatrix(byte[,] matrix, float strength = 0f) => new OrderedDitherer(matrix, strength);

        #endregion

        #region Instance Methods

        IDitheringSession IDitherer.Initialize(IBitmapDataAccessor source, IQuantizingSession quantizer)
            => new OrderedDitheringSession(quantizer, this);

        #endregion

        #endregion
    }
}
