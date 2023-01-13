#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedPaletteQuantizer.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a quantizer that can generate an optimized set of colors best matching to the original image.
    /// Use the static methods to retrieve an instance. For using predefined colors see the <see cref="PredefinedColorsQuantizer"/> class.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="OptimizedPaletteQuantizer"/> class can be used to reduce colors of an <see cref="IReadableBitmapData"/> using a
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
    /// <term>With default settings usually slower than the <see cref="Wu">Wu</see>'s algorithm but faster than <see cref="MedianCut">MedianCut</see>.
    /// When using high <see cref="ConfigureBitLevel">bit levels</see> and the source is a true color image, then it is generally faster for high requested colors,
    /// and can be faster even than <see cref="Wu">Wu</see>'s algorithm using the same bit level.</term>
    /// <term>In most cases this is the slowest one of the three algorithms, especially for larger images.</term>
    /// <term>With default settings this is almost always the fastest one of the three algorithms
    /// (still much slower though than the quantizers of the <see cref="PredefinedColorsQuantizer"/> class).
    /// When using high <see cref="ConfigureBitLevel">bit levels</see> it can be the slowest one for small images and gets to be the fastest one for larger image sizes.</term>
    /// </item>
    /// <item>
    /// <term><strong>Memory consumption<sup>*</sup></strong></term>
    /// <term>Generating the palette may consume quite a large amount of memory but it also depends on the number of different colors
    /// of the source image and the requested color count. The memory is continuously allocated on demand and in extreme cases it may consume a huge amount of memory.
    /// The memory usage can be limited by the <see cref="ConfigureBitLevel">ConfigureBitLevel</see> method.</term>
    /// <term>The memory usage mainly depends on the image size and somewhat on the requested color count. Quantizing a large image may consume a large amount of memory
    /// even if the image itself consist of just a few colors.</term>
    /// <term>This quantizer consumes a fairly large fix amount of memory, even if the source has few colors and the requested color count is small.
    /// Most of the memory is allocated at once, regardless of the image size or its actual colors, and a smaller portion is allocated dynamically, which depends on the number of requested colors.
    /// On platforms where available, array pooling is used, which releases the used memory only after a while if the buffers are not re-used within a time interval.
    /// The memory usage can be adjusted by the <see cref="ConfigureBitLevel">ConfigureBitLevel</see> method.</term>
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
    /// <item>Banding may appear in images with large low-frequency areas (eg. sky or water in photos).
    /// By default, banding may appear for monochromatic images even if the requested number of colors would allow a banding-free result
    /// but this can be configured by the <see cref="ConfigureBitLevel">ConfigureBitLevel</see> method.</item>
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
    /// as if the source image had been prequantized by the <see cref="PredefinedColorsQuantizer.Argb1555">PredefinedColorsQuantizer.Argb1555</see> quantizer first.
    /// You get this result if you use the <see cref="ConfigureBitLevel">ConfigureBitLevel</see> method with 5 bits (which is the default for Wu with 256 colors).
    /// The banding can be reduced by using higher bit levels, which increases also memory usage and processing time.</para>
    /// </div></term>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="IQuantizer"/>
    /// <seealso cref="PredefinedColorsQuantizer"/>
    /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer?, IDitherer?)"/>
    /// <seealso cref="BitmapDataExtensions.Quantize(IReadWriteBitmapData, IQuantizer)"/>
    /// <seealso cref="BitmapDataExtensions.Dither(IReadWriteBitmapData, IQuantizer, IDitherer)"/>
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

            public Color32 BackColor => quantizer.BackColor;
            public byte AlphaThreshold => quantizer.AlphaThreshold;
            public bool LinearBlending => quantizer.LinearBlending;
            public bool IsGrayscale => Palette?.IsGrayscale ?? false;

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
                alg.Initialize(quantizer.MaxColors, quantizer.bitLevel, source);
                int width = source.Width;
                IReadableBitmapDataRowMovable row = source.FirstRow;
                context.Progress?.New(DrawingOperation.InitializingQuantizer, source.Height);
                do
                {
                    if (context.IsCancellationRequested)
                        return null;

                    for (int x = 0; x < width; x++)
                    {
                        Color32 c = row[x];

                        // Handling alpha including full transparency.
                        // TODO: Here we could allow alpha pixels if all algorithms supported it in AddColor
                        if (c.A != Byte.MaxValue)
                            c = c.A < quantizer.AlphaThreshold ? default : c.BlendWithBackground(quantizer.BackColor, quantizer.LinearBlending);
                        alg.AddColor(c);
                    }
                    context.Progress?.Increment();
                } while (row.MoveNextRow());

                Color32[]? palette = alg.GeneratePalette(context);
                return context.IsCancellationRequested ? null : new Palette(palette!, quantizer.BackColor, quantizer.AlphaThreshold, quantizer.LinearBlending, null);
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly Algorithm algorithm;
        private readonly byte? bitLevel;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets a <see cref="KnownPixelFormat"/>, which is compatible with this <see cref="OptimizedPaletteQuantizer"/> instance.
        /// </summary>
        public KnownPixelFormat PixelFormatHint => MaxColors switch
        {
            > 256 => AlphaThreshold == 0 ? KnownPixelFormat.Format24bppRgb : KnownPixelFormat.Format32bppArgb,
            > 16 => KnownPixelFormat.Format8bppIndexed,
            > 2 => KnownPixelFormat.Format4bppIndexed,
            _ => KnownPixelFormat.Format1bppIndexed
        };

        /// <summary>
        /// Gets the back color used by this <see cref="OptimizedPaletteQuantizer"/>. This value will be returned also by
        /// the <see cref="IQuantizingSession.BackColor"/> property once an <see cref="IQuantizingSession"/> is created from this instance.
        /// The <see cref="Color32.A"/> field of the returned color is always 255.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="IQuantizingSession.AlphaThreshold">IQuantizingSession.AlphaThreshold</see> property for details.
        /// </summary>
        public Color32 BackColor { get; }

        /// <summary>
        /// Gets the alpha threshold value used by this <see cref="OptimizedPaletteQuantizer"/>. This value will be returned also by
        /// the <see cref="IQuantizingSession.AlphaThreshold"/> property once an <see cref="IQuantizingSession"/> is created from this instance.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="IQuantizingSession.AlphaThreshold">IQuantizingSession.AlphaThreshold</see> property for details.
        /// </summary>
        public byte AlphaThreshold { get; }

        /// <summary>
        /// Gets the maximum number of colors this <see cref="OptimizedPaletteQuantizer"/> is allowed to use.
        /// Once an <see cref="IQuantizingSession"/> is created from this instance the <see cref="IQuantizingSession.Palette"/> property
        /// will contain no more colors than the value of this property.
        /// </summary>
        public int MaxColors { get; }

        #endregion

        #region Internal Properties

        internal bool LinearBlending { get; }

        #endregion

        #region Explicitly Implemented Interface Properties

        bool IQuantizer.InitializeReliesOnContent => true;

        #endregion

        #endregion

        #region Constructors

        private OptimizedPaletteQuantizer(Algorithm algorithm, int maxColors, Color32 backColor, byte alphaThreshold)
        {
            const int max = 1 << 16;
            if (maxColors is < 2 or > max)
                throw new ArgumentOutOfRangeException(nameof(maxColors), PublicResources.ArgumentMustBeBetween(2, max));
            this.algorithm = algorithm;
            MaxColors = maxColors;
            BackColor = backColor.ToOpaque();
            AlphaThreshold = alphaThreshold;
        }

        private OptimizedPaletteQuantizer(OptimizedPaletteQuantizer original, byte? bitLevel, bool useLinearBlending)
            : this(original.algorithm, original.MaxColors, original.BackColor, original.AlphaThreshold)
        {
            this.bitLevel = bitLevel;
            LinearBlending = useLinearBlending;
        }

        #endregion

        #region Methods

        #region Static Methods

        /// <summary>
        /// Gets an <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image using the Octree quantizing algorithm.
        /// </summary>
        /// <param name="maxColors">The upper limit of generated colors. Must be between 2 and 65536, inclusive bounds. This parameter is optional.
        /// <br/>Default value: <c>256</c>.</param>
        /// <param name="backColor">Colors with alpha above the <paramref name="alphaThreshold"/> will be blended with this color before quantizing.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If 0, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image using the Octree quantizing algorithm.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxColors"/> must be between 2 and 65536, inclusive bounds.</exception>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <note>This example requires to reference the <a href="https://www.nuget.org/packages/KGySoft.Drawing/" target="_blank">KGySoft.Drawing</a> package. When targeting .NET 7 or later it can be executed on Windows only.</note>
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
            => new OptimizedPaletteQuantizer(Algorithm.Octree, maxColors, new Color32(backColor), alphaThreshold);

        /// <summary>
        /// Gets an <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image using the Median Cut quantizing algorithm.
        /// </summary>
        /// <param name="maxColors">The upper limit of generated colors. Must be between 2 and 65536, inclusive bounds. This parameter is optional.
        /// <br/>Default value: <c>256</c>.</param>
        /// <param name="backColor">Colors with alpha above the <paramref name="alphaThreshold"/> will be blended with this color before quantizing.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If 0, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image using the Median Cut quantizing algorithm.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxColors"/> must be between 2 and 65536, inclusive bounds.</exception>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <note>This example requires to reference the <a href="https://www.nuget.org/packages/KGySoft.Drawing/" target="_blank">KGySoft.Drawing</a> package. When targeting .NET 7 or later it can be executed on Windows only.</note>
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
            => new OptimizedPaletteQuantizer(Algorithm.MedianCut, maxColors, new Color32(backColor), alphaThreshold);

        /// <summary>
        /// Gets an <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image using Xiaolin Wu's quantizing algorithm.
        /// </summary>
        /// <param name="maxColors">The upper limit of generated colors. Must be between 2 and 65536, inclusive bounds. This parameter is optional.
        /// <br/>Default value: <c>256</c>.</param>
        /// <param name="backColor">Colors with alpha above the <paramref name="alphaThreshold"/> will be blended with this color before quantizing.
        /// The <see cref="Color.A">Color.A</see> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies a threshold value for the <see cref="Color.A">Color.A</see> property, under which a quantized color is considered transparent.
        /// If 0, then the quantized colors will never be transparent. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image by Xiaolin Wu's quantizing algorithm.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxColors"/> must be between 2 and 65536, inclusive bounds.</exception>
        /// <example>
        /// The following example demonstrates how to use the quantizer returned by this method:
        /// <note>This example requires to reference the <a href="https://www.nuget.org/packages/KGySoft.Drawing/" target="_blank">KGySoft.Drawing</a> package. When targeting .NET 7 or later it can be executed on Windows only.</note>
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
            => new OptimizedPaletteQuantizer(Algorithm.Wu, maxColors, new Color32(backColor), alphaThreshold);

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Configures the bit level per color channel to be used while optimizing the palette.
        /// If the input image is a monochromatic one, then may determine the bit depth of the result, depending on the used algorithm.
        /// Affects the quality, speed and memory usage.
        /// </summary>
        /// <param name="bitLevel">Specifies the desired bit level. If <see langword="null"/>, then the value is automatically set by the chosen algorithm.</param>
        /// <returns>An <see cref="OptimizedPaletteQuantizer"/> instance that has the specified bit level.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bitLevel"/> must be either <see langword="null"/>, or between 1 and 8.</exception>
        /// <remarks>
        /// <para>As a primary effect, <paramref name="bitLevel"/> determines the upper limit of the possible colors in the generated palette.
        /// For example, if <paramref name="bitLevel"/> is 1, then the result palette will not have more than 8 colors, or when it is 2, more than 64 colors.
        /// If you want to quantize an image using the allowed maximum of 65536 colors, then <paramref name="bitLevel"/> should be at least 6 because 5 allows up to 32768 colors.</para>
        /// <para>When using the <see cref="MedianCut">MedianCut</see> algorithm, configuring the bit level has no other effects.
        /// When using the <see cref="Octree">Octree</see> or <see cref="Wu">Wu</see>'s algorithms, <paramref name="bitLevel"/> determines also the amount of
        /// minimum distinguishable monochromatic shades. For example, when <paramref name="bitLevel"/> is 5, then up to 32 monochromatic shades can be differentiated
        /// so close shades might be merged even if the requested number of colors would allow returning all the shades.</para>
        /// <para>For the <see cref="Octree">Octree</see> algorithm the default value is the ceiling of the base 2 logarithm of the requested number of colors
        /// (eg. 1 for 2 colors, 8 for 129 or more colors). This is alright for most cases. You can increase the default value if the image has only a few but very close colors
        /// or decrease it if the image has so many colors that the quantization would use too much memory.</para>
        /// <para>For <see cref="Wu">Wu</see>'s algorithm the default value is 5 for no more than 256 colors (requires about 1.5 MB fix memory) and 6 for more colors (requires about 10 MB).
        /// This provides good enough quality in most cases but may cause visible banding if the input image is monochrome. To avoid that you can increase the bit level,
        /// which dramatically increases also the memory requirement: 7 bits requires about 80 MB memory, whereas 8 bits demands about 650 MB, regardless of
        /// the actual number of colors in the source image.</para>
        /// </remarks>
        [SuppressMessage("ReSharper", "ParameterHidesMember", Justification = "Intended, the method assigns exactly that field.")]
        public OptimizedPaletteQuantizer ConfigureBitLevel(int? bitLevel)
        {
            if (this.bitLevel == bitLevel)
                return this;
            if (bitLevel is < 1 or > 8)
                throw new ArgumentOutOfRangeException(nameof(bitLevel), PublicResources.ArgumentMustBeBetween(1, 8));
            return new OptimizedPaletteQuantizer(this, (byte?)bitLevel, LinearBlending);
        }

        /// <summary>
        /// Configures whether the generated <see cref="Palette"/> should perform blending in the linear color space instead of the sRGB color space when looking up nearest colors with alpha.
        /// </summary>
        /// <param name="useLinearBlending"><see langword="true"/> to perform blending in the linear color space; otherwise, <see langword="false"/>.</param>
        /// <returns>An <see cref="OptimizedPaletteQuantizer"/> instance that has the specified blending mode.</returns>
        public OptimizedPaletteQuantizer ConfigureBlendingMode(bool useLinearBlending)
            => useLinearBlending == LinearBlending ? this : new OptimizedPaletteQuantizer(this, bitLevel, useLinearBlending);

        #endregion

        #region Explicitly Implemented Interface Methods

        IQuantizingSession IQuantizer.Initialize(IReadableBitmapData source, IAsyncContext? context)
        {
            context ??= AsyncHelper.DefaultContext;
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
