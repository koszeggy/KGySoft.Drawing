#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PredefinedColorsQuantizer.cs
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
using System.Linq;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a quantizer to arbitrary colors. Use the static members to retrieve an instance.
    /// </summary>
    public class PredefinedColorsQuantizer : IQuantizer
    {
        #region Nested classes

        #region QuantizingSessionHiColorNoAlpha class

        private class QuantizingSessionHiColorNoAlpha : IQuantizingSession
        {
            #region Fields

            private readonly Func<Color32, Color32> transform;
            private readonly Color32 backColor;

            #endregion

            #region Properties

            public Color32[] Palette => null;

            #endregion

            #region Constructors

            internal QuantizingSessionHiColorNoAlpha(Func<Color32, Color32> transform, Color32 backColor)
            {
                this.transform = transform;
                this.backColor = backColor;
            }

            #endregion

            #region Methods

            public void Dispose()
            {
            }

            public virtual Color32 GetQuantizedColor(Color32 c) =>
                transform.Invoke(c.A == Byte.MaxValue ? c : c.BlendWithBackground(backColor));

            #endregion
        }

        #endregion

        #region QuantizingSessionHiColorSingleBitAlpha class

        private sealed class QuantizingSessionHiColorSingleBitAlpha : QuantizingSessionHiColorNoAlpha
        {
            #region Fields

            private readonly byte alphaThreshold;

            #endregion

            #region Constructors

            internal QuantizingSessionHiColorSingleBitAlpha(Func<Color32, Color32> transform, Color32 backColor, byte alphaThreshold)
                : base(transform, backColor)
            {
                this.alphaThreshold = alphaThreshold;
            }

            #endregion

            #region Methods

            public override Color32 GetQuantizedColor(Color32 c)
                => c.A >= alphaThreshold ? base.GetQuantizedColor(c) : default;

            #endregion
        }

        #endregion

        #region QuantizingSessionIndexed class

        private sealed class QuantizingSessionIndexed : IQuantizingSession
        {
            #region Fields

            private readonly Palette palette;

            #endregion

            #region Properties

            public Color32[] Palette => palette.Entries;

            #endregion

            #region Constructors

            internal QuantizingSessionIndexed(Palette palette) => this.palette = palette;

            #endregion

            #region Methods

            public void Dispose()
            {
            }

            public Color32 GetQuantizedColor(Color32 c) => palette.GetNearestColor(c);

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        #region Static Fields

        private static Color32[] rgb332Palette;
        private static Color32[] grayscalePalette;
        private static Color32[] blackAndWhitePalette;
        private static Color32[] system8BppPalette;
        private static Color32[] system4BppPalette;
        private static Color32[] system1BppPalette;

        #endregion

        #region Instance Fields

        private readonly Func<Color32, Color32> transform;
        private readonly Color32 backColor;
        private readonly byte? alphaThreshold;
        private readonly Palette palette;

        #endregion

        #endregion

        #region Properties

        private static Color32[] Rgb332Palette
        {
            get
            {
                if (rgb332Palette != null)
                    return rgb332Palette;

                var result = new Color32[256];
                for (int i = 0; i < 256; i++)
                {
                    byte r = (byte)(i & 0b11100000);
                    r |= (byte)((r >> 3) | (r >> 6));
                    byte g = (byte)((i & 0b00011100) << 3);
                    g |= (byte)((g >> 3) | (g >> 6));
                    byte b = (byte)((i & 0b00000011) << 6);
                    b |= (byte)((b >> 2) | (b >> 4) | (b >> 6));
                    result[i] = new Color32(r, g, b);
                }

                return rgb332Palette = result;
            }
        }

        private static Color32[] GrayscalePalette
        {
            get
            {
                if (grayscalePalette != null)
                    return grayscalePalette;

                var result = new Color32[256];
                for (int i = 0; i < 256; i++)
                    result[i] = Color32.FromGray((byte)i);

                return grayscalePalette = result;
            }
        }

        private static Color32[] System8BppPalette
        {
            get
            {
                if (system8BppPalette != null)
                    return system8BppPalette;

                using (var bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
                    return system8BppPalette = bmp.Palette.Entries.Select(c => new Color32(c)).ToArray();
            }
        }

        private static Color32[] System4BppPalette
        {
            get
            {
                if (system4BppPalette != null)
                    return system4BppPalette;

                using (var bmp = new Bitmap(1, 1, PixelFormat.Format4bppIndexed))
                    return system4BppPalette = bmp.Palette.Entries.Select(c => new Color32(c)).ToArray();
            }
        }

        private static Color32[] System1BppPalette
        {
            get
            {
                if (system1BppPalette != null)
                    return system1BppPalette;

                using (var bmp = new Bitmap(1, 1, PixelFormat.Format1bppIndexed))
                    return system1BppPalette = bmp.Palette.Entries.Select(c => new Color32(c)).ToArray();
            }
        }

        private static Color32[] BlackAndWhitePalette
            => blackAndWhitePalette ??= new[] { Color32.FromGray(0), Color32.FromGray(255) };

        #endregion

        #region Constructors

        private PredefinedColorsQuantizer(Func<Color32, Color32> transform, Color backColor, byte? alphaThreshold = null)
        {
            this.transform = transform;
            this.backColor = new Color32(backColor);
            this.alphaThreshold = alphaThreshold;
        }

        private PredefinedColorsQuantizer(Color[] colors, Color backColor, byte alphaThreshold = 0)
        {
            palette = new Palette(colors) { BackColor = new Color32(backColor), AlphaThreshold = alphaThreshold };
        }

        private PredefinedColorsQuantizer(Color32[] colors, Color32 backColor, byte alphaThreshold = 0)
        {
            palette = new Palette(colors) { BackColor = backColor, AlphaThreshold = alphaThreshold };
        }

        #endregion

        #region Methods

        #region Static Methods

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 24 bit RGB ones.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended by the specified <paramref name="backColor"/> before quantization. The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 24 bit RGB ones.</returns>
        public static PredefinedColorsQuantizer Rgb888(Color backColor = default)
        {
            // just returning the already blended color
            static Color32 Transform(Color32 c) => c;

            return new PredefinedColorsQuantizer(Transform, backColor);
        }

        public static PredefinedColorsQuantizer Rgb565(Color backColor = default)
        {
            static Color32 Transform(Color32 c) => new Color16Rgb565(c).ToColor32();

            return new PredefinedColorsQuantizer(Transform, backColor);
        }

        public static PredefinedColorsQuantizer Rgb555(Color backColor = default)
        {
            static Color32 Transform(Color32 c) => new Color16Rgb555(c).ToColor32();

            return new PredefinedColorsQuantizer(Transform, backColor);
        }

        public static PredefinedColorsQuantizer Argb1555(Color backColor = default, byte alphaThreshold = 128)
        {
            static Color32 Transform(Color32 c) => new Color16Argb1555(c).ToColor32();

            return new PredefinedColorsQuantizer(Transform, backColor, alphaThreshold);
        }

        public static PredefinedColorsQuantizer Rgb332(Color backColor = default)
            => new PredefinedColorsQuantizer(Rgb332Palette, new Color32(backColor));

        public static PredefinedColorsQuantizer Grayscale(Color backColor = default)
            => new PredefinedColorsQuantizer(GrayscalePalette, new Color32(backColor));

        public static PredefinedColorsQuantizer BlackAndWhite(Color backColor = default)
            => new PredefinedColorsQuantizer(BlackAndWhitePalette, new Color32(backColor));

        public static PredefinedColorsQuantizer SystemDefault8BppPalette(Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(System8BppPalette, new Color32(backColor), alphaThreshold);

        public static PredefinedColorsQuantizer SystemDefault4BppPalette(Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(System4BppPalette, new Color32(backColor), alphaThreshold);

        public static PredefinedColorsQuantizer SystemDefault1BppPalette(Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(System1BppPalette, new Color32(backColor), alphaThreshold);

        public static PredefinedColorsQuantizer FromCustomPalette(Color[] palette, Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(palette, backColor, alphaThreshold);

        #endregion

        #region Instance Methods

        IQuantizingSession IQuantizer.Initialize(IBitmapDataAccessor source)
            => palette != null ? new QuantizingSessionIndexed(palette)
                : alphaThreshold == null ? new QuantizingSessionHiColorNoAlpha(transform, backColor)
                : (IQuantizingSession)new QuantizingSessionHiColorSingleBitAlpha(transform, backColor, alphaThreshold.Value);

        #endregion

        #endregion
    }
}
