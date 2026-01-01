#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ErrorDiffusionDitherer.Raster.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

#region Used Namespaces
using System;

using KGySoft.Collections;
#endregion

#region Used Aliases
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using Rgb = System.Numerics.Vector3;
#else
using Rgb = KGySoft.Drawing.Imaging.RgbF;
#endif 
#endregion

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
            private readonly CircularList<Rgb[]> errorsBuffer;

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
                errorsBuffer = new CircularList<Rgb[]>(ditherer.matrixHeight);
                for (int i = 0; i < ditherer.matrixHeight; i++)
                    errorsBuffer.Add(new Rgb[ImageWidth]);
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
                    errorsBuffer.AddLast(new Rgb[ImageWidth]);

                LastRow = y;
            }

            protected Color32 DoErrorDiffusion(Color32 origColor, int x, int y)
            {
                Debug.Assert(errorsBuffer.Count > 0 && (uint)x < (uint)errorsBuffer[0].Length, "Invalid usage. Was IsSequential respected?");
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
                ref Rgb errorPropagated = ref errorsBuffer[0][x];
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                currentColor = new Color32((currentColor.R + errorPropagated.X).ClipToByte(),
                    (currentColor.G + errorPropagated.Y).ClipToByte(),
                    (currentColor.B + errorPropagated.Z).ClipToByte());
#else
                currentColor = new Color32((currentColor.R + errorPropagated.R).ClipToByte(),
                    (currentColor.G + errorPropagated.G).ClipToByte(),
                    (currentColor.B + errorPropagated.B).ClipToByte());
#endif

                // getting the quantized result for the current pixel + errors
                Color32 quantizedColor = quantizer.GetQuantizedColor(currentColor);

                // Determining the quantization error for the current pixel.
                // The error values are not 0..1 values in the sRGB color space so using Rgb only for possible vectorization if supported.
                Rgb errorCurrent = byBrightness
                    ? new Rgb(currentColor.GetBrightness() - quantizedColor.GetBrightness())
                    : new Rgb(currentColor.R - quantizedColor.R, currentColor.G - quantizedColor.G, currentColor.B - quantizedColor.B);

                // no error, nothing to propagate further (errorCurrent always consists of int values so no tolerant zero check is needed)
                if (errorCurrent == default)
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
                        errorsBuffer[my][targetX] += errorCurrent * coefficient;
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
            private readonly CircularList<Rgb[]> errorsBuffer;

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
                errorsBuffer = new CircularList<Rgb[]>(ditherer.matrixHeight);
                for (int i = 0; i < ditherer.matrixHeight; i++)
                    errorsBuffer.Add(new Rgb[ImageWidth]);
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
                    errorsBuffer.AddLast(new Rgb[ImageWidth]);

                LastRow = y;
            }

            protected Color32 DoErrorDiffusion(Color32 origColor, int x, int y)
            {
                Debug.Assert(errorsBuffer.Count > 0 && (uint)x < (uint)errorsBuffer[0].Length, "Invalid usage. Was IsSequential respected?");
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
                ref Rgb errorPropagated = ref errorsBuffer[0][x];
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                Rgb currentColorF = (new RgbF(currentColor).Rgb + errorPropagated).ClipF();
#else
                Rgb currentColorF = (new Rgb(currentColor) + errorPropagated).Clip();
#endif
                currentColor = currentColorF.ToColor32();

                // getting the quantized result for the current pixel + errors
                Color32 quantizedColor = quantizer.GetQuantizedColor(currentColor);
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                Rgb quantizedColorF = new RgbF(quantizedColor).Rgb;
#else
                Rgb quantizedColorF = new Rgb(quantizedColor);
#endif

                // determining the quantization error for the current pixel
                Rgb errorCurrent = byBrightness
                    ? new Rgb(currentColorF.GetBrightness() - quantizedColorF.GetBrightness())
                    : currentColorF - quantizedColorF;

                // no error, nothing to propagate further
                if (errorCurrent.TolerantIsZero())
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
                        errorsBuffer[my][targetX] += errorCurrent * coefficient;
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
