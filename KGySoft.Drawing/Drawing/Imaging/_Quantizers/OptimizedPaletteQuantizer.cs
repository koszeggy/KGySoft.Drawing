#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedPaletteQuantizer.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a quantizer that can generate an optimized set of colors best matching to the original image.
    /// Use the static methods to retrieve an instance. For using predefined colors see the <see cref="PredefinedColorsQuantizer"/> class.
    /// <br/>See the <strong>Remarks</strong> section for details and results comparison.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="OptimizedPaletteQuantizer"/> class can be used to reduce colors of a <see cref="Bitmap"/> using a
    /// palette of up to 65536 colors where the palette entries are optimized for the quantized image.
    /// <note>Though more than 256 colors are supported, the typical goal of palette optimization is to adjust the colors for an indexed pixel format.
    /// Natively supported indexed formats cannot have more than 256 colors, though you are allowed to create images with custom pixel format
    /// by using the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataFactory.CreateBitmapData">BitmapDataFactory.CreateBitmapData</see> methods that
    /// have <see cref="PixelFormatInfo"/> parameters. Please note though that a large palette may have impact on both memory usage and performance.</note></para>
    /// <para>This class supports palette optimization by three different algorithms (see the
    /// <see cref="Octree">Octree</see>, <see cref="MedianCut">MedianCut</see> and <see cref="Wu">Wu</see> methods)</para>
    /// <para>The following table compares the algorithms supported by the <see cref="OptimizedPaletteQuantizer"/> class:
    /// <list type="table">
    /// <listheader><term></term><term><see cref="Octree">Octree</see></term><term><see cref="MedianCut">MedianCut</see></term><term><see cref="Wu">Wu</see></term></listheader>
    /// <item>
    /// <term><strong>Speed</strong></term>
    /// <term>Slower than the <see cref="Wu">Wu</see>'s algorithm but faster than <see cref="MedianCut">MedianCut</see>.</term>
    /// <term>This is the slowest one of the three algorithms.</term>
    /// <term>This is the fastest one of the three algorithms (still much slower though than the quantizers of the <see cref="PredefinedColorsQuantizer"/> class).</term>
    /// </item>
    /// <item>
    /// <term><strong>Memory consumption<sup>*</sup></strong></term>
    /// <term>Generating the palette may consume quite a large amount of memory but it also depends on the number of different colors
    /// of the source image and the requested color count. The memory is continuously allocated on demand and in extreme cases it may consume more memory than <see cref="Wu">Wu</see>'s algorithm.
    /// The memory usage can limited by the <see cref="ConfigureBitLevel">ConfigureBitLevel</see> method.</term>
    /// <term>The memory usage mainly depends on the image size and somewhat on the requested color count. Quantizing a large image may consume a large amount of memory
    /// even if the image itself consist of just a few colors.</term>
    /// <term>In general case this quantizer consumes the most memory, even if the source has few colors and the requested color count is small.
    /// Most of the memory is allocated at once, regardless of the image size or its actual colors, and a smaller portion is allocated dynamically, which depends on the number of requested colors.
    /// On platforms where available, array pooling is used, which release the used memory only after a while if the buffers are not re-used within a time interval.
    /// The memory usage can limited by the <see cref="ConfigureBitLevel">ConfigureBitLevel</see> method.</term>
    /// </item>
    /// <item>
    /// <term><strong>Quality</strong></term>
    /// <term><list type="bullet">
    /// <item>Usually poorer quality for smaller palettes (below 16 colors).</item>
    /// <item>Banding may appear in images with large low-frequency areas (eg. sky or water in photos).</item>
    /// <item>Balanced quality for larger palettes and photo-like images.</item>
    /// </list></term>
    /// <term><list type="bullet">
    /// <item>Usually better quality for smaller palettes.</item>t
    /// <item>Excellent, nearly banding-free results with images with large low-frequency areas (eg. sky or water in photos).</item>
    /// <item>May provide poorer quality for small areas with unique colors (eg. a smaller human face in a large photo).</item>
    /// </list></term>
    /// <term><list type="bullet">
    /// <item>Usually very good quality even for smaller palettes.</item>
    /// <item>Banding may appear in images with large low-frequency areas (eg. sky or water in photos), though not as heavily as in case of the <see cref="Octree">Octree</see> algorithm
    /// (and it can be blended by dithering anyway). By default, banding may appear for monochromatic images even if the requested number of colors would allow a banding-free result
    /// but this can be configured by the <see cref="ConfigureBitLevel">ConfigureBitLevel</see> method.
    /// </item>
    /// <item>Very good quality for photo-like images, especially if the image has no homogeneous low-frequency areas.</item>
    /// </list></term>
    /// </item>
    /// </list>
    /// <note>
    /// <para><sup>*</sup>Memory consumption mentioned in the table affects palette generation only.
    /// That occurs when the <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method of an <see cref="OptimizedPaletteQuantizer"/> instance
    /// is called. As soon as this method returns with an <see cref="IQuantizingSession"/> instance, the memory mentioned in the table can be reclaimed
    /// (which does not necessarily happen immediately on platforms that support array pooling, which is utilized by the <see cref="Wu">Wu</see>'s algorithm).</para>
    /// <para>On the other hand, the <see cref="IQuantizingSession"/> can also consume a large amount of memory during the quantization
    /// because its <see cref="Palette"/> caches the quantization results of the source image pixels, though this caching does not
    /// depend on the chosen algorithm and the used memory can also be reclaimed when the <see cref="IQuantizingSession"/> is disposed.</para>
    /// <para>Keeping a reference to an <see cref="OptimizedPaletteQuantizer"/> consumes almost no memory when there is no active quantization session in progress.</para>
    /// </note>
    /// </para>
    /// <para>The following table compares the results of the <see cref="OptimizedPaletteQuantizer"/> instances returned by the
    /// <see cref="Octree">Octree</see>, <see cref="MedianCut">MedianCut</see> and <see cref="Wu">Wu</see> methods.
    /// <note>For better comparison none of the images are dithered in the examples, though the visual quality can be improved by using dithering.
    /// See the <see cref="OrderedDitherer"/>, <see cref="ErrorDiffusionDitherer"/>, <see cref="RandomNoiseDitherer"/> and <see cref="InterleavedGradientNoiseDitherer"/>
    /// classes for some built-in <see cref="IDitherer"/> implementations.</note>
    /// <list type="table">
    /// <listheader><term>Original image</term><term>Quantized image</term></listheader>
    /// <item>
    /// <term><div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
    /// <br/>Color hues with alpha gradient</para></div></term>
    /// <term><div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/AlphaGradientOctree256Silver.gif" alt="Color hues quantized by Octree algorithm using 256 colors, silver background, zero alpha threshold"/>
    /// <br/><see cref="Octree">Octree</see> algorithm, 256 colors, silver background, zero alpha threshold</para>
    /// <para><img src="../Help/Images/AlphaGradientMedianCut256Silver.gif" alt="Color hues quantized by Median Cut algorithm using 256 colors, silver background, zero alpha threshold"/>
    /// <br/><see cref="MedianCut">MedianCut</see> algorithm, 256 colors, silver background, zero alpha threshold</para>
    /// <para><img src="../Help/Images/AlphaGradientWu256Silver.gif" alt="Color hues quantized by Wu's algorithm using 256 colors, silver background, zero alpha threshold"/>
    /// <br/><see cref="Wu">Wu</see>'s algorithm, 256 colors, silver background, zero alpha threshold</para>
    /// </div></term>
    /// </item>
    /// <item>
    /// <term><div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/Information256.png" alt="Information icon with transparent background"/>
    /// <br/>Information icon with transparency</para></div></term>
    /// <term><div style="text-align:center;width:512px">
    /// <para><img src="../Help/Images/InformationOctree4Silver.gif" alt="Information icon quantized by Octree algorithm using 4 colors, silver background, zero alpha threshold"/>
    /// <br/><see cref="Octree">Octree</see> algorithm, 4 colors, silver background, zero alpha threshold</para>
    /// <para><img src="../Help/Images/InformationMedianCut4Silver.gif" alt="Information icon quantized by Median Cut algorithm using 4 colors, silver background, zero alpha threshold"/>
    /// <br/><see cref="MedianCut">MedianCut</see> algorithm, 4 colors, silver background, zero alpha threshold</para>
    /// <para><img src="../Help/Images/InformationWu4Silver.gif" alt="Information icon quantized by Wu's algorithm using 4 colors, silver background, zero alpha threshold"/>
    /// <br/><see cref="Wu">Wu</see>'s algorithm, 4 colors, silver background, zero alpha threshold</para>
    /// <para><img src="../Help/Images/InformationOctree256Black.gif" alt="Information icon quantized by Octree algorithm using 256 colors, black background, alpha threshold = 128"/>
    /// <br/><see cref="Octree">Octree</see> algorithm, 256 colors, black background, alpha threshold = 128. Banding appeared in the result.</para>
    /// <para><img src="../Help/Images/InformationMedianCut256Black.gif" alt="Information icon quantized by Median Cut algorithm using 256 colors, black background, alpha threshold = 128"/>
    /// <br/><see cref="MedianCut">MedianCut</see> algorithm, 256 colors, black background, alpha threshold = 128. Practically there is no banding in the result.</para>
    /// <para><img src="../Help/Images/InformationWu256Black.gif" alt="Information icon quantized by Wu's algorithm using 256 colors, black background, alpha threshold = 128"/>
    /// <br/><see cref="Wu">Wu</see>'s algorithm, 256 colors, black background, alpha threshold = 128. A slight banding can be observed,
    /// as if the source image had been quantized by the <see cref="PredefinedColorsQuantizer.Argb1555">PredefinedColorsQuantizer.Argb1555</see> quantizer first.
    /// It used to be result of earlier versions and starting with version 6.3.0 you get this result if you use the <see cref="ConfigureBitLevel">ConfigureBitLevel</see> method with 5 bits.
    /// The new default bit level for <see cref="Wu">Wu</see>'s algorithm is 7, which reduces the banding even more, while uses more memory.</para>
    /// </div></term>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="IQuantizer"/>
    /// <seealso cref="PredefinedColorsQuantizer"/>
    /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/>
    /// <seealso cref="BitmapExtensions.Quantize"/>
    /// <seealso cref="BitmapExtensions.Dither"/>
    public sealed partial class OptimizedPaletteQuantizer : IQuantizer
    {
        #region Nested types

        #region Enumerations

        private enum Algorithm
        {
            Octree,
            MedianCut,
            Wu
        }

        #endregion

        #region Nested interfaces

        private interface IOptimizedPaletteQuantizer : IDisposable
        {
            #region Methods

            void Initialize(int requestedColors, byte? bitLevel, IBitmapData source);

            void AddColor(Color32 c);

            Color32[]? GeneratePalette(IAsyncContext context);

            #endregion
        }

        #endregion

        #region Nested classes

        private sealed class OptimizedPaletteQuantizerSession<TAlg> : IQuantizingSession
            where TAlg : IOptimizedPaletteQuantizer, new()
        {
            #region Fields

            private readonly OptimizedPaletteQuantizer quantizer;

            #endregion

            #region Properties

            public Palette? Palette { get; }

            public Color32 BackColor => quantizer.backColor;
            public byte AlphaThreshold => quantizer.alphaThreshold;

            #endregion

            #region Constructors

            public OptimizedPaletteQuantizerSession(OptimizedPaletteQuantizer quantizer, IReadableBitmapData source, IAsyncContext context)
            {
                this.quantizer = quantizer;
                Palette = InitializePalette(source, context);
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose()
            {
            }

            public Color32 GetQuantizedColor(Color32 origColor)
                // palette is null only if the initialization was canceled, but then this method is not called
                => Palette!.GetNearestColor(origColor);

            #endregion

            #region Private Methods

            private Palette? InitializePalette(IReadableBitmapData source, IAsyncContext context)
            {
                using var alg = new TAlg();
                alg.Initialize(quantizer.maxColors, quantizer.bitLevel, source);
                int width = source.Width;
                IReadableBitmapDataRow row = source.FirstRow;
                context.Progress?.New(DrawingOperation.InitializingQuantizer, source.Height);
                do
                {
                    if (context.IsCancellationRequested)
                        return null;

                    for (int x = 0; x < width; x++)
                    {
                        Color32 c = row[x];

                        // handling alpha including full transparency
                        if (c.A != Byte.MaxValue)
                            c = c.A < quantizer.alphaThreshold ? default : c.BlendWithBackground(quantizer.backColor);
                        alg.AddColor(c);
                    }
                    context.Progress?.Increment();
                } while (row.MoveNextRow());

                Color32[]? palette = alg.GeneratePalette(context);
                return context.IsCancellationRequested ? null : new Palette(palette!, quantizer.backColor, quantizer.alphaThreshold);
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly int maxColors;
        private readonly Color32 backColor;
        private readonly byte alphaThreshold;
        private readonly Algorithm algorithm;
        private readonly byte? bitLevel;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets a <see cref="PixelFormat"/>, which is compatible with this <see cref="OptimizedPaletteQuantizer"/> instance.
        /// </summary>
        public PixelFormat PixelFormatHint => maxColors switch
        {
            > 256 => alphaThreshold == 0 ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb,
            > 16 => PixelFormat.Format8bppIndexed,
            > 2 => PixelFormat.Format4bppIndexed,
            _ => PixelFormat.Format1bppIndexed
        };

        #endregion

        #region Explicitly Implemented Interface Properties
        
        bool IQuantizer.InitializeReliesOnContent => true;

        #endregion

        #endregion

        #region Constructors

        private OptimizedPaletteQuantizer(Algorithm algorithm, int maxColors, Color backColor, byte alphaThreshold)
        {
            const int max = 1 << 16;
            if (maxColors is < 2 or > max)
                throw new ArgumentOutOfRangeException(nameof(maxColors), PublicResources.ArgumentMustBeBetween(2, max));
            this.algorithm = algorithm;
            this.maxColors = maxColors;
            this.backColor = new Color32(backColor).ToOpaque();
            this.alphaThreshold = alphaThreshold;
        }

        private OptimizedPaletteQuantizer(OptimizedPaletteQuantizer original, byte? bitLevel)
            : this(original.algorithm, original.maxColors, original.backColor.ToColor(), original.alphaThreshold)
        {
            this.bitLevel = bitLevel;
        }

        #endregion

        #region Methods

        #region Static Methods

        /// <summary>
        /// Gets an <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image using the Octree quantizing algorithm.
        /// <br/>See the <strong>Examples</strong> section for an example,
        /// and the <strong>Remarks</strong> section of the <see cref="OptimizedPaletteQuantizer"/> for details and results comparison with the other algorithms.
        /// </summary>
        /// <param name="maxColors">The upper limit of generated colors. Must be between 2 and 256, inclusive bounds. This parameter is optional.
        /// <br/>Default value: <c>256</c>.</param>
        /// <param name="backColor">Colors with alpha above the <paramref name="alphaThreshold"/> will be blended with this color before quantizing.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If 0, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image using the Octree quantizing algorithm.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxColors"/> must be between 2 and 256, inclusive bounds.</exception>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// Bitmap bmpOriginal = Icons.Shield.ExtractBitmap(new Size(256, 256));
        /// bmpOriginal.SaveAsPng(@"c:\temp\original.png");
        ///
        /// IQuantizer quantizer = OptimizedPaletteQuantizer.Octree(256);
        /// Bitmap bmpConverted = bmpOriginal.ConvertPixelFormat(PixelFormat.Format8bppIndexed, quantizer);
        /// bmpConverted.SaveAsGif(@"c:\temp\converted.gif");]]></code>
        /// <para>The example above produces the following result:
        /// <list type="table">
        /// <item><term><c>original.png</c></term><term><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/></term></item>
        /// <item><term><c>converted.gif</c></term><term><img src="../Help/Images/ShieldOctree256Black.gif" alt="Shield icon quantized to 256 colors using the Octree algorithm"/></term></item>
        /// </list></para>
        /// <note type="tip">For more image examples and side-by-side comparison with the other algorithms see the <strong>Remarks</strong> section of the <see cref="OptimizedPaletteQuantizer"/> class.</note>
        /// </example>
        /// <seealso cref="OptimizedPaletteQuantizer"/>
        /// <seealso cref="MedianCut"/>
        /// <seealso cref="Wu"/>
        public static OptimizedPaletteQuantizer Octree(int maxColors = 256, Color backColor = default, byte alphaThreshold = 128)
            => new OptimizedPaletteQuantizer(Algorithm.Octree, maxColors, backColor, alphaThreshold);

        /// <summary>
        /// Gets an <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image using the Median Cut quantizing algorithm.
        /// <br/>See the <strong>Examples</strong> section for an example,
        /// and the <strong>Remarks</strong> section of the <see cref="OptimizedPaletteQuantizer"/> for details and results comparison with the other algorithms.
        /// </summary>
        /// <param name="maxColors">The upper limit of generated colors. Must be between 2 and 256, inclusive bounds. This parameter is optional.
        /// <br/>Default value: <c>256</c>.</param>
        /// <param name="backColor">Colors with alpha above the <paramref name="alphaThreshold"/> will be blended with this color before quantizing.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If 0, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image using the Median Cut quantizing algorithm.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxColors"/> must be between 2 and 256, inclusive bounds.</exception>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// Bitmap bmpOriginal = Icons.Shield.ExtractBitmap(new Size(256, 256));
        /// bmpOriginal.SaveAsPng(@"c:\temp\original.png");
        ///
        /// IQuantizer quantizer = OptimizedPaletteQuantizer.MedianCut(256);
        /// Bitmap bmpConverted = bmpOriginal.ConvertPixelFormat(PixelFormat.Format8bppIndexed, quantizer);
        /// bmpConverted.SaveAsGif(@"c:\temp\converted.gif");]]></code>
        /// <para>The example above produces the following result:
        /// <list type="table">
        /// <item><term><c>original.png</c></term><term><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/></term></item>
        /// <item><term><c>converted.gif</c></term><term><img src="../Help/Images/ShieldMedianCut256Black.gif" alt="Shield icon quantized to 256 colors using the Median Cut algorithm"/></term></item>
        /// </list></para>
        /// <note type="tip">For more image examples and side-by-side comparison with the other algorithms see the <strong>Remarks</strong> section of the <see cref="OptimizedPaletteQuantizer"/> class.</note>
        /// </example>
        /// <seealso cref="OptimizedPaletteQuantizer"/>
        /// <seealso cref="Octree"/>
        /// <seealso cref="Wu"/>
        public static OptimizedPaletteQuantizer MedianCut(int maxColors = 256, Color backColor = default, byte alphaThreshold = 128)
            => new OptimizedPaletteQuantizer(Algorithm.MedianCut, maxColors, backColor, alphaThreshold);

        /// <summary>
        /// Gets an <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image using Xiaolin Wu's quantizing algorithm.
        /// <br/>See the <strong>Examples</strong> section for an example,
        /// and the <strong>Remarks</strong> section of the <see cref="OptimizedPaletteQuantizer"/> for details and results comparison with the other algorithms.
        /// </summary>
        /// <param name="maxColors">The upper limit of generated colors. Must be between 2 and 256, inclusive bounds. This parameter is optional.
        /// <br/>Default value: <c>256</c>.</param>
        /// <param name="backColor">Colors with alpha above the <paramref name="alphaThreshold"/> will be blended with this color before quantizing.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If 0, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image by Xiaolin Wu's quantizing algorithm.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxColors"/> must be between 2 and 256, inclusive bounds.</exception>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <code lang="C#"><![CDATA[
        /// Bitmap bmpOriginal = Icons.Shield.ExtractBitmap(new Size(256, 256));
        /// bmpOriginal.SaveAsPng(@"c:\temp\original.png");
        ///
        /// IQuantizer quantizer = OptimizedPaletteQuantizer.Wu(256);
        /// Bitmap bmpConverted = bmpOriginal.ConvertPixelFormat(PixelFormat.Format8bppIndexed, quantizer);
        /// bmpConverted.SaveAsGif(@"c:\temp\converted.gif");]]></code>
        /// <para>The example above produces the following result:
        /// <list type="table">
        /// <item><term><c>original.png</c></term><term><img src="../Help/Images/Shield256.png" alt="Shield icon with transparent background"/></term></item>
        /// <item><term><c>converted.gif</c></term><term><img src="../Help/Images/ShieldWu256Black.gif" alt="Shield icon quantized to 256 colors by Wu's algorithm"/></term></item>
        /// </list></para>
        /// <note type="tip">For more image examples and side-by-side comparison with the other algorithms see the <strong>Remarks</strong> section of the <see cref="OptimizedPaletteQuantizer"/> class.</note>
        /// </example>
        /// <seealso cref="OptimizedPaletteQuantizer"/>
        /// <seealso cref="Octree"/>
        /// <seealso cref="MedianCut"/>
        public static OptimizedPaletteQuantizer Wu(int maxColors = 256, Color backColor = default, byte alphaThreshold = 128)
            => new OptimizedPaletteQuantizer(Algorithm.Wu, maxColors, backColor, alphaThreshold);

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Configures the bit level per color channel to be used while optimizing the palette.
        /// If the input image is a monochromatic one, then determines the bit depth of the result.
        /// Affects the quality, speed and memory usage.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="bitLevel">Specifies the desired bit level. If <see langword="null"/>, then the value is automatically set by the chosen algorithm.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bitLevel"/> must be either <see langword="null"/>, or between 1 and 8.</exception>
        /// <remarks>
        /// <para>As a primary effect, <paramref name="bitLevel"/> determines the upper limit of the possible colors in the generated palette.
        /// For example, if <paramref name="bitLevel"/> is 1, then the result palette cannot have more than 8 colors, or when it is 2, no more than 64 colors.
        /// If you want to quantize an image using the allowed maximum of 65536 colors, then <paramref name="bitLevel"/> should be at least 6 because 5 allows up to 32768 colors.</para>
        /// <para>When using the <see cref="MedianCut">MedianCut</see> algorithm, configuring the bit level has no other effects.
        /// When using the <see cref="Octree">Octree</see> or <see cref="Wu">Wu</see>'s algorithms, <paramref name="bitLevel"/> determines also the amount of
        /// minimum distinguishable monochromatic shades. For example, when <see cref="bitLevel"/> is 5, then up to 32 monochromatic shades can be differentiated
        /// so close shades might be merged even if the requested number of colors could allow returning all the shades.</para>
        /// <para>For the <see cref="Octree">Octree</see> algorithm the default value is the ceiling of the base 2 logarithm of the requested number of colors
        /// (eg. 1 for 2 colors, 8 for 129 or more colors). This is alright for most cases. You can increase the default value if the image has only a few but very close colors
        /// or decrease it if the image has so many colors that the quantization would use too much memory.</para>
        /// <para>For <see cref="Wu">Wu</see>'s algorithm the default value is 7, which means about 80 MB memory usage, regardless of the number of requested colors.
        /// You can decrease it if this is also too much (for example, 5 requires about 1.5 MB of memory), or you can increase it to 8 if you want to be able to
        /// differentiate 256 monochromatic shades, but this pushes up the memory consumption to about 650 MB).</para>
        /// </remarks>
        public OptimizedPaletteQuantizer ConfigureBitLevel(int? bitLevel)
        {
            if (this.bitLevel == bitLevel)
                return this;
            if (bitLevel is < 1 or > 8)
                throw new ArgumentOutOfRangeException(nameof(bitLevel), PublicResources.ArgumentMustBeBetween(1, 8));
            return new OptimizedPaletteQuantizer(this, (byte?)bitLevel);
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        IQuantizingSession IQuantizer.Initialize(IReadableBitmapData source, IAsyncContext? context)
        {
            context ??= AsyncContext.Null;
            return algorithm switch
            {
                Algorithm.Octree => new OptimizedPaletteQuantizerSession<OctreeQuantizer>(this, source, context),
                Algorithm.MedianCut => new OptimizedPaletteQuantizerSession<MedianCutQuantizer>(this, source, context),
                Algorithm.Wu => new OptimizedPaletteQuantizerSession<WuQuantizer>(this, source, context),
                _ => throw new InvalidOperationException(Res.InternalError($"Unexpected algorithm: {algorithm}"))
            };
        }

        #endregion

        #endregion

        #endregion
    }
}
