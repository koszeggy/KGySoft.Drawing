﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OrderedDitherer.cs
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
using System.Drawing;
using System.Security;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides an <see cref="IDitherer"/> implementation for dithering patterns that are based on an ordered matrix.
    /// Use the static properties of this class to use predefined patterns or the <see cref="OrderedDitherer(byte[,],float)">constructor</see> to create a custom ordered ditherer.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="OrderedDitherer(byte[,],float)">constructor</see> can be used to create an ordered ditherer using a custom matrix.</para>
    /// <para>Use the static properties to obtain an instance with a predefined pattern. For the best results use the <see cref="Bayer8x8">Bayer8x8</see>
    /// or <see cref="BlueNoise">BlueNoise</see> properties. Or you can use the <see cref="DottedHalftone">DottedHalftone</see> property for artistic results.</para>
    /// <para>The <see cref="OrderedDitherer"/> class offers a very fast dithering technique based on an ordered pattern specified in a matrix of bytes.
    /// The more different values the matrix has the more number of different patterns can be mapped to the shades of the original pixels.
    /// While quantizing lighter and lighter colors, the different patterns appear in the order of the values in the specified matrix.</para>
    /// <para>The following table demonstrates the effect of the dithering:
    /// <table class="table is-hoverable">
    /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
    /// <tbody>
    /// <tr><td><div style="text-align:center;">
    /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
    /// <br/>Color hues with alpha gradient</para></div></td>
    /// <td><div style="text-align:center;">
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilver.gif" alt="Color hues with system default 8 BPP palette and silver background"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see>, no dithering</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredB8.gif" alt="Color hues with system default 8 BPP palette, silver background and Bayer 8x8 ordered dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see> and <see cref="Bayer8x8">Bayer 8x8</see> dithering</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredBN.gif" alt="Color hues with system default 8 BPP palette, using silver background and blue noise dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see> and <see cref="BlueNoise">blue noise</see> dithering</para>
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredDH.gif" alt="Color hues with system default 8 BPP palette, using silver background and rectangular 7x7 dotted halftone pattern dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see> and <see cref="DottedHalftone">dotted halftone pattern</see> dithering</para></div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
    /// <br/>Grayscale color shades</para></div></td>
    /// <td><div style="text-align:center;">
    /// <para><img src="../Help/Images/GrayShadesBW.gif" alt="Grayscale color shades with black and white palette"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see>, no dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredB8.gif" alt="Grayscale color shades with black and white palette, using Bayer 8x8 ordered dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see> and <see cref="Bayer8x8">Bayer 8x8</see> dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredBN.gif" alt="Grayscale color shades with black and white palette using blue noise dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see> and <see cref="BlueNoise">blue noise</see> dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredDH.gif" alt="Grayscale color shades with black and white palette using rectangular 7x7 dotted halftone pattern dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see> and <see cref="DottedHalftone">dotted halftone pattern</see> dithering</para></div></td>
    /// </tr>
    /// </tbody></table></para>
    /// <para>Unlike in case of the <see cref="ErrorDiffusionDitherer"/>, ordered dithering does not adjust strength to the quantization error of a pixel
    /// but simply uses the specified matrix values based on pixel coordinates to determine the quantized result.
    /// Therefore, a strength can be specified (see the <see cref="OrderedDitherer(byte[,],float)">constructor</see> and the <see cref="ConfigureStrength">ConfigureStrength</see> method),
    /// whose ideal value depends on the colors that a quantizer can return. If the strength is too low, then banding may appear in the result in place of gradients in the original image;
    /// whereas if the strength is too high, then dithering patterns may appear even in colors without quantization error (overdithering).</para>
    /// <para>Every static property in the <see cref="OrderedDitherer"/> returns an instance with auto strength, meaning that
    /// strength will be calibrated for each dithering session so that neither the black, nor the white colors will suffer from overdithering in the result.</para>
    /// <para>Auto strength can use different calibration strategies. The default strategy is usually correct if the quantizer returns evenly distributed colors.
    /// Otherwise, you can apply the <see cref="AutoStrengthMode.Interpolated"/> auto strength mode by the <see cref="ConfigureAutoStrengthMode">ConfigureAutoStrengthMode</see>
    /// method that calibrates the strength both for the black and white colors and uses a dynamic strength to each pixel based on its brightness.
    /// If none of the auto strength modes provide the desired result you can obtain an <see cref="OrderedDitherer"/> instance with custom strength
    /// by the <see cref="ConfigureStrength">ConfigureStrength</see> method.</para>
    /// <para>The following table demonstrates the effect of different strengths:
    /// <table class="table is-hoverable">
    /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
    /// <tbody>
    /// <tr><td><div style="text-align:center;">
    /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
    /// <br/>Grayscale color shades</para></div></td>
    /// <td><div style="text-align:center;">
    /// <para><img src="../Help/Images/GrayShadesDefault4bpp.gif" alt="Grayscale color shades with system default 4 BPP palette"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault4BppPalette">system default 4 BPP palette</see>, no dithering. The asymmetry is due to the uneven distribution of gray shades of this palette.</para>
    /// <para><img src="../Help/Images/GrayShadesDefault4bppDitheredB8.gif" alt="Grayscale color shades with system default 4 BPP palette using Bayer 8x8 ordered dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault4BppPalette">system default 4 BPP palette</see> and <see cref="Bayer8x8">Bayer 8x8</see> dithering using auto strength. Darker shades have banding.</para>
    /// <para><img src="../Help/Images/GrayShadesDefault4bppDitheredB8Str-5.gif" alt="Grayscale color shades with system default 4 BPP palette using a stronger Bayer 8x8 ordered dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault4BppPalette">system default 4 BPP palette</see> and <see cref="Bayer8x8">Bayer 8x8</see> dithering using strength = 0.5. Now there is no banding but white suffers from overdithering.</para>
    /// <para><img src="../Help/Images/GrayShadesDefault4bppDitheredB8Interpolated.gif" alt="Grayscale color shades with system default 4 BPP palette using 8x8 ordered dithering with interpolated ato strength"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault4BppPalette">system default 4 BPP palette</see> and <see cref="Bayer8x8">Bayer 8x8</see> dithering using <see cref="AutoStrengthMode.Interpolated"/> auto strength strategy.
    /// Now there is neither banding nor overdithering for black or white colors.</para></div></td>
    /// </tr></tbody></table></para>
    /// <note type="tip">See the <strong>Examples</strong> section of the static properties for more examples.</note>
    /// </remarks>
    /// <seealso cref="IDitherer" />
    /// <seealso cref="ErrorDiffusionDitherer"/>
    /// <seealso cref="RandomNoiseDitherer"/>
    /// <seealso cref="InterleavedGradientNoiseDitherer"/>
    /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer?, IDitherer?)"/>
    /// <seealso cref="BitmapDataExtensions.Dither(IReadWriteBitmapData, IQuantizer, IDitherer)"/>
    public sealed class OrderedDitherer : IDitherer
    {
        #region Nested Classes
        
        #region OrderedDitheringSessionSrgb class

        private sealed class OrderedDitheringSessionSrgb : VariableStrengthDitheringSessionSrgbBase
        {
            #region Fields

            private readonly OrderedDitherer ditherer;

            #endregion

            #region Properties

#if DEBUG
            public override bool IsSequential => true;
#else
            public override bool IsSequential => false; 
#endif

            #endregion

            #region Constructors

            internal OrderedDitheringSessionSrgb(IQuantizingSession quantizingSession, OrderedDitherer ditherer)
                : base(quantizingSession)
            {
                this.ditherer = ditherer;
                if (ditherer.strength > 0f)
                {
                    Strength = ditherer.strength;
                    return;
                }

                Strength = CalibrateStrength(ditherer.matrixMinValue, ditherer.matrixMaxValue, ditherer.autoStrengthMode == AutoStrengthMode.Interpolated);
            }

            #endregion

            #region Methods

            protected override sbyte GetOffset(int x, int y) => ditherer.premultipliedMatrix[y % ditherer.matrixHeight, x % ditherer.matrixWidth];

            #endregion
        }

        #endregion

        #region OrderedDitheringSessionLinear class

        private sealed class OrderedDitheringSessionLinear : VariableStrengthDitheringSessionLinearBase
        {
            #region Fields

            private readonly CastArray2D<byte, float> offsets;

            private ArraySection<byte> offsetsBuffer;

            #endregion

            #region Properties

            public override bool IsSequential => false;

            #endregion

            #region Constructors

            [SecuritySafeCritical]
            internal OrderedDitheringSessionLinear(IQuantizingSession quantizingSession, OrderedDitherer ditherer)
                : base(quantizingSession)
            {
                const float norm = 256f;
                offsetsBuffer = new ArraySection<byte>(ditherer.matrixHeight * ditherer.matrixWidth * sizeof(float));
                offsets = new CastArray2D<byte, float>(offsetsBuffer, ditherer.matrixHeight, ditherer.matrixWidth);
                for (int y = 0; y < offsets.Height; y++)
                for (int x = 0; x < offsets.Width; x++)
                    offsets.GetElementReferenceUnsafe(y, x) = ditherer.premultipliedMatrix[y, x] / norm;

                if (ditherer.strength > 0f)
                {
                    Strength = ditherer.strength;
                    return;
                }

                Strength = CalibrateStrength(ditherer.matrixMinValue / norm, ditherer.matrixMaxValue / norm, ditherer.autoStrengthMode != AutoStrengthMode.Constant);
            }

            #endregion

            #region Methods

            [SecuritySafeCritical]
            protected override float GetOffset(int x, int y) => offsets.GetElementUnsafe(y % offsets.Height, x % offsets.Width);

            protected override void Dispose(bool disposing)
            {
                offsetsBuffer.Release();
                base.Dispose(disposing);
            }

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        #region Static Fields
        // ReSharper disable InconsistentNaming - x in names are meant to be lowercase

        private static OrderedDitherer? bayer2x2;
        private static OrderedDitherer? bayer3x3;
        private static OrderedDitherer? bayer4x4;
        private static OrderedDitherer? bayer8x8;
        private static OrderedDitherer? dottedHalftone;
        private static OrderedDitherer? blueNoise64;

        // ReSharper restore InconsistentNaming
        #endregion

        #region Instance Fields

        private readonly sbyte[,] premultipliedMatrix;
        private readonly int matrixWidth;
        private readonly int matrixHeight;
        private readonly sbyte matrixMinValue;
        private readonly sbyte matrixMaxValue;
        private readonly float strength;
        private readonly AutoStrengthMode autoStrengthMode;

        #endregion

        #endregion

        #region Properties

        #region Static Properties
        // ReSharper disable InconsistentNaming - x in names are meant to be lowercase

        /// <summary>
        /// Gets an <see cref="OrderedDitherer"/> using the standard Bayer 2x2 matrix.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredBayer2x2(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = OrderedDitherer.Bayer2x2;
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
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredB2.gif" alt="Color hues with system default 8 BPP palette, using silver background and Bayer 2x2 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredB2.gif" alt="Grayscale color shades with black and white palette using Bayer 2x2 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer"/> class for more details and examples.</note>
        /// </example>
        public static OrderedDitherer Bayer2x2 => bayer2x2 ??=
            new OrderedDitherer(new byte[,]
            {
                { 0, 2 },
                { 3, 1 },
            });

        /// <summary>
        /// Gets an <see cref="OrderedDitherer"/> using the standard Bayer 3x3 matrix.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredBayer3x3(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = OrderedDitherer.Bayer3x3;
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
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredB3.gif" alt="Color hues with system default 8 BPP palette, using silver background and Bayer 3x3 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredB3.gif" alt="Grayscale color shades with black and white palette using Bayer 3x3 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer"/> class for more details and examples.</note>
        /// </example>
        public static OrderedDitherer Bayer3x3 => bayer3x3 ??=
            new OrderedDitherer(new byte[,]
            {
                { 0, 7, 3 },
                { 6, 5, 2 },
                { 4, 1, 8 },
            });

        /// <summary>
        /// Gets an <see cref="OrderedDitherer"/> using the standard Bayer 4x4 matrix.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredBayer4x4(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = OrderedDitherer.Bayer4x4;
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
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredB4.gif" alt="Color hues with system default 8 BPP palette, using silver background and Bayer 4x4 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredB4.gif" alt="Grayscale color shades with black and white palette using Bayer 4x4 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer"/> class for more details and examples.</note>
        /// </example>
        public static OrderedDitherer Bayer4x4 => bayer4x4 ??=
            new OrderedDitherer(new byte[,]
            {
                { 0, 8, 2, 10 },
                { 12, 4, 14, 6 },
                { 3, 11, 1, 9 },
                { 15, 7, 13, 5 },
            });

        /// <summary>
        /// Gets an <see cref="OrderedDitherer"/> using the standard Bayer 8x8 matrix.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredBayer8x8(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = OrderedDitherer.Bayer8x8;
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
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredB8.gif" alt="Color hues with system default 8 BPP palette, using silver background and Bayer 8x8 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para>
        /// <para><img src="../Help/Images/AlphaGradientRgb111SilverDitheredB8.gif" alt="Color hues with RGB111 palette and silver background, using Bayer 8x8 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.FromCustomPalette(Color[],Color,byte)">custom 8-color palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredB8.gif" alt="Grayscale color shades with black and white palette using Bayer 8x8 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see></para>
        /// <para><img src="../Help/Images/GrayShades2bppDitheredB8.gif" alt="Grayscale color shades with 2 BPP grayscale palette, using nearest color lookup and Bayer 8x8 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.Grayscale4">4-color grayscale palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldDefault8bppBlackDitheredB8.gif" alt="Shield icon with system default 8 BPP palette using Bayer 8x8 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GirlWithAPearlEarring.png" alt="Test image &quot;Girl with a Pearl Earring&quot;"/>
        /// <br/>Original test image "Girl with a Pearl Earring"</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GirlWithAPearlEarringDefault8bppDitheredB8Srgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with system default 8 BPP palette, quantized in the sRGB color space using Bayer 8x8 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer"/> class for more details and examples.</note>
        /// </example>
        public static OrderedDitherer Bayer8x8 => bayer8x8 ??=
            new OrderedDitherer(new byte[,]
            {
                { 0, 48, 12, 60, 3, 51, 15, 63 },
                { 32, 16, 44, 28, 35, 19, 47, 31 },
                { 8, 56, 4, 52, 11, 59, 7, 55 },
                { 40, 24, 36, 20, 43, 27, 39, 23 },
                { 2, 50, 14, 62, 1, 49, 13, 61 },
                { 34, 18, 46, 30, 33, 17, 45, 29 },
                { 10, 58, 6, 54, 9, 57, 5, 53 },
                { 42, 26, 38, 22, 41, 25, 37, 21 }
            });

        /// <summary>
        /// Gets an <see cref="OrderedDitherer"/> using a 8x8 matrix of a dotted halftone pattern.
        /// </summary>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredDottedHalftone(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = OrderedDitherer.DottedHalftone;
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
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredDH.gif" alt="Color hues with system default 8 BPP palette, using silver background and dotted halftone dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredDH.gif" alt="Grayscale color shades with black and white palette using dotted halftone dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer"/> class for more details and examples.</note>
        /// </example>
        public static OrderedDitherer DottedHalftone => dottedHalftone ??=
            // credits to http://caca.zoy.org/study/part3.html, where this matrix is taken from
            new OrderedDitherer(new byte[,]
            {
                { 24, 10, 12, 26, 35, 47, 49, 37 },
                { 8, 0, 2, 14, 45, 59, 61, 51 },
                { 22, 6, 4, 16, 43, 57, 63, 53 },
                { 30, 20, 18, 28, 33, 41, 55, 39 },
                { 34, 46, 48, 36, 25, 11, 13, 27 },
                { 44, 58, 60, 50, 9, 1, 3, 15 },
                { 42, 56, 62, 52, 23, 7, 5, 17 },
                { 32, 40, 54, 38, 31, 21, 19, 29 },
            });

        /// <summary>
        /// Gets an <see cref="OrderedDitherer"/> using a fixed 64x64 blue noise pattern of 256 different values.
        /// </summary>
        /// <remarks>
        /// <note>Generating random blue noise patterns is a very resource intensive operation but this method uses a pregenerated fix pattern, which is very fast.
        /// To dither images with real random noise use the <see cref="RandomNoiseDitherer"/>, which applies white noise to the quantized source.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the ditherer returned by this property:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredBlueNoise(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = OrderedDitherer.BlueNoise;
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
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredBN.gif" alt="Color hues with system default 8 BPP palette, using silver background and blue noise dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredBN.gif" alt="Grayscale color shades with black and white palette using blue noise dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer"/> class for more details and examples.</note>
        /// </example>
        public static OrderedDitherer BlueNoise => blueNoise64 ??=
            // Credits to https://github.com/bartwronski/BlueNoiseGenerator from where this pattern is taken from
            // The source repository is under the MIT license, which is available here: https://opensource.org/licenses/MIT
            new OrderedDitherer(new byte[,]
            {
                { 65, 247, 203, 177, 54, 149, 96, 135, 122, 62, 109, 206, 27, 217, 152, 103, 250, 78, 122, 228, 3, 83, 233, 160, 45, 242, 108, 40, 125, 93, 201, 35, 231, 187, 254, 207, 147, 13, 87, 134, 246, 197, 177, 224, 59, 92, 132, 169, 49, 183, 140, 3, 58, 165, 27, 204, 12, 83, 196, 4, 159, 183, 92, 197 },
                { 170, 140, 24, 127, 109, 255, 35, 210, 79, 193, 178, 141, 168, 11, 69, 130, 182, 27, 147, 47, 191, 170, 66, 13, 187, 76, 0, 197, 161, 66, 146, 172, 104, 134, 58, 97, 182, 232, 162, 115, 34, 73, 2, 238, 162, 188, 6, 243, 218, 31, 69, 193, 244, 87, 146, 130, 248, 172, 225, 104, 235, 21, 218, 117 },
                { 236, 49, 87, 155, 228, 69, 15, 166, 235, 24, 48, 86, 119, 238, 195, 90, 6, 221, 165, 105, 20, 255, 120, 146, 211, 129, 88, 236, 21, 52, 245, 17, 73, 158, 24, 7, 126, 43, 64, 190, 218, 95, 128, 23, 207, 46, 113, 145, 85, 102, 229, 119, 40, 106, 222, 66, 49, 152, 31, 126, 46, 145, 57, 10 },
                { 191, 104, 213, 3, 42, 197, 182, 104, 147, 1, 223, 252, 60, 34, 161, 45, 244, 61, 208, 133, 89, 199, 37, 56, 245, 29, 174, 152, 114, 190, 212, 127, 179, 238, 216, 195, 246, 109, 26, 240, 170, 51, 155, 108, 81, 249, 28, 195, 60, 175, 153, 19, 208, 177, 15, 187, 114, 211, 93, 72, 178, 203, 82, 162 },
                { 28, 72, 179, 242, 160, 83, 120, 55, 214, 128, 156, 100, 180, 136, 214, 106, 144, 117, 30, 231, 71, 155, 177, 106, 94, 224, 47, 69, 229, 99, 83, 4, 45, 114, 87, 141, 72, 156, 203, 79, 139, 13, 232, 181, 137, 67, 159, 212, 10, 130, 254, 77, 52, 160, 235, 80, 5, 241, 192, 18, 254, 111, 227, 131 },
                { 248, 147, 115, 59, 133, 207, 26, 248, 91, 67, 31, 202, 13, 78, 229, 16, 201, 82, 183, 52, 240, 18, 221, 7, 139, 163, 202, 12, 134, 32, 164, 224, 198, 63, 33, 170, 51, 224, 101, 19, 116, 211, 61, 198, 36, 226, 121, 93, 236, 38, 200, 97, 141, 123, 33, 102, 139, 165, 58, 133, 157, 4, 96, 41 },
                { 199, 13, 219, 98, 16, 227, 144, 39, 189, 172, 237, 113, 53, 189, 126, 67, 173, 156, 4, 101, 141, 114, 205, 63, 191, 79, 118, 241, 185, 57, 143, 248, 102, 154, 229, 121, 0, 178, 38, 150, 186, 254, 89, 4, 101, 173, 17, 186, 54, 112, 167, 0, 215, 247, 63, 203, 227, 42, 85, 220, 34, 207, 64, 173 },
                { 80, 51, 186, 37, 171, 73, 110, 161, 9, 220, 81, 140, 164, 241, 25, 95, 253, 38, 215, 194, 170, 43, 85, 125, 250, 21, 40, 149, 108, 208, 76, 21, 130, 12, 183, 252, 94, 210, 241, 129, 68, 44, 165, 127, 242, 47, 152, 82, 142, 223, 71, 28, 179, 86, 189, 150, 24, 176, 122, 104, 184, 141, 239, 120 },
                { 225, 135, 89, 253, 125, 193, 243, 60, 97, 123, 44, 5, 212, 104, 148, 50, 223, 135, 120, 74, 248, 29, 229, 158, 53, 177, 214, 88, 5, 168, 42, 192, 90, 213, 74, 28, 135, 59, 83, 9, 225, 110, 27, 145, 215, 70, 205, 251, 22, 194, 155, 243, 133, 46, 8, 115, 73, 249, 14, 233, 77, 47, 23, 154 },
                { 106, 165, 205, 2, 152, 49, 24, 206, 232, 150, 183, 251, 66, 34, 203, 185, 20, 86, 57, 10, 150, 96, 186, 3, 137, 234, 102, 63, 254, 221, 119, 232, 174, 53, 148, 202, 162, 115, 195, 173, 154, 203, 235, 79, 189, 114, 6, 131, 105, 43, 91, 118, 59, 226, 162, 95, 213, 136, 55, 194, 163, 94, 212, 10 },
                { 240, 28, 64, 232, 104, 84, 178, 137, 74, 17, 198, 89, 131, 171, 77, 113, 160, 236, 199, 225, 128, 65, 209, 108, 75, 34, 155, 196, 132, 29, 157, 67, 112, 36, 240, 105, 19, 46, 220, 32, 93, 53, 10, 178, 58, 33, 226, 169, 63, 182, 235, 206, 18, 107, 198, 236, 39, 157, 205, 1, 127, 252, 177, 72 },
                { 191, 122, 42, 143, 199, 12, 218, 119, 35, 108, 54, 159, 23, 240, 219, 0, 143, 100, 177, 36, 166, 243, 15, 173, 224, 123, 19, 183, 49, 82, 15, 244, 141, 3, 217, 81, 185, 250, 142, 73, 106, 246, 124, 137, 99, 156, 240, 86, 202, 10, 135, 35, 170, 143, 69, 182, 27, 87, 109, 66, 147, 30, 115, 53 },
                { 150, 174, 221, 76, 164, 241, 57, 156, 248, 173, 226, 214, 99, 120, 60, 43, 247, 67, 16, 110, 51, 144, 89, 200, 56, 246, 94, 208, 146, 105, 179, 205, 94, 190, 128, 65, 170, 7, 122, 229, 22, 193, 162, 218, 15, 196, 46, 123, 29, 159, 73, 217, 84, 255, 13, 53, 130, 244, 174, 230, 43, 220, 202, 86 },
                { 245, 22, 93, 131, 33, 114, 95, 190, 2, 84, 140, 40, 9, 146, 179, 194, 130, 210, 80, 191, 219, 25, 119, 41, 139, 163, 11, 70, 239, 219, 124, 57, 162, 44, 228, 152, 97, 56, 209, 156, 180, 66, 42, 85, 253, 71, 143, 103, 224, 248, 113, 178, 44, 124, 224, 104, 150, 216, 23, 81, 185, 101, 137, 6 },
                { 111, 210, 52, 229, 180, 17, 213, 45, 131, 236, 63, 188, 206, 81, 232, 93, 28, 163, 121, 253, 152, 70, 237, 187, 80, 215, 116, 43, 169, 6, 34, 77, 253, 12, 113, 29, 239, 199, 38, 83, 131, 3, 232, 112, 27, 167, 214, 1, 188, 53, 96, 148, 4, 196, 161, 75, 189, 7, 119, 58, 159, 17, 234, 67 },
                { 185, 158, 11, 193, 65, 247, 144, 72, 204, 26, 122, 105, 255, 158, 52, 12, 223, 141, 40, 6, 92, 207, 172, 1, 31, 102, 252, 142, 191, 91, 234, 197, 135, 182, 87, 211, 138, 16, 116, 248, 98, 212, 146, 201, 182, 59, 131, 82, 153, 17, 207, 241, 63, 91, 238, 32, 205, 96, 144, 250, 198, 126, 169, 39 },
                { 254, 98, 139, 121, 86, 170, 107, 160, 92, 183, 168, 15, 70, 33, 117, 174, 106, 62, 233, 183, 55, 132, 107, 158, 230, 198, 59, 23, 128, 64, 111, 151, 21, 222, 53, 166, 74, 177, 191, 62, 30, 172, 52, 121, 92, 238, 38, 246, 171, 69, 34, 128, 184, 23, 117, 49, 168, 67, 222, 34, 88, 51, 214, 78 },
                { 29, 57, 206, 233, 40, 21, 221, 7, 250, 54, 228, 152, 198, 133, 215, 245, 75, 204, 169, 101, 22, 244, 44, 66, 124, 88, 180, 226, 157, 212, 175, 47, 102, 68, 127, 246, 106, 46, 226, 158, 136, 242, 77, 9, 155, 19, 105, 198, 118, 225, 142, 105, 229, 153, 215, 138, 246, 17, 130, 176, 229, 3, 116, 148 },
                { 129, 173, 6, 75, 153, 199, 59, 117, 34, 138, 80, 43, 242, 87, 20, 186, 149, 9, 128, 81, 220, 194, 140, 213, 16, 148, 49, 8, 81, 32, 249, 0, 231, 205, 155, 27, 4, 147, 88, 12, 109, 219, 41, 186, 228, 208, 138, 49, 21, 190, 85, 167, 14, 56, 80, 101, 187, 42, 111, 74, 156, 103, 240, 195 },
                { 90, 225, 111, 244, 178, 127, 238, 190, 210, 100, 218, 2, 112, 165, 56, 98, 35, 48, 249, 156, 114, 34, 177, 78, 250, 166, 110, 241, 99, 199, 123, 143, 88, 172, 39, 194, 217, 125, 255, 204, 24, 194, 96, 128, 61, 164, 77, 234, 97, 59, 212, 42, 243, 201, 175, 0, 230, 150, 210, 192, 25, 61, 180, 16 },
                { 69, 35, 144, 50, 99, 28, 88, 70, 151, 173, 125, 65, 181, 140, 200, 232, 119, 214, 191, 18, 68, 236, 7, 97, 200, 39, 219, 184, 138, 55, 72, 187, 16, 115, 79, 236, 99, 66, 181, 79, 57, 166, 148, 251, 31, 114, 7, 176, 149, 253, 9, 133, 71, 114, 31, 126, 64, 88, 12, 247, 142, 220, 45, 208 },
                { 249, 158, 189, 216, 15, 137, 164, 47, 10, 22, 193, 235, 31, 222, 14, 70, 163, 142, 60, 91, 168, 146, 121, 57, 133, 19, 65, 119, 13, 230, 161, 213, 241, 58, 137, 177, 51, 160, 33, 134, 240, 118, 1, 71, 216, 90, 189, 221, 32, 121, 162, 184, 94, 226, 157, 252, 204, 166, 53, 98, 122, 82, 164, 134 },
                { 118, 0, 82, 63, 202, 253, 185, 228, 109, 246, 91, 146, 48, 103, 128, 84, 242, 1, 108, 227, 201, 45, 216, 187, 238, 154, 90, 207, 174, 44, 25, 95, 36, 153, 223, 6, 119, 210, 16, 222, 92, 175, 50, 197, 139, 243, 45, 131, 70, 106, 204, 22, 144, 195, 47, 106, 22, 137, 217, 35, 173, 231, 28, 95 },
                { 236, 175, 224, 125, 103, 39, 76, 215, 132, 57, 201, 77, 159, 253, 209, 28, 175, 188, 37, 132, 254, 26, 85, 107, 164, 30, 247, 76, 147, 107, 252, 132, 202, 109, 22, 249, 88, 193, 149, 107, 40, 231, 211, 23, 105, 154, 18, 166, 237, 51, 82, 246, 61, 6, 86, 179, 77, 240, 112, 185, 68, 10, 193, 55 },
                { 108, 43, 23, 167, 148, 8, 118, 154, 27, 168, 38, 121, 178, 6, 62, 154, 95, 223, 54, 76, 153, 176, 4, 70, 222, 51, 128, 190, 2, 218, 81, 169, 65, 184, 75, 166, 45, 233, 71, 186, 11, 158, 76, 124, 181, 62, 84, 193, 2, 215, 175, 36, 219, 130, 237, 149, 40, 192, 3, 131, 249, 153, 205, 143 },
                { 213, 73, 198, 243, 85, 230, 179, 65, 96, 209, 240, 19, 219, 111, 195, 43, 135, 117, 208, 12, 101, 124, 233, 141, 201, 15, 101, 231, 60, 117, 195, 48, 12, 125, 206, 101, 139, 25, 127, 245, 54, 141, 98, 247, 35, 227, 208, 96, 118, 137, 153, 100, 114, 165, 207, 15, 223, 60, 161, 90, 47, 104, 79, 18 },
                { 255, 156, 135, 57, 31, 204, 48, 248, 1, 187, 136, 69, 90, 143, 233, 79, 248, 21, 164, 243, 184, 59, 194, 34, 114, 172, 151, 40, 180, 23, 156, 235, 224, 146, 244, 35, 220, 62, 174, 86, 115, 206, 191, 5, 169, 52, 142, 251, 29, 64, 233, 9, 190, 49, 69, 122, 102, 142, 200, 235, 29, 225, 126, 180 },
                { 93, 4, 120, 97, 187, 111, 137, 162, 78, 104, 226, 46, 165, 30, 183, 10, 203, 66, 145, 83, 217, 44, 159, 93, 252, 65, 208, 85, 243, 140, 99, 30, 89, 57, 1, 160, 113, 199, 7, 214, 163, 20, 66, 221, 129, 112, 11, 160, 180, 44, 198, 76, 255, 92, 27, 175, 245, 83, 19, 116, 168, 188, 61, 36 },
                { 165, 50, 223, 173, 12, 218, 21, 233, 125, 151, 14, 199, 251, 57, 102, 125, 171, 48, 110, 31, 134, 16, 238, 78, 8, 132, 226, 19, 124, 72, 205, 171, 115, 191, 80, 180, 93, 251, 151, 37, 236, 46, 94, 148, 79, 237, 201, 71, 90, 221, 126, 18, 157, 136, 231, 187, 37, 210, 54, 71, 216, 9, 147, 231 },
                { 69, 193, 240, 76, 147, 60, 90, 193, 37, 55, 177, 114, 131, 214, 157, 224, 92, 239, 196, 229, 98, 206, 119, 177, 215, 49, 96, 166, 197, 5, 255, 44, 135, 239, 216, 23, 131, 50, 103, 77, 123, 178, 254, 26, 187, 39, 103, 20, 242, 147, 105, 171, 209, 58, 111, 11, 162, 124, 150, 252, 134, 99, 206, 112 },
                { 141, 17, 33, 207, 127, 252, 169, 72, 212, 245, 92, 26, 82, 3, 71, 39, 18, 150, 181, 1, 167, 69, 143, 24, 155, 188, 35, 146, 55, 109, 220, 65, 19, 152, 39, 70, 233, 189, 15, 227, 136, 196, 109, 161, 215, 59, 170, 132, 189, 54, 33, 82, 227, 40, 145, 74, 195, 93, 0, 178, 42, 83, 25, 246 },
                { 125, 89, 107, 157, 45, 100, 29, 121, 5, 158, 203, 235, 145, 188, 244, 208, 116, 78, 61, 129, 46, 249, 192, 57, 105, 246, 116, 235, 178, 82, 158, 185, 98, 202, 122, 173, 145, 61, 166, 205, 28, 55, 0, 85, 121, 140, 231, 210, 3, 118, 249, 200, 8, 98, 246, 215, 50, 234, 220, 107, 199, 158, 56, 175 },
                { 41, 212, 235, 177, 7, 200, 227, 185, 106, 134, 65, 44, 167, 108, 54, 175, 140, 255, 23, 220, 90, 113, 36, 231, 83, 2, 70, 200, 15, 31, 126, 230, 9, 84, 250, 107, 10, 213, 115, 90, 156, 72, 223, 242, 14, 32, 75, 45, 87, 158, 176, 68, 133, 190, 168, 116, 20, 132, 33, 64, 241, 13, 226, 188 },
                { 2, 148, 61, 82, 136, 239, 53, 149, 82, 220, 17, 99, 226, 31, 126, 8, 193, 100, 160, 204, 185, 16, 148, 209, 127, 172, 219, 136, 93, 242, 143, 52, 214, 164, 47, 197, 32, 78, 247, 41, 235, 144, 102, 173, 205, 183, 152, 99, 238, 216, 17, 108, 151, 28, 86, 61, 181, 154, 78, 171, 143, 116, 95, 75 },
                { 167, 221, 192, 20, 115, 68, 165, 13, 40, 253, 176, 196, 76, 154, 237, 87, 44, 230, 30, 136, 76, 239, 164, 95, 47, 22, 157, 61, 43, 206, 181, 71, 118, 25, 140, 94, 224, 179, 132, 7, 184, 200, 126, 48, 64, 249, 112, 195, 26, 127, 56, 234, 42, 219, 239, 5, 207, 250, 91, 189, 27, 211, 134, 253 },
                { 103, 121, 48, 248, 182, 95, 217, 129, 192, 58, 117, 138, 22, 202, 60, 169, 216, 68, 109, 52, 122, 5, 64, 195, 227, 183, 102, 250, 113, 167, 6, 103, 244, 192, 64, 237, 150, 54, 162, 97, 67, 20, 34, 163, 90, 134, 6, 168, 69, 142, 182, 201, 78, 124, 162, 142, 104, 39, 123, 12, 233, 49, 66, 32 },
                { 144, 14, 90, 160, 36, 205, 24, 109, 232, 92, 159, 0, 245, 111, 94, 130, 16, 183, 153, 247, 176, 222, 141, 31, 74, 133, 10, 212, 28, 147, 81, 222, 38, 174, 124, 2, 111, 21, 209, 121, 221, 253, 110, 214, 17, 229, 53, 220, 37, 254, 93, 1, 171, 100, 50, 71, 192, 223, 56, 202, 109, 162, 182, 198 },
                { 238, 209, 227, 64, 140, 243, 153, 74, 172, 33, 212, 49, 179, 219, 38, 250, 144, 205, 9, 84, 211, 44, 106, 254, 118, 233, 86, 191, 67, 236, 197, 133, 15, 156, 86, 217, 74, 245, 194, 44, 84, 171, 143, 188, 77, 150, 117, 203, 81, 159, 120, 30, 212, 247, 14, 232, 26, 134, 168, 148, 74, 245, 8, 83 },
                { 53, 26, 170, 126, 3, 84, 50, 201, 9, 241, 132, 85, 69, 149, 25, 191, 77, 117, 35, 163, 96, 20, 150, 181, 56, 167, 40, 155, 122, 48, 95, 58, 254, 203, 46, 185, 167, 35, 137, 154, 10, 56, 235, 99, 40, 246, 178, 102, 13, 189, 228, 60, 135, 186, 154, 113, 177, 83, 6, 228, 41, 93, 154, 130 },
                { 180, 73, 98, 196, 111, 230, 184, 123, 62, 146, 106, 188, 231, 123, 165, 54, 103, 223, 63, 242, 129, 198, 79, 8, 203, 25, 138, 221, 0, 181, 164, 24, 114, 100, 144, 234, 61, 106, 91, 226, 181, 72, 131, 1, 210, 60, 22, 138, 240, 47, 107, 148, 75, 38, 89, 209, 62, 255, 99, 120, 214, 20, 206, 114 },
                { 37, 223, 147, 254, 43, 19, 166, 97, 222, 20, 207, 41, 10, 97, 211, 14, 236, 172, 140, 189, 52, 230, 160, 217, 92, 110, 239, 73, 103, 247, 214, 141, 229, 72, 8, 28, 130, 207, 13, 250, 112, 198, 30, 162, 121, 194, 91, 156, 68, 215, 9, 200, 237, 19, 225, 127, 46, 198, 32, 186, 136, 173, 63, 248 },
                { 160, 190, 11, 58, 135, 217, 151, 33, 248, 79, 174, 157, 254, 65, 196, 130, 42, 87, 26, 1, 109, 69, 30, 123, 246, 60, 172, 197, 131, 20, 64, 35, 188, 170, 216, 155, 241, 79, 172, 26, 50, 150, 222, 242, 80, 171, 226, 34, 129, 164, 88, 175, 123, 101, 168, 2, 142, 161, 239, 70, 50, 234, 104, 0 },
                { 92, 120, 79, 174, 199, 89, 70, 118, 191, 136, 55, 115, 30, 141, 81, 245, 155, 184, 120, 210, 252, 177, 143, 46, 185, 149, 16, 38, 89, 159, 205, 82, 125, 44, 89, 117, 54, 193, 146, 124, 212, 87, 103, 44, 141, 17, 51, 252, 112, 187, 27, 248, 42, 67, 195, 243, 80, 108, 14, 151, 87, 29, 196, 140 },
                { 46, 211, 232, 25, 106, 245, 13, 49, 232, 5, 94, 201, 222, 181, 4, 107, 217, 72, 233, 149, 39, 85, 101, 227, 4, 81, 211, 229, 53, 179, 112, 237, 18, 197, 250, 179, 3, 98, 39, 237, 63, 168, 6, 185, 68, 209, 100, 200, 2, 78, 222, 55, 139, 155, 217, 31, 58, 176, 205, 218, 126, 165, 225, 68 },
                { 18, 243, 155, 128, 37, 209, 183, 154, 212, 169, 73, 238, 45, 122, 163, 55, 34, 17, 97, 59, 165, 192, 22, 204, 134, 164, 106, 121, 143, 244, 4, 153, 98, 139, 66, 32, 224, 204, 73, 187, 138, 23, 249, 110, 228, 130, 152, 174, 63, 144, 119, 204, 8, 111, 183, 92, 121, 230, 23, 98, 6, 252, 111, 182 },
                { 145, 99, 52, 72, 168, 139, 60, 101, 128, 28, 110, 149, 20, 89, 230, 193, 145, 175, 203, 129, 11, 218, 116, 237, 54, 68, 255, 28, 190, 74, 40, 217, 56, 228, 163, 113, 133, 159, 107, 11, 219, 120, 201, 156, 36, 12, 87, 231, 43, 243, 160, 89, 234, 74, 24, 251, 149, 41, 136, 192, 78, 56, 38, 204 },
                { 171, 6, 194, 223, 113, 2, 227, 80, 41, 251, 186, 59, 172, 210, 68, 132, 247, 80, 110, 239, 47, 75, 157, 91, 36, 199, 176, 11, 222, 94, 169, 129, 184, 11, 78, 240, 19, 55, 254, 174, 91, 50, 76, 178, 58, 244, 122, 24, 192, 102, 35, 18, 189, 171, 132, 51, 201, 167, 64, 243, 180, 157, 130, 82 },
                { 28, 117, 252, 88, 178, 23, 242, 161, 200, 11, 220, 135, 242, 7, 103, 42, 26, 220, 3, 183, 138, 249, 174, 14, 127, 149, 100, 47, 137, 62, 208, 24, 105, 201, 45, 176, 212, 85, 36, 148, 230, 26, 132, 96, 217, 187, 71, 211, 135, 168, 218, 125, 64, 210, 98, 14, 225, 84, 105, 33, 119, 11, 239, 216 },
                { 232, 134, 59, 34, 144, 206, 95, 120, 67, 145, 83, 99, 34, 156, 202, 117, 167, 62, 155, 93, 30, 64, 107, 213, 186, 241, 81, 231, 160, 119, 251, 85, 233, 145, 124, 96, 152, 195, 116, 66, 207, 161, 238, 2, 142, 164, 46, 110, 8, 78, 52, 250, 146, 39, 240, 160, 116, 4, 210, 229, 140, 196, 94, 66 },
                { 43, 184, 162, 215, 77, 50, 190, 32, 234, 180, 48, 125, 189, 77, 255, 141, 88, 236, 210, 122, 199, 150, 225, 24, 56, 1, 206, 111, 26, 196, 5, 152, 37, 68, 220, 0, 29, 243, 138, 8, 186, 41, 107, 197, 30, 88, 255, 151, 180, 230, 94, 197, 5, 108, 76, 185, 58, 145, 172, 72, 22, 51, 165, 107 },
                { 147, 200, 8, 100, 245, 127, 154, 9, 109, 166, 22, 217, 232, 14, 55, 181, 19, 190, 51, 13, 231, 42, 83, 120, 142, 70, 169, 38, 182, 77, 54, 177, 114, 192, 249, 59, 183, 75, 225, 100, 126, 83, 248, 55, 118, 225, 18, 62, 202, 31, 118, 157, 176, 221, 139, 29, 195, 253, 43, 91, 184, 247, 207, 14 },
                { 125, 71, 237, 115, 21, 174, 60, 221, 253, 88, 199, 66, 112, 173, 128, 224, 36, 110, 78, 134, 176, 102, 190, 163, 252, 95, 219, 127, 245, 139, 215, 237, 91, 16, 166, 132, 108, 157, 50, 16, 216, 169, 151, 73, 176, 207, 129, 101, 240, 137, 68, 21, 84, 48, 236, 94, 123, 17, 216, 132, 153, 114, 81, 224 },
                { 27, 92, 151, 45, 225, 194, 138, 75, 39, 129, 157, 4, 144, 43, 95, 72, 239, 146, 165, 251, 62, 5, 237, 32, 202, 48, 153, 9, 62, 99, 13, 161, 128, 48, 80, 213, 36, 232, 176, 200, 63, 25, 228, 7, 137, 36, 81, 169, 0, 45, 185, 247, 211, 129, 10, 203, 68, 166, 104, 231, 1, 62, 37, 179 },
                { 255, 56, 209, 168, 86, 3, 101, 211, 182, 54, 235, 102, 245, 214, 195, 158, 0, 202, 96, 27, 213, 154, 74, 133, 16, 108, 188, 87, 227, 198, 110, 32, 203, 228, 148, 190, 7, 92, 115, 253, 145, 97, 193, 109, 184, 246, 51, 218, 192, 148, 228, 104, 164, 61, 151, 175, 245, 50, 79, 31, 189, 239, 139, 161 },
                { 7, 192, 130, 33, 67, 249, 122, 15, 148, 25, 206, 79, 32, 169, 20, 120, 63, 219, 45, 126, 194, 113, 90, 222, 178, 67, 234, 27, 173, 149, 51, 254, 71, 20, 102, 244, 65, 139, 26, 80, 37, 131, 47, 238, 14, 93, 159, 112, 74, 123, 89, 33, 15, 115, 227, 37, 110, 143, 208, 158, 121, 87, 214, 104 },
                { 228, 112, 176, 234, 142, 162, 198, 230, 91, 172, 117, 188, 133, 50, 87, 249, 139, 174, 84, 241, 11, 52, 170, 38, 246, 143, 117, 206, 41, 123, 84, 167, 138, 182, 120, 42, 170, 197, 156, 236, 209, 163, 70, 213, 146, 60, 230, 25, 11, 210, 56, 179, 206, 75, 191, 88, 5, 182, 224, 57, 11, 199, 45, 73 },
                { 149, 82, 18, 97, 49, 27, 111, 58, 41, 247, 69, 10, 220, 151, 107, 200, 12, 35, 106, 181, 226, 147, 207, 124, 2, 54, 161, 76, 13, 237, 189, 222, 2, 58, 208, 86, 224, 126, 52, 1, 181, 117, 21, 85, 172, 124, 201, 136, 253, 167, 234, 131, 147, 250, 47, 136, 236, 20, 128, 94, 251, 171, 133, 25 },
                { 203, 55, 218, 188, 241, 75, 180, 221, 158, 140, 97, 239, 61, 180, 228, 75, 234, 155, 59, 134, 72, 19, 101, 186, 85, 213, 97, 250, 136, 62, 25, 96, 113, 241, 152, 30, 14, 249, 94, 67, 104, 226, 195, 250, 32, 103, 43, 187, 65, 100, 42, 3, 85, 23, 160, 100, 197, 73, 168, 39, 113, 66, 184, 235 },
                { 164, 116, 0, 126, 151, 209, 18, 84, 123, 3, 194, 163, 18, 125, 41, 24, 186, 118, 208, 253, 31, 163, 238, 65, 230, 22, 194, 179, 108, 215, 159, 202, 37, 175, 133, 73, 185, 111, 217, 167, 18, 138, 56, 153, 4, 223, 78, 161, 29, 150, 199, 119, 184, 222, 62, 212, 119, 29, 244, 145, 218, 17, 100, 35 },
                { 138, 252, 69, 171, 40, 103, 135, 252, 33, 214, 48, 112, 204, 93, 145, 167, 97, 48, 5, 193, 92, 116, 46, 140, 155, 37, 129, 49, 5, 146, 74, 127, 52, 90, 231, 211, 161, 44, 147, 201, 232, 41, 91, 129, 179, 205, 244, 116, 89, 219, 242, 71, 105, 238, 36, 176, 153, 53, 204, 77, 191, 157, 242, 86 },
                { 215, 23, 192, 90, 228, 12, 202, 170, 70, 182, 235, 80, 30, 254, 213, 65, 243, 221, 141, 77, 171, 218, 197, 7, 105, 171, 226, 89, 241, 33, 229, 180, 248, 21, 105, 4, 60, 84, 130, 32, 76, 186, 238, 110, 68, 50, 139, 24, 174, 8, 135, 53, 169, 13, 134, 88, 1, 229, 105, 131, 8, 51, 124, 61 },
                { 179, 108, 48, 144, 244, 64, 115, 51, 145, 101, 129, 154, 57, 175, 2, 84, 128, 159, 109, 16, 57, 244, 127, 80, 251, 203, 70, 118, 165, 191, 102, 15, 67, 166, 196, 142, 242, 190, 10, 251, 120, 158, 9, 216, 166, 14, 98, 234, 61, 188, 38, 209, 151, 196, 113, 255, 67, 186, 24, 91, 173, 227, 200, 12 },
                { 234, 159, 211, 122, 30, 163, 86, 196, 219, 22, 9, 225, 188, 136, 115, 196, 19, 36, 237, 184, 152, 40, 27, 181, 59, 15, 150, 25, 55, 80, 137, 209, 153, 221, 124, 38, 113, 225, 100, 175, 63, 208, 83, 29, 255, 194, 152, 204, 123, 81, 251, 95, 21, 76, 47, 218, 144, 163, 240, 211, 41, 110, 151, 79 },
                { 39, 98, 8, 75, 223, 187, 5, 239, 42, 161, 247, 74, 95, 41, 233, 52, 170, 204, 63, 96, 213, 135, 112, 208, 96, 138, 223, 178, 216, 251, 7, 116, 49, 86, 26, 75, 170, 53, 213, 21, 149, 46, 103, 142, 119, 37, 73, 227, 17, 108, 159, 216, 125, 233, 181, 99, 38, 118, 58, 137, 71, 251, 29, 133 },
            });

        // ReSharper restore InconsistentNaming
        #endregion

        #region Instance Properties

        bool IDitherer.InitializeReliesOnContent => false;

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDitherer"/> class using the specified <paramref name="matrix"/> and <paramref name="strength"/>.
        /// </summary>
        /// <param name="matrix">A matrix to be used as the coefficients of the dithering. Ideally contains every value between zero
        /// and the maximum value in the matrix. Repeated values will appear always together for the same input colors.</param>
        /// <param name="strength">The strength of the dithering effect between 0 and 1 (inclusive bounds).
        /// Specify 0 to use an auto value for each dithering session based on the used quantizer.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ConfigureStrength">ConfigureStrength</see> method for details about dithering strength. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="matrix"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="matrix"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="strength"/> must be between 0 and 1, inclusive bounds.</exception>
        /// <example>
        /// The following example demonstrates how to use a custom ditherer using the <see cref="OrderedDitherer"/> constructor.
        /// It produces a similar dotted halftone pattern to the result of the <see cref="DottedHalftone"/> property but in a rectangular
        /// arrangement and with less different patterns:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToCustomDithered(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     // Using a dotted halftone pattern. As it uses only 11 values in a 7x7 matrix it is much less optimal
        ///     // than the DottedHalftone property but demonstrates the behavior of the ordered dithering quite well.
        ///     byte[,] matrix =
        ///     {
        ///        {  0,  2,  4,  5,  4,  2,  1 },
        ///        {  2,  3,  6,  7,  6,  3,  2 },
        ///        {  4,  6,  8,  9,  8,  6,  4 },
        ///        {  5,  7,  9, 10,  9,  7,  5 },
        ///        {  4,  6,  8,  9,  8,  6,  4 },
        ///        {  2,  3,  6,  7,  6,  3,  2 },
        ///        {  1,  2,  4,  5,  4,  2,  1 },
        ///     };
        ///
        ///     IDitherer ditherer = new OrderedDitherer(matrix);
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
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredOC.gif" alt="Color hues with system default 8 BPP palette, using silver background and a custom dotted halftone dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredOC.gif" alt="Grayscale color shades with black and white palette using a custom dotted halftone dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// <note type="tip"><list type="bullet">
        /// <item>Use the static properties to perform dithering with predefined patterns.</item>
        /// <item>See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer"/> class for more details and image examples.</item>
        /// </list></note>
        /// </example>
        public OrderedDitherer(byte[,] matrix, float strength = 0f)
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

            // matrix.Max() cannot be used so using explicit loop
            foreach (byte b in matrix)
            {
                if (b <= shades)
                    continue;

                shades = b;
                if (shades == Byte.MaxValue)
                    break;
            }

            // adding two levels for total black and white
            shades += 2;

            // Elements in premultiplied matrix are between -127..127
            premultipliedMatrix = new sbyte[matrixHeight, matrixWidth];
            matrixMinValue = SByte.MaxValue;
            matrixMaxValue = SByte.MinValue;
            for (int y = 0; y < matrixHeight; y++)
            {
                for (int x = 0; x < matrixWidth; x++)
                {
                    // +1 for separating total black from the first pattern, -127 for balancing brightness level
                    sbyte value = (sbyte)((matrix[y, x] + 1) * 255 / shades - 127);
                    premultipliedMatrix[y, x] = value;
                    if (value < matrixMinValue)
                        matrixMinValue = value;
                    if (value > matrixMaxValue)
                        matrixMaxValue = value;
                }
            }
        }

        #endregion

        #region Private Constructors

        private OrderedDitherer(OrderedDitherer original, float strength, AutoStrengthMode autoStrengthMode)
        {
            if (Single.IsNaN(strength) || strength < 0f || strength > 1f)
                throw new ArgumentOutOfRangeException(nameof(strength), PublicResources.ArgumentMustBeBetween(0, 1));
            if (!autoStrengthMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(autoStrengthMode), PublicResources.EnumOutOfRange(autoStrengthMode));
            this.strength = strength;
            this.autoStrengthMode = autoStrengthMode;
            premultipliedMatrix = original.premultipliedMatrix;
            matrixWidth = original.matrixWidth;
            matrixHeight = original.matrixHeight;
            matrixMinValue = original.matrixMinValue;
            matrixMaxValue = original.matrixMaxValue;
        }

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets a new <see cref="OrderedDitherer"/> instance that has the specified dithering <paramref name="strength"/>.
        /// </summary>
        /// <param name="strength">The strength of the dithering effect between 0 and 1 (inclusive bounds).
        /// Specify 0 to use an auto value for each dithering session based on the used quantizer.
        /// The auto strength strategy can be specified by the <see cref="ConfigureAutoStrengthMode">ConfigureAutoStrengthMode</see> method.</param>
        /// <returns>A new <see cref="OrderedDitherer"/> instance that has the specified dithering <paramref name="strength"/>.</returns>
        /// <remarks>
        /// <note>This method always returns a new <see cref="OrderedDitherer"/> instance instead of changing the strength of the original one.
        /// This is required for the static properties so they can return a cached instance.</note>
        /// <para>If <paramref name="strength"/> is too low, then banding may appear in the result in place of gradients in the original image;
        /// whereas if <paramref name="strength"/> is too high, then dithering patterns may appear even in colors without quantization error (overdithering).</para>
        /// <para>If <paramref name="strength"/> is 0, then strength will be calibrated for each dithering session so that neither the black, nor the white colors will suffer from overdithering in the result.
        /// This is the default for <see cref="OrderedDitherer"/> instances returned by the static properties.</para>
        /// <para>The auto strength strategy itself can be specified by the <see cref="ConfigureAutoStrengthMode">ConfigureAutoStrengthMode</see> method.</para>
        /// <para>The following table demonstrates the effect of different strengths:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesDefault4bpp.gif" alt="Grayscale color shades with system default 4 BPP palette"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault4BppPalette">system default 4 BPP palette</see>, no dithering. The asymmetry is due to the uneven distribution of gray shades of this palette.</para>
        /// <para><img src="../Help/Images/GrayShadesDefault4bppDitheredB8.gif" alt="Grayscale color shades with system default 4 BPP palette using Bayer 8x8 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault4BppPalette">system default 4 BPP palette</see> and <see cref="Bayer8x8">Bayer 8x8</see> dithering using auto strength. Darker shades have banding.</para>
        /// <para><img src="../Help/Images/GrayShadesDefault4bppDitheredB8Str-5.gif" alt="Grayscale color shades with system default 4 BPP palette using a stronger Bayer 8x8 ordered dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault4BppPalette">system default 4 BPP palette</see> and <see cref="Bayer8x8">Bayer 8x8</see> dithering using strength = 0.5. Now there is no banding but white suffers from overdithering.</para>
        /// <para><img src="../Help/Images/GrayShadesDefault4bppDitheredB8Interpolated.gif" alt="Grayscale color shades with system default 4 BPP palette using 8x8 ordered dithering with interpolated ato strength"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault4BppPalette">system default 4 BPP palette</see> and <see cref="Bayer8x8">Bayer 8x8</see> dithering using <see cref="AutoStrengthMode.Interpolated"/> auto strength strategy.
        /// Now there is neither banding nor overdithering for black or white colors.</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to specify the strength for a predefined ordered ditherer:
        /// <code lang="C#"><![CDATA[
        /// // getting a predefined ditherer with custom strength:
        /// IDitherer ditherer = OrderedDitherer.Bayer8x8.ConfigureStrength(0.5f);
        /// 
        /// // getting a predefined ditherer with custom auto strength strategy:
        /// ditherer = OrderedDitherer.Bayer8x8.ConfigureAutoStrengthMode(AutoStrengthMode.Interpolated);
        /// ]]></code>
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="strength"/> must be between 0 and 1, inclusive bounds.</exception>
        [SuppressMessage("ReSharper", "ParameterHidesMember", Justification = "No conflict, a new instance is created")]
        public OrderedDitherer ConfigureStrength(float strength) => new OrderedDitherer(this, strength, autoStrengthMode);

        /// <summary>
        /// Gets a new <see cref="OrderedDitherer"/> instance that uses auto strength using the specified <paramref name="autoStrengthMode"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="ConfigureStrength">ConfigureStrength</see> method for details and image examples.
        /// </summary>
        /// <param name="autoStrengthMode">An <see cref="AutoStrengthMode"/> value specifying the desired behavior for calibrating auto strength.</param>
        /// <returns>A new <see cref="OrderedDitherer"/> instance that has the specified <paramref name="autoStrengthMode"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="autoStrengthMode"/> is not one of the defined values.</exception>
        [SuppressMessage("ReSharper", "ParameterHidesMember", Justification = "No conflict, a new instance is created")]
        public OrderedDitherer ConfigureAutoStrengthMode(AutoStrengthMode autoStrengthMode) => new OrderedDitherer(this, 0f, autoStrengthMode);

        #endregion

        #region Explicitly Implemented Interface Methods

        [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract",
            Justification = "It CAN be null, just must no be. Null check is in the called ctor.")]
        IDitheringSession IDitherer.Initialize(IReadableBitmapData source, IQuantizingSession quantizingSession, IAsyncContext? context)
            => quantizingSession?.WorkingColorSpace == WorkingColorSpace.Linear
                ? new OrderedDitheringSessionLinear(quantizingSession, this)
                : new OrderedDitheringSessionSrgb(quantizingSession!, this);

        #endregion

        #endregion
    }
}
