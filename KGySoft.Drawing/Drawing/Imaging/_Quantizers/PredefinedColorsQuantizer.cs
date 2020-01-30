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
    /// Represents a quantizer with predefined set of colors. Use the static members to retrieve an instance.
    /// <br/>For using optimized colors for a source image see also the <see cref="OptimizedPaletteQuantizer"/> class.
    /// </summary>
    public sealed class PredefinedColorsQuantizer : IQuantizer
    {
        #region Nested classes

        #region QuantizingSessionCustomMapping class

        private class QuantizingSessionCustomMapping : IQuantizingSession
        {
            #region Fields

            private readonly PredefinedColorsQuantizer quantizer;
            private readonly Func<Color32, Color32> transform;

            #endregion

            #region Properties

            public Palette Palette { get; }
            public Color32 BackColor => quantizer.backColor;
            public byte AlphaThreshold => quantizer.alphaThreshold;

            #endregion

            #region Constructors

            internal QuantizingSessionCustomMapping(PredefinedColorsQuantizer quantizer, Func<Color32, Color32> transform, Palette palette)
            {
                this.quantizer = quantizer;
                this.transform = transform;
                this.Palette = palette;
            }

            #endregion

            #region Methods

            public void Dispose()
            {
            }

            public virtual Color32 GetQuantizedColor(Color32 c)
                => c.A < AlphaThreshold
                    ? default
                    : transform.Invoke(c.A == Byte.MaxValue ? c : c.BlendWithBackground(BackColor));

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
            public Color32 BackColor => quantizer.backColor;
            public byte AlphaThreshold => quantizer.alphaThreshold;

            #endregion

            #region Constructors

            internal QuantizingSessionIndexed(PredefinedColorsQuantizer quantizer, Palette palette)
            {
                this.quantizer = quantizer;
                this.Palette = palette;
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

        #endregion

        #region Fields

        #region Static Fields

        private static Color32[] rgb332Palette;
        private static Color32[] grayscale256Palette;
        private static Color32[] grayscale16Palette;
        private static Color32[] grayscale4Palette;
        private static Color32[] blackAndWhitePalette;
        private static Color32[] system8BppPalette;
        private static Color32[] system4BppPalette;
        private static Color32[] system1BppPalette;

        #endregion

        #region Instance Fields

        private readonly Func<Color32, Color32> transform;
        private readonly Color32 backColor;
        private readonly byte alphaThreshold;
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

        private static Color32[] Grayscale256Palette
        {
            get
            {
                if (grayscale256Palette != null)
                    return grayscale256Palette;

                var result = new Color32[256];
                for (int i = 0; i < 256; i++)
                    result[i] = Color32.FromGray((byte)i);

                return grayscale256Palette = result;
            }
        }

        private static Color32[] Grayscale16Palette
        {
            get
            {
                if (grayscale16Palette != null)
                    return grayscale16Palette;

                var result = new Color32[16];
                for (int i = 0; i < 16; i++)
                    result[i] = Color32.FromGray((byte)((i << 4) | i));

                return grayscale16Palette = result;
            }
        }

        private static Color32[] Grayscale4Palette
        {
            get
            {
                if (grayscale4Palette != null)
                    return grayscale4Palette;

                var result = new Color32[4];
                for (int i = 0; i < 4; i++)
                {
                    byte br = (byte)((i & 0b00000011) << 6);
                    br |= (byte)((br >> 2) | (br >> 4) | (br >> 6));
                    result[i] = Color32.FromGray(br);
                }

                return grayscale4Palette = result;
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

        private static Color32[] BlackAndWhitePalette => blackAndWhitePalette ??= new[] { Color32.Black, Color32.White };

        #endregion

        #region Constructors

        private PredefinedColorsQuantizer(Palette palette)
        {
            this.palette = palette ?? throw new ArgumentNullException(nameof(palette), PublicResources.ArgumentNull);
            backColor = palette.BackColor;
            alphaThreshold = palette.AlphaThreshold;
        }

        private PredefinedColorsQuantizer(Color[] colors, Color backColor, byte alphaThreshold = 0)
            : this(new Palette(colors, backColor, alphaThreshold))
        {
        }

        private PredefinedColorsQuantizer(Color32[] colors, Color32 backColor, byte alphaThreshold = 0)
            : this(new Palette(colors, backColor, alphaThreshold))
        {
        }

        private PredefinedColorsQuantizer(Func<Color32, Color32> transform, Color32 backColor, byte alphaThreshold = 0)
        {
            this.transform = transform;
            this.backColor = Color32.FromArgb(Byte.MaxValue, backColor);
            this.alphaThreshold = alphaThreshold;
        }

        #endregion

        #region Methods

        #region Static Methods

        #region Public Methods

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 24 bit RGB ones.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended by the specified <paramref name="backColor"/> before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes colors to 24 bit RGB ones.</returns>
        public static PredefinedColorsQuantizer Rgb888(Color backColor = default)
        {
            // just returning the already blended color
            static Color32 Transform(Color32 c) => c;

            return new PredefinedColorsQuantizer(Transform, new Color32(backColor));
        }

        public static PredefinedColorsQuantizer Rgb565(Color backColor = default)
        {
            static Color32 Transform(Color32 c) => new Color16Rgb565(c).ToColor32();

            return new PredefinedColorsQuantizer(Transform, new Color32(backColor));
        }

        public static PredefinedColorsQuantizer Rgb555(Color backColor = default)
        {
            static Color32 Transform(Color32 c) => new Color16Rgb555(c).ToColor32();

            return new PredefinedColorsQuantizer(Transform, new Color32(backColor));
        }

        public static PredefinedColorsQuantizer Argb1555(Color backColor = default, byte alphaThreshold = 128)
        {
            static Color32 Transform(Color32 c) => new Color16Argb1555(c).ToColor32();

            return new PredefinedColorsQuantizer(Transform, new Color32(backColor), alphaThreshold);
        }

        public static PredefinedColorsQuantizer Rgb332(Color backColor = default)
            => new PredefinedColorsQuantizer(Rgb332Palette, new Color32(backColor));

        public static PredefinedColorsQuantizer Grayscale(Color backColor = default)
        {
            var bg = new Color32(backColor);

            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(bg);
                return c.GetBrightness();
            }

            return new PredefinedColorsQuantizer(new Palette(Grayscale256Palette, bg, 0, GetNearestColorIndex));
        }

        public static PredefinedColorsQuantizer Grayscale16(Color backColor = default)
        {
            var bg = new Color32(backColor);

            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(bg);
                return c.GetBrightness() >> 4;
            }

            return new PredefinedColorsQuantizer(new Palette(Grayscale16Palette, bg, 0, GetNearestColorIndex));
        }

        public static PredefinedColorsQuantizer Grayscale4(Color backColor = default)
        {
            var bg = new Color32(backColor);

            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(bg);
                return c.GetBrightness() >> 6;
            }

            return new PredefinedColorsQuantizer(new Palette(Grayscale4Palette, bg, 0, GetNearestColorIndex));
        }

        /// <summary>
        /// Gets a <see cref="PredefinedColorsQuantizer"/> instance that quantizes every color to black or white.
        /// </summary>
        /// <param name="backColor">Colors with alpha (transparency) will be blended by the specified <paramref name="backColor"/> before quantization.
        /// The <see cref="Color.A"/> property of the background color is ignored. This parameter is optional.
        /// <br/>Default value: <see cref="Color.Empty"/>, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="whiteThreshold">Non completely black and white pixels are measured by brightness, which must be greater or equal to the specified
        /// threshold to consider the pixel white.</param>
        /// <returns>A <see cref="PredefinedColorsQuantizer"/> instance that quantizes every color to black or white.</returns>
        public static PredefinedColorsQuantizer BlackAndWhite(Color backColor = default, byte whiteThreshold = 128)
        {
            var bg = new Color32(backColor);

            int GetNearestColorIndex(Color32 c)
            {
                if (c.A < Byte.MaxValue)
                    c = c.BlendWithBackground(bg);

                return c == Color32.Black ? 0
                    : c == Color32.White ? 1
                    : c.GetBrightness() >= whiteThreshold ? 1 : 0;
            }

            return new PredefinedColorsQuantizer(new Palette(BlackAndWhitePalette, bg, 0, GetNearestColorIndex));
        }

        public static PredefinedColorsQuantizer SystemDefault8BppPalette(Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(System8BppPalette, new Color32(backColor), alphaThreshold);

        public static PredefinedColorsQuantizer SystemDefault4BppPalette(Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(System4BppPalette, new Color32(backColor), alphaThreshold);

        public static PredefinedColorsQuantizer SystemDefault1BppPalette(Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(System1BppPalette, new Color32(backColor), alphaThreshold);

        public static PredefinedColorsQuantizer FromCustomPalette(Color[] palette, Color backColor = default, byte alphaThreshold = 128)
            => new PredefinedColorsQuantizer(palette, backColor, alphaThreshold);

        public static PredefinedColorsQuantizer FromCustomPalette(Palette palette) => new PredefinedColorsQuantizer(palette);

        public static PredefinedColorsQuantizer FromBitmap(Bitmap bitmap, Color backColor = default, byte alphaThreshold = 128)
        {
            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format16bppArgb1555:
                    return Argb1555(backColor, alphaThreshold);
                case PixelFormat.Format16bppRgb565:
                    return Rgb565(backColor);
                case PixelFormat.Format16bppRgb555:
                    return Rgb555(backColor);
                case PixelFormat.Format16bppGrayScale:
                    return Grayscale(backColor);
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    return FromCustomPalette(bitmap.Palette.Entries, backColor, alphaThreshold);
                default:
                    return Rgb888(backColor);
            }
        }

        #endregion

        #region Internal Methods

        internal static PredefinedColorsQuantizer FromBitmapData(BitmapDataAccessorBase bitmapData)
        {
            switch (bitmapData.PixelFormat)
            {
                case PixelFormat.Format16bppArgb1555:
                    return Argb1555(bitmapData.BackColor.ToColor(), bitmapData.AlphaThreshold);
                case PixelFormat.Format16bppRgb565:
                    return Rgb565(bitmapData.BackColor.ToColor());
                case PixelFormat.Format16bppRgb555:
                    return Rgb555(bitmapData.BackColor.ToColor());
                case PixelFormat.Format16bppGrayScale:
                    return Grayscale(bitmapData.BackColor.ToColor());
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    return FromCustomPalette(bitmapData.Palette);
                default:
                    return Rgb888(bitmapData.BackColor.ToColor());
            }
        }

        #endregion

        #endregion

        #region Instance Methods

        IQuantizingSession IQuantizer.Initialize(IReadableBitmapData source)
            => transform != null
                ? new QuantizingSessionCustomMapping(this, transform, palette)
                : (IQuantizingSession)new QuantizingSessionIndexed(this, palette);

        #endregion

        #endregion
    }
}
