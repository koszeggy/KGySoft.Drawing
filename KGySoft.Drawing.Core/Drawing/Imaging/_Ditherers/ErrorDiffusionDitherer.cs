#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ErrorDiffusionDitherer.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;

using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides an <see cref="IDitherer"/> implementation for error diffusion dithering.
    /// Use the static properties of this class to use predefined error diffusion filters or the <see cref="ErrorDiffusionDitherer(byte[,],int,int,bool,bool?)">constructor</see> to create a custom one.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="ErrorDiffusionDitherer(byte[,],int,int,bool,bool?)">constructor</see> can be used to create an error diffusion ditherer using a custom matrix.</para>
    /// <para>Use the static properties to obtain an instance with predefined parameters.</para>
    /// <para>The <see cref="ErrorDiffusionDitherer"/> class uses an adaptive dithering technique, which disperses the quantization error of each pixels to the neighboring ones.
    /// Thereby the strength of the dithering is automatically adjusted by the algorithm itself, which provides good results also for palettes with uneven color distribution
    /// (which is not the case for <see cref="OrderedDitherer">ordered dithering</see>, for example).</para>
    /// <para>As the dithered result of a pixel depends on the already processed pixels, the <see cref="ErrorDiffusionDitherer"/> does not support parallel processing, which makes
    /// it slower than most of the other dithering methods.</para>
    /// <para>The following table demonstrates the effect of the dithering:
    /// <table class="table is-hoverable">
    /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
    /// <tbody><tr><td><div style="text-align:center;">
    /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
    /// <br/>Color hues with alpha gradient</para></div></td>
    /// <td><div style="text-align:center;">
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilver.gif" alt="Color hues with system default 8 BPP palette and silver background"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see>, no dithering</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredFS.gif" alt="Color hues with system default 8 BPP palette, silver background and Floyd-Steinberg dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredS3.gif" alt="Color hues with system default 8 BPP palette, using silver background and Sierra 3 dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see> and <see cref="Sierra3">Sierra 3</see> dithering</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredSA.gif" alt="Color hues with system default 8 BPP palette, using silver background and Stevenson-Arce dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see> and <see cref="StevensonArce">Stevenson-Arce</see> dithering</para></div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
    /// <br/>Grayscale color shades</para></div></td>
    /// <td><div style="text-align:center;">
    /// <para><img src="../Help/Images/GrayShadesBW.gif" alt="Grayscale color shades with black and white palette"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see>, no dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredFS.gif" alt="Grayscale color shades with black and white palette, using Floyd-Steinberg dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredS3.gif" alt="Grayscale color shades with black and white palette using Sierra 3 dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see> and <see cref="Sierra3">Sierra 3</see> dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredSA.gif" alt="Grayscale color shades with black and white palette using Stevenson-Arce dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see> and <see cref="StevensonArce">Stevenson-Arce</see> dithering</para></div></td>
    /// </tr>
    /// </tbody></table></para>
    /// <para>Calculation of the quantization error may happen in two ways. The publicly available algorithms usually calculate the error for each color channels,
    /// which usually provides good results with color palettes. However, when quantizing color images with a black and white or grayscale palette,
    /// this approach may fail. For example, if the quantizer returns black for a fully saturated blue pixel, the quantization error is zero on the red and green channels and
    /// 100% on the blue channel. The problem is that this error cannot be propagated to the neighboring pixels if they have the same color because adding any more blue
    /// to already fully saturated blue pixels will not change anything. Therefore, the <see cref="ErrorDiffusionDitherer"/> can propagate quantization error
    /// by brightness based on human perception, which is more appropriate for palettes with grayscale colors.
    /// The <see cref="ErrorDiffusionDitherer"/> tries to auto detect the strategy for each dithering session but this can be overridden by
    /// the <see cref="ConfigureErrorDiffusionMode">ConfigureErrorDiffusionMode</see> method.</para>
    /// <para>The following table demonstrates the effect of different strategies:
    /// <table class="table is-hoverable">
    /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
    /// <tbody><tr><td><div style="text-align:center;">
    /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
    /// <br/>Color hues with alpha gradient</para></div></td>
    /// <td><div style="text-align:center;">
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredFS.gif" alt="Color hues with system default 8 BPP palette, silver background and Floyd-Steinberg dithering, using error diffusion by RGB channels"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using error diffusion by RGB channels (the default strategy for non-grayscale palettes)</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredFSByBr.gif" alt="Color hues with system default 8 BPP palette, silver background and Floyd-Steinberg dithering, using error diffusion by brightness"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using error diffusion by brightness</para></div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <para><img src="../Help/Images/ColorWheel.png" alt="Color wheel"/>
    /// <br/>Color wheel</para></div></td>
    /// <td><div style="text-align:center;">
    /// <para><img src="../Help/Images/ColorWheelBWBlueDitheredFSByBr.gif" alt="Color wheel with black and white palette, blue background and Floyd-Steinberg dithering, using error diffusion by brightness (the default strategy for grayscale palettes)"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using blue background and error diffusion by brightness (the default strategy for grayscale palettes). All colors appear in the result with different patterns.</para>
    /// <para><img src="../Help/Images/ColorWheelBWBlueDitheredFSByRgb.gif" alt="Color wheel with black and white palette, blue background and Floyd-Steinberg dithering, using error diffusion by RGB channels"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using blue background and error diffusion by RGB channels. The fully saturated colors turned completely black or white.</para></div></td>
    /// </tr>
    /// </tbody></table></para>
    /// <para>A typical artifact of error diffusion dithering is a ripple effect, which often appears in homogeneous areas of the dithered image. This is due to the fact that most filters propagate quantization errors mostly to right and down,
    /// whereas pixels are processed left-to-right in each line while lines are scanned top-down (raster processing). The ripple effect can be reduced if every second line is processed in the opposite direction (serpentine processing).
    /// You can use the <see cref="ConfigureProcessingDirection">ConfigureProcessingDirection</see> method to obtain an <see cref="ErrorDiffusionDitherer"/> with serpentine processing mode,
    /// which processes even lines left-to-right and odd lines right-to-left.</para>
    /// <para>The following table demonstrates the effect of different processing directions:
    /// <table class="table is-hoverable">
    /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
    /// <tbody><tr><td><div style="text-align:center;">
    /// <para><img src="../Help/Images/Cameraman.png" alt="Test image &quot;Cameraman&quot;"/>
    /// <br/>Original test image "Cameraman"</para></div></td>
    /// <td><div style="text-align:center;">
    /// <para><img src="../Help/Images/CameramanBWDitheredFS.gif" alt="Test image &quot;Cameraman&quot; with black and white palette, using Floyd-Steinberg dithering with raster processing"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using raster processing. The ripple effect is clearly visible on the coat.</para>
    /// <para><img src="../Help/Images/CameramanBWDitheredFSSerpentine.gif" alt="Test image &quot;Cameraman&quot; with black and white palette, using Floyd-Steinberg dithering with serpentine processing"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using serpentine processing. The ripple effect is gone.</para></div></td>
    /// </tr>
    /// </tbody></table></para>
    /// <note type="tip">See the <strong>Examples</strong> section of the static properties for more examples.</note>
    /// </remarks>
    /// <seealso cref="IDitherer" />
    /// <seealso cref="OrderedDitherer"/>
    /// <seealso cref="RandomNoiseDitherer"/>
    /// <seealso cref="InterleavedGradientNoiseDitherer"/>
    /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer?, IDitherer?)"/>
    /// <seealso cref="BitmapDataExtensions.Dither(IReadWriteBitmapData, IQuantizer, IDitherer)"/>
    public sealed partial class ErrorDiffusionDitherer : IDitherer
    {
        #region Fields

        #region Static Fields

        private static ErrorDiffusionDitherer? floydSteinberg;
        private static ErrorDiffusionDitherer? jarvisJudiceNinke;
        private static ErrorDiffusionDitherer? stucki;
        private static ErrorDiffusionDitherer? burkes;
        private static ErrorDiffusionDitherer? sierra3;
        private static ErrorDiffusionDitherer? sierra2;
        private static ErrorDiffusionDitherer? sierraLite;
        private static ErrorDiffusionDitherer? stevensonArce;
        private static ErrorDiffusionDitherer? atkinson;

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

        #region Static Properties

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the original filter proposed by Floyd and Steinberg in 1975 when they came out with the idea of error diffusion dithering.
        /// Uses a small, 3x2 matrix so the processing is somewhat faster than by the other alternatives.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredFloydSteinberg(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = ErrorDiffusionDitherer.FloydSteinberg;
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap data:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized and dithered image</div></th></tr></thead>
        /// <tbody><tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredFS.gif" alt="Color hues with system default 8 BPP palette, using silver background and Floyd-Steinberg dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredFS.gif" alt="Grayscale color shades with black and white palette using Floyd-Steinberg dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldDefault8bppSilverA1DitheredFS.gif" alt="Shield icon with system default 8 BPP palette using silver background, alpha threshold = 1 and Floyd-Steinberg dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see></para>
        /// <para><img src="../Help/Images/ShieldBWSilverDitheredFS.gif" alt="Shield icon with black and white palette, silver background, using Floyd-Steinberg dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Cameraman.png" alt="Test image &quot;Cameraman&quot;"/>
        /// <br/>Original test image "Cameraman"</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Cameraman2bppDirectDitheredFS.gif" alt="Test image &quot;Cameraman&quot; with 2 BPP grayscale palette using Floyd-Steinberg dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.Grayscale4(Color32,bool,byte)">grayscale 4 color palette</see></para>
        /// <para><img src="../Help/Images/CameramanBWDitheredFS.gif" alt="Test image &quot;Cameraman&quot; with black and white palette using Floyd-Steinberg dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Lena.png" alt="Test image &quot;Lena&quot;"/>
        /// <br/>Original test image "Lena"</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/LenaRgb332DMFloydSteinberg.gif" alt="Test image &quot;Lena&quot; with RGB332 palette using direct color mapping and Floyd-Steinberg dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.Rgb332(Color32,bool,byte)">RGB 332 palette</see></para>
        /// <para><img src="../Help/Images/LenaRgb111DitheredFS.gif" alt="Test image &quot;Lena&quot; with RGB111 palette and Floyd-Steinberg dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.FromCustomPalette(Palette)">custom 8-color palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ErrorDiffusionDitherer"/> class for more details and examples.</note>
        /// </example>
        public static ErrorDiffusionDitherer FloydSteinberg => floydSteinberg ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 7 },
                { 3, 5, 1 },
            }, 16, 2);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the filter proposed by Jarvis, Judice and Ninke in 1976.
        /// Uses a 5x3 matrix so the processing is slower than by the original <see cref="FloydSteinberg">Floyd-Steinberg</see> filter but distributes errors in a wider range.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredJarvisJudiceNinke(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = ErrorDiffusionDitherer.JarvisJudiceNinke;
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap data:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized and dithered image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredJJN.gif" alt="Color hues with system default 8 BPP palette, using silver background and Jarvis-Judice-Ninke dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredJJN.gif" alt="Grayscale color shades with black and white palette using Jarvis-Judice-Ninke dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ErrorDiffusionDitherer"/> class for more details and examples.</note>
        /// </example>
        public static ErrorDiffusionDitherer JarvisJudiceNinke => jarvisJudiceNinke ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 7, 5 },
                { 3, 5, 7, 5, 3 },
                { 1, 3, 5, 3, 1 },
            }, 48, 3);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the filter proposed by P. Stucki in 1981.
        /// Uses a 5x3 matrix so the processing is slower than by the original <see cref="FloydSteinberg">Floyd-Steinberg</see> filter but distributes errors in a wider range.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredStucki(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = ErrorDiffusionDitherer.Stucki;
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap data:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized and dithered image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredSt.gif" alt="Color hues with system default 8 BPP palette, using silver background and Stucki dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredSt.gif" alt="Grayscale color shades with black and white palette using Stucki dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ErrorDiffusionDitherer"/> class for more details and examples.</note>
        /// </example>
        public static ErrorDiffusionDitherer Stucki => stucki ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 8, 4 },
                { 2, 4, 8, 4, 2 },
                { 1, 2, 4, 2, 1 },
            }, 42, 3);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the filter proposed by D. Burkes in 1988.
        /// Uses a 5x2 matrix, which is actually the same as the first two lines of the matrix used by the <see cref="Stucki"/> filter.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredBurkes(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = ErrorDiffusionDitherer.Burkes;
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap data:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized and dithered image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredBrk.gif" alt="Color hues with system default 8 BPP palette, using silver background and Burkes dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredBrk.gif" alt="Grayscale color shades with black and white palette using Burkes dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ErrorDiffusionDitherer"/> class for more details and examples.</note>
        /// </example>
        public static ErrorDiffusionDitherer Burkes => burkes ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 8, 4 },
                { 2, 4, 8, 4, 2 },
            }, 32, 3);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the three-line filter proposed by Frankie Sierra in 1989.
        /// Uses a 5x3 matrix so this is the slowest Sierra filter but this disperses errors to the furthest among them.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredSierra3(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = ErrorDiffusionDitherer.Sierra3;
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap data:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized and dithered image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredS3.gif" alt="Color hues with system default 8 BPP palette, using silver background and Sierra 3 dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredS3.gif" alt="Grayscale color shades with black and white palette using Sierra 3 dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ErrorDiffusionDitherer"/> class for more details and examples.</note>
        /// </example>
        public static ErrorDiffusionDitherer Sierra3 => sierra3 ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 5, 3 },
                { 2, 4, 5, 4, 2 },
                { 0, 2, 3, 2, 0 },
            }, 32, 3);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the two-line filter proposed by Frankie Sierra in 1990.
        /// Uses a 5x2 matrix so this somewhat faster than the <see cref="Sierra3">three-line version</see> and still provides a similar quality.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredSierra2(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = ErrorDiffusionDitherer.Sierra2;
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap data:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized and dithered image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredS2.gif" alt="Color hues with system default 8 BPP palette, using silver background and Sierra 2 dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredS2.gif" alt="Grayscale color shades with black and white palette using Sierra 2 dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ErrorDiffusionDitherer"/> class for more details and examples.</note>
        /// </example>
        public static ErrorDiffusionDitherer Sierra2 => sierra2 ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 4, 3 },
                { 1, 2, 3, 2, 1 },
            }, 16, 3);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using a small two-line filter proposed by Frankie Sierra.
        /// Uses a 3x2 matrix so it has the same performance as the <see cref="FloydSteinberg">Floyd-Steinberg</see> algorithm and also produces a quite similar result.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredSierraLite(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = ErrorDiffusionDitherer.SierraLite;
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap data:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized and dithered image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredSL.gif" alt="Color hues with system default 8 BPP palette, using silver background and Sierra Lite dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredSL.gif" alt="Grayscale color shades with black and white palette using Sierra Lite dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ErrorDiffusionDitherer"/> class for more details and examples.</note>
        /// </example>
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
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredStevensonArce(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = ErrorDiffusionDitherer.StevensonArce;
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap data:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized and dithered image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredSA.gif" alt="Color hues with system default 8 BPP palette, using silver background and Stevenson-Arce dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredSA.gif" alt="Grayscale color shades with black and white palette using Stevenson-Arce dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ErrorDiffusionDitherer"/> class for more details and examples.</note>
        /// </example>
        public static ErrorDiffusionDitherer StevensonArce => stevensonArce ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 0, 0, 0, 32, 0 },
                { 12, 0, 26, 0, 30, 0, 16 },
                { 0, 12, 0, 26, 0, 12, 0 },
                { 5, 0, 12, 0, 12, 0, 5 },
            }, 200, 4);

        /// <summary>
        /// Gets an <see cref="ErrorDiffusionDitherer"/> instance using the filter used by the Apple programmer Bill Atkinson.
        /// Uses a 4x3 matrix of only 6 effective values, and propagates only the 75% of the quantization error, which may cause
        /// total loss of details of light and dark areas (result may seem under- or overexposed) whereas midtones have higher contrast
        /// and preserve details better.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredAtkinson(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = ErrorDiffusionDitherer.Atkinson;
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap data:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized and dithered image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredAtk.gif" alt="Color hues with system default 8 BPP palette, using silver background and Atkinson dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredAtk.gif" alt="Grayscale color shades with black and white palette using Atkinson dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="ErrorDiffusionDitherer"/> class for more details and examples.</note>
        /// </example>
        public static ErrorDiffusionDitherer Atkinson => atkinson ??=
            new ErrorDiffusionDitherer(new byte[,]
            {
                { 0, 0, 1, 1 },
                { 1, 1, 1, 0 },
                { 0, 1, 0, 0 },
            }, 8, 2);

        #endregion

        #region Instance Properties

        bool IDitherer.InitializeReliesOnContent => isSerpentineProcessing;

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDiffusionDitherer" /> class using the specified <paramref name="matrix"/>, <paramref name="divisor"/> and <paramref name="matrixFirstPixelIndex"/>.
        /// </summary>
        /// <param name="matrix">A matrix to be used as the coefficients for the quantization errors to be propagated to the neighboring pixels.</param>
        /// <param name="divisor">Each elements in the <paramref name="matrix"/> will be divided by this value. If less than the sum of the elements
        /// in the <paramref name="matrix"/>, then only a fraction of the error will be propagated.</param>
        /// <param name="matrixFirstPixelIndex">Specifies the first effective index in the first row of the matrix. If larger than zero, then the error will be propagated also to the bottom-left direction.
        /// Must be between 0 and <paramref name="matrix"/> width, excluding upper bound.</param>
        /// <param name="serpentineProcessing"><see langword="true"/> to process odd lines right-to-left and even lines left-to-right;
        /// <see langword="false"/> to process all lines left-to-right.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ConfigureProcessingDirection">ConfigureProcessingDirection</see> method for details. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <param name="byBrightness"><see langword="true"/> to apply the same quantization error on every color channel determined by brightness difference;
        /// <see langword="false"/> to handle quantization errors on each color channels independently; <see langword="null"/> to auto select strategy.
        /// Deciding by brightness can produce a better result when fully saturated colors are mapped to a grayscale palette.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ConfigureErrorDiffusionMode">ConfigureErrorDiffusionMode</see> method for details. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <example>
        /// The following example demonstrates how to use a custom ditherer using the <see cref="ErrorDiffusionDitherer"/> constructor:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToCustomDithered(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     // This is actually the Fan dithering (by Zhihang Fan), and uses the same coefficients
        ///     // as the Floyd-Steinberg dithering in a slightly different arrangement:
        ///     byte[,] matrix =
        ///     {
        ///         { 0, 0, 0, 7 },
        ///         { 1, 3, 5, 0 },
        ///     };
        ///
        ///     // The matrix values will be divided by this value to determine the portion
        ///     // of the quantization error to propagate to neighboring pixels:
        ///     int divisor = 16;
        ///
        ///     // The current pixel to be processed is always one pixel left from this index.
        ///     // This also means that if larger than 1, then some error is propagated also towards the
        ///     // (bottom-)left direction. For the matrix above value "3" means that whenever a pixel is
        ///     // processed, 7/16 of the error is propagated to the right, 1/16 and 3/16 to the
        ///     // bottom-left direction and 5/16 one pixel down from the current pixel.
        ///     int firstPixelIndex = 3;
        ///
        ///     IDitherer ditherer = new ErrorDiffusionDitherer(matrix, divisor, firstPixelIndex);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap data:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized and dithered image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredFan.gif" alt="Color hues with system default 8 BPP palette, using silver background and Fan dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredFan.gif" alt="Grayscale color shades with black and white palette using Fan dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip"><list type="bullet">
        /// <item>Use the static properties to perform dithering with predefined filters.</item>
        /// <item>See the <strong>Remarks</strong> section of the <see cref="ErrorDiffusionDitherer"/> class for more details and image examples.</item>
        /// </list></note>
        /// </example>
        public ErrorDiffusionDitherer(byte[,] matrix, int divisor, int matrixFirstPixelIndex, bool serpentineProcessing = false, bool? byBrightness = null)
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
            isSerpentineProcessing = serpentineProcessing;
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

        /// <summary>
        /// Gets a new <see cref="ErrorDiffusionDitherer"/> instance that has the specified error diffusion mode.
        /// </summary>
        /// <param name="byBrightness"><see langword="true"/> to apply the same quantization error on every color channel determined by brightness difference;
        /// <see langword="false"/> to handle quantization errors on each color channels independently; <see langword="null"/> to auto select strategy.
        /// Deciding by brightness can produce a better result when fully saturated colors are mapped to a grayscale palette.</param>
        /// <returns>A new <see cref="ErrorDiffusionDitherer"/> instance that has the specified error diffusion mode.</returns>
        /// <remarks>
        /// <note>This method always returns a new <see cref="ErrorDiffusionDitherer"/> instance instead of changing the error diffusion mode of the original one.
        /// This is required for the static properties so they can return a cached instance.</note>
        /// <para>Calculation of the quantization error may happen in two ways. The publicly available algorithms usually calculate the error for each color channels,
        /// which usually provides good results with color palettes. However, when quantizing color images with a black and white or grayscale palette,
        /// this approach may fail. For example, if the quantizer returns black for a fully saturated blue pixel, the quantization error is zero on the red and green channels and
        /// 100% on the blue channel. The problem is that this error cannot be propagated to the neighboring pixels if they have the same color because adding any more blue
        /// to already fully saturated blue pixels will not change anything. Therefore, the <see cref="ErrorDiffusionDitherer"/> can propagate quantization error
        /// by brightness based on human perception, which is more appropriate for palettes with grayscale colors.</para>
        /// <para>The following table demonstrates the effect of different strategies:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredFS.gif" alt="Color hues with system default 8 BPP palette, silver background and Floyd-Steinberg dithering, using error diffusion by RGB channels"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using error diffusion by RGB channels (the default strategy for non-grayscale palettes)</para>
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredFSByBr.gif" alt="Color hues with system default 8 BPP palette, silver background and Floyd-Steinberg dithering, using error diffusion by brightness"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette(Color32,byte)">system default 8 BPP palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using error diffusion by brightness</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ColorWheel.png" alt="Color wheel"/>
        /// <br/>Color wheel</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ColorWheelBWBlueDitheredFSByBr.gif" alt="Color wheel with black and white palette, blue background and Floyd-Steinberg dithering, using error diffusion by brightness (the default strategy for grayscale palettes)"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using blue background and error diffusion by brightness (the default strategy for grayscale palettes). All colors appear in the result with different patterns.</para>
        /// <para><img src="../Help/Images/ColorWheelBWBlueDitheredFSByRgb.gif" alt="Color wheel with black and white palette, blue background and Floyd-Steinberg dithering, using error diffusion by RGB channels"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using blue background and error diffusion by RGB channels. The fully saturated colors turned completely black or white.</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to specify the error diffusion mode for a predefined filter:
        /// <code lang="C#"><![CDATA[
        /// // getting a predefined ditherer that disperses quantization error by brightness:
        /// IDitherer ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureErrorDiffusionMode(byBrightness: true);
        /// ]]></code>
        /// </example>
        // ReSharper disable once ParameterHidesMember - No conflict, a new instance is created
        public ErrorDiffusionDitherer ConfigureErrorDiffusionMode(bool? byBrightness) => new ErrorDiffusionDitherer(this, isSerpentineProcessing, byBrightness);

        /// <summary>
        /// Gets a new <see cref="ErrorDiffusionDitherer"/> instance that has the specified processing direction.
        /// </summary>
        /// <param name="serpentine"><see langword="true"/> to process odd lines right-to-left and even lines left-to-right;
        /// <see langword="false"/> to process all lines left-to-right.</param>
        /// <returns>A new <see cref="ErrorDiffusionDitherer"/> instance that has the specified processing direction.</returns>
        /// <remarks>
        /// <note>This method always returns a new <see cref="ErrorDiffusionDitherer"/> instance instead of changing the processing direction of the original one.
        /// This is required for the static properties so they can return a cached instance.</note>
        /// <para>A typical artifact of error diffusion dithering is a ripple effect, which often appears in homogeneous areas of the dithered image. This is due to the fact that most filters propagate quantization errors mostly to right and down,
        /// whereas pixels are processed left-to-right in each line while lines are scanned top-down (raster processing). The ripple effect can be reduced if every second line is processed in the opposite direction (serpentine processing).</para>
        /// <para>The following table demonstrates the effect of different processing directions:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Cameraman.png" alt="Test image &quot;Cameraman&quot;"/>
        /// <br/>Original test image "Cameraman"</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/CameramanBWDitheredFS.gif" alt="Test image &quot;Cameraman&quot; with black and white palette, using Floyd-Steinberg dithering with raster processing"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using raster processing. The ripple effect is clearly visible on the coat.</para>
        /// <para><img src="../Help/Images/CameramanBWDitheredFSSerpentine.gif" alt="Test image &quot;Cameraman&quot; with black and white palette, using Floyd-Steinberg dithering with serpentine processing"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite(Color32,byte,byte)">black and white palette</see> and <see cref="FloydSteinberg">Floyd-Steinberg</see> dithering, using serpentine processing. The ripple effect is gone.</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to specify the processing direction for a predefined filter:
        /// <code lang="C#"><![CDATA[
        /// // getting a predefined ditherer with serpentine processing:
        /// IDitherer ditherer = ErrorDiffusionDitherer.FloydSteinberg.ConfigureProcessingDirection(serpentine: true);
        /// ]]></code>
        /// </example>
        public ErrorDiffusionDitherer ConfigureProcessingDirection(bool serpentine) => new ErrorDiffusionDitherer(this, serpentine, byBrightness);

        #endregion

        #region Explicitly Implemented Interface Methods

        [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract",
            Justification = "It CAN be null, just must no be. Null check is in the called ctor.")]
        IDitheringSession IDitherer.Initialize(IReadableBitmapData source, IQuantizingSession quantizingSession, IAsyncContext? context)
            => quantizingSession?.WorkingColorSpace == WorkingColorSpace.Linear
                ? isSerpentineProcessing
                    ? new DitheringSessionSerpentineLinear(quantizingSession, this, source)
                    : new DitheringSessionRasterLinear(quantizingSession, this, source)
                : isSerpentineProcessing
                    ? new DitheringSessionSerpentineSrgb(quantizingSession!, this, source)
                    : new DitheringSessionRasterSrgb(quantizingSession!, this, source);

        #endregion

        #endregion
    }
}
