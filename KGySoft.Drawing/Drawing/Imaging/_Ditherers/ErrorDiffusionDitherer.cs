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
using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
#pragma warning disable CA1814 // arrays in this class are better to be matrices than jagged arrays as they are always rectangular

    /// <summary>
    /// Provides an <see cref="IDitherer"/> implementation for error diffusion dithering.
    /// Use the static properties of this class to use predefined parameters or the <see cref="ErrorDiffusionDitherer(byte[,],int,int,bool,bool?)">constructor</see> to create a custom error diffusion ditherer.
    /// <br/>See the <strong>Remarks</strong> section for details and results comparison.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="ErrorDiffusionDitherer(byte[,],int,int,bool,bool?)">constructor</see> can be used to create an error diffusion ditherer using a custom matrix.</para>
    /// <para>Use the static properties to create an instance with predefined parameters.</para>
    /// <para>The <see cref="ErrorDiffusionDitherer"/> class uses an adaptive dithering technique, which disperses the quantization error of each pixels to the neighboring ones.
    /// Thereby the strength of the dithering is automatically adjusted by the algorithm itself, which provides good results also for palettes with uneven color distribution
    /// (which is not the case for <see cref="OrderedDitherer">ordered dithering</see>, for example).</para>
    /// <para>As the dithered result of a pixel depends on the already processed pixels, the <see cref="ErrorDiffusionDitherer"/> does not support parallel processing, which makes
    /// it slower than most of the other dithering methods.</para>
    /// <para>The following table demonstrates the effect of the dithering:
    /// <list type="table">
    /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
    /// <item>
    /// <term><div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
    /// <br/>Color hues with alpha gradient</para></div></term>
    /// <term>
    /// <div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilver.gif" alt="Color hues with system default 8 BPP palette and silver background"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see>, no dithering</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredFS.gif" alt="Color hues with system default 8 BPP palette, silver background and Floyd-Steinberg dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredS3.gif" alt="Color hues with system default 8 BPP palette, using silver background and Sierra 3 dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see> and <see cref="Sierra3">Sierra 3</see> dithering</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredSA.gif" alt="Color hues with system default 8 BPP palette, using silver background and Stevenson-Arce dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see> and <see cref="StevensonArce">Stevenson-Arce</see> dithering</para></div></term>
    /// </item>
    /// <item>
    /// <term><div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
    /// <br/>Grayscale color shades</para></div></term>
    /// <term>
    /// <div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/GrayShadesBW.gif" alt="Grayscale color shades with black and white palette"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see>, no dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredFS.gif" alt="Grayscale color shades with black and white palette, using Floyd-Steinberg dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredS3.gif" alt="Grayscale color shades with black and white palette using Sierra 3 dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see> and <see cref="Sierra3">Sierra 3</see> dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredSA.gif" alt="Grayscale color shades with black and white palette using Stevenson-Arce dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see> and <see cref="StevensonArce">Stevenson-Arce</see> dithering</para></div></term>
    /// </item>
    /// </list></para>
    /// <para>Calculation of the quantization error may happen in two ways. The publicly available algorithms usually calculate the error for each color channels,
    /// which usually provide good results with color palettes. However, when quantizing color images with a black and white or grayscale palette,
    /// this approach may fail. For example, if the quantizer returns black for a fully saturated blue pixel, the quantization error is zero on the red and green channels and
    /// 100% on the blue channel. The problem is that this error cannot be propagated to the neighboring pixels if they have the same color because adding any more blue
    /// to already fully saturated blue pixels will not change anything. Therefore, the <see cref="ErrorDiffusionDitherer"/> can propagate quantization error
    /// by brightness based on human perception, which is more appropriate for palettes with grayscale colors.
    /// The <see cref="ErrorDiffusionDitherer"/> tries to auto detect the strategy for each dithering session but this can be overridden by
    /// the <see cref="ConfigureErrorDiffusionMode">ConfigureErrorDiffusionMode</see> method. </para>
    /// <para>The following table demonstrates the effect of different strategies:
    /// <list type="table">
    /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
    /// <item>
    /// <term><div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
    /// <br/>Color hues with alpha gradient</para></div></term>
    /// <term>
    /// <div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredFS.gif" alt="Color hues with system default 8 BPP palette, silver background and Floyd-Steinberg dithering, using error diffusion by RGB channels"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see>, using error diffusion by RGB channels (the default strategy for non-grayscale palettes)</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredFSByBr.gif" alt="Color hues with system default 8 BPP palette, silver background and Floyd-Steinberg dithering, using error diffusion by brightness"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see>, using error diffusion by brightness</para></div></term>
    /// </item>
    /// <item>
    /// <term><div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/ColorWheel.png" alt="Color wheel"/>
    /// <br/>Color wheel</para></div></term>
    /// <term>
    /// <div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/ColorWheelBWBlueDitheredFSByBr.gif" alt="Color wheel with black and white palette, blue background and Floyd-Steinberg dithering, using error diffusion by brightness (the default strategy for grayscale palettes)"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see>, using blue background and error diffusion by brightness (the default strategy for grayscale palettes). All colors appear in the result with different patterns.</para>
    /// <para><img src="../Help/Images/ColorWheelBWBlueDitheredFSByRgb.gif" alt="Color wheel with black and white palette, blue background and Floyd-Steinberg dithering, using error diffusion by RGB channels"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see>, using blue background and error diffusion by RGB channels. The fully saturated colors turned completely black or white.</para></div></term>
    /// </item>
    /// </list></para>
    /// <note type="tip">See the <strong>Examples</strong> section of the static properties for more examples.</note>
    /// </remarks>
    /// <seealso cref="IDitherer" />
    /// <seealso cref="OrderedDitherer"/>
    /// <seealso cref="RandomNoiseDitherer"/>
    /// <seealso cref="InterleavedGradientNoiseDitherer"/>
    /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/>
    /// <seealso cref="BitmapExtensions.Dither"/>
    public sealed partial class ErrorDiffusionDitherer : IDitherer
    {
        #region Fields

        #region Static Fields

        private static ErrorDiffusionDitherer floydSteinberg;
        private static ErrorDiffusionDitherer jarvisJudiceNinke;
        private static ErrorDiffusionDitherer stucki;
        private static ErrorDiffusionDitherer burkes;
        private static ErrorDiffusionDitherer sierra3;
        private static ErrorDiffusionDitherer sierra2;
        private static ErrorDiffusionDitherer sierraLite;
        private static ErrorDiffusionDitherer stevensonArce;

        #endregion

        #region Instance Fields

        private readonly float[,] coefficientsMatrix;
        private readonly int matrixWidth;
        private readonly int matrixHeight;
        private readonly int matrixFirstPixelIndex;
        private readonly bool? byBrightness;
        private readonly bool isSerpentineProcessing;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the original filter proposed by Floyd and Steinberg in 1975 when they came out with the idea of error diffusion dithering.
        /// Uses a small, 3x2 matrix so the processing is somewhat faster than the other alternatives.
        /// </summary>
        public static ErrorDiffusionDitherer FloydSteinberg => floydSteinberg ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 7 },
                { 3, 5, 1 },
            }, 16, 2);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the filter proposed by Jarvis, Judice and Ninke in 1976.
        /// Uses a 5x3 matrix so the processing is slower than by the original Floyd-Steinberg filter but distributes errors in a wider range.
        /// </summary>
        public static ErrorDiffusionDitherer JarvisJudiceNinke => jarvisJudiceNinke ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 7, 5 },
                { 3, 5, 7, 5, 3 },
                { 1, 3, 5, 3, 1 },
            }, 48, 3);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the filter proposed by P. Stucki in 1981.
        /// Uses a 5x3 matrix so the processing is slower than by the original Floyd-Steinberg filter but distributes errors in a wider range.
        /// </summary>
        public static ErrorDiffusionDitherer Stucki => stucki ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 8, 4 },
                { 2, 4, 8, 4, 2 },
                { 1, 2, 4, 2, 1 },
            }, 42, 3);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the filter proposed by D. Burkes in 1988.
        /// Uses a 5x2 matrix, which is actually the same as the first two lines of the matrix used by the Stucki filter.
        /// </summary>
        public static ErrorDiffusionDitherer Burkes => burkes ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 8, 4 },
                { 2, 4, 8, 4, 2 },
            }, 32, 3);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the three-line filter proposed by Frankie Sierra in 1989.
        /// Uses a 5x3 matrix so this is the slowest Sierra filter but this produces the best result among them.
        /// </summary>
        public static ErrorDiffusionDitherer Sierra3 => sierra3 ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 5, 3 },
                { 2, 4, 5, 4, 2 },
                { 0, 2, 3, 2, 0 },
            }, 32, 3);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the two-line filter proposed by Frankie Sierra in 1990.
        /// Uses a 5x2 matrix so this somewhat faster than the three-line version and still provides a similar quality.
        /// </summary>
        public static ErrorDiffusionDitherer Sierra2 => sierra2 ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 4, 3 },
                { 1, 2, 3, 2, 1 },
            }, 16, 3);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using a small two-line filter proposed by Frankie Sierra.
        /// Uses a 3x2 matrix so it has the same performance as the Floyd-Steinberg algorithm and also produces a quite similar result.
        /// </summary>
        public static ErrorDiffusionDitherer SierraLite => sierraLite ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 2 },
                { 1, 1, 0 },
            }, 4, 2);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the hexagonal filter proposed by Stevenson and Arce in 1985.
        /// Uses a fairly large, 7x4 matrix, but due to the hexagonal arrangement of the coefficients the processing performance is comparable to a rectangular 5x3 matrix.
        /// </summary>
        public static ErrorDiffusionDitherer StevensonArce => stevensonArce ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 0, 0, 32, 0 },
                { 12, 0, 26, 0, 30, 0, 16 },
                { 0, 12, 0, 26, 0, 12, 0 },
                { 5, 0, 12, 0, 12, 0, 5 },
            }, 200, 4);

        #endregion

        #region Constructors

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDiffusionDitherer" /> class using the specified <paramref name="matrix"/>, <paramref name="divisor"/> and <paramref name="matrixFirstPixelIndex"/>.
        /// <br/>For some well known error diffusion ditherers see the static properties.
        /// </summary>
        /// <param name="matrix">A matrix to be used as the coefficients for the quantization errors to be propagated to the neighboring pixels.</param>
        /// <param name="divisor">Each elements in the <paramref name="matrix"/> will be divided by this value. If less than the sum of the elements
        /// in the <paramref name="matrix"/>, then only a fraction of the error will be propagated.</param>
        /// <param name="matrixFirstPixelIndex">Specifies the first effective index in the first row of the matrix. If larger than zero, then the error will be propagated also to the bottom-left direction.
        /// Must be between 0 and <paramref name="matrix"/> width, excluding upper bound.</param>
        /// <param name="byBrightness"><see langword="true"/>&#160;to apply the same quantization error on every color channel determined by brightness difference;
        /// <see langword="false"/>&#160;to apply handle quantization errors on each color channels independently; <see langword="null"/>&#160;to auto select strategy.
        /// Deciding by brightness can produce a better result when fully saturated colors are mapped to a grayscale palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="OrderedDitherer"/> instance using the specified <paramref name="matrix"/>, <paramref name="divisor"/> and <paramref name="matrixFirstPixelIndex"/>.</returns>
        public ErrorDiffusionDitherer(byte[,] matrix, int divisor, int matrixFirstPixelIndex, bool isSerpentineProcessing = false, bool? byBrightness = null)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix), PublicResources.ArgumentNull);
            if (matrix.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(matrix));
            if (divisor <= 0)
                throw new ArgumentOutOfRangeException(nameof(divisor), PublicResources.ArgumentMustBeGreaterThan(0));

            matrixWidth = matrix.GetUpperBound(1) + 1;
            matrixHeight = matrix.GetUpperBound(0) + 1;
            if (matrixFirstPixelIndex >= matrixWidth)
                throw new ArgumentOutOfRangeException(nameof(matrixFirstPixelIndex), PublicResources.ArgumentMustBeBetween(0, matrixWidth - 1));

            this.matrixFirstPixelIndex = matrixFirstPixelIndex;
            this.isSerpentineProcessing = isSerpentineProcessing;
            this.byBrightness = byBrightness;

            // Applying divisor to the provided matrix elements into a new float matrix. This has two benefits:
            // 1. Applying the error will be a simple multiplication, which alone is faster even for a float than an
            //    int multiplication combined with bit shifting (or division if divisor is not power of 2)
            // 2. By not losing the fractions after bit shifting, small errors can stack up that would be lost otherwise,
            //    which prevents some artifacts and provides better results for almost completely black/white/saturated areas.
            coefficientsMatrix = new float[matrixHeight, matrixWidth];
            for (int y = 0; y < matrixHeight; y++)
            {
                for (int x = 0; x < matrixWidth; x++)
                    coefficientsMatrix[y, x] = matrix[y, x] / (float)divisor;
            }
        }

        #endregion

        #region Private Constructors

        private ErrorDiffusionDitherer(ErrorDiffusionDitherer original, bool isSerpentineProcessing, bool? byBrightness)
        {
            this.isSerpentineProcessing = isSerpentineProcessing;
            this.byBrightness = byBrightness;
            coefficientsMatrix = original.coefficientsMatrix;
            matrixWidth = original.matrixWidth;
            matrixHeight = original.matrixHeight;
            matrixFirstPixelIndex = original.matrixFirstPixelIndex;
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        public ErrorDiffusionDitherer ConfigureErrorDiffusionMode(bool? byBrightness) => new ErrorDiffusionDitherer(this, isSerpentineProcessing, byBrightness);

        public ErrorDiffusionDitherer ConfigureProcessingDirection(bool serpentine) => new ErrorDiffusionDitherer(this, serpentine, byBrightness);

        #endregion

        #region Explicitly Implemented Interface Methods

        IDitheringSession IDitherer.Initialize(IReadableBitmapData source, IQuantizingSession quantizer)
            => isSerpentineProcessing
                ? new DitheringSessionSerpentine(quantizer, this, source)
                : (IDitheringSession)new DitheringSessionRaster(quantizer, this, source);

        #endregion

        #endregion
    }
}
