#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RandomNoiseDitherer.cs
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

using KGySoft.CoreLibraries;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides an <see cref="IDitherer"/> implementation for random noise dithering. This class applies a random white noise to the result. For other noise-like
    /// ditherers see the <see cref="OrderedDitherer.BlueNoise">OrderedDitherer.BlueNoise</see> property and the <see cref="InterleavedGradientNoiseDitherer"/> class.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="RandomNoiseDitherer"/> generates some random white noise to the quantized source.</para>
    /// <note type="tip">There are also a couple of ditherers with non-random noise-like patterns in this library. See also
    /// the <see cref="OrderedDitherer.BlueNoise">OrderedDitherer.BlueNoise</see> property and the <see cref="InterleavedGradientNoiseDitherer"/> class.</note>
    /// <para>To get always the same result for the same source image and quantizer you can specify a <em>seed</em> when initializing a <see cref="RandomNoiseDitherer"/> instance.
    /// Please note though that specifying a seed prevents parallel processing, which makes performance worse on multi-core systems.</para>
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
    /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredRN.gif" alt="Color hues with system default 8 BPP palette, using silver background and random noise dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see> and random noise dithering</para></div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
    /// <br/>Grayscale color shades</para></div></td>
    /// <td><div style="text-align:center;">
    /// <para><img src="../Help/Images/GrayShadesBW.gif" alt="Grayscale color shades with black and white palette"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see>, no dithering</para>
    /// <para><img src="../Help/Images/GrayShadesBWDitheredRN.gif" alt="Grayscale color shades with black and white palette using random noise dithering"/>
    /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see> and random noise dithering</para></div></td>
    /// </tr>
    /// </tbody></table></para>
    /// </remarks>
    /// <seealso cref="IDitherer" />
    /// <seealso cref="OrderedDitherer" />
    /// <seealso cref="ErrorDiffusionDitherer" />
    /// <seealso cref="InterleavedGradientNoiseDitherer" />
    public sealed class RandomNoiseDitherer : IDitherer
    {
        #region Nested Classes
        
        #region RandomNoiseDitheringSessionSrgb class

        private sealed class RandomNoiseDitheringSessionSrgb : VariableStrengthDitheringSessionSrgbBase
        {
            #region Fields

            private readonly Random random;

            #endregion

            #region Properties

            public override bool IsSequential { get; }

            #endregion

            #region Constructors

            internal RandomNoiseDitheringSessionSrgb(IQuantizingSession quantizingSession, RandomNoiseDitherer ditherer)
                : base(quantizingSession)
            {
                // if we have a seed we need to produce a consistent result
                IsSequential = ditherer.seed.HasValue;

                // If we have don't have a seed, we must use a thread safe random generator because pixels can be queried in any order
                random = ditherer.seed == null ? ThreadSafeRandom.Instance : new FastRandom(ditherer.seed.Value);

                if (ditherer.strength > 0f)
                {
                    Strength = ditherer.strength;
                    return;
                }

                Strength = CalibrateStrength(-127, 127, ditherer.autoStrengthMode == AutoStrengthMode.Interpolated);
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

        #region RandomNoiseDitheringSessionLinear class

        private sealed class RandomNoiseDitheringSessionLinear : VariableStrengthDitheringSessionLinearBase
        {
            #region Fields

            private readonly Random random;

            #endregion

            #region Properties

            public override bool IsSequential { get; }

            #endregion

            #region Constructors

            internal RandomNoiseDitheringSessionLinear(IQuantizingSession quantizingSession, RandomNoiseDitherer ditherer)
                : base(quantizingSession)
            {
                // if we have a seed we need to produce a consistent result
                IsSequential = ditherer.seed.HasValue;

                // If we have don't have a seed, we must use a thread safe random generator because pixels can be queried in any order
                random = ditherer.seed == null ? ThreadSafeRandom.Instance : new FastRandom(ditherer.seed.Value);

                if (ditherer.strength > 0f)
                {
                    Strength = ditherer.strength;
                    return;
                }

                Strength = CalibrateStrength(MinOffset, MaxOffset, ditherer.autoStrengthMode != AutoStrengthMode.Constant);
            }

            #endregion

            #region Methods

#if NET6_0_OR_GREATER
            protected override float GetOffset(int x, int y) => random.NextSingle() * (MaxOffset - MinOffset) + MinOffset;
#else
            protected override float GetOffset(int x, int y) => (float)random.NextDouble() * (MaxOffset - MinOffset) + MinOffset;
#endif

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly int? seed;
        private readonly float strength;
        private readonly AutoStrengthMode autoStrengthMode;

        #endregion

        #region Properties

        bool IDitherer.InitializeReliesOnContent => false;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomNoiseDitherer"/> class.
        /// </summary>
        /// <param name="strength">The strength of the dithering effect between 0 and 1 (inclusive bounds).
        /// Specify 0 to use an auto value for each dithering session based on the used quantizer.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer.ConfigureStrength">OrderedDitherer.ConfigureStrength</see> method
        /// for more details and some examples regarding dithering strength.
        /// The same applies also for the <see cref="RandomNoiseDitherer"/> class. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <param name="seed">If <see langword="null"/>, then a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_CoreLibraries_ThreadSafeRandom.htm">ThreadSafeRandom</a>
        /// instance will be used internally with a time-dependent seed value, and the dithering session will allow parallel processing.
        /// If not <see langword="null"/>, then a <see cref="Random"/> instance will be created for each dithering session with the specified <paramref name="seed"/>, and the dithering session will not allow parallel processing.</param>
        /// <example>
        /// The following example demonstrates how to use the <see cref="RandomNoiseDitherer"/> class.
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDitheredRandomNoise(IReadWriteBitmapData source, IQuantizer quantizer)
        /// {
        ///     IDitherer ditherer = new RandomNoiseDitherer();
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
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverDitheredRN.gif" alt="Color hues with system default 8 BPP palette, using silver background and random noise dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP palette</see></para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBWDitheredRN.gif" alt="Grayscale color shades with black and white palette using random noise dithering"/>
        /// <br/>Quantizing with <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white palette</see></para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="strength"/> must be between 0 and 1, inclusive bounds.</exception>
        public RandomNoiseDitherer(float strength = 0f, int? seed = null)
        {
            if (Single.IsNaN(strength) || strength < 0f || strength > 1f)
                throw new ArgumentOutOfRangeException(nameof(strength), PublicResources.ArgumentMustBeBetween(0, 1));
            this.strength = strength;
            this.seed = seed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomNoiseDitherer"/> class with a specific auto strength strategy.
        /// </summary>
        /// <param name="autoStrengthMode">An <see cref="AutoStrengthMode"/> value specifying the desired behavior for calibrating auto strength.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer.ConfigureStrength">OrderedDitherer.ConfigureStrength</see> method
        /// for more details and some examples regarding dithering strength. The same applies also for the <see cref="RandomNoiseDitherer"/> class.</param>
        /// <param name="seed">If <see langword="null"/>, then a <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_CoreLibraries_ThreadSafeRandom.htm">ThreadSafeRandom</a>
        /// instance will be used internally with a time-dependent seed value, and the dithering session will allow parallel processing.
        /// If not <see langword="null"/>, then a <see cref="Random"/> instance will be created for each dithering session with the specified <paramref name="seed"/>, and the dithering session will not allow parallel processing.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="autoStrengthMode"/> is not one of the defined values.</exception>
        public RandomNoiseDitherer(AutoStrengthMode autoStrengthMode, int? seed = null)
        {
            if (!autoStrengthMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(autoStrengthMode), PublicResources.EnumOutOfRange(autoStrengthMode));
            this.autoStrengthMode = autoStrengthMode;
            this.seed = seed;
        }

        #endregion

        #region Methods

        [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract",
            Justification = "It CAN be null, just must no be. Null check is in the called ctor.")]
        IDitheringSession IDitherer.Initialize(IReadableBitmapData source, IQuantizingSession quantizer, IAsyncContext? context)
            => quantizer?.WorkingColorSpace == WorkingColorSpace.Linear
                ? new RandomNoiseDitheringSessionLinear(quantizer, this)
                : new RandomNoiseDitheringSessionSrgb(quantizer!, this);

        #endregion
    }
}
