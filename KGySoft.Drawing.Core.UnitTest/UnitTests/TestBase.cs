#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TestBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
#if !USE_SKIA
using System.Drawing.Imaging;
#endif
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
#if USE_SKIA
using System.Runtime.InteropServices;
#endif

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#if USE_SKIA
using SkiaSharp;
#endif

#endregion

namespace KGySoft.Drawing.UnitTests
{
    public abstract class TestBase
    {
        #region Nested Structs

#if USE_SKIA
        /// <summary>
        /// Represents the same color as <see cref="PColor32"/> with RGBA order.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct ColorRgba8888
        {
            #region Fields

            [FieldOffset(0)]private readonly byte r;
            [FieldOffset(1)]private readonly byte g;
            [FieldOffset(2)]private readonly byte b;
            [FieldOffset(3)]private readonly byte a;

            #endregion

            #region Constructors

            internal ColorRgba8888(Color32 c)
            {
                r = c.R;
                g = c.G;
                b = c.B;
                a = c.A;
            }

            internal ColorRgba8888(PColor32 c)
            {
                r = c.R;
                g = c.G;
                b = c.B;
                a = c.A;
            }

            #endregion

            #region Methods

            internal Color32 AsColor32() => new Color32(a, r, g, b);
            internal PColor32 AsPColor32() => new PColor32(a, r, g, b);

            #endregion
        } 
#endif

        #endregion

        #region Fields

        private static readonly bool addTimestamp = true;

        private static Color32[] paletteRgb565;
        private static Color32[] paletteArgb4444;
        private static Color32[] paletteArgb2222;
        private static Color32[] paletteGray8Alpha;

        #endregion

        #region Properties

        protected static bool SaveToFile => false;

        protected static Color32[] PaletteArgb4444
        {
            get
            {
                if (paletteArgb4444 == null)
                {
                    var colors = new Color32[65536];
                    for (int a = 15; a >= 0; a--)
                    for (int r = 0; r < 16; r++)
                    for (int g = 0; g < 16; g++)
                    for (int b = 0; b < 16; b++)
                        colors[((15 - a) << 12) | (r << 8) | (g << 4) | b] = new Color32((byte)(a * 17), (byte)(r * 17), (byte)(g * 17), (byte)(b * 17));
                    paletteArgb4444 = colors;
                }

                return paletteArgb4444;
            }
        }

        protected static Color32[] PaletteRgb565
        {
            get
            {
                if (paletteRgb565 == null)
                {
                    var colors = new Color32[65536];
                    for (int i = 0; i < 65536; i++)
                    {
                        byte r = (byte)((i & 0b11111000_00000000) >> 8);
                        r |= (byte)(r >> 5);
                        byte g = (byte)((i & 0b00000111_11100000) >> 3);
                        g |= (byte)(g >> 6);
                        byte b = (byte)((i & 0b00011111) << 3);
                        b |= (byte)(b >> 5);
                        colors[i] = new Color32(r, g, b);
                    }

                    paletteRgb565 = colors;
                }

                return paletteRgb565;
            }
        }

        protected static Color32[] PaletteArgb2222
        {
            get
            {
                if (paletteArgb2222 == null)
                {
                    var colors = new Color32[256];
                    for (int a = 3; a >= 0; a--)
                    for (int r = 0; r < 4; r++)
                    for (int g = 0; g < 4; g++)
                    for (int b = 0; b < 4; b++)
                        colors[((3 - a) << 6) | (r << 4) | (g << 2) | b] = new Color32((byte)(a * 85), (byte)(r * 85), (byte)(g * 85), (byte)(b * 85));
                    paletteArgb2222 = colors;
                }

                return paletteArgb2222;
            }
        }

        protected static Color32[] PaletteGray8Alpha
        {
            get
            {
                if (paletteGray8Alpha == null)
                {
                    var colors = new Color32[256];
                    for (int a = 15; a >= 0; a--)
                    for (int br = 0; br < 16; br++)
                        colors[((15 - a) << 4) | br] = Color32.FromArgb((byte)(a * 17), Color32.FromGray((byte)(br * 17)));
                    paletteGray8Alpha = colors;
                }

                return paletteGray8Alpha;
            }
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected static void SaveStream(string streamName, MemoryStream ms, string extension = "gif", [CallerMemberName]string testName = null)
        {
            if (!SaveToFile)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fileName = Path.Combine(dir, $"{testName}{(streamName == null ? null : $"_{streamName}")}{GetTimestamp()}.{extension}");
            CheckFileName(fileName);
            using (var fs = File.Create(fileName))
                ms.WriteTo(fs);
        }

        protected static void SaveBitmapData(string imageName, IReadWriteBitmapData source, [CallerMemberName]string testName = null)
        {
            if (!SaveToFile)
                return;

            if (source.PixelFormat.Indexed && source.PixelFormat.BitsPerPixel <= 8 && !source.Palette!.HasMultiLevelAlpha)
            {
                SaveGif(imageName, source, testName);
                return;
            }

            SaveWithCodec(imageName, source, testName);
        }

        protected static void SaveGif(string imageName, IReadWriteBitmapData source, [CallerMemberName]string testName = null)
        {
            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string fileName = Path.Combine(dir, $"{testName}{(imageName == null ? null : $"_{imageName}")}{GetTimestamp()}");
            CheckFileName($"{fileName}.gif");
            Assert.IsTrue(source.PixelFormat.BitsPerPixel <= 8 && source.Palette != null);
            using var stream = File.Create($"{fileName}.gif");
            GifEncoder.EncodeImage(source, stream);
        }

        protected static void EncodeAnimatedGif(AnimatedGifConfiguration config, bool performCompare = true, string streamName = null, [CallerMemberName]string testName = null)
        {
            using var ms = new MemoryStream();
            GifEncoder.EncodeAnimation(config, ms);
            SaveStream(streamName, ms, testName: testName);
            IReadableBitmapData[] sourceFrames = config.Frames.ToArray(); // actually 2nd enumeration
            ms.Position = 0;

            var actualFrames = ExtractGifFrames(ms);
            try
            {
                int expectedLength = sourceFrames.Length + (config.AnimationMode == AnimationMode.PingPong ? Math.Max(0, sourceFrames.Length - 2) : 0);
                Assert.AreEqual(expectedLength, actualFrames.Length);
#if WINDOWS
                if (!performCompare)
                    return;

                var quantizer = config.Quantizer ?? OptimizedPaletteQuantizer.Wu();
                for (int i = 0; i < actualFrames.Length; i++)
                {
                    IReadableBitmapData sourceFrame = sourceFrames[i];
                    if (sourceFrame.IsDisposed)
                        continue;
                    Console.Write($"Frame #{i}: ");
                    using IReadableBitmapData actualFrame = ToBitmapData(actualFrames[i]);
                    IReadWriteBitmapData expectedFrame;
                    if (sourceFrame.Size == actualFrame.Size)
                        expectedFrame = sourceFrames[i].Clone(KnownPixelFormat.Format8bppIndexed, quantizer, config.Ditherer);
                    else
                    {
                        Assert.AreNotEqual(AnimationFramesSizeHandling.ErrorIfDiffers, config.SizeHandling);
                        expectedFrame = BitmapDataFactory.CreateBitmapData(actualFrame.Size);
                        if (config.SizeHandling == AnimationFramesSizeHandling.Resize)
                            sourceFrame.DrawInto(expectedFrame, new Rectangle(Point.Empty, expectedFrame.Size), quantizer, config.Ditherer);
                        else
                            sourceFrame.DrawInto(expectedFrame, new Point(expectedFrame.Width / 2 - sourceFrame.Width / 2, expectedFrame.Height / 2 - expectedFrame.Width / 2), quantizer, config.Ditherer);
                    }

                    try
                    {
                        AssertAreEqual(expectedFrame, actualFrame, true);
                    }
                    finally
                    {
                        expectedFrame.Dispose();
                    }

                    Console.WriteLine("Equals");
                }
#endif
            }
            finally
            {
                actualFrames.ForEach(f => f.Dispose());
            }
        }

#if USE_SKIA
        protected static SKBitmap[] ExtractGifFrames(Stream stream)
        {
            var codec = SKCodec.Create(stream);
            var info = codec.Info;
            var frames = codec.FrameInfo;
            var result = new SKBitmap[frames.Length];
            for (int i = 0; i < result.Length; i++)
            {
                var options = new SKCodecOptions(i);
                var frame = new SKBitmap(codec.Info);
                codec.GetPixels(info, frame.GetPixels(), options);
                result[i] = frame;
            }

            return result;
        }
#else
        protected static Bitmap[] ExtractGifFrames(Stream stream)
        {
            var image = new Bitmap(stream);
            var dimension = FrameDimension.Time;
            int frameCount = image.FrameDimensionsList.Length == 0 ? 1 : image.GetFrameCount(dimension);

            if (frameCount <= 1)
                return new Bitmap[] { image.Clone(new Rectangle(Point.Empty, image.Size), image.PixelFormat) };

            // extracting frames
            Bitmap[] result = new Bitmap[frameCount];
            for (int frame = 0; frame < frameCount; frame++)
            {
                image.SelectActiveFrame(dimension, frame);
                result[frame] = image.Clone(new Rectangle(Point.Empty, image.Size), image.PixelFormat);
            }

            // selecting first frame again
            image.SelectActiveFrame(dimension, 0);

            return result;
        }
#endif

        protected static IReadWriteBitmapData GenerateAlphaGradientBitmapData(Size size)
        {
            var result = BitmapDataFactory.CreateBitmapData(size);
            GenerateAlphaGradient(result);
            return result;
        }

        protected static IReadWriteBitmapData GenerateAlphaGradientBitmapData(Size size, bool linear)
        {
            var result = BitmapDataFactory.CreateBitmapData(size, linear ? KnownPixelFormat.Format128bppRgba : KnownPixelFormat.Format32bppArgb, linear ? WorkingColorSpace.Linear: WorkingColorSpace.Srgb);
            if (linear)
                GenerateAlphaGradientLinear(result);
            else
                GenerateAlphaGradient(result);
            return result;
        }

        protected static void GenerateAlphaGradient(IReadWriteBitmapData bitmapData)
        {
            var firstRow = bitmapData.FirstRow;
            float ratio = 255f / (bitmapData.Width / 6f);
            float limit = bitmapData.Width / 6f;

            for (int x = 0; x < bitmapData.Width; x++)
            {
                // red -> yellow
                if (x < limit)
                    firstRow[x] = new Color32(255, (x * ratio).ClipToByte(), 0);
                // yellow -> green
                else if (x < limit * 2)
                    firstRow[x] = new Color32((255 - (x - limit) * ratio).ClipToByte(), 255, 0);
                // green -> cyan
                else if (x < limit * 3)
                    firstRow[x] = new Color32(0, 255, ((x - limit * 2) * ratio).ClipToByte());
                // cyan -> blue
                else if (x < limit * 4)
                    firstRow[x] = new Color32(0, (255 - (x - limit * 3) * ratio).ClipToByte(), 255);
                // blue -> magenta
                else if (x < limit * 5)
                    firstRow[x] = new Color32(((x - limit * 4) * ratio).ClipToByte(), 0, 255);
                // magenta -> red
                else
                    firstRow[x] = new Color32(255, 0, (255 - (x - limit * 5) * ratio).ClipToByte());
            }

            if (bitmapData.Height < 2)
                return;

            var row = bitmapData.GetMovableRow(1);
            ratio = 255f / bitmapData.Height;
            do
            {
                byte a = (255 - row.Index * ratio).ClipToByte();
                for (int x = 0; x < bitmapData.Width; x++)
                    row[x] = Color32.FromArgb(a, firstRow[x]);

            } while (row.MoveNextRow());
        }

        protected static void GenerateAlphaGradientLinear(IReadWriteBitmapData bitmapData)
        {
            var firstRow = bitmapData.FirstRow;
            float ratio = 1f / (bitmapData.Width / 6f);
            float limit = bitmapData.Width / 6f;

            for (int x = 0; x < bitmapData.Width; x++)
            {
                // red -> yellow
                if (x < limit)
                    firstRow.SetColorF(x, new ColorF(1, 1, (x * ratio), 0));
                // yellow -> green
                else if (x < limit * 2)
                    firstRow.SetColorF(x, new ColorF(1, (1 - (x - limit) * ratio), 1, 0));
                // green -> cyan
                else if (x < limit * 3)
                    firstRow.SetColorF(x, new ColorF(1, 0, 1, ((x - limit * 2) * ratio)));
                // cyan -> blue
                else if (x < limit * 4)
                    firstRow.SetColorF(x, new ColorF(1, 0, (1 - (x - limit * 3) * ratio), 1));
                // blue -> magenta
                else if (x < limit * 5)
                    firstRow.SetColorF(x, new ColorF(1, ((x - limit * 4) * ratio), 0, 1));
                // magenta -> red
                else
                    firstRow.SetColorF(x, new ColorF(1, 1, 0, (1 - (x - limit * 5) * ratio)));
            }

            if (bitmapData.Height < 2)
                return;

            var row = bitmapData.GetMovableRow(1);
            ratio = 1f / bitmapData.Height;
            do
            {
                float a = 1f - row.Index * ratio;
                for (int x = 0; x < bitmapData.Width; x++)
                    row.SetColorF(x, ColorF.FromArgb(a, firstRow.GetColorF(x)));

            } while (row.MoveNextRow());
        }

        protected static IReadWriteBitmapData GetInfoIcon256() => GetBitmapData(@"..\..\..\..\Help\Images\Information256.png");
        protected static IReadWriteBitmapData GetInfoIcon64() => GetBitmapData(@"..\..\..\..\Help\Images\Information64.png");
        protected static IReadWriteBitmapData GetInfoIcon48() => GetBitmapData(@"..\..\..\..\Help\Images\Information48.png");
        protected static IReadWriteBitmapData GetInfoIcon16() => GetBitmapData(@"..\..\..\..\Help\Images\Information16.png");
        protected static IReadWriteBitmapData[] GetInfoIconImages() => new[]
        {
            GetInfoIcon256(),
            GetInfoIcon64(),
            GetInfoIcon48(),
            GetBitmapData(@"..\..\..\..\Help\Images\Information32.png"),
            GetBitmapData(@"..\..\..\..\Help\Images\Information24.png"),
            GetBitmapData(@"..\..\..\..\Help\Images\Information20.png"),
            GetInfoIcon16()
        };

        protected static IReadWriteBitmapData GetShieldIcon16() => GetBitmapData(@"..\..\..\..\Help\Images\Shield16.png");
        protected static IReadWriteBitmapData GetShieldIcon256() => GetBitmapData(@"..\..\..\..\Help\Images\Shield256.png");

#if USE_SKIA
        protected static SKBitmap LoadBitmap(Stream stream)
        {
            using var data = SKData.Create(stream); // SKBitmap.Decode disposes the stream, so using SKData to prevent it: https://github.com/mono/SkiaSharp/issues/2263
            return SKBitmap.Decode(data);
        }

        protected static IReadWriteBitmapData ToBitmapData(SKBitmap bmp, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default, Color32 backColor = default)
        {
            var info = bmp.Info;
            Assert.IsTrue(info.ColorType is SKColorType.Bgra8888 or SKColorType.Rgba8888);
            Assert.IsTrue(info.ColorSpace?.IsSrgb != false);

            if (info.ColorType == SKColorType.Bgra8888)
                return BitmapDataFactory.CreateBitmapData(bmp.GetPixels(), new Size(info.Width, info.Height), info.RowBytes,
                    info.AlphaType is SKAlphaType.Premul ? KnownPixelFormat.Format32bppPArgb : KnownPixelFormat.Format32bppArgb,
                    workingColorSpace, backColor).Clone();

            // Rgba8888 (used eg. on Android)
            return BitmapDataFactory.CreateBitmapData(bmp.GetPixels(), new Size(info.Width, info.Height), info.RowBytes,
                new CustomBitmapDataConfig
                {
                    PixelFormat = new PixelFormatInfo(32) { HasPremultipliedAlpha = info.AlphaType is SKAlphaType.Premul },
                    BackColor = backColor,
                    BackBufferIndependentPixelAccess = true,
                    WorkingColorSpace = workingColorSpace,
                    RowGetColor32 = info.AlphaType is not SKAlphaType.Premul ? (row, x) => row.UnsafeGetRefAs<ColorRgba8888>(x).AsColor32() : null,
                    RowSetColor32 = info.AlphaType is not SKAlphaType.Premul ? (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888>(x) = new ColorRgba8888(c) : null,
                    RowGetPColor32 = info.AlphaType is SKAlphaType.Premul ? (row, x) => row.UnsafeGetRefAs<ColorRgba8888>(x).AsPColor32() : null,
                    RowSetPColor32 = info.AlphaType is SKAlphaType.Premul ? (row, x, c) => row.UnsafeGetRefAs<ColorRgba8888>(x) = new ColorRgba8888(c) : null,
                }).Clone();
        }
#else
        protected static Bitmap LoadBitmap(Stream stream) => new Bitmap(stream);

        protected static IReadWriteBitmapData ToBitmapData(Bitmap bmp, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default, Color32 backColor = default)
        {
            Assert.IsTrue(bmp.PixelFormat.In(PixelFormat.Format32bppArgb, PixelFormat.Format8bppIndexed, PixelFormat.Format24bppRgb));
            Palette palette = ((int)bmp.PixelFormat & (int)PixelFormat.Indexed) != 0
                ? new Palette(bmp.Palette.Entries.Select(c => c.ToColor32()), workingColorSpace, backColor)
                : null;
            BitmapData bitmapData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadOnly, bmp.PixelFormat);
            using var src = palette == null
                ? BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, bmp.Size, bitmapData.Stride, (KnownPixelFormat)bmp.PixelFormat, workingColorSpace, backColor, disposeCallback: () => bmp.UnlockBits(bitmapData))
                : BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, bmp.Size, bitmapData.Stride, (KnownPixelFormat)bmp.PixelFormat, palette, disposeCallback: () => bmp.UnlockBits(bitmapData));
            return src.Clone();
        }
#endif

        protected static IReadWriteBitmapData GetBitmapData(string fileName, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default, Color32 backColor = default)
        {
#if !WINDOWS
            fileName = fileName.Replace('\\', Path.DirectorySeparatorChar);
#endif
            using var bmp = LoadBitmap(File.OpenRead(Path.Combine(Files.GetExecutingPath(), fileName)));
            return ToBitmapData(bmp, workingColorSpace, backColor);
        }

        protected static void AssertAreEqual(IReadableBitmapData source, IReadableBitmapData target, bool allowDifferentPixelFormats = false, Rectangle sourceRectangle = default, Point targetLocation = default, int tolerance = 0)
        {
            if (sourceRectangle == default)
                sourceRectangle = new Rectangle(Point.Empty, source.Size);

            Assert.AreEqual(sourceRectangle.Size, target.Size);
            if (!allowDifferentPixelFormats)
                Assert.AreEqual(source.PixelFormat, target.PixelFormat);
            
            IReadableBitmapDataRowMovable rowSrc = source.GetMovableRow(sourceRectangle.Y);
            IReadableBitmapDataRowMovable rowDst = target.GetMovableRow(targetLocation.Y);

            do
            {
                if (tolerance > 0)
                {
                    for (int x = 0; x < sourceRectangle.Width; x++)
                    {
                        Color32 c1 = rowSrc[x + sourceRectangle.X];
                        Color32 c2 = rowDst[x + targetLocation.X];

                        // this is faster than the asserts below
                        if (!(c1.A == 0 && c2.A == 0) &&
                            (c1.A != c2.A
                            || Math.Abs(c1.R - c2.R) > tolerance
                            || Math.Abs(c1.G - c2.G) > tolerance
                            || Math.Abs(c1.B - c2.B) > tolerance))
                            Assert.Fail($"Diff at {x}; {rowSrc.Index}: {c1} vs. {c2}");

                        //Assert.AreEqual(c1.A, c2.A, $"Diff at {x}; {rowSrc.Index}");
                        //Assert.That(() => Math.Abs(c1.R - c2.R), new LessThanOrEqualConstraint(tolerance), $"Diff at {x}; {rowSrc.Index}");
                        //Assert.That(() => Math.Abs(c1.G - c2.G), new LessThanOrEqualConstraint(tolerance), $"Diff at {x}; {rowSrc.Index}");
                        //Assert.That(() => Math.Abs(c1.B - c2.B), new LessThanOrEqualConstraint(tolerance), $"Diff at {x}; {rowSrc.Index}");
                    }

                    continue;
                }

                for (int x = 0; x < sourceRectangle.Width; x++)
                {
                    Color32 c1 = rowSrc[x + sourceRectangle.X];
                    Color32 c2 = rowDst[x + targetLocation.X];
                    if (!(c1.A == 0 && c2.A == 0) && c1.ToArgb() != c2.ToArgb())
                        Assert.Fail($"Diff at {x}; {rowSrc.Index}: {c1} vs. {c2}");
                    //Assert.AreEqual(rowSrc[x + sourceRectangle.X], rowDst[x + targetLocation.X], $"Diff at {x}; {rowSrc.Index}");
                }
            } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());
        }

        #endregion

        #region Private Methods

        private static string GetTimestamp() => addTimestamp ? $".{DateTime.Now:yyyyMMddHHmmssffff}" : String.Empty;
        private static void CheckFileName(string fileName) => Assert.IsFalse(File.Exists(fileName), $"File already exists: {fileName}");

#if USE_SKIA
        private static void SaveWithCodec(string imageName, IReadWriteBitmapData source, string testName)
        {
            using var bmp = new SKBitmap(source.Width, source.Height, SKColorType.Bgra8888, source.HasAlpha() ? SKAlphaType.Unpremul : SKAlphaType.Opaque);

            // copying content
            using (var target = BitmapDataFactory.CreateBitmapData(bmp.GetPixels(), source.Size, bmp.Info.RowBytes, source.HasAlpha() ? KnownPixelFormat.Format32bppArgb : KnownPixelFormat.Format32bppRgb,
                disposeCallback: bmp.NotifyPixelsChanged))
            {
                source.CopyTo(target);
            }

            SaveBitmap(imageName, bmp, testName);
        }

        private static void SaveBitmap(string imageName, SKBitmap bitmap, string testName)
        {
            if (!SaveToFile)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string fileName = Path.Combine(dir, $"{testName}{(imageName == null ? null : $"_{imageName}")}{GetTimestamp()}");

            CheckFileName($"{fileName}.png");
            using var file = File.Create($"{fileName}.png");
            bitmap.Encode(file, SKEncodedImageFormat.Png, 100);
        }
#else
        private static void SaveWithCodec(string imageName, IReadWriteBitmapData source, string testName)
        {
            using var bmp = new Bitmap(source.Width, source.Height,
                source.HasAlpha() ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);

            // copying content
            BitmapData bitmapData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.WriteOnly, bmp.PixelFormat);
            using (var target = BitmapDataFactory.CreateBitmapData(bitmapData.Scan0, bmp.Size, bitmapData.Stride, (KnownPixelFormat)bmp.PixelFormat,
                       source.Palette, disposeCallback: () => bmp.UnlockBits(bitmapData)))
            {
                source.CopyTo(target);
            }

            SaveBitmap(imageName, bmp, testName);
        }

        private static void SaveBitmap(string imageName, Bitmap bitmap, string testName)
        {
            if (!SaveToFile)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string fileName = Path.Combine(dir, $"{testName}{(imageName == null ? null : $"_{imageName}")}{GetTimestamp()}");

            int bpp = Image.GetPixelFormatSize(bitmap.PixelFormat);
            if (bpp > 8)
            {
                CheckFileName($"{fileName}.png");
                bitmap.Save($"{fileName}.png", ImageFormat.Png);
                return;
            }

            // to prevent GIF encoder from re-quantizing the image
            Bitmap toSave = bitmap;
            if (bpp < 8)
            {
                toSave = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format8bppIndexed);
                toSave.Palette = bitmap.Palette;
                BitmapData srcBitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                BitmapData dstBitmapData = toSave.LockBits(new Rectangle(0, 0, toSave.Width, toSave.Height), ImageLockMode.WriteOnly, toSave.PixelFormat);
                using (IReadWriteBitmapData src = BitmapDataFactory.CreateBitmapData(srcBitmapData.Scan0, bitmap.Size, srcBitmapData.Stride, (KnownPixelFormat)bitmap.PixelFormat))
                using (IReadWriteBitmapData dst = BitmapDataFactory.CreateBitmapData(dstBitmapData.Scan0, toSave.Size, dstBitmapData.Stride, KnownPixelFormat.Format8bppIndexed))
                    src.CopyTo(dst);
                bitmap.UnlockBits(srcBitmapData);
                toSave.UnlockBits(dstBitmapData);
            }

            CheckFileName($"{fileName}.gif");
            bitmap.Save($"{fileName}.gif", ImageFormat.Gif);
            if (!ReferenceEquals(bitmap, toSave))
                toSave.Dispose();
        }
#endif

        #endregion

        #endregion
    }
}