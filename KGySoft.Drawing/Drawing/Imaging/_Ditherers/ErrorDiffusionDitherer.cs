#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ErrorDiffusionDitherer.cs
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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
#pragma warning disable CA1814 // arrays in this class are better to be matrices than jagged arrays as they are always rectangular

    public sealed class ErrorDiffusionDitherer : IDitherer
    {
        #region ErrorDiffusionDitheringSession class

        private class ErrorDiffusionDitheringSession : IDitheringSession
        {
            #region Fields

            private readonly IQuantizingSession quantizer;
            private readonly ErrorDiffusionDitherer ditherer;
            private readonly int imageWidth;
            private readonly int imageHeight;

            private readonly CircularList<(int R, int G, int B)[]> errorsBuffer;
            private int lastRow;

            #endregion

            #region Properties

            public bool IsSequential => true;

            #endregion

            #region Constructors

            internal ErrorDiffusionDitheringSession(IQuantizingSession quantizer, ErrorDiffusionDitherer ditherer, IBitmapDataAccessor source)
            {
                this.quantizer = quantizer;
                this.ditherer = ditherer;
                imageWidth = source.Width;
                imageHeight = source.Height;

                // Initializing a circular buffer for the diffused errors.
                // This helps to minimize used memory because it needs only a few lines to be stored.
                errorsBuffer = new CircularList<(int, int, int)[]>(ditherer.matrixHeight);
                for (int i = 0; i < ditherer.matrixHeight; i++)
                    errorsBuffer.Add(new (int, int, int)[imageWidth]);
            }

            #endregion

            #region Methods

            public void Dispose()
            {
            }

            public Color32 GetDitheredColor(Color32 origColor, int x, int y)
            {
                static byte ToByteSafe(int value)
                    => value < Byte.MinValue ? Byte.MinValue
                    : value > Byte.MaxValue ? Byte.MaxValue
                    : (byte)value;

                // new line
                if (y != lastRow)
                {
                    errorsBuffer.RemoveFirst();
                    errorsBuffer.AddLast(new (int, int, int)[imageWidth]);

                    lastRow = y;
                }

                Color32 currentColor;

                // handling alpha
                if (origColor.A != Byte.MaxValue)
                {
                    // the color can be considered fully transparent
                    if (origColor.A < quantizer.AlphaThreshold)
                    {
                        // and even the quantizer returns a transparent color
                        currentColor = quantizer.GetQuantizedColor(origColor);
                        if (currentColor.A == 0)
                            return currentColor;
                    }

                    // the color will not be transparent in the end: blending
                    currentColor = origColor.BlendWithBackground(quantizer.BackColor);
                }
                else
                    currentColor = origColor;

                // applying propagated errors to the current pixel
                ref var error = ref errorsBuffer[0][x];
                currentColor = new Color32(ToByteSafe(currentColor.R + error.R),
                    ToByteSafe(currentColor.G + error.G),
                    ToByteSafe(currentColor.B + error.B));

                // getting the quantized result for the current pixel + errors
                Color32 quantizedColor = quantizer.GetQuantizedColor(currentColor);

                // determining the quantization error for the curent pixel
                int errR = currentColor.R - quantizedColor.R;
                int errG = currentColor.G - quantizedColor.G;
                int errB = currentColor.B - quantizedColor.B;

                // no error, nothing to propagate further
                if (errR == 0 && errG == 0 && errB == 0)
                    return quantizedColor;

                // processing the whole matrix and propagating the current error to neighbors
                for (int my = 0; my < ditherer.matrixHeight; my++)
                {
                    // beyond last row
                    if (y + my >= imageHeight)
                        continue;

                    for (int mx = 0; mx < ditherer.matrixWidth; mx++)
                    {
                        int targetX = x + mx - ditherer.matrixFirstPixelOffset + 1;

                        // ignored coefficient or beyond first / last column
                        if (my == 0 && mx < ditherer.matrixFirstPixelOffset || targetX <= 0 || targetX >= imageWidth)
                            continue;

                        float coefficient = ditherer.coefficientsMatrix[my, mx];

                        // applying the error in our buffer
                        error = ref errorsBuffer[my][targetX];
                        error.R += (int)(errR * coefficient);
                        error.G += (int)(errG * coefficient);
                        error.B += (int)(errB * coefficient);
                    }
                }

                return quantizedColor;
            }

            #endregion
        }

        #endregion

        #region Fields

        #region Static Fields

        private static byte[,] floydSteinberg =
        {
            { 0, 0, 7 },
            { 3, 5, 1 },
        };

        #endregion

        #region Instance Fields

        private readonly float[,] coefficientsMatrix;
        private readonly int matrixWidth;
        private readonly int matrixHeight;
        private readonly int matrixFirstPixelOffset;

        #endregion

        #endregion

        #region Properties

        public static ErrorDiffusionDitherer FloydSteinberg => new ErrorDiffusionDitherer(floydSteinberg, 16, 2);

        #endregion

        #region Constructors

        private ErrorDiffusionDitherer(byte[,] matrix, float divisor, int matrixFirstPixelOffset)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix), PublicResources.ArgumentNull);
            if (matrix.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(matrix));
            if (Single.IsNaN(divisor) || divisor <= 0f)
                throw new ArgumentOutOfRangeException(nameof(divisor), PublicResources.ArgumentMustBeGreaterThan(0));

            matrixWidth = matrix.GetUpperBound(1) + 1;
            matrixHeight = matrix.GetUpperBound(0) + 1;
            if (matrixFirstPixelOffset >= matrixWidth)
                throw new ArgumentOutOfRangeException(nameof(matrixFirstPixelOffset), PublicResources.ArgumentMustBeBetween(0, matrixWidth - 1));

            this.matrixFirstPixelOffset = matrixFirstPixelOffset;

            // Applying divisor to the provided matrix elements
            coefficientsMatrix = new float[matrixHeight, matrixWidth];
            for (int y = 0; y < matrixHeight; y++)
            {
                for (int x = 0; x < matrixWidth; x++)
                    coefficientsMatrix[y, x] = matrix[y, x] / divisor;
            }
        }

        #endregion

        #region Methods

        IDitheringSession IDitherer.Initialize(IBitmapDataAccessor source, IQuantizingSession quantizer)
            => new ErrorDiffusionDitheringSession(quantizer, this, source);

        #endregion
    }
}
