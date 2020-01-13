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
        #region Nested classes

        #region OrderedDitheringSession class

        private sealed class OrderedDitheringSession : IDitheringSession
        {
            #region Fields

            private readonly IQuantizingSession quantizer;
            private readonly OrderedDitherer ditherer;
            private readonly int softness;

            #endregion

            #region Properties

            public bool IsSequential => false;

            #endregion

            #region Constructors

            internal OrderedDitheringSession(IQuantizingSession quantizer, OrderedDitherer ditherer)
            {
                this.quantizer = quantizer ?? throw new ArgumentNullException(nameof(quantizer), PublicResources.ArgumentNull);
                this.ditherer = ditherer;
                if (ditherer.softness.HasValue)
                {
                    softness = ditherer.softness.Value;
                    return;
                }

                // Auto softness is calculated by color count. For 256 color images the correct value actually depends on the
                // used quantizer and the image. In general case (with not so perfect colors) 3 is better.
                int colorCount = quantizer.Palette?.Length ?? 0;
                softness = colorCount == 0 ? 5
                    //: colorCount == 256 ? 4
                    : colorCount >= 128 ? 3
                    : colorCount >= 16 ? 2
                    : colorCount >= 4 ? 1
                    : 0;
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

                // applying the matrix and brightness/softness adjustments
                int offset = ditherer.brightness + (ditherer.premultipliedMatrix[y % ditherer.matrixHeight, x % ditherer.matrixWidth] >> softness);
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

        private static byte[,] bayer2x2 =
        {
            { 0, 2 },
            { 3, 1 },
        };

        private static byte[,] bayer3x3 =
        {
            { 0, 7, 3 },
            { 6, 5, 2 },
            { 4, 1, 8 },
        };

        private static byte[,] bayer4x4 =
        {
            { 0, 8, 2, 10 },
            { 12, 4, 14, 6 },
            { 3, 11, 1, 9 },
            { 15, 7, 13, 5 },
        };

        private static byte[,] bayer8x8 =
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

        #endregion

        #region Instance Fields

        private readonly sbyte[,] premultipliedMatrix;
        private readonly int matrixWidth;
        private readonly int matrixHeight;
        private readonly short brightness;
        private readonly byte? softness;

        #endregion

        #endregion

        #region Constructors

        private OrderedDitherer(byte[,] matrix, byte? softness, short brightness)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix), PublicResources.ArgumentNull);
            if (matrix.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(matrix));

            this.softness = softness;
            this.brightness = brightness;

            matrixWidth = matrix.GetUpperBound(1) + 1;
            matrixHeight = matrix.GetUpperBound(0) + 1;

            // Sum of 1/size x [matrix] supposed to be 1 but we not check that
            int size = matrixWidth * matrixHeight;

            // Elements in premultiplied matrix are between -128..127
            premultipliedMatrix = new sbyte[matrixHeight, matrixWidth];
            for (int y = 0; y < matrixHeight; y++)
            {
                for (int x = 0; x < matrixWidth; x++)
                    premultipliedMatrix[y, x] = (sbyte)((matrix[y, x] * 255 - 128) / size);
            }
        }

        #endregion

        #region Methods

        #region Static Methods

        public static OrderedDitherer Bayer2x2(byte? softness = null, short brightness = 0) => new OrderedDitherer(bayer2x2, softness, brightness);

        public static OrderedDitherer Bayer3x3(byte? softness = null, short brightness = 0) => new OrderedDitherer(bayer3x3, softness, brightness);

        public static OrderedDitherer Bayer4x4(byte? softness = null, short brightness = 0) => new OrderedDitherer(bayer4x4, softness, brightness);

        public static OrderedDitherer Bayer8x8(byte? softness = null, short brightness = 0) => new OrderedDitherer(bayer8x8, softness, brightness);

        #endregion

        #region Instance Methods

        IDitheringSession IDitherer.Initialize(IBitmapDataAccessor source, IQuantizingSession quantizer)
            => new OrderedDitheringSession(quantizer, this);

        #endregion

        #endregion
    }
}
