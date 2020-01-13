#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedPaletteQuantizer.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a quantizer with an optimized set of colors best matching to the original image.
    /// Use the static members to retrieve an instance.
    /// <br/>For using predefined colors see also the <see cref="PredefinedColorsQuantizer"/> class.
    /// </summary>
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

        private interface IOptimizedPaletteQuantizer
        {
            #region Methods

            void Initialize(int requestedColors, IBitmapDataAccessor source);

            void AddColor(Color32 c);

            Color32[] GeneratePalette();

            #endregion
        }

        #endregion

        #region Nested classes

        private sealed class OptimizedPaletteQuantizerSession<TAlg> : IQuantizingSession
            where TAlg : IOptimizedPaletteQuantizer, new()
        {
            #region Fields

            private readonly OptimizedPaletteQuantizer quantizer;
            private readonly Palette palette;

            #endregion

            #region Properties

            public Color32[] Palette => palette.Entries;
            public Color32 BackColor => quantizer.backColor;
            public byte AlphaThreshold => quantizer.alphaThreshold;

            #endregion

            #region Constructors

            public OptimizedPaletteQuantizerSession(OptimizedPaletteQuantizer quantizer, IBitmapDataAccessor source)
            {
                this.quantizer = quantizer;
                palette = InitializePalette(source);
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose()
            {
            }

            public Color32 GetQuantizedColor(Color32 origColor) => palette.GetNearestColor(origColor);

            #endregion

            #region Private Methods

            private Palette InitializePalette(IBitmapDataAccessor source)
            {
                TAlg alg = new TAlg();
                alg.Initialize(quantizer.maxColors, source);
                int width = source.Width;
                IBitmapDataRow row = source.FirstRow;
                do
                {
                    // TODO: parallel if possible
                    for (int x = 0; x < width; x++)
                    {
                        Color32 c = row[x];

                        // handling alpha including full transparency
                        if (c.A != Byte.MaxValue)
                            c = c.A < quantizer.alphaThreshold ? default : c.BlendWithBackground(quantizer.backColor);
                        alg.AddColor(c);
                    }
                } while (row.MoveNextRow());

                return new Palette(alg.GeneratePalette())
                {
                    AlphaThreshold = quantizer.alphaThreshold,
                    BackColor = quantizer.backColor
                };
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

        #endregion

        #region Constructors

        private OptimizedPaletteQuantizer(Algorithm algorithm, int maxColors, Color backColor, byte alphaThreshold)
        {
            if (maxColors < 2 || maxColors > 256)
                throw new ArgumentOutOfRangeException(nameof(maxColors), PublicResources.ArgumentMustBeBetween(2, 256));
            this.algorithm = algorithm;
            this.maxColors = maxColors;
            this.backColor = new Color32(backColor);
            this.alphaThreshold = alphaThreshold;
        }

        #endregion

        #region Methods

        #region Static Methods

        public static OptimizedPaletteQuantizer Octree(int maxColors = 256, Color backColor = default, byte alphaThreshold = 128)
            => new OptimizedPaletteQuantizer(Algorithm.Octree, maxColors, backColor, alphaThreshold);

        public static OptimizedPaletteQuantizer MedianCut(int maxColors = 256, Color backColor = default, byte alphaThreshold = 128)
            => new OptimizedPaletteQuantizer(Algorithm.MedianCut, maxColors, backColor, alphaThreshold);

        /// <summary>
        /// Gets an <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image
        /// by using Xiaolin Wu's quantizing algorithm.
        /// </summary>
        /// <param name="maxColors">The upper limit of generated colors. Must be between <c>2</c> and <c>256</c>, inclusive. This parameter is optional.
        /// <br/>Default value: <c>256</c>.</param>
        /// <param name="backColor">Colors with alpha above the <paramref name="alphaThreshold"/> will be blended with this color before quantizing.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">Specifies the threshold for determining whether a quantized color will be transparent.
        /// <br/>See the also <strong>Remarks</strong> section if the <see cref="IQuantizingSession.AlphaThreshold"/> property for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A <see cref="OptimizedPaletteQuantizer"/> instance that quantizes colors of an image by Xiaolin Wu's quantizing algorithm.</returns>
        public static OptimizedPaletteQuantizer Wu(int maxColors = 256, Color backColor = default, byte alphaThreshold = 128)
            => new OptimizedPaletteQuantizer(Algorithm.Wu, maxColors, backColor, alphaThreshold);

        #endregion

        #region Instance Methods

        IQuantizingSession IQuantizer.Initialize(IBitmapDataAccessor source)
        {
            switch (algorithm)
            {
                case Algorithm.Octree:
                    return new OptimizedPaletteQuantizerSession<OctreeQuantizer>(this, source);
                case Algorithm.MedianCut:
                    return new OptimizedPaletteQuantizerSession<MedianCutQuantizer>(this, source);
                case Algorithm.Wu:
                    return new OptimizedPaletteQuantizerSession<WuQuantizer>(this, source);
                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected algorithm: {algorithm}"));
            }
        }

        #endregion

        #endregion
    }
}
