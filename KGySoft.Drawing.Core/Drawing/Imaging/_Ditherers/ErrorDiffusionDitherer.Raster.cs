#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ErrorDiffusionDitherer.Raster.cs
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

using KGySoft.Collections;
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public partial class ErrorDiffusionDitherer
    {
        #region DitheringSessionRasterSrgb class

        private class DitheringSessionRasterSrgb : IDitheringSession
        {
            #region Fields

            private readonly IQuantizingSession quantizer;
            private readonly ErrorDiffusionDitherer ditherer;
            private readonly int imageHeight;
            private readonly bool byBrightness;
            private readonly CircularList<(float R, float G, float B)[]> errorsBuffer;

            #endregion

            #region Properties

            #region Public Properties

            public bool IsSequential => true;

            #endregion
            
            #region Protected Properties

            protected int ImageWidth { get; }
            protected int LastRow { get; private set; }
            protected bool IsRightToLeft { get; set; }

            #endregion

            #endregion

            #region Constructors

            internal DitheringSessionRasterSrgb(IQuantizingSession quantizingSession, ErrorDiffusionDitherer ditherer, IBitmapData source)
            {
                quantizer = quantizingSession ?? throw new ArgumentNullException(nameof(quantizingSession), PublicResources.ArgumentNull);
                this.ditherer = ditherer;
                ImageWidth = source.Width;
                imageHeight = source.Height;
                byBrightness = ditherer.byBrightness ?? quantizingSession.IsGrayscale;

                // Initializing a circular buffer for the diffused errors.
                // This helps to minimize used memory because it needs only a few lines to be stored.
                // Another solution could be to store resulting colors instead of just the errors but then the color
                // entries would be clipped not just in the end but in every iteration, and small errors would be lost
                // that could stack up otherwise.
                // See also the ErrorDiffusionDitherer constructor for more comments on why using floats.
                errorsBuffer = new CircularList<(float, float, float)[]>(ditherer.matrixHeight);
                for (int i = 0; i < ditherer.matrixHeight; i++)
                    errorsBuffer.Add(new (float, float, float)[ImageWidth]);
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose()
            {
            }

            public virtual Color32 GetDitheredColor(Color32 origColor, int x, int y)
            {
                if (y != LastRow)
                    PrepareNewRow(y);
                return DoErrorDiffusion(origColor, x, y);
            }

            #endregion

            #region Protected Methods

            protected virtual void PrepareNewRow(int y)
            {
                errorsBuffer.RemoveFirst();

                if (y + ditherer.matrixHeight <= imageHeight)
                    errorsBuffer.AddLast(new (float, float, float)[ImageWidth]);

                LastRow = y;
            }

            protected Color32 DoErrorDiffusion(Color32 origColor, int x, int y)
            {
                Color32 currentColor;

                // handling alpha
                if (origColor.A != Byte.MaxValue)
                {
                    currentColor = quantizer.BlendOrMakeTransparent(origColor);
                    if (currentColor.A == 0)
                        return currentColor;
                }
                else
                    currentColor = origColor;

                // applying propagated errors to the current pixel
                ref var error = ref errorsBuffer[0][x];
                currentColor = new Color32((currentColor.R + (int)error.R).ClipToByte(),
                    (currentColor.G + (int)error.G).ClipToByte(),
                    (currentColor.B + (int)error.B).ClipToByte());

                // getting the quantized result for the current pixel + errors
                Color32 quantizedColor = quantizer.GetQuantizedColor(currentColor);

                // determining the quantization error for the current pixel
                int errR;
                int errG;
                int errB;

                if (byBrightness)
                    errR = errG = errB = currentColor.GetBrightness() - quantizedColor.GetBrightness();
                else
                {
                    errR = currentColor.R - quantizedColor.R;
                    errG = currentColor.G - quantizedColor.G;
                    errB = currentColor.B - quantizedColor.B;
                }

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
                        int offset = mx - ditherer.matrixFirstPixelIndex + 1;
                        int targetX = x + (IsRightToLeft ? -offset : offset);

                        // ignored coefficient or beyond first / last column
                        if (my == 0 && mx < ditherer.matrixFirstPixelIndex || targetX < 0 || targetX >= ImageWidth)
                            continue;

                        float coefficient = ditherer.coefficientsMatrix[my, mx];

                        // ReSharper disable once CompareOfFloatsByEqualityOperator - this is intended
                        if (coefficient == 0f)
                            continue;

                        // applying the error in our buffer
                        error = ref errorsBuffer[my][targetX];
                        error.R += errR * coefficient;
                        error.G += errG * coefficient;
                        error.B += errB * coefficient;
                    }
                }

                return quantizedColor;
            }

            #endregion

            #endregion
        }

        #endregion

        #region DitheringSessionRasterLinear class

        private class DitheringSessionRasterLinear : IDitheringSession
        {
            #region Fields

            private readonly IQuantizingSession quantizer;
            private readonly ErrorDiffusionDitherer ditherer;
            private readonly int imageHeight;
            private readonly bool byBrightness;
            private readonly CircularList<(float R, float G, float B)[]> errorsBuffer;

            #endregion

            #region Properties

            #region Public Properties

            public bool IsSequential => true;

            #endregion
            
            #region Protected Properties

            protected int ImageWidth { get; }
            protected int LastRow { get; private set; }
            protected bool IsRightToLeft { get; set; }

            #endregion

            #endregion

            #region Constructors

            internal DitheringSessionRasterLinear(IQuantizingSession quantizingSession, ErrorDiffusionDitherer ditherer, IBitmapData source)
            {
                quantizer = quantizingSession ?? throw new ArgumentNullException(nameof(quantizingSession), PublicResources.ArgumentNull);
                this.ditherer = ditherer;
                ImageWidth = source.Width;
                imageHeight = source.Height;
                byBrightness = ditherer.byBrightness ?? quantizingSession.IsGrayscale;

                // Initializing a circular buffer for the diffused errors.
                // This helps to minimize used memory because it needs only a few lines to be stored.
                // Another solution could be to store resulting colors instead of just the errors but then the color
                // entries would be clipped not just in the end but in every iteration, and small errors would be lost
                // that could stack up otherwise.
                // See also the ErrorDiffusionDitherer constructor for more comments on why using floats.
                errorsBuffer = new CircularList<(float, float, float)[]>(ditherer.matrixHeight);
                for (int i = 0; i < ditherer.matrixHeight; i++)
                    errorsBuffer.Add(new (float, float, float)[ImageWidth]);
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose()
            {
            }

            public virtual Color32 GetDitheredColor(Color32 origColor, int x, int y)
            {
                if (y != LastRow)
                    PrepareNewRow(y);
                return DoErrorDiffusion(origColor, x, y);
            }

            #endregion

            #region Protected Methods

            protected virtual void PrepareNewRow(int y)
            {
                errorsBuffer.RemoveFirst();

                if (y + ditherer.matrixHeight <= imageHeight)
                    errorsBuffer.AddLast(new (float, float, float)[ImageWidth]);

                LastRow = y;
            }

            protected Color32 DoErrorDiffusion(Color32 origColor, int x, int y)
            {
                Color32 currentColor;

                // handling alpha
                if (origColor.A != Byte.MaxValue)
                {
                    currentColor = quantizer.BlendOrMakeTransparent(origColor);
                    if (currentColor.A == 0)
                        return currentColor;
                }
                else
                    currentColor = origColor;

                // applying propagated errors to the current pixel
                ref var error = ref errorsBuffer[0][x];
                //currentColor = new Color32((currentColor.R + (int)error.R).ClipToByte(),
                //    (currentColor.G + (int)error.G).ClipToByte(),
                //    (currentColor.B + (int)error.B).ClipToByte());
                var currentF = new ColorF(currentColor);
                currentF = (currentF + new RgbF(error.R, error.G, error.B)).Clip();
                currentColor = currentF.ToColor32();

                // getting the quantized result for the current pixel + errors
                Color32 quantizedColor = quantizer.GetQuantizedColor(currentColor);
                var quantizedF = quantizedColor.ToColorF();

                // determining the quantization error for the current pixel
                float errR;
                float errG;
                float errB;

                if (byBrightness)
                    //errR = errG = errB = ColorSpaceHelper.SrgbToLinear(currentColor.GetBrightness()) - ColorSpaceHelper.SrgbToLinear(quantizedColor.GetBrightness());
                    errR = errG = errB = currentF.GetBrightness() - quantizedF.GetBrightness();
                else
                {
                    //errR = currentColor.R - quantizedColor.R;
                    //errG = currentColor.G - quantizedColor.G;
                    //errB = currentColor.B - quantizedColor.B;
                    errR = currentF.R - quantizedF.R;
                    errG = currentF.G - quantizedF.G;
                    errB = currentF.B - quantizedF.B;
                }

                // no error, nothing to propagate further
                //if (errR == 0 && errG == 0 && errB == 0)
                //    return quantizedColor;
                if (errR.TolerantIsZero() && errG.TolerantIsZero()&& errB.TolerantIsZero())
                    return quantizedColor;

                // processing the whole matrix and propagating the current error to neighbors
                for (int my = 0; my < ditherer.matrixHeight; my++)
                {
                    // beyond last row
                    if (y + my >= imageHeight)
                        continue;

                    for (int mx = 0; mx < ditherer.matrixWidth; mx++)
                    {
                        int offset = mx - ditherer.matrixFirstPixelIndex + 1;
                        int targetX = x + (IsRightToLeft ? -offset : offset);

                        // ignored coefficient or beyond first / last column
                        if (my == 0 && mx < ditherer.matrixFirstPixelIndex || targetX < 0 || targetX >= ImageWidth)
                            continue;

                        float coefficient = ditherer.coefficientsMatrix[my, mx];

                        // ReSharper disable once CompareOfFloatsByEqualityOperator - this is intended
                        if (coefficient == 0f)
                            continue;

                        // applying the error in our buffer
                        error = ref errorsBuffer[my][targetX];
                        error.R += errR * coefficient;
                        error.G += errG * coefficient;
                        error.B += errB * coefficient;
                    }
                }

                return quantizedColor;
            }

            #endregion

            #endregion
        }

        #endregion
    }
}
