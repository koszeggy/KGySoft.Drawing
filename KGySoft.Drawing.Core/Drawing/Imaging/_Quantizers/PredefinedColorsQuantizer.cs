#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PredefinedColorsQuantizer.cs
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
#if NET35 || NET40
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endif
using System.Drawing;
using System.Linq;
using System.Threading;

#if NET35 || NET40
using KGySoft.Collections; 
#endif
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a quantizer with predefined set of colors. Use the static members to retrieve an instance.
    /// For using optimized colors for a specific source image see the <see cref="OptimizedPaletteQuantizer"/> class.
    /// <br/>See the <strong>Remarks</strong> section of the static methods of this class for details and image examples.
    /// </summary>
    /// <seealso cref="IQuantizer" />
    /// <seealso cref="OptimizedPaletteQuantizer"/>
    /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer?, IDitherer?)"/>
    /// <seealso cref="BitmapDataExtensions.Quantize(IReadWriteBitmapData, IQuantizer)"/>
    public sealed class PredefinedColorsQuantizer : IQuantizer
    {
        #region Nested classes

        #region QuantizingSessionCustomMapping class

        private sealed class QuantizingSessionCustomMapping : IQuantizingSession
        {
            #region Fields

            private readonly PredefinedColorsQuantizer quantizer;
            private readonly Func<Color32, Color32> quantizingFunction;

            #endregion

            #region Properties

            public Palette? Palette => null;
            public Color32 BackColor => quantizer.BackColor;
            public byte AlphaThreshold => quantizer.AlphaThreshold;
            public WorkingColorSpace WorkingColorSpace => quantizer.WorkingColorSpace;
            public bool IsGrayscale => quantizer.PixelFormatHint.IsGrayscale();

            #endregion

            #region Constructors

            internal QuantizingSessionCustomMapping(PredefinedColorsQuantizer quantizer, Func<Color32, Color32> quantizingFunction)
            {
                this.quantizer = quantizer;
                this.quantizingFunction = quantizingFunction;
            }

            #endregion

            #region Methods

            public void Dispose()
            {
            }

            public Color32 GetQuantizedColor(Color32 c)
                => c.A == Byte.MaxValue || !quantizer.blendAlphaBeforeQuantize && c.A >= AlphaThreshold ? quantizingFunction.Invoke(c)
                    : c.A < AlphaThreshold ? default
                    : quantizingFunction.Invoke(c.BlendWithBackground(BackColor, quantizer.WorkingColorSpace));

            #endregion
        }

        #endregion

        #region QuantizingSessionIndexed class

        private sealed class QuantizingSessionIndexed : IQuantizingSession
        {
            #region Fields

            private readonly PredefinedColorsQuantizer quantizer;

            #endregion

            #region Properties

            public Palette Palette { get; }
            public Color32 BackColor => quantizer.BackColor;
            public byte AlphaThreshold => quantizer.AlphaThreshold;
            public WorkingColorSpace WorkingColorSpace => quantizer.WorkingColorSpace;
            public bool IsGrayscale => Palette.IsGrayscale;

            #endregion

            #region Constructors

            internal QuantizingSessionIndexed(PredefinedColorsQuantizer quantizer, Palette palette)
            {
                this.quantizer = quantizer;
                Palette = palette;
            }

            #endregion

            #region Methods

            public void Dispose()
            {
            }

            public Color32 GetQuantizedColor(Color32 c) => Palette.GetNearestColor(c);

            #endregion
        }

        #endregion

        #region QuantizingSessionByCustomBitmapData class

        private sealed class QuantizingSessionByCustomBitmapData : IQuantizingSession
        {
            #region Fields

            #region Static Fields

#if NET35 || NET40
            private static readonly LockFreeCacheOptions cacheOptions = new()
            {
                InitialCapacity = Environment.ProcessorCount,
                ThresholdCapacity = Environment.ProcessorCount,
                HashingStrategy = HashingStrategy.Modulo,
            };
#endif

            #endregion

            #region Instance Fields

            private readonly PredefinedColorsQuantizer quantizer;
#if NET35 || NET40
            private readonly IThreadSafeCacheAccessor<int, IBitmapDataRowInternal> rowsCache;
            private readonly List<IBitmapData> bitmapDataList;
#else
            private readonly ThreadLocal<IBitmapDataRowInternal> rowsCache;
#endif

            #endregion

            #endregion

            #region Properties

            public Palette? Palette => null;
            public Color32 BackColor => quantizer.BackColor;
            public byte AlphaThreshold => quantizer.AlphaThreshold;
            public WorkingColorSpace WorkingColorSpace => quantizer.WorkingColorSpace;
            public bool IsGrayscale => quantizer.isGrayscale;

            #endregion

            #region Constructors

#if NET35 || NET40
            [SuppressMessage("VisualStudio.Style", "IDE0039: Use local function instead of lambda", Justification = "False alarm, it would be converted to a delegate anyway.")]
            [SuppressMessage("ReSharper", "ConvertToLocalFunction", Justification = "False alarm, it would be converted to a delegate anyway.")]
#endif
            internal QuantizingSessionByCustomBitmapData(PredefinedColorsQuantizer quantizer, Func<Size, WorkingColorSpace, IBitmapDataInternal> compatibleBitmapDataFactory)
            {
                this.quantizer = quantizer;
#if NET35 || NET40
                bitmapDataList = new List<IBitmapData>(Environment.ProcessorCount);
                Func<int, IBitmapDataRowInternal> createRowFactory = _ =>
                {
                    var result = compatibleBitmapDataFactory.Invoke(new Size(1, 1), WorkingColorSpace).GetRowUncached(0);
                    lock (bitmapDataList)
                        bitmapDataList.Add(result.BitmapData);
                    return result;
                };
                rowsCache = ThreadSafeCacheFactory.Create(createRowFactory, cacheOptions);
#else
                rowsCache = new ThreadLocal<IBitmapDataRowInternal>(
                    () => compatibleBitmapDataFactory.Invoke(new Size(1, 1), WorkingColorSpace).GetRowUncached(0), true);
#endif
            }

            #endregion

            #region Methods

            public Color32 GetQuantizedColor(Color32 origColor)
            {
#if NET35 || NET40
                IBitmapDataRowInternal row = rowsCache[EnvironmentHelper.CurrentThreadId];
#else
                IBitmapDataRowInternal row = rowsCache.Value!;
#endif
                row.DoSetColor32(0, origColor);
                return row.DoGetColor32(0);
            }

            public void Dispose()
            {
#if NET35 || NET40
                foreach (IBitmapData bitmapData in bitmapDataList)
                    bitmapData.Dispose();
#else
                foreach (IBitmapDataRowInternal row in rowsCache.Values)
                    row.BitmapData.Dispose();
                rowsCache.Dispose();
#endif
            }

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly Func<Color32, Color32>? quantizingFunction;
        private readonly Func<Size, WorkingColorSpace, IBitmapDataInternal>? compatibleBitmapDataFactory;
        private readonly bool blendAlphaBeforeQuantize;
        private readonly bool isGrayscale;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets a <see cref="KnownPixelFormat"/> that is compatible with this <see cref="PredefinedColorsQuantizer"/> instance.
        /// If this <see cref="PredefinedColorsQuantizer"/> was not initialized with custom color mapping logic,
        /// then this is the possible lowest bits-per-pixel value format.
        /// </summary>
        public KnownPixelFormat PixelFormatHint { get; }

        /// <summary>
        /// Gets the back color used by this <see cref="PredefinedColorsQuantizer"/>. This value will be returned also by
        /// the <see cref="IQuantizingSession.BackColor"/> property once an <see cref="IQuantizingSession"/> is created from this instance.
        /// The <see cref="Color32.A"/> field of the returned color is always 255.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="IQuantizingSession.AlphaThreshold">IQuantizingSession.AlphaThreshold</see> property for details.
        /// </summary>
        public Color32 BackColor { get; }

        /// <summary>
        /// Gets the alpha threshold value used by this <see cref="PredefinedColorsQuantizer"/>. This value will be returned also by
        /// the <see cref="IQuantizingSession.AlphaThreshold"/> property once an <see cref="IQuantizingSession"/> is created from this instance.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="IQuantizingSession.AlphaThreshold">IQuantizingSession.AlphaThreshold</see> property for details.
        /// </summary>
        public byte AlphaThreshold { get; }

        /// <summary>
        /// If this <see cref="PredefinedColorsQuantizer"/> is associated with a specific palette, then returns the same <see cref="Imaging.Palette"/> that will be returned also by
        /// the <see cref="IQuantizingSession.Palette"/> property once an <see cref="IQuantizingSession"/> is created from this instance;
        /// otherwise, returns <see langword="null"/>.
        /// </summary>
        public Palette? Palette { get; }

        /// <summary>
        /// Gets the preferred color space of this <see cref="PredefinedColorsQuantizer"/> instance for quantizing. This value will be returned also by
        /// the <see cref="IQuantizingSession.WorkingColorSpace"/> property once an <see cref="IQuantizingSession"/> is created from this instance.
        /// You can use the <see cref="ConfigureColorSpace">ConfigureColorSpace</see> method to create a clone of this <see cref="PredefinedColorsQuantizer"/>
        /// using a different working color space.
        /// </summary>
        /// <remarks>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Imaging.WorkingColorSpace"/> enumeration for details and
        /// image examples about using the different color spaces in various operations.</note>
        /// <para>If the value of this property is <see cref="Imaging.WorkingColorSpace.Default"/>, then the sRGB color space is used
        /// because the <see cref="IQuantizingSession.GetQuantizedColor">IQuantizingSession.GetQuantizedColor</see> method works with sRGB colors anyway.</para>
        /// <para>If this <see cref="PredefinedColorsQuantizer"/> instance uses a custom quantizing functions, then it depends on the function whether it
        /// considers the value of this property. When using a high color quantizer, then the value of this property may only affect possible alpha blending
        /// with the <see cref="BackColor"/> property.</para>
        /// </remarks>
        public WorkingColorSpace WorkingColorSpace { get; }

        #endregion

        #region Explicitly Implemented Interface Properties

        bool IQuantizer.InitializeReliesOnContent => false;

        #endregion

        #endregion

        #region Constructors

        private PredefinedColorsQuantizer(Palette palette)
        {
            Palette = palette ?? throw new ArgumentNullException(nameof(palette), PublicResources.ArgumentNull);
            BackColor = palette.BackColor;
            AlphaThreshold = palette.AlphaThreshold;
            WorkingColorSpace = palette.WorkingColorSpace;
            isGrayscale = palette.IsGrayscale;
            PixelFormatHint = palette.Count switch
            {
                > 256 => palette.HasAlpha ? KnownPixelFormat.Format32bppArgb : KnownPixelFormat.Format24bppRgb,
                > 16 => KnownPixelFormat.Format8bppIndexed,
                > 2 => KnownPixelFormat.Format4bppIndexed,
                _ => KnownPixelFormat.Format1bppIndexed
            };
        }

        private PredefinedColorsQuantizer(Func<Color32, Color32> quantizingFunction, KnownPixelFormat pixelFormatHint, Color32 backColor, byte alphaThreshold = 0, bool blend = true)
            : this(quantizingFunction, pixelFormatHint)
        {
            BackColor = backColor.ToOpaque();
            AlphaThreshold = alphaThreshold;
            blendAlphaBeforeQuantize = blend;
        }

        private PredefinedColorsQuantizer(Func<Color32, Color32> quantizingFunction, KnownPixelFormat pixelFormatHint)
        {
            this.quantizingFunction = quantizingFunction ?? throw new ArgumentNullException(nameof(quantizingFunction), PublicResources.ArgumentNull);
            if (!pixelFormatHint.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormatHint), Res.PixelFormatInvalid(pixelFormatHint));
            PixelFormatHint = pixelFormatHint;
        }

        private PredefinedColorsQuantizer(ICustomBitmapData customBitmapData)
        {
            compatibleBitmapDataFactory = customBitmapData.CreateCompatibleBitmapDataFactory;
            isGrayscale = customBitmapData.IsGrayscale();
            PixelFormatHint = customBitmapData.HasAlpha() ? KnownPixelFormat.Format32bppArgb : KnownPixelFormat.Format24bppRgb;
            BackColor = customBitmapData.BackColor;
            AlphaThreshold = customBitmapData.AlphaThreshold;
            WorkingColorSpace = customBitmapData.WorkingColorSpace;
        }

        private PredefinedColorsQuantizer(PredefinedColorsQuantizer original, WorkingColorSpace workingColorSpace)
        {
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            quantizingFunction = original.quantizingFunction;
            compatibleBitmapDataFactory = original.compatibleBitmapDataFactory;
            blendAlphaBeforeQuantize = original.blendAlphaBeforeQuantize;
            isGrayscale = original.isGrayscale;
            PixelFormatHint = original.PixelFormatHint;
            BackColor = original.BackColor;
            AlphaThreshold = original.AlphaThreshold;
            Palette = original.Palette == null ? null
                : original.Palette.WorkingColorSpace == workingColorSpace ? original.Palette
                : new Palette(original.Palette, workingColorSpace, BackColor, AlphaThreshold);
            WorkingColorSpace = workingColorSpace;
        }

        #endregion

        #region Methods

        #region Static Methods

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to the 32-bit ARGB color space.
        /// </summary>
        /// <param name="backColor">Determines the <see cref="IQuantizingSession.BackColor"/> property of the returned quantizer.
        /// Considering that this quantizer can return alpha colors it has effect only when the returned quantizer is used with
        /// a ditherer that does not support partial transparency.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color
        /// is considered completely transparent. If 0, then the quantized colors will preserve their original alpha value. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to the 32-bit ARGB color space.</returns>
        /// <remarks>
        /// <para>If <paramref name="alphaThreshold"/> is zero, then the returned <see cref="PredefinedColorsQuantizer"/> instance is practically just a pass-through filter in the 32-bit color space
        /// and it is effective only for some bitmap data operations (eg. <see cref="BitmapDataExtensions.Clone(IReadableBitmapData,Rectangle,KnownPixelFormat,IQuantizer,IDitherer)">Clone</see>),
        /// which could possibly preserve wide color information (<see cref="KnownPixelFormat"/>s with 48/64 bpp) without specifying a quantizer.</para>
        /// <para>If <paramref name="alphaThreshold"/> is not zero, then every partially transparent pixel with lower <see cref="Color.A">Color.A</see> value than the threshold will turn completely transparent.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format32bppArgb"/> pixel format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToArgb8888(IReadWriteBitmapData source, Color backColor = default,
        ///     byte alphaThreshold = 128, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Argb8888(backColor, alphaThreshold);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format32bppArgb, quantizer, ditherer);
        ///
        ///     // b.) when converting to Format32bppArgb format without dithering, this produces the same result:
        ///     if (ditherer == null && alphaThreshold == 0)
        ///         return source.Clone(KnownPixelFormat.Format32bppArgb);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientArgb8888BlackA128.png" alt="Color hues with ARGB8888 pixel format, black background and default alpha threshold"/>
        /// <br/>Default optional parameter values (black background, alpha threshold = 128). The top-half of the image preserved the original transparency,
        /// while bottom half turned completely transparent. Without dithering the back color is irrelevant.</para>
        /// <para><img src="../Help/Images/AlphaGradientArgb8888SilverA1.png" alt="Color hues with ARGB8888 pixel format, silver background and alpha threshold = 1"/>
        /// <br/>Silver background, alpha threshold = 1. Only the bottom line is completely transparent, otherwise the image preserved its original transparency,
        /// so the result is practically the same as the original image. Without dithering the back color is irrelevant.</para>
        /// <para><img src="../Help/Images/AlphaGradientArgb8888SilverDitheredA1.png" alt="Color hues with ARGB8888 pixel format, silver background, alpha threshold = 1, using Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, alpha threshold = 1, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering.
        /// As dithering does not support partial transparency only the bottom line is transparent, otherwise the image was blended with back color.
        /// No dithering pattern appeared in the result due to the auto <see cref="OrderedDitherer.ConfigureStrength">strength</see> calibration.
        /// This also demonstrates why dithering is practically useless for true color results.</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldArgb8888lackA128.png" alt="Shield icon with ARGB8888 pixel format, black background and default alpha threshold"/>
        /// <br/>Default optional parameter values (black background, alpha threshold = 128). Without dithering the back color is irrelevant but pixels with alpha &lt; 128 turned completely transparent.</para>
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with ARGB8888 pixel format, silver background and alpha threshold = 1"/>
        /// <br/>Silver background, alpha threshold = 1. Practically the same as the original image. Without dithering the back color is irrelevant.</para>
        /// <para><img src="../Help/Images/ShieldArgb8888SilverA1Dithered.png" alt="Shield icon with ARGB8888 pixel format, silver background, alpha threshold = 1, using Floyd-Steinberg dithering"/>
        /// <br/>Silver background, alpha threshold = 1, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering.
        /// As dithering does not support partial transparency alpha pixels were blended with back color. No dithering pattern appeared in the result as there was no quantization error during the process.
        /// This also demonstrates why dithering is practically useless for true color results.</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        public static PredefinedColorsQuantizer Argb8888(Color backColor = default, byte alphaThreshold = 128)
        {
            Color32 Quantize(Color32 c) => c;

            return new PredefinedColorsQuantizer(Quantize, KnownPixelFormat.Format32bppArgb, new Color32(backColor), alphaThreshold, false);
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 24-bit ones where each color component is encoded in 8 bits.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 24-bit ones where each color component is encoded in 8 bits.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 256<sup>3</sup> (16,777,216) colors.
        /// It practically just removes transparency and does not change colors without alpha.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format24bppRgb"/> pixel format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToRgb888(IReadWriteBitmapData source, Color backColor = default)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Rgb888(backColor);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format24bppRgb, quantizer);
        ///
        ///     // b.) when converting to Format24bppRgb format, this produces the same result:
        ///     return source.Clone(KnownPixelFormat.Format24bppRgb, backColor);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     source.Quantize(quantizer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientRgb888Black.png" alt="Color hues with black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb888Silver.png" alt="Color hues with silver background"/>
        /// <br/>Silver background</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldRgb888Black.png" alt="Shield icon with black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/ShieldRgb888Silver.png" alt="Shield icon with silver background"/>
        /// <br/>Silver background</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        public static PredefinedColorsQuantizer Rgb888(Color backColor = default)
        {
            // just returning the already blended color
            static Color32 Quantize(Color32 c) => c;

            return new PredefinedColorsQuantizer(Quantize, KnownPixelFormat.Format24bppRgb, new Color32(backColor));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where red,
        /// green and blue components are encoded in 5, 6 and 5 bits, respectively.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where red,
        /// green and blue components are encoded in 5, 6 and 5 bits, respectively.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 65,536 colors.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format16bppRgb565"/> pixel format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToRgb565(IReadWriteBitmapData source, Color backColor = default, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Rgb565(backColor);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format16bppRgb565, quantizer, ditherer);
        ///
        ///     // b.) when converting to Format16bppRgb565 format without dithering, this produces the same result:
        ///     if (ditherer == null)
        ///         return source.Clone(KnownPixelFormat.Format16bppRgb565, backColor);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientRgb565Black.png" alt="Color hues with RGB565 pixel format and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb565Silver.png" alt="Color hues with RGB565 pixel format and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb565SilverDithered.png" alt="Color hues with RGB565 pixel format, silver background and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldRgb565Black.png" alt="Shield icon with RGB565 pixel format and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/ShieldRgb565Silver.png" alt="Shield icon with RGB565 pixel format and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/ShieldRgb565SilverDithered.png" alt="Shield icon with RGB565 pixel format, silver background and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        public static PredefinedColorsQuantizer Rgb565(Color backColor = default)
        {
            static Color32 Quantize(Color32 c) => new Color16Rgb565(c).ToColor32();

            return new PredefinedColorsQuantizer(Quantize, KnownPixelFormat.Format16bppRgb565, new Color32(backColor));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where each color component is encoded in 5 bits.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where each color component is encoded in 5 bits.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 32,768 colors.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format16bppRgb555"/> pixel format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToRgb555(IReadWriteBitmapData source, Color backColor = default, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Rgb555(backColor);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format16bppRgb555, quantizer, ditherer);
        ///
        ///     // b.) when converting to Format16bppRgb555 format without dithering, this produces the same result:
        ///     if (ditherer == null)
        ///         return source.Clone(KnownPixelFormat.Format16bppRgb555, backColor);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientRgb555Black.png" alt="Color hues with RGB555 pixel format and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb555Silver.png" alt="Color hues with RGB555 pixel format and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb555SilverDithered.png" alt="Color hues with RGB555 pixel format, silver background and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldRgb555Black.png" alt="Shield icon with RGB555 pixel format and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/ShieldRgb555Silver.png" alt="Shield icon with RGB555 pixel format and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/ShieldRgb555SilverDithered.png" alt="Shield icon with RGB555 pixel format, silver background and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        public static PredefinedColorsQuantizer Rgb555(Color backColor = default)
        {
            static Color32 Quantize(Color32 c) => new Color16Rgb555(c).ToColor32();

            return new PredefinedColorsQuantizer(Quantize, KnownPixelFormat.Format16bppRgb555, new Color32(backColor));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where alpha, red,
        /// green and blue components are encoded in 1, 5, 5 and 5 bits, respectively.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency), whose <see cref="Color.A">Color.A</see> property
        /// is equal to or greater than <paramref name="alphaThreshold"/> will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If 0, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 16-bit ones where each color component is encoded in 5 bits.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 32,768 colors, and a transparent color.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format16bppArgb1555"/> pixel format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToArgb1555(IReadWriteBitmapData source, Color backColor = default,
        ///     byte alphaThreshold = 128, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Argb1555(backColor, alphaThreshold);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format16bppArgb1555, quantizer, ditherer);
        ///
        ///     // b.) when converting to Format16bppArgb1555 format without dithering, this produces the same result:
        ///     if (ditherer == null)
        ///         return source.Clone(KnownPixelFormat.Format16bppArgb1555, backColor, alphaThreshold);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientArgb1555BlackA128.png" alt="Color hues with ARGB1555 pixel format, black background and default alpha threshold"/>
        /// <br/>Default optional parameter values (black background, alpha threshold = 128). The bottom half of the image is transparent.</para>
        /// <para><img src="../Help/Images/AlphaGradientArgb1555SilverA1.png" alt="Color hues with ARGB1555 pixel format, silver background and alpha threshold = 1"/>
        /// <br/>Silver background, alpha threshold = 1. Only the bottom line is transparent.</para>
        /// <para><img src="../Help/Images/AlphaGradientArgb1555SilverDithered.png" alt="Color hues with ARGB1555 pixel format, silver background, default alpha threshold and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, default alpha threshold, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering. The bottom half of the image is transparent.</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldArgb1555BlackA128.png" alt="Shield icon with ARGB1555 pixel format, black background and default alpha threshold"/>
        /// <br/>Default optional parameter values (black background, alpha threshold = 128)</para>
        /// <para><img src="../Help/Images/ShieldArgb1555SilverA1.png" alt="Shield icon with ARGB1555 pixel format, silver background and alpha threshold = 1"/>
        /// <br/>Silver background, alpha threshold = 1</para>
        /// <para><img src="../Help/Images/ShieldArgb1555SilverA128Dithered.png" alt="Shield icon with ARGB1555 pixel format, silver background, default alpha threshold and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, default alpha threshold, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        public static PredefinedColorsQuantizer Argb1555(Color backColor = default, byte alphaThreshold = 128)
        {
            static Color32 Quantize(Color32 c) => new Color16Argb1555(c).ToColor32();

            return new PredefinedColorsQuantizer(Quantize, KnownPixelFormat.Format16bppArgb1555, new Color32(backColor), alphaThreshold);
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 8-bit ones where red,
        /// green and blue components are encoded in 3, 3 and 2 bits, respectively.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/> to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but without dithering may end up in a noticeably poorer result and higher contrast;
        /// <see langword="false"/> to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 8-bit ones where red,
        /// green and blue components are encoded in 3, 3 and 2 bits, respectively.</returns>
        /// <remarks>
        /// <para>If <paramref name="directMapping"/> is <see langword="true"/>, then the result of the quantization may have a higher contrast than without direct color mapping,
        /// though this can be compensated if the returned quantizer is combined with an <see cref="ErrorDiffusionDitherer"/>. Other ditherers preserve the effect of the <paramref name="directMapping"/> parameter.</para>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 256 colors.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format8bppIndexed"/> pixel format.</para>
        /// <para>The palette of this quantizer does not contain the transparent color.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToRgb332(IReadWriteBitmapData source, Color backColor = default, bool directMapping = false, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Rgb332(backColor, directMapping);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format8bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientRgb332Black.gif" alt="Color hues with RGB332 palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (black background, nearest color lookup)</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb332Silver.gif" alt="Color hues with RGB332 palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb332SilverDM.gif" alt="Color hues with RGB332 palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb332SilverDMDitheredB8.gif" alt="Color hues with RGB332 palette, silver background, using direct color mapping and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, direct color mapping, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesRgb332.gif" alt="Grayscale color shades with RGB332 palette using nearest color lookup"/>
        /// <br/>Nearest color lookup</para>
        /// <para><img src="../Help/Images/GrayShadesRgb332Direct.gif" alt="Grayscale color shades with RGB332 palette using direct color mapping"/>
        /// <br/>Direct color mapping</para>
        /// <para><img src="../Help/Images/GrayShadesRgb332DitheredB8.gif" alt="Grayscale color shades with RGB332 palette, using nearest color lookup and Bayer 8x8 ordered dithering"/>
        /// <br/>Nearest color lookup, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para>
        /// <para><img src="../Help/Images/GrayShadesRgb332DirectDitheredB8.gif" alt="Grayscale color shades with RGB332 palette, using direct color mapping and Bayer 8x8 ordered dithering"/>
        /// <br/>Direct color mapping, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldRgb332Black.gif" alt="Shield icon with RGB332 palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (nearest color lookup)</para>
        /// <para><img src="../Help/Images/ShieldRgb332Silver.gif" alt="Shield icon with RGB332 palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/ShieldRgb332SilverDM.gif" alt="Shield icon with RGB332 palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/ShieldRgb332SilverDMDithered.gif" alt="Shield icon with RGB332 palette, silver background, using direct color mapping and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, direct color mapping, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Lena.png" alt="Test image &quot;Lena&quot;"/>
        /// <br/>Original test image "Lena"</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/LenaRgb332.gif" alt="Test image &quot;Lena&quot; with RGB332 palette using nearest color lookup"/>
        /// <br/>Nearest color lookup</para>
        /// <para><img src="../Help/Images/LenaRgb332DM.gif" alt="Test image &quot;Lena&quot; with RGB332 palette using direct color mapping"/>
        /// <br/>Direct color mapping</para>
        /// <para><img src="../Help/Images/LenaRgb332DMFloydSteinberg.gif" alt="Test image &quot;Lena&quot; with RGB332 palette using direct color mapping and Floyd-Steinberg dithering"/>
        /// <br/>Direct color mapping, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        /// <seealso cref="O:KGySoft.Drawing.Imaging.Palette.Rgb332">Palette.Rgb332 Methods</seealso>
        public static PredefinedColorsQuantizer Rgb332(Color backColor = default, bool directMapping = false)
            => new PredefinedColorsQuantizer(Palette.Rgb332(new Color32(backColor), directMapping));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 8-bit grayscale ones of 256 shades.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 8-bit grayscale ones.</returns>
        /// <remarks>
        /// <para>The returned quantizer uses direct mapping to grayscale colors based on human perception, which makes quantization very fast while it is very accurate at the same time.</para>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 256 possible shades of gray.</para>
        /// <para>The palette of this quantizer does not contain the transparent color. To make a bitmap data grayscale with transparency you can use the
        /// <see cref="BitmapDataExtensions.ToGrayscale">ToGrayscale</see> and <see cref="BitmapDataExtensions.MakeGrayscale">MakeGrayscale</see> extension methods.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format8bppIndexed"/> pixel format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToGrayscale(IReadWriteBitmapData source, Color backColor = default)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Grayscale(backColor);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format8bppIndexed, quantizer);
        ///     
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     source.Quantize(quantizer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientGray8bppBlack.gif" alt="Grayscale color hues with 8 BPP grayscale palette and black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/AlphaGradientGray8bppSilver.gif" alt="Graayscale color hues with 8 BPP grayscale palette and silver background"/>
        /// <br/>Silver background</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldGray8bppBlack.gif" alt="Shield icon with 8 BPP grayscale palette and black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/ShieldGray8bppSilver.gif" alt="Shield icon with 8 BPP grayscale palette and silver background"/>
        /// <br/>Silver background</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        /// <seealso cref="O:KGySoft.Drawing.Imaging.Palette.Grayscale256">Palette.Grayscale256 Methods</seealso>
        public static PredefinedColorsQuantizer Grayscale(Color backColor = default) => new PredefinedColorsQuantizer(Palette.Grayscale256(new Color32(backColor)));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 4-bit grayscale ones of 16 shades.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/> to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but may end up in a result of a bit higher contrast than the original image;
        /// <see langword="false"/> to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 4-bit grayscale ones.</returns>
        /// <remarks>
        /// <para>If <paramref name="directMapping"/> is <see langword="true"/>, then the result of the quantization may have a higher contrast than without direct color mapping,
        /// though this can be compensated if the returned quantizer is combined with an <see cref="ErrorDiffusionDitherer"/>. Other ditherers preserve the effect of the <paramref name="directMapping"/> parameter.</para>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 16 possible shades of gray.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format4bppIndexed"/> pixel format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToGrayscale16(IReadWriteBitmapData source, Color backColor = default, bool directMapping = false, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Grayscale16(backColor, directMapping);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format4bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientGray4bppBlack.gif" alt="Color hues with 4 BPP grayscale palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (black background, nearest color lookup)</para>
        /// <para><img src="../Help/Images/AlphaGradientGray4bppSilver.gif" alt="Color hues with 4 BPP grayscale palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/AlphaGradientGray4bppSilverDirect.gif" alt="Color hues with 4 BPP grayscale palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/AlphaGradientGray4bppSilverDitheredB8.gif" alt="Color hues with 4 BPP grayscale palette, silver background, using nearest color lookup and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, nearest color lookup, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades4bpp.gif" alt="Grayscale color shades with 4 BPP grayscale palette using nearest color lookup"/>
        /// <br/>Nearest color lookup</para>
        /// <para><img src="../Help/Images/GrayShades4bppDirect.gif" alt="Grayscale color shades with 2 BPP grayscale palette using direct color mapping"/>
        /// <br/>Direct color mapping</para>
        /// <para><img src="../Help/Images/GrayShades4bppDitheredB8.gif" alt="Grayscale color shades with 4 BPP grayscale palette, using nearest color lookup and Bayer 8x8 ordered dithering"/>
        /// <br/>Nearest color lookup, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldGray4bppBlack.gif" alt="Shield icon with 4 BPP grayscale palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (nearest color lookup)</para>
        /// <para><img src="../Help/Images/ShieldGray4bppSilver.gif" alt="Shield icon with 4 BPP grayscale palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/ShieldGray4bppSilverDirect.gif" alt="Shield icon with 4 BPP grayscale palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/ShieldGray4bppSilverDirectDitheredFS.gif" alt="Shield icon with 4 BPP grayscale palette, silver background, using direct color mapping and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, direct color mapping, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        /// <seealso cref="O:KGySoft.Drawing.Imaging.Palette.Grayscale16">Palette.Grayscale16 Methods</seealso>
        public static PredefinedColorsQuantizer Grayscale16(Color backColor = default, bool directMapping = false)
            => new PredefinedColorsQuantizer(Palette.Grayscale16(new Color32(backColor), directMapping));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 2-bit grayscale ones of 4 shades.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="directMapping"><see langword="true"/> to map any color directly to an index instead of searching for a nearest color,
        /// which is very fast but may end up in a result of a bit higher contrast than the original image;
        /// <see langword="false"/> to perform a lookup to determine nearest colors, which may be slower but more accurate. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 2-bit grayscale ones.</returns>
        /// <remarks>
        /// <para>If <paramref name="directMapping"/> is <see langword="true"/>, then the result of the quantization may have a higher contrast than without direct color mapping,
        /// though this can be compensated if the returned quantizer is combined with an <see cref="ErrorDiffusionDitherer"/>. Other ditherers preserve the effect of the <paramref name="directMapping"/> parameter.</para>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 4 possible shades of gray.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format4bppIndexed"/> pixel format, though only 4 palette entries are used instead of the possible maximum of 16.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToGrayscale4(IReadWriteBitmapData source, Color backColor = default, bool directMapping = false, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.Grayscale4(backColor, directMapping);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format4bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientGray2bppBlack.gif" alt="Color hues with 2 BPP grayscale palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (black background, nearest color lookup)</para>
        /// <para><img src="../Help/Images/AlphaGradientGray2bppSilver.gif" alt="Color hues with 2 BPP grayscale palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/AlphaGradientGray2bppSilverDirect.gif" alt="Color hues with 2 BPP grayscale palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/AlphaGradientGray2bppSilverDitheredB8.gif" alt="Color hues with 2 BPP grayscale palette, silver background, using nearest color lookup and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, nearest color lookup, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades2bpp.gif" alt="Grayscale color shades with 2 BPP grayscale palette using nearest color lookup"/>
        /// <br/>Nearest color lookup</para>
        /// <para><img src="../Help/Images/GrayShades2bppDirect.gif" alt="Grayscale color shades with 2 BPP grayscale palette using direct color mapping"/>
        /// <br/>Direct color mapping</para>
        /// <para><img src="../Help/Images/GrayShades2bppDitheredB8.gif" alt="Grayscale color shades with 2 BPP grayscale palette, using nearest color lookup and Bayer 8x8 ordered dithering"/>
        /// <br/>Nearest color lookup, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldGray2bppBlack.gif" alt="Shield icon with 2 BPP grayscale palette and black background using nearest color lookup"/>
        /// <br/>Default optional parameter values (nearest color lookup)</para>
        /// <para><img src="../Help/Images/ShieldGray2bppSilver.gif" alt="Shield icon with 2 BPP grayscale palette and silver background using nearest color lookup"/>
        /// <br/>Silver background, nearest color lookup</para>
        /// <para><img src="../Help/Images/ShieldGray2bppSilverDirect.gif" alt="Shield icon with 2 BPP grayscale palette and silver background using direct color mapping"/>
        /// <br/>Silver background, direct color mapping</para>
        /// <para><img src="../Help/Images/ShieldGray2bppSilverDirectDitheredFS.gif" alt="Shield icon with 2 BPP grayscale palette, silver background, using direct color mapping and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, direct color mapping, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Cameraman.png" alt="Test image &quot;Cameraman&quot;"/>
        /// <br/>Original test image "Cameraman"</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Cameraman2bpp.gif" alt="Test image &quot;Cameraman&quot; with 2 BPP grayscale palette using nearest color lookup"/>
        /// <br/>Nearest color lookup</para>
        /// <para><img src="../Help/Images/Cameraman2bppDirect.gif" alt="Test image &quot;Cameraman&quot; with 2 BPP grayscale palette using direct color mapping"/>
        /// <br/>Direct color mapping</para>
        /// <para><img src="../Help/Images/Cameraman2bppDirectDitheredFS.gif" alt="Test image &quot;Cameraman&quot; with 2 BPP grayscale palette using direct color mapping and Floyd-Steinberg dithering"/>
        /// <br/>Direct color mapping, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        /// <seealso cref="O:KGySoft.Drawing.Imaging.Palette.Grayscale4">Palette.Grayscale4 Methods</seealso>
        public static PredefinedColorsQuantizer Grayscale4(Color backColor = default, bool directMapping = false)
            => new PredefinedColorsQuantizer(Palette.Grayscale4(new Color32(backColor), directMapping));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes every color to black or white.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with the specified <paramref name="backColor"/> before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="whiteThreshold">Specifies a threshold value for the brightness of the colors, under which a quantized color is considered black.
        /// If 0, then the complete result will be white. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes every color to black or white.</returns>
        /// <remarks>
        /// <para>If the returned quantizer is combined with an <see cref="ErrorDiffusionDitherer"/>, then the effect of the <paramref name="whiteThreshold"/> parameter is
        /// mostly compensated. Other ditherers preserve the effect of the <paramref name="whiteThreshold"/> parameter.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format1bppIndexed"/> pixel format.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToBlackAndWhite(IReadWriteBitmapData source, Color backColor = default,
        ///     byte whiteThreshold = 128, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.BlackAndWhite(backColor, whiteThreshold);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format1bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientBWBlack.gif" alt="Color hues with black and white palette and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/AlphaGradientBWSilver.gif" alt="Color hues with black and white palette and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/AlphaGradientBWSilverDitheredB8.gif" alt="Color hues with black and white palette, silver background, using Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBW.gif" alt="Grayscale color shades with black and white palette"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/GrayShadesBWThr32.gif" alt="Grayscale color shades with black and white palette, white threshold = 32"/>
        /// <br/>White threshold = 32</para>
        /// <para><img src="../Help/Images/GrayShadesBWThr224.gif" alt="Grayscale color shades with black and white palette, white threshold = 224"/>
        /// <br/>White threshold = 224</para>
        /// <para><img src="../Help/Images/GrayShadesBWDitheredB8.gif" alt="Grayscale color shades with black and white palette, using Bayer 8x8 ordered dithering"/>
        /// <br/>Default white threshold, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldBWBlack.gif" alt="Shield icon with black and white palette and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/ShieldBWSilver.gif" alt="Shield icon with black and white palette and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/ShieldBWSilverDitheredFS.gif" alt="Shield icon with black and white palette, silver background, using Floyd-Steinberg dithering"/>
        /// <br/>Silver background, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Cameraman.png" alt="Test image &quot;Cameraman&quot;"/>
        /// <br/>Original test image "Cameraman"</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/CameramanBW.gif" alt="Test image &quot;Cameraman&quot; with black and white palette"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/CameramanBWThr96.gif" alt="Test image &quot;Cameraman&quot; with black and white palette, white threshold = 96"/>
        /// <br/>White threshold = 96</para>
        /// <para><img src="../Help/Images/CameramanBWThr96DitheredB8.gif" alt="Test image &quot;Cameraman&quot; with black and white palette, using Bayer 8x8 dithering and white threshold = 96"/>
        /// <br/>White threshold = 96, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering. The ordered dithering preserves the white threshold value.</para>
        /// <para><img src="../Help/Images/CameramanBWThr96DitheredFS.gif" alt="Test image &quot;Cameraman&quot; with black and white palette, using Floyd-Steinberg dithering and white threshold = 96"/>
        /// <br/>White threshold = 96, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering. The error diffusion dithering compensates the white threshold value.</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        /// <seealso cref="O:KGySoft.Drawing.Imaging.Palette.BlackAndWhite">Palette.BlackAndWhite Methods</seealso>
        public static PredefinedColorsQuantizer BlackAndWhite(Color backColor = default, byte whiteThreshold = 128)
            => new PredefinedColorsQuantizer(Palette.BlackAndWhite(new Color32(backColor), whiteThreshold));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 8-bit palette.
        /// This palette contains the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>,
        /// the "web-safe" palette of 216 colors as well as 24 transparent entries.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency), which are considered opaque will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If the system default 8-bit palette contains a transparent color on the current operating system,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If 0, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 8-bit palette.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 256 colors.
        /// Actually this amount is somewhat smaller because of some redundant entries in the palette.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format8bppIndexed"/> pixel format.</para>
        /// <para>The palette of this quantizer contains transparent entries.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDefault8Bpp(IReadWriteBitmapData source, Color backColor = default,
        ///     byte alphaThreshold = 128, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.SystemDefault8BppPalette(backColor, alphaThreshold);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format8bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) when converting to Format8bppIndexed format without dithering, this produces the same result:
        ///     if (ditherer == null)
        ///         return source.Clone(KnownPixelFormat.Format8bppIndexed, backColor, alphaThreshold);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppBlackA128.gif" alt="Color hues with system default 8 BPP palette, black background and default alpha threshold"/>
        /// <br/>Default optional parameter values (black background, alpha threshold = 128). The bottom half of the image is transparent.</para>
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverA1.gif" alt="Color hues with system default 8 BPP palette, silver background and alpha threshold = 1"/>
        /// <br/>Silver background, alpha threshold = 1. Only the bottom line is transparent.</para>
        /// <para><img src="../Help/Images/AlphaGradientDefault8bppSilverA128DitheredB8.gif" alt="Color hues with system default 8 BPP palette, silver background, default alpha threshold and Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, default alpha threshold, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering. The bottom half of the image is transparent.</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesDefault8bpp.gif" alt="Grayscale color shades with system default 8 BPP palette"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/GrayShadesDefault8bppDitheredB8.gif" alt="Grayscale color shades with system default 8 BPP palette using Bayer 8x8 ordered dithering"/>
        /// <br/><see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldDefault8bppBlack.gif" alt="Shield icon with system default 8 BPP palette"/>
        /// <br/>Default optional parameter values (black background, alpha threshold = 128)</para>
        /// <para><img src="../Help/Images/ShieldDefault8bppBlackDitheredB8.gif" alt="Shield icon with system default 8 BPP palette using Bayer 8x8 ordered dithering"/>
        /// <br/>Default background and alpha threshold, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para>
        /// <para><img src="../Help/Images/ShieldDefault8bppSilverA1DitheredFS.gif" alt="Shield icon with system default 8 BPP palette using silver background, alpha threshold = 1 and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, alpha threshold = 1, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GirlWithAPearlEarring.png" alt="Test image &quot;Girl with a Pearl Earring&quot;"/>
        /// <br/>Original test image "Girl with a Pearl Earring"</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GirlWithAPearlEarringDefault8bppSrgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with system default 8 BPP palette, quantized in the sRGB color space"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/GirlWithAPearlEarringDefault8bppDitheredB8Srgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with system default 8 BPP palette, quantized in the sRGB color space using Bayer 8x8 ordered dithering"/>
        /// <br/><see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para>
        /// <para><img src="../Help/Images/GirlWithAPearlEarringDefault8bppDitheredFSSrgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with system default 8 BPP palette, quantized in the sRGB color space using Floyd-Steinberg dithering"/>
        /// <br/><see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        /// <seealso cref="O:KGySoft.Drawing.Imaging.Palette.SystemDefault8BppPalette">Palette.SystemDefault8BppPalette Methods</seealso>
        public static PredefinedColorsQuantizer SystemDefault8BppPalette(Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(Palette.SystemDefault8BppPalette(new Color32(backColor), alphaThreshold));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 4-bit palette.
        /// This palette consists of the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a>.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 4-bit palette.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 16 colors.</para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format4bppIndexed"/> pixel format.</para>
        /// <para>The palette of this quantizer is not expected to contain transparent entries.
        /// The palette consists of the 16 standard <a href="https://www.w3.org/TR/REC-html40/types.html#h-6.5" target="_blank">basic sRGB colors</a></para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToDefault4Bpp(IReadWriteBitmapData source, Color backColor = default, IDitherer ditherer = null)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.SystemDefault4BppPalette(backColor);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format8bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) when converting to Format4bppIndexed format without dithering, this produces the same result:
        ///     if (ditherer == null)
        ///         return source.Clone(KnownPixelFormat.Format4bppIndexed, backColor);
        ///
        ///     // c.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// } ]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientDefault4bppBlack.gif" alt="Color hues with system default 4 BPP palette and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/AlphaGradientDefault4bppSilver.gif" alt="Color hues with system default 4 BPP palette and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/AlphaGradientDefault4bppSilverDitheredB8.gif" alt="Color hues with system default 4 BPP palette, using silver background and a stronger Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering with strength = 0.5</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesDefault4bpp.gif" alt="Grayscale color shades with system default 4 BPP palette"/>
        /// <br/>Default optional parameter values. The asymmetry is due to the uneven distribution of gray shades of this palette.</para>
        /// <para><img src="../Help/Images/GrayShadesDefault4bppDitheredB8.gif" alt="Grayscale color shades with system default 4 BPP palette using Bayer 8x8 ordered dithering"/>
        /// <br/><see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering using auto strength. Darker shades have banding.</para>
        /// <para><img src="../Help/Images/GrayShadesDefault4bppDitheredB8Str-5.gif" alt="Grayscale color shades with system default 4 BPP palette using a stronger Bayer 8x8 ordered dithering"/>
        /// <br/><see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering using strength = 0.5. Now there is no banding but white suffers from overdithering.</para>
        /// <para><img src="../Help/Images/GrayShadesDefault4bppDitheredB8Interpolated.gif" alt="Grayscale color shades with system default 4 BPP palette using 8x8 ordered dithering with interpolated ato strength"/>
        /// <br/><see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering using <see cref="AutoStrengthMode.Interpolated"/> auto strength strategy. Now there is neither banding nor overdithering for black or white colors.</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldDefault4bppBlack.gif" alt="Shield icon with system default 4 BPP palette and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/ShieldDefault4bppSilver.gif" alt="Shield icon with system default 4 BPP palette and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/ShieldDefault4bppSilverDitheredFS.gif" alt="Shield icon with system default 4 BPP palette using silver background and Floyd-Steinberg dithering"/>
        /// <br/>Silver background, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        /// <seealso cref="O:KGySoft.Drawing.Imaging.Palette.SystemDefault4BppPalette">Palette.SystemDefault4BppPalette Methods</seealso>
        public static PredefinedColorsQuantizer SystemDefault4BppPalette(Color backColor = default)
            => new PredefinedColorsQuantizer(Palette.SystemDefault4BppPalette(new Color32(backColor)));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 1-bit palette.
        /// This palette consists of the black and white colors.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the system default 1-bit palette.</returns>
        /// <remarks>
        /// <para>The returned <see cref="PredefinedColorsQuantizer"/> instance can return up to 2 colors.
        /// The system 1-bit palette expected to have the black and white colors on most operating systems.
        /// <note type="tip">To make sure that you use a black and white palette use the <see cref="BlackAndWhite">BlackAndWhite</see> method instead, which provides white threshold adjustment as well.
        /// <br/>For more details and examples see the <strong>Examples</strong> section of the <see cref="BlackAndWhite">BlackAndWhite</see> method.</note></para>
        /// <para>This quantizer fits well for the <see cref="KnownPixelFormat.Format1bppIndexed"/> pixel format.</para>
        /// </remarks>
        /// <seealso cref="O:KGySoft.Drawing.Imaging.Palette.SystemDefault1BppPalette">Palette.SystemDefault1BppPalette Methods</seealso>
        public static PredefinedColorsQuantizer SystemDefault1BppPalette(Color backColor = default)
            => new PredefinedColorsQuantizer(Palette.SystemDefault1BppPalette(new Color32(backColor)));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the colors in the specified <paramref name="palette"/>.
        /// </summary>
        /// <param name="palette">The array of colors to be used by the returned instance.</param>
        /// <param name="backColor">Colors with alpha (transparency), which are considered opaque will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">If the specified <paramref name="palette"/> contains a transparent color,
        /// then specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If 0, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the colors in the specified <paramref name="palette"/>.</returns>
        /// <remarks>
        /// <para>The <see cref="PredefinedColorsQuantizer"/> instance returned by this method will use a <see cref="Imaging.Palette"/> internally, created from
        /// the colors specified in the <paramref name="palette"/> parameter. When quantizing, best matching colors might be looked up sequentially and results
        /// might be cached.</para>
        /// <para>If a color to be quantized can be mapped to a color index directly, then create a <see cref="Imaging.Palette"/> instance explicitly,
        /// specifying the custom mapping logic and use the <see cref="FromCustomPalette(Imaging.Palette)"/> overload instead.</para>
        /// <para>If a color to be quantized can be transformed to a result color directly, and the quantized result is not needed to be an indexed image,
        /// then use the <see cref="O:KGySoft.Drawing.Imaging.PredefinedColorsQuantizer.FromCustomFunction">FromCustomFunction</see> overloads instead.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToRgb111(IReadWriteBitmapData source,
        ///     Color backColor = default, IDitherer ditherer = null, WorkingColorSpace colorSpace = default)
        /// {
        ///     Color[] colors =
        ///     {
        ///         Color.Black, Color.Red, Color.Lime, Color.Blue,
        ///         Color.Magenta, Color.Yellow, Color.Cyan, Color.White
        ///     };
        ///
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.FromCustomPalette(new Palette(colors, colorSpace, backColor));
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format4bppIndexed, quantizer, ditherer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     if (ditherer == null)
        ///         source.Quantize(quantizer);
        ///     else
        ///         source.Dither(quantizer, ditherer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientRgb111Black.gif" alt="Color hues with RGB111 palette and black background"/>
        /// <br/>Default optional parameter values (black background). The bottom half of the result is black.</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb111Silver.gif" alt="Color hues with RGB111 palette and silver background"/>
        /// <br/>Silver background. The bottom part of the result is white.</para>
        /// <para><img src="../Help/Images/AlphaGradientRgb111SilverDitheredB8.gif" alt="Color hues with RGB111 palette and silver background, using Bayer 8x8 ordered dithering"/>
        /// <br/>Silver background, <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GrayShadesBW.gif" alt="Grayscale color shades with RGB111 palette"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/GrayShadesBWDitheredB8.gif" alt="Grayscale color shades with RGB111 palette, using Bayer 8x8 ordered dithering"/>
        /// <br/><see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldRgb111Black.gif" alt="Shield icon with RGB111 palette and black background"/>
        /// <br/>Default optional parameter values (black background)</para>
        /// <para><img src="../Help/Images/ShieldRgb111Silver.gif" alt="Shield icon with RGB111 palette and silver background"/>
        /// <br/>Silver background</para>
        /// <para><img src="../Help/Images/ShieldRgb111SilverDitheredFS.gif" alt="Shield icon with RGB111 palette, silver background, using Floyd-Steinberg dithering"/>
        /// <br/>Silver background, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GirlWithAPearlEarring.png" alt="Test image &quot;Girl with a Pearl Earring&quot;"/>
        /// <br/>Original test image "Girl with a Pearl Earring"</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/GirlWithAPearlEarringRgb111Srgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with RGB111 palette, quantized in the sRGB color space"/>
        /// <br/>Default optional parameter values</para>
        /// <para><img src="../Help/Images/GirlWithAPearlEarringRgb111DitheredFSSrgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with RGB111 palette, quantized in the sRGB color space using Floyd-Steinberg dithering"/>
        /// <br/><see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para>
        /// <para><img src="../Help/Images/GirlWithAPearlEarringRgb111Linear.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with RGB111 palette, quantized in the linear color space"/>
        /// <br/><see cref="Imaging.WorkingColorSpace.Linear"/> color space</para>
        /// <para><img src="../Help/Images/GirlWithAPearlEarringRgb111DitheredFSLinear.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with RGB111 palette, quantized in the linear color space using Floyd-Steinberg dithering"/>
        /// <br/><see cref="Imaging.WorkingColorSpace.Linear"/> color space, <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        public static PredefinedColorsQuantizer FromCustomPalette(Color[] palette, Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(new Palette((palette ?? throw new ArgumentNullException(nameof(palette), PublicResources.ArgumentNull))
                .Select(c => c.ToColor32()), backColor.ToColor32(), alphaThreshold));

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the specified <paramref name="palette"/>.
        /// </summary>
        /// <param name="palette">The <see cref="Palette"/> to be used by the returned instance.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the specified <paramref name="palette"/>.</returns>
        /// <remarks>
        /// <para>If a color to be quantized can be transformed to a result color directly, and the quantized result is not needed to be an indexed image,
        /// then use the <see cref="O:KGySoft.Drawing.Imaging.PredefinedColorsQuantizer.FromCustomFunction">FromCustomFunction</see> overloads instead.</para>
        /// <note>For examples see the <strong>Examples</strong> section of the <see cref="FromCustomPalette(Color[], Color, byte)"/> overload.</note>
        /// </remarks>
        public static PredefinedColorsQuantizer FromCustomPalette(Palette palette) => new PredefinedColorsQuantizer(palette);

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the custom quantizer function specified in the <paramref name="quantizingFunction"/> parameter.
        /// </summary>
        /// <param name="quantizingFunction">A delegate that specifies the custom quantization logic. It must be thread-safe for parallel invoking and it is expected to be fast.
        /// The results returned by the delegate are not cached.</param>
        /// <param name="backColor">Colors with alpha (transparency), whose <see cref="Color.A">Color.A</see> property
        /// is equal to or greater than <paramref name="alphaThreshold"/> will be blended with this color before invoking the <paramref name="quantizingFunction"/> delegate.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored.</param>
        /// <param name="pixelFormatHint">The <see cref="KnownPixelFormat"/> value that the <see cref="PixelFormatHint"/> property of the returned instance will return. This parameter is optional.
        /// <br/>Default value: <see cref="KnownPixelFormat.Format24bppRgb"/>, which is valid only if <paramref name="alphaThreshold"/> has the default zero value.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If 0, then even the completely transparent colors will be blended with <paramref name="backColor"/> before invoking the <paramref name="quantizingFunction"/> delegate. This parameter is optional.
        /// <br/>Default value: <c>0</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the custom quantizer function specified in the <paramref name="quantizingFunction"/> parameter.</returns>
        /// <remarks>
        /// <para>The quantizer returned by this method does not have a palette. If you need to create an indexed result using a custom mapping function that
        /// uses up to 256 different colors, then create a <see cref="Imaging.Palette"/> instance specifying a custom function and call the <see cref="FromCustomPalette(Imaging.Palette)"/> method instead.</para>
        /// <para>This overload never calls the <paramref name="quantizingFunction"/> delegate with a color with alpha. Depending on <paramref name="alphaThreshold"/> either a completely
        /// transparent color will be returned or the color will be blended with <paramref name="backColor"/> before invoking the delegate.
        /// In order to allow invoking <paramref name="quantizingFunction"/> with alpha colors use the <see cref="FromCustomFunction(Func{Color32, Color32},KnownPixelFormat)"/>
        /// or <see cref="FromCustomFunction(Func{Color32, Color32}, Color, byte, bool, KnownPixelFormat)"/> overloads instead.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToGrayscaleOpaque(IReadWriteBitmapData source, Color backColor = default)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.FromCustomFunction(c => c.ToGray(), backColor, 0);
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format24bppRgb, quantizer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     source.Quantize(quantizer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientGray8bppBlack.gif" alt="Grayscale color hues with black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/AlphaGradientGray8bppSilver.gif" alt="Graayscale color hues with silver background"/>
        /// <br/>Silver background</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldGray8bppBlack.gif" alt="Grayscale shield icon with black background"/>
        /// <br/>Default (black) background</para>
        /// <para><img src="../Help/Images/ShieldGray8bppSilver.gif" alt="Grayscale shield icon with silver background"/>
        /// <br/>Silver background</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        public static PredefinedColorsQuantizer FromCustomFunction(Func<Color32, Color32> quantizingFunction, Color backColor, KnownPixelFormat pixelFormatHint = KnownPixelFormat.Format24bppRgb, byte alphaThreshold = 0)
            => new PredefinedColorsQuantizer(quantizingFunction, pixelFormatHint, new Color32(backColor), alphaThreshold);

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the custom quantizer function specified in the <paramref name="quantizingFunction"/> parameter.
        /// </summary>
        /// <param name="quantizingFunction">A delegate that specifies the custom quantization logic. It must be thread-safe for parallel invoking and it is expected to be fast.
        /// The results returned by the delegate are not cached.</param>
        /// <param name="pixelFormatHint">The <see cref="KnownPixelFormat"/> value that the <see cref="PixelFormatHint"/> property of the returned instance will return. This parameter is optional.
        /// <br/>Default value: <see cref="KnownPixelFormat.Format32bppArgb"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the custom quantizer function specified in the <paramref name="quantizingFunction"/> parameter.</returns>
        /// <remarks>
        /// <para>The quantizer returned by this method does not have a palette. If you need to create an indexed result using a custom mapping function that
        /// uses up to 256 different colors, then create a <see cref="Imaging.Palette"/> instance specifying a custom function and call the <see cref="FromCustomPalette(Imaging.Palette)"/> method instead.</para>
        /// <para>This overload always calls the <paramref name="quantizingFunction"/> delegate without preprocessing the input colors.
        /// In order to pass only opaque colors to the <paramref name="quantizingFunction"/> delegate use the <see cref="FromCustomFunction(Func{Color32, Color32}, Color, KnownPixelFormat, byte)"/> overload instead.</para>
        /// <para>This overload always creates a quantizer with black <see cref="BackColor"/> and zero <see cref="AlphaThreshold"/>. If <paramref name="quantizingFunction"/> can return colors with alpha,
        /// then the background color and alpha threshold are relevant only when this quantizer is used together with an <see cref="IDitherer"/>, which does not support partial transparency.
        /// Use the <see cref="FromCustomFunction(Func{Color32, Color32}, Color, byte, bool, KnownPixelFormat)"/> overload to specify the <see cref="BackColor"/> and <see cref="AlphaThreshold"/> properties.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// public static IReadWriteBitmapData ToGrayscalePreserveAlpha(IReadWriteBitmapData source)
        /// {
        ///     IQuantizer quantizer = PredefinedColorsQuantizer.FromCustomFunction(c => c.ToGray());
        ///
        ///     // a.) this solution returns a new bitmap data and does not change the original one:
        ///     return source.Clone(KnownPixelFormat.Format32bppArgb, quantizer);
        ///
        ///     // b.) alternatively, you can perform the quantization directly on the source bitmap data:
        ///     source.Quantize(quantizer);
        ///     return source;
        /// }]]></code>
        /// <para>The example above may produce the following results:
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Original image</div></th><th width="50%"><div style="text-align:center;">Quantized image</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/AlphaGradientGrayscale.png" alt="Grayscale color hues with alpha preserved"/>
        /// <br/>Alpha has been preserved</para></div></td>
        /// </tr>
        /// <tr><td><div style="text-align:center;">
        /// <para><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/>
        /// <br/>Shield icon with transparency</para></div></td>
        /// <td><div style="text-align:center;">
        /// <para><img src="../Help/Images/ShieldGrayscale.png" alt="Grayscale shield icon with alpha preserved"/>
        /// <br/>Alpha has been preserved</para></div></td>
        /// </tr>
        /// </tbody></table></para>
        /// </example>
        public static PredefinedColorsQuantizer FromCustomFunction(Func<Color32, Color32> quantizingFunction, KnownPixelFormat pixelFormatHint = KnownPixelFormat.Format32bppArgb)
            => new PredefinedColorsQuantizer(quantizingFunction, pixelFormatHint);

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the custom quantizer function specified in the <paramref name="quantizingFunction"/> parameter.
        /// </summary>
        /// <param name="quantizingFunction">A delegate that specifies the custom quantization logic. It must be thread-safe for parallel invoking and it is expected to be fast.
        /// The results returned by the delegate are not cached.</param>
        /// <param name="backColor">Determines the <see cref="BackColor"/> property of the result. The <see cref="Color.A">Color.A</see> property of the background color is ignored.
        /// <br/>If <paramref name="autoBlend"/> is <see langword="true"/>, then colors with alpha (transparency), whose <see cref="Color.A">Color.A</see> property
        /// is equal to or greater than <paramref name="alphaThreshold"/> will be blended with this color before invoking the <paramref name="quantizingFunction"/> delegate.
        /// <br/>If <paramref name="autoBlend"/> is <see langword="false"/>, then this parameter matters only if a consumer considers the <see cref="BackColor"/> property, such as an <see cref="IDitherer"/> instance that does not support partial transparency.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent without invoking <paramref name="quantizingFunction"/>.
        /// <br/>If <paramref name="autoBlend"/> is <see langword="true"/>, then <paramref name="quantizingFunction"/> will never be invoked with colors with alpha. Instead, colors whose alpha
        /// equal to or greater than this parameter will be blended with <paramref name="backColor"/> before invoking <paramref name="quantizingFunction"/>.
        /// <br/>If <paramref name="autoBlend"/> is <see langword="false"/>, then colors with alpha equal to or greater than this parameter
        /// are allowed to be passed to <paramref name="quantizingFunction"/> without blending with <paramref name="backColor"/>.</param>
        /// <param name="autoBlend"><see langword="true"/> to always apply <paramref name="backColor"/> and <paramref name="alphaThreshold"/> to the input color before invoking <paramref name="quantizingFunction"/>.
        /// <br/><see langword="false"/> to apply only <paramref name="alphaThreshold"/> to the input colors and allowing <paramref name="quantizingFunction"/> to be invoked with partially transparent colors.</param>
        /// <param name="pixelFormatHint">The <see cref="KnownPixelFormat"/> value that the <see cref="PixelFormatHint"/> property of the returned instance will return. This parameter is optional.
        /// <br/>Default value: <see cref="KnownPixelFormat.Format32bppArgb"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors using the custom quantizer function specified in the <paramref name="quantizingFunction"/> parameter.</returns>
        public static PredefinedColorsQuantizer FromCustomFunction(Func<Color32, Color32> quantizingFunction, Color backColor, byte alphaThreshold, bool autoBlend, KnownPixelFormat pixelFormatHint = KnownPixelFormat.Format32bppArgb)
            => new PredefinedColorsQuantizer(quantizingFunction, pixelFormatHint, backColor, alphaThreshold, autoBlend);

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that is compatible with the <see cref="IBitmapData.PixelFormat"/> of the specified <paramref name="bitmapData"/>
        /// and uses its <see cref="IBitmapData.Palette"/> if <paramref name="bitmapData"/> represents an indexed bitmap.
        /// </summary>
        /// <param name="bitmapData">The <see cref="IBitmapData"/> to get a compatible quantizer for.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that is compatible with the specified <paramref name="bitmapData"/>.</returns>
        /// <remarks>
        /// <para>If the <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> is <see cref="KnownPixelFormat.Format24bppRgb"/>, <see cref="KnownPixelFormat.Format48bppRgb"/> or <see cref="KnownPixelFormat.Format32bppRgb"/>,
        /// then this method returns the same quantizer as the <see cref="Rgb888">Rgb888</see> method.</para>
        /// <para>If the <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> is <see cref="KnownPixelFormat.Format16bppArgb1555"/>,
        /// then this method returns the same quantizer as the <see cref="Argb1555">Argb1555</see> method.</para>
        /// <para>If the <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> is <see cref="KnownPixelFormat.Format16bppRgb565"/>,
        /// then this method returns the same quantizer as the <see cref="Rgb565">Rgb565</see> method.</para>
        /// <para>If the <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> is <see cref="KnownPixelFormat.Format16bppRgb555"/>,
        /// then this method returns the same quantizer as the <see cref="Rgb555">Rgb555</see> method.</para>
        /// <para>If the <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> is <see cref="KnownPixelFormat.Format16bppGrayScale"/>,
        /// then this method returns the same quantizer as the <see cref="Grayscale">Grayscale</see> method.</para>
        /// <para>If the <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> is an indexed format,
        /// then this method returns the same quantizer as the <see cref="FromCustomPalette(Imaging.Palette)"/> method using the <see cref="IBitmapData.Palette"/> of the specified <paramref name="bitmapData"/>.</para>
        /// <para>If none of above and the <paramref name="bitmapData"/> has been created by one of the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataFactory.CreateBitmapData">BitmapDataFactory.CreateBitmapData</see> methods
        /// that create bitmap data with custom pixel format, then a special quantizer is returned that produces exactly the same colors as the specified <paramref name="bitmapData"/>.</para>
        /// <para>Otherwise, this method returns either the same quantizer as the <see cref="Argb8888">Argb8888</see> method (if the <see cref="IBitmapData.PixelFormat"/> of <paramref name="bitmapData"/> supports alpha);
        /// otherwise, the same one as returned by the <see cref="Rgb888">Rgb888</see> method.</para>
        /// <note>For examples see the <strong>Examples</strong> section of the mentioned methods above.</note>
        /// </remarks>
        public static PredefinedColorsQuantizer FromBitmapData(IBitmapData bitmapData)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData), PublicResources.ArgumentNull);
            return (bitmapData.PixelFormat.AsKnownPixelFormatInternal switch
            {
                // if palette is null, the exception will be thrown from PredefinedColorsQuantizer
                KnownPixelFormat.Format8bppIndexed or KnownPixelFormat.Format4bppIndexed or KnownPixelFormat.Format1bppIndexed => FromCustomPalette(bitmapData.Palette!),
                KnownPixelFormat.Format16bppArgb1555 => Argb1555(bitmapData.BackColor.ToColor(), bitmapData.AlphaThreshold),
                KnownPixelFormat.Format16bppRgb565 => Rgb565(bitmapData.BackColor.ToColor()),
                KnownPixelFormat.Format16bppRgb555 => Rgb555(bitmapData.BackColor.ToColor()),
                KnownPixelFormat.Format16bppGrayScale => Grayscale(bitmapData.BackColor.ToColor()),
                KnownPixelFormat.Format24bppRgb or KnownPixelFormat.Format32bppRgb or KnownPixelFormat.Format48bppRgb => Rgb888(bitmapData.BackColor.ToColor()),
                _ => bitmapData is ICustomBitmapData customBitmapData ? new PredefinedColorsQuantizer(customBitmapData)
                    : bitmapData.Palette is Palette palette ? FromCustomPalette(palette)
                    : bitmapData.HasAlpha() ? Argb8888(bitmapData.BackColor.ToColor(), bitmapData.AlphaThreshold)
                    : Rgb888(bitmapData.BackColor.ToColor())
            }).ConfigureColorSpace(bitmapData.GetPreferredColorSpaceOrDefault());
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that is compatible with the specified <paramref name="pixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="pixelFormat">The <see cref="KnownPixelFormat"/> to get a compatible quantizer for.</param>
        /// <param name="backColor">Colors with alpha (transparency), which are considered opaque will be blended with this color before quantization.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If 0, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that is compatible with the specified <paramref name="pixelFormat"/>.</returns>
        /// <remarks>
        /// <para>If <paramref name="pixelFormat"/> is <see cref="KnownPixelFormat.Format24bppRgb"/>, <see cref="KnownPixelFormat.Format48bppRgb"/> or <see cref="KnownPixelFormat.Format32bppRgb"/>,
        /// then this method returns the same quantizer as the <see cref="Rgb888">Rgb888</see> method.</para>
        /// <para>If <paramref name="pixelFormat"/> is <see cref="KnownPixelFormat.Format16bppArgb1555"/>,
        /// then this method returns the same quantizer as the <see cref="Argb1555">Argb1555</see> method.</para>
        /// <para>If <paramref name="pixelFormat"/> is <see cref="KnownPixelFormat.Format16bppRgb565"/>,
        /// then this method returns the same quantizer as the <see cref="Rgb565">Rgb565</see> method.</para>
        /// <para>If <paramref name="pixelFormat"/> is <see cref="KnownPixelFormat.Format16bppRgb555"/>,
        /// then this method returns the same quantizer as the <see cref="Rgb555">Rgb555</see> method.</para>
        /// <para>If <paramref name="pixelFormat"/> is <see cref="KnownPixelFormat.Format16bppGrayScale"/>,
        /// then this method returns the same quantizer as the <see cref="Grayscale">Grayscale</see> method.</para>
        /// <para>If <paramref name="pixelFormat"/> is <see cref="KnownPixelFormat.Format8bppIndexed"/>,
        /// then this method returns the same quantizer as the <see cref="SystemDefault8BppPalette">SystemDefault8BppPalette</see> method.</para>
        /// <para>If <paramref name="pixelFormat"/> is <see cref="KnownPixelFormat.Format4bppIndexed"/>,
        /// then this method returns the same quantizer as the <see cref="SystemDefault4BppPalette">SystemDefault4BppPalette</see> method.</para>
        /// <para>If <paramref name="pixelFormat"/> is <see cref="KnownPixelFormat.Format1bppIndexed"/>,
        /// then this method returns the same quantizer as the <see cref="SystemDefault1BppPalette">SystemDefault1BppPalette</see> method.</para>
        /// <para>In any other case than the ones above this method the same quantizer as the <see cref="Argb8888">Argb8888</see> method.</para>
        /// <note>For examples see the <strong>Examples</strong> section of the mentioned methods above.</note>
        /// </remarks>
        public static PredefinedColorsQuantizer FromPixelFormat(KnownPixelFormat pixelFormat, Color backColor = default, byte alphaThreshold = 128)
        {
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            return pixelFormat switch
            {
                KnownPixelFormat.Format8bppIndexed => SystemDefault8BppPalette(backColor, alphaThreshold),
                KnownPixelFormat.Format4bppIndexed => SystemDefault4BppPalette(backColor),
                KnownPixelFormat.Format1bppIndexed => SystemDefault1BppPalette(backColor),
                KnownPixelFormat.Format16bppArgb1555 => Argb1555(backColor, alphaThreshold),
                KnownPixelFormat.Format16bppRgb565 => Rgb565(backColor),
                KnownPixelFormat.Format16bppRgb555 => Rgb555(backColor),
                KnownPixelFormat.Format16bppGrayScale => Grayscale(backColor),
                KnownPixelFormat.Format24bppRgb or KnownPixelFormat.Format48bppRgb or KnownPixelFormat.Format32bppRgb => Rgb888(backColor),
                _ => Argb8888(backColor, alphaThreshold)
            };
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Configures the preferred working color space for this <see cref="PredefinedColorsQuantizer"/>.
        /// The configuration might be ignored if this instance was created from a custom function.
        /// The configuration may affect alpha blending, nearest color lookup if this quantizer has a <see cref="Imaging.Palette"/>, and also the behavior of ditherers that use this quantizer.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="Imaging.WorkingColorSpace"/> enumeration for details and
        /// image examples about using the different color spaces in various operations.
        /// </summary>
        /// <param name="workingColorSpace">Specifies the working color space for the generated <see cref="Palette"/>.</param>
        /// <returns>An <see cref="OptimizedPaletteQuantizer"/> instance that uses the specified color space.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="workingColorSpace"/> is not one of the defined values.</exception>
        public PredefinedColorsQuantizer ConfigureColorSpace(WorkingColorSpace workingColorSpace)
            => workingColorSpace == WorkingColorSpace ? this : new PredefinedColorsQuantizer(this, workingColorSpace);

        #endregion

        #region Explicitly Implemented Interface Methods

        IQuantizingSession IQuantizer.Initialize(IReadableBitmapData source, IAsyncContext? context)
            => compatibleBitmapDataFactory == null
                ? Palette != null
                    ? new QuantizingSessionIndexed(this, Palette)
                    : new QuantizingSessionCustomMapping(this, quantizingFunction!)
                : new QuantizingSessionByCustomBitmapData(this, compatibleBitmapDataFactory);

        #endregion

        #endregion

        #endregion
    }
}
