#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RandomNoiseDitherer.cs
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
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides an <see cref="IDitherer"/> implementation for random noise dithering. This class applies a random white noise to the result. For other noise-like
    /// ditherers see the <see cref="OrderedDitherer.BlueNoise">OrderedDitherer.BlueNoise</see> property and the <see cref="InterleavedGradientNoiseDitherer"/> class.
    /// <br/>See the <strong>Remarks</strong> section for details and some examples.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="RandomNoiseDitherer"/> generates some random white noise to the quantized source.</para>
    /// <note type="tip">There are also a couple of ditherers with non-random noise-like patterns in this library. See also
    /// the <see cref="OrderedDitherer.BlueNoise">OrderedDitherer.BlueNoise</see> property and the <see cref="InterleavedGradientNoiseDitherer"/> class.</note>
    /// <para>To get always the same result for the same source image and quantizer you can specify a <em>seed</em> when initializing a <see cref="RandomNoiseDitherer"/> instance.
    /// Please note though that specifying a seed prevents parallel processing, which makes performance worse on multi-core systems.</para>
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
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredRN.gif" alt="Color hues with system default 8 BPP palette, using silver background and random noise dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see> and random noise dithering</para></div></term>
    /// </item>
    /// <item>
    /// <term><div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
    /// <br/>Grayscale color shades</para></div></term>
    /// <term>
    /// <div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/GrayShadesBW.gif" alt="Grayscale color shades with black and white palette"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see>, no dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredRN.gif" alt="Grayscale color shades with black and white palette using random noise dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see> and random noise dithering</para></div></term>
    /// </item>
    /// </list></para>
    /// </remarks>
    /// <seealso cref="IDitherer" />
    /// <seealso cref="OrderedDitherer" />
    /// <seealso cref="ErrorDiffusionDitherer" />
    /// <seealso cref="InterleavedGradientNoiseDitherer" />
    public sealed class RandomNoiseDitherer : IDitherer
    {
        #region RandomNoiseDitheringSession class

        private sealed class RandomNoiseDitheringSession : VariableStrengthDitheringSessionBase
        {
            #region Fields

            private readonly RandomNoiseDitherer ditherer;
            private readonly Random random;

            #endregion

            #region Properties

            // if we have a seed we need to produce a consistent result
            public override bool IsSequential => ditherer.seed.HasValue;

            #endregion

            #region Constructors

            internal RandomNoiseDitheringSession(IQuantizingSession quantizingSession, RandomNoiseDitherer ditherer)
                : base(quantizingSession)
            {
                this.ditherer = ditherer;

                // If we have don't have a seed, we must use a thread safe random generator because pixels can be queried in any order
                random = ditherer.seed == null ? new ThreadSafeRandom() : new Random(ditherer.seed.Value);

                if (ditherer.strength > 0f)
                {
                    Strength = ditherer.strength;
                    return;
                }

                CalibrateStrength(-127, 127);
            }

            #endregion

            #region Methods

            protected override sbyte GetOffset(int x, int y)
            {
                // generating random numbers between -127 and 127 so completely white/black pixels will not change
                return random.NextSByte(-127, 127, true);
            }

            #endregion
        }

        #endregion

        #region Fields

        #region Instance Fields

        private readonly int? seed;
        private readonly float strength;

        #endregion

        #endregion

        #region Properties

        bool IDitherer.InitializeReliesOnContent => false;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomNoiseDitherer"/> class.
        /// <br/>See the <strong>Examples</strong> section for some examples.
        /// </summary>
        /// <param name="strength">The strength of the dithering effect between 0 and 1 (inclusive bounds).
        /// Specify 0 to use an auto value for each dithering session based on the used quantizer.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer"/> class for more details and some examples regarding dithering strength.
        /// The same applies also for the <see cref="RandomNoiseDitherer"/> class. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="seed">If <see langword="null"/>, then a <a href="https://docs.kgysoft.net/corelibraries/?topic=html/T_KGySoft_CoreLibraries_ThreadSafeRandom.htm" target="_blank">ThreadSafeRandom</a>
        /// instance will be used internally with a time-dependent seed value, and the dithering session will allow parallel processing.
        /// If not <see langword="null"/>, then a <see cref="Random"/> instance will be created for each dithering session with the specified <paramref name="seed"/>, and the dithering session will not allow parallel processing.</param>
        /// <example>
        /// The following example demonstrates how to use the <see cref="RandomNoiseDitherer"/> class.
        /// <code lang="C#"><![CDATA[
        /// public static Bitmap ToDitheredRandomNoise(Bitmap source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = new RandomNoiseDitherer();
        ///
        ///     // a.) this solution returns a new bitmap and does not change the original one:
        ///     return source.ConvertPixelFormat(quantizer.PixelFormatHint, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the dithering directly on the source bitmap:
        ///     source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <list type="table">
        /// <listheader><term>Original image</term><term>Quantized and dithered image</term></listheader>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredRN.gif" alt="Color hues with system default 8 BPP palette, using silver background and random noise dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para></div></term>
        /// </item>
        /// <item>
        /// <term><div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></term>
        /// <term>
        /// <div style="text-align:center;width:512px">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredRN.gif" alt="Grayscale color shades with black and white palette using random noise dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see></para></div></term>
        /// </item>
        /// </list></para>
        /// </example>
        public RandomNoiseDitherer(float strength = 0f, int? seed = null)
        {
            if (Single.IsNaN(strength) || strength < 0f || strength > 1f)
                throw new ArgumentOutOfRangeException(nameof(strength), PublicResources.ArgumentMustBeBetween(0, 1));
            this.strength = strength;
            this.seed = seed;
        }

        #endregion

        #region Methods

        IDitheringSession IDitherer.Initialize(IReadableBitmapData source, IQuantizingSession quantizer)
            => new RandomNoiseDitheringSession(quantizer, this);

        #endregion
    }
}
