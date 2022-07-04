#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapSourceExtensionsTest.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
#if !NET35
using System.Threading.Tasks;
#endif
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.Wpf.UnitTests
{
    [TestFixture]
    public class BitmapSourceExtensionsTest : TestBase
    {
        #region AsyncTestState class

        private sealed class AsyncTestState
        {
            #region Properties

            internal Action<ManualResetEvent> Callback { get; init; } = default!;
            internal ManualResetEvent WaitHandle { get; init; } = default!;
            internal Exception? Error { get; set; }

            #endregion
        }

        #endregion

        #region Fields

        private static Color testColor = Color.FromRgb(0x80, 0xFF, 0x40);
        private static Color testColorAlpha = Color.FromArgb(0x80, 0x80, 0xFF, 0x40);
        private static Color testColorBlended = Color.FromArgb(0xFF, 0x40, 0x80, 0x20);

        private static readonly object[][] setGetPixelTestSource =
        {
            new object[] { "BGRA32", PixelFormats.Bgra32, testColor, testColor, 0xFF_80_FF_40 },
            new object[] { "BGRA32 Alpha", PixelFormats.Bgra32, testColorAlpha, testColorAlpha, 0x80_80_FF_40 },
            new object[] { "BGRA32 Transparent", PixelFormats.Bgra32, Colors.Transparent, Colors.Transparent, 0x00_FF_FF_FF },

            new object[] { "PBGRA32", PixelFormats.Pbgra32, testColor, testColor, 0xFF_80_FF_40 },
            new object[] { "PBGRA32 Alpha 50%", PixelFormats.Pbgra32, testColorAlpha, testColorAlpha, 0x80_40_80_20 },
            new object[] { "PBGRA32 Transparent", PixelFormats.Pbgra32, Colors.Transparent, default(Color), 0x00_00_00_00 },
            new object[] { "PBGRA32 Alpha 1", PixelFormats.Pbgra32, Color.FromArgb(1, 0, 0, 255), Color.FromArgb(1, 0, 0, 255), 0x01_00_00_01 },
            new object[] { "PBGRA32 Alpha 254", PixelFormats.Pbgra32, Color.FromArgb(254, 0, 0, 255), Color.FromArgb(254, 0, 0, 255), 0xFE_00_00_FE },

            new object[] { "BGR32", PixelFormats.Bgr32, testColor, testColor, 0xFF_80_FF_40 },
            new object[] { "BGR32 Alpha", PixelFormats.Bgr32, testColorAlpha, testColorBlended, 0xFF_40_80_20 },
            new object[] { "BGR32 Transparent", PixelFormats.Bgr32, Colors.Transparent, Colors.Black, 0xFF_00_00_00 },

            new object[] { "BGR24", PixelFormats.Bgr24, testColor, testColor, 0x80_FF_40 },
            new object[] { "BGR24 Alpha", PixelFormats.Bgr24, testColorAlpha, testColorBlended, 0x40_80_20 },
            new object[] { "BGR24 Transparent", PixelFormats.Bgr24, Colors.Transparent, Colors.Black, 0x00_00_00 },

            new object[] { "RGB24", PixelFormats.Rgb24, testColor, testColor, 0x40_FF_80 },
            new object[] { "RGB24 Alpha", PixelFormats.Rgb24, testColorAlpha, testColorBlended, 0x20_80_40 },
            new object[] { "RGB24 Transparent", PixelFormats.Rgb24, Colors.Transparent, Colors.Black, 0x00_00_00 },

            new object[] { "I1", PixelFormats.Indexed1, testColor, Colors.White, 1 },
            new object[] { "I1 Alpha", PixelFormats.Indexed1, testColorAlpha, Colors.Black, 0 },
            new object[] { "I1 Transparent", PixelFormats.Indexed1, Colors.Transparent, Colors.Black, 0 },

            new object[] { "I2", PixelFormats.Indexed2, testColor, Colors.Silver, 2 },
            new object[] { "I2 Alpha", PixelFormats.Indexed2, testColorAlpha, Colors.Gray, 1 },
            new object[] { "I2 Transparent", PixelFormats.Indexed2, Colors.Transparent, Colors.Black, 0 },

            new object[] { "I4", PixelFormats.Indexed4, testColor, Colors.Olive, 3 },
            new object[] { "I4 Alpha", PixelFormats.Indexed4, testColorAlpha, Colors.Green, 2 },
            new object[] { "I4 Transparent", PixelFormats.Indexed4, Colors.Transparent, Colors.Black, 0 },

            new object[] { "I8", PixelFormats.Indexed8, testColor, Color.FromRgb(0x99, 0xFF, 0x33), 179 },
            new object[] { "I8 Alpha", PixelFormats.Indexed8, testColorAlpha, Color.FromRgb(0x33, 0x99, 0x33), 95 },
            new object[] { "I8 Transparent", PixelFormats.Indexed8, Colors.Transparent, default(Color), 16 },

            new object[] { "BW", PixelFormats.BlackWhite, testColor, Colors.White, 1 },
            new object[] { "BW Alpha", PixelFormats.BlackWhite, testColorAlpha, Colors.Black, 0 },
            new object[] { "BW Transparent", PixelFormats.BlackWhite, Colors.Transparent, Colors.Black, 0 },

            new object[] { "Gray2", PixelFormats.Gray2, testColor, Color.FromRgb(0xAA, 0xAA, 0xAA), 2 },
            new object[] { "Gray2 Alpha", PixelFormats.Gray2, testColorAlpha, Color.FromRgb(0x55, 0x55, 0x55), 1 },
            new object[] { "Gray2 Transparent", PixelFormats.Gray2, Colors.Transparent, Colors.Black, 0 },

            new object[] { "Gray4", PixelFormats.Gray4, testColor, Color.FromRgb(0xBB, 0xBB, 0xBB), 11 },
            new object[] { "Gray4 Alpha", PixelFormats.Gray4, testColorAlpha, Color.FromRgb(0x66, 0x66, 0x66), 6 },
            new object[] { "Gray4 Transparent", PixelFormats.Gray4, Colors.Transparent, Colors.Black, 0 },

            new object[] { "Gray8", PixelFormats.Gray8, testColor, Color.FromRgb(0xC3, 0xC3, 0xC3), 0xC3 },
            new object[] { "Gray8 Alpha", PixelFormats.Gray8, testColorAlpha, Color.FromRgb(0x61, 0x61, 0x61), 0x61 },
            new object[] { "Gray8 Transparent", PixelFormats.Gray8, Colors.Transparent, Colors.Black, 0 },

            new object[] { "BGR555", PixelFormats.Bgr555, testColor, Color.FromRgb(0b10000100, 0b11111111, 0b01000010), 0b10000_11111_01000 },
            new object[] { "BGR555 Alpha", PixelFormats.Bgr555, testColorAlpha, Color.FromRgb(0b01000010, 0b10000100, 0b00100001), 0b01000_10000_00100 },
            new object[] { "BGR555 Transparent", PixelFormats.Bgr555, Colors.Transparent, Colors.Black, 0 },

            new object[] { "BGR565", PixelFormats.Bgr565, testColor, Color.FromRgb(0b10000100, 0b11111111, 0b01000010), 0b10000_111111_01000 },
            new object[] { "BGR565 Alpha", PixelFormats.Bgr565, testColorAlpha, Color.FromRgb(0b01000010, 0b10000010, 0b00100001), 0b01000_100000_00100 },
            new object[] { "BGR565 Transparent", PixelFormats.Bgr565, Colors.Transparent, Colors.Black, 0 },

            new object[] { "Gray16", PixelFormats.Gray16, testColor, Color.FromRgb(0xC4, 0xC4, 0xC4), 0xC404 },
            new object[] { "Gray16 Alpha", PixelFormats.Gray16, testColorAlpha, Color.FromRgb(0x62, 0x62, 0x62), 0x624D },
            new object[] { "Gray16 Transparent", PixelFormats.Gray16, Colors.Transparent, Colors.Black, 0 },

            new object[] { "Gray32", PixelFormats.Gray32Float, testColor, Color.FromRgb(0xC3, 0xC3, 0xC3), 0x3F0C1C96 },
            new object[] { "Gray32 Alpha", PixelFormats.Gray32Float, testColorAlpha, Color.FromRgb(0x62, 0x62, 0x62), 0x3DF9B636 },
            new object[] { "Gray32 Transparent", PixelFormats.Gray32Float, Colors.Transparent, Colors.Black, 0 },

            new object[] { "BGR101010", PixelFormats.Bgr101010, testColor, testColor, 0b1000000010_1111111111_0100000001 },
            new object[] { "BGR101010 Alpha", PixelFormats.Bgr101010, testColorAlpha, testColorBlended, 0b0100000001_1000000010_0010000000 },
            new object[] { "BGR101010 Transparent", PixelFormats.Bgr101010, Colors.Transparent, Colors.Black, 0 },

            new object[] { "RGB48", PixelFormats.Rgb48, testColor, testColor, 0x4040_FFFF_8080 },
            new object[] { "RGB48 Alpha", PixelFormats.Rgb48, testColorAlpha, testColorBlended, 0x2020_8080_4040 },
            new object[] { "RGB48 Transparent", PixelFormats.Rgb48, Colors.Transparent, Colors.Black, 0 },

            new object[] { "RGBA64", PixelFormats.Rgba64, testColor, testColor, unchecked((long)0xFFFF_4040_FFFF_8080) },
            new object[] { "RGBA64 Alpha", PixelFormats.Rgba64, testColorAlpha, testColorAlpha, unchecked((long)0x8080_4040_FFFF_8080) },
            new object[] { "RGBA64 Transparent", PixelFormats.Rgba64, Colors.Transparent, Colors.Transparent, 0x0000_FFFF_FFFF_FFFF },

            new object[] { "PRGBA64", PixelFormats.Prgba64, testColor, testColor, unchecked((long)0xFFFF_4040_FFFF_8080) },
            new object[] { "PRGBA64 Alpha", PixelFormats.Prgba64, testColorAlpha, testColorAlpha, unchecked((long)0x8080_2040_8080_4080) },
            new object[] { "PRGBA64 Transparent", PixelFormats.Prgba64, Colors.Transparent, default(Color), 0x0000_0000_0000_0000 },

            new object[] { "RGBA128", PixelFormats.Rgba128Float, testColor, testColor, 0x3F800000_3E5D0A8B /*only R and G as float */ },
            new object[] { "RGBA128 Alpha", PixelFormats.Rgba128Float, testColorAlpha, testColorAlpha, 0x3F800000_3E5D0A8B /*only R and G as float */ },
            new object[] { "RGBA128 Transparent", PixelFormats.Rgba128Float, Colors.Transparent, Colors.Transparent, 0x3F800000_3F800000 /*only R and G as float */ },

            new object[] { "PRGBA128", PixelFormats.Prgba128Float, testColor, testColor, 0x3F800000_3E5D0A8B /* only R and G as float */ },
            new object[] { "PRGBA128 Alpha", PixelFormats.Prgba128Float, testColorAlpha, testColorAlpha, 0x3F008081_3DDDE874 /* only R and G as float */ },
            new object[] { "PRGBA128 Transparent", PixelFormats.Prgba128Float, Colors.Transparent, default(Color), 0x00000000_00000000 /* only R and G as float */ },

            new object[] { "RGB128", PixelFormats.Rgb128Float, testColor, testColor, 0x3F800000_3E5D0A8B /* only R and G as float */ },
            new object[] { "RGB128 Alpha", PixelFormats.Rgb128Float, testColorAlpha, testColorBlended, 0x3E5D0A8B_3D51FFEF /* only R and G as float */ },
            new object[] { "RGB128 Transparent", PixelFormats.Rgb128Float, Colors.Transparent, Colors.Black, 0x00000000_00000000 /* only R and G as float */ },

            new object[] { "CMYK32", PixelFormats.Cmyk32, testColor, testColor, 0x00_BF_00_7E },
            new object[] { "CMYK32 Alpha", PixelFormats.Cmyk32, testColorAlpha, testColorBlended, 0x7E_BF_00_7F },
            new object[] { "CMYK32 Transparent", PixelFormats.Cmyk32, Colors.Transparent, Colors.Black, 0xFF_00_00_00 },
        };

        private static readonly object[][] sourceConvertPixelFormatDirectTest =
        {
            new object[] { PixelFormats.Indexed8, Colors.Black, (byte)0 },
            new object[] { PixelFormats.Indexed8, Colors.Black, (byte)1 },
            new object[] { PixelFormats.Indexed8, Colors.Black, (byte)128 },
            new object[] { PixelFormats.Indexed8, Colors.Black, (byte)255 },
            new object[] { PixelFormats.Indexed8, Colors.White, (byte)1 },
            new object[] { PixelFormats.Indexed8, Color.FromRgb(0x88, 0x88, 0x88), (byte)1 },
            new object[] { PixelFormats.Indexed4, Colors.Black, (byte)0 },
            new object[] { PixelFormats.Indexed4, Colors.White, (byte)0 },
            new object[] { PixelFormats.Bgr565, Colors.Black, (byte)0 },
            new object[] { PixelFormats.Bgr565, Colors.White, (byte)0 },
            new object[] { PixelFormats.Gray16, Colors.Black, (byte)0 },
            new object[] { PixelFormats.Gray16, Colors.White, (byte)0 },
            new object[] { PixelFormats.Rgb48, Colors.Black, (byte)0 },
            new object[] { PixelFormats.Rgb48, Colors.White, (byte)0 },
            new object[] { PixelFormats.Cmyk32, Colors.Black, (byte)128 },
        };

        private static readonly object?[][] convertPixelFormatCustomTestSource =
        {
            new object?[] { "To 8bpp 256 color no dithering", PixelFormats.Indexed8, PredefinedColorsQuantizer.SystemDefault8BppPalette(), null },
            new object?[] { "To 8bpp 256 color dithering", PixelFormats.Indexed8, PredefinedColorsQuantizer.SystemDefault8BppPalette(), OrderedDitherer.Bayer2x2 },
            new object?[] { "To 8bpp 16 color no dithering", PixelFormats.Indexed8, PredefinedColorsQuantizer.SystemDefault4BppPalette(), null },
            new object?[] { "To 4bpp 2 color dithering", PixelFormats.Indexed4, PredefinedColorsQuantizer.BlackAndWhite(), OrderedDitherer.DottedHalftone },
            new object?[] { "To BGR555 256 color dithering", PixelFormats.Bgr555, PredefinedColorsQuantizer.SystemDefault8BppPalette(), new RandomNoiseDitherer(), },
            new object?[] { "To BGR555 32K color dithering", PixelFormats.Bgr555, PredefinedColorsQuantizer.Argb1555(), new RandomNoiseDitherer(), },
            new object?[] { "To BGR555 16.7M color dithering", PixelFormats.Bgr555, PredefinedColorsQuantizer.Rgb888(), new RandomNoiseDitherer(), },
            new object?[] { "To 1bpp 2 color dithering auto select quantizer", PixelFormats.Indexed1, null, OrderedDitherer.Bayer8x8 },
        };

        #endregion

        #region Methods

        #region Static Methods

        private static BitmapPalette? GetDefaultPalette(PixelFormat pixelFormat)
        {
            var result = pixelFormat == PixelFormats.Indexed1 ? Palette.BlackAndWhite()
                : pixelFormat == PixelFormats.Indexed2 ? new Palette(new[] { Color32.FromGray(0), Color32.FromGray(0x80), Color32.FromGray(0xC0), Color32.FromGray(0xFF) })
                : pixelFormat == PixelFormats.Indexed4 ? Palette.SystemDefault4BppPalette()
                : pixelFormat == PixelFormats.Indexed8 ? Palette.SystemDefault8BppPalette()
                : null;
            return result == null ? null : new BitmapPalette(result.GetEntries().Select(c => Color.FromArgb(c.A, c.R, c.G, c.B)).ToArray());
        }

        private static long GetRawValue(PixelFormat pixelFormat, IReadableBitmapDataRow row)
        {
            return pixelFormat.BitsPerPixel switch
            {
                64 or 128 => row.ReadRaw<long>(0),
                48 => row.ReadRaw<uint>(0) | ((long)row.ReadRaw<ushort>(2) << 32),
                32 => row.ReadRaw<uint>(0),
                24 => row.ReadRaw<ushort>(0) | ((long)row.ReadRaw<byte>(2) << 16),
                16 => row.ReadRaw<ushort>(0),
                8 => row.ReadRaw<byte>(0),
                4 => row.ReadRaw<byte>(0) >> 4,
                2 => row.ReadRaw<byte>(0) >> 6,
                1 => row.ReadRaw<byte>(0) >> 7,
                _ => throw new InvalidOperationException($"Unexpected pixel format: {pixelFormat}")
            };
        }

        /// <summary>
        /// Executes <paramref name="test"/> on a dedicated thread that starts the dispatcher so
        /// the thread will neither exit nor be blocked until the test completes.
        /// Without this even a simple test containing await would be blocked if contains sync callbacks.
        /// </summary>
        private static void ExecuteAsyncTestWithDispatcher(Action<ManualResetEvent> test)
        {
            // This will be executed on a new thread
            static void Execute(object? state)
            {
                var asyncState = (AsyncTestState)state!;
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

                // Assuring that the dispatcher (and thus this thread) exits when the test finishes
                ThreadPool.RegisterWaitForSingleObject(asyncState.WaitHandle, (_, _) => Dispatcher.CurrentDispatcher.InvokeShutdown(), null, Timeout.Infinite, true);
                try
                {
                    // Invoking the callback that will set the wait handle when finishes
                    asyncState.Callback.Invoke(asyncState.WaitHandle);
                }
                catch (Exception e)
                {
                    // In case of error we save the exception so it can be thrown by the test case
                    // and manually set the wait handle (assuming the callback did not set it due to the error)
                    asyncState.Error = e;
                    asyncState.WaitHandle.Set();
                    return;
                }

                // Starting the dispatcher that prevents the thread from exiting and processes callbacks
                Dispatcher.Run();
            }

            var waitHandle = new ManualResetEvent(false);
            var state = new AsyncTestState
            {
                Callback = test,
                WaitHandle = waitHandle
            };

            var thread = new Thread(Execute)
            {
#if NETFRAMEWORK
                ApartmentState = ApartmentState.STA
#endif
            };

            thread.Start(state);
            waitHandle.WaitOne();
            if (state.Error != null)
                ExceptionDispatchInfo.Capture(state.Error).Throw();
        }

        #endregion

        #region Instance Methods

        [TestCaseSource(nameof(setGetPixelTestSource))]
        public void SetGetPixelTest(string testName, PixelFormat pixelFormat, Color color, Color expectedResult, long expectedRawValue)
        {
            Console.WriteLine($"{testName}: {pixelFormat} + {color} => {expectedResult} (0x{expectedRawValue:X8})");

            var bmp = new WriteableBitmap(1, 1, 96, 96, pixelFormat, GetDefaultPalette(pixelFormat));
            Color32 expectedColor = expectedResult.ToColor32();
            using (var writableBitmapData = bmp.GetWritableBitmapData())
                writableBitmapData.FirstRow[0] = color.ToColor32();

            using (var readableBitmapData = bmp.GetReadableBitmapData())
            {
                Color32 actualColor = readableBitmapData.FirstRow[0];
                Assert.IsTrue(expectedColor.TolerantEquals(actualColor, 1), $"Expected vs. read color: {expectedColor} <=> {actualColor}");
                long actualRawValue = GetRawValue(pixelFormat, readableBitmapData[0]);
                Assert.AreEqual(expectedRawValue, actualRawValue, $"Raw value {expectedRawValue:X8} was expected but it was {actualRawValue:X8}");
            }

            // The code above just tests self-consistency. To test whether WPF handles the color the same way we convert the bitmap in WPF.
            // CMYK is handled by WPF oddly: it uses a non-reversible transformation (but above test proves that our transformation is 100% reversible)
            var converted = new WriteableBitmap(new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, default));
            using IReadWriteBitmapData convertedBitmapData = converted.GetReadWriteBitmapData();
            Color32 convertedColor = convertedBitmapData[0][0];
            Assert.IsTrue(pixelFormat == PixelFormats.Cmyk32 || expectedColor.TolerantEquals(convertedColor, 1), $"Expected vs. converted color: {expectedColor} <=> {convertedColor}");
        }

        [TestCaseSource(nameof(sourceConvertPixelFormatDirectTest))]
        public void ConvertPixelFormatDirectTest(PixelFormat pixelFormat, Color backColor, byte alphaThreshold)
        {
            BitmapSource ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            WriteableBitmap converted = ref32bpp.ConvertPixelFormat(pixelFormat, backColor, alphaThreshold);
            Assert.AreEqual(pixelFormat, converted.Format);
            SaveBitmap($"{pixelFormat} - {backColor} (A={alphaThreshold})", converted);
        }

        [TestCaseSource(nameof(convertPixelFormatCustomTestSource))]
        public void ConvertPixelFormatCustomTest(string testName, PixelFormat pixelFormat, IQuantizer quantizer, IDitherer ditherer)
        {
            BitmapSource source = GetInfoIcon256();
            WriteableBitmap converted = source.ConvertPixelFormat(pixelFormat, quantizer, ditherer);
            Assert.AreEqual(pixelFormat, converted.Format);
            SaveBitmap(testName, converted);
        }

        [Test]
        public void ConvertPixelFormatIndexedSourceTest()
        {
            WriteableBitmap source = GetInfoIcon256().ConvertPixelFormat(PixelFormats.Indexed4, OptimizedPaletteQuantizer.Wu(16));
            var converted = source.ConvertPixelFormat(PixelFormats.Indexed8);
            Assert.AreEqual(PixelFormats.Indexed8, converted.Format);
            AssertAreEqual(source, converted, true);
            SaveBitmap(null, converted);
        }

        [Test]
        public void BeginEndConvertPixelFormatBlockingWaitSucceedsTest()
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            // when using a predefined quantizer, no sync callback is used (only for disposing but that is not awaited)
            IAsyncResult ar = ref32bpp.BeginConvertPixelFormat(PixelFormats.Indexed8, PredefinedColorsQuantizer.Rgb332());
            Assert.IsFalse(ar.IsCompleted);
            WriteableBitmap? result = ar.EndConvertPixelFormat();
            Assert.IsTrue(ar.IsCompleted);
            Assert.IsFalse(ar.CompletedSynchronously);
            Assert.IsNotNull(result);
            SaveBitmap(null, result!);
        }

        [Test]
        public void BeginEndConvertPixelFormatBlockingWaitDeadlockTest()
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            // a non-predefined quantizer causes that the result bitmap is created by a callback, which causes a deadlock with blocking wait
            IAsyncResult ar = ref32bpp.BeginConvertPixelFormat(PixelFormats.Indexed8, OptimizedPaletteQuantizer.Wu());
            Assert.IsFalse(ar.IsCompleted);
            Assert.Throws<InvalidOperationException>(() => ar.EndConvertPixelFormat(), Res.BitmapSourceExtensionsDeadlock);
            Assert.IsTrue(ar.IsCompleted);
            Assert.IsFalse(ar.CompletedSynchronously);
        }

        [Test]
        public void BeginEndConvertPixelFormatActiveWaitingTest()
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            IAsyncResult ar = ref32bpp.BeginConvertPixelFormat(PixelFormats.Indexed8, OptimizedPaletteQuantizer.Wu());
            while (!ar.IsCompleted)
            {
                // A very non-recommended way (it's like Application.DoEvents in WinForms)
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
                Thread.Sleep(1);
            }

            WriteableBitmap result = ar.EndConvertPixelFormat()!;
            Assert.IsTrue(ar.IsCompleted);
            Assert.IsFalse(ar.CompletedSynchronously);
            Assert.IsNotNull(result);
            SaveBitmap(null, result);
        }

        [Test]
        public void BeginEndConvertPixelFormatWithCallbackTest() => ExecuteAsyncTestWithDispatcher(finished =>
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            var context = SynchronizationContext.Current!;
            WriteableBitmap? result = null;
            IAsyncResult asyncResult = ref32bpp.BeginConvertPixelFormat(PixelFormats.Indexed8, OptimizedPaletteQuantizer.Wu(),
                asyncConfig: new AsyncConfig(CompletedCallback));

            // 1.) This part executes immediately
            Assert.IsFalse(asyncResult.IsCompleted);
            Assert.IsNull(result);

            // 2.) Here this method returns but the caller ExecuteAsyncTest starts the dispatcher and prevents the thread from exiting or blocking

            // 3.) This is executed when the async operation finishes
            void CompletedCallback(IAsyncResult ar)
            {
                result = ar.EndConvertPixelFormat();
                Assert.IsNotNull(result);
                Assert.IsTrue(ar.IsCompleted);
                Assert.IsFalse(ar.CompletedSynchronously);
                context.Post(_ => SaveBitmap(null, result!), null);

                // to let the dispatcher shut down and the test end
                finished.Set();
            }
        });

        [Test]
        public void BeginEndConvertPixelFormatImmediateCancelTest()
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            IAsyncResult ar = ref32bpp.BeginConvertPixelFormat(PixelFormats.Indexed8,
                asyncConfig: new AsyncConfig(null, () => true));
            Assert.IsTrue(ar.IsCompleted);
            Assert.IsTrue(ar.CompletedSynchronously);
            Assert.Throws<OperationCanceledException>(() => ar.EndConvertPixelFormat());
        }

        [Test]
        public void BeginEndConvertPixelFormatCancelWithDefaultValueTest()
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            IAsyncResult ar = ref32bpp.BeginConvertPixelFormat(PixelFormats.Indexed8,
                asyncConfig: new AsyncConfig(null, () => true) { ThrowIfCanceled = false });
            Assert.IsTrue(ar.IsCompleted);
            Assert.IsTrue(ar.CompletedSynchronously);
            Assert.IsNull(ar.EndConvertPixelFormat());
        }

#if !NET35
        [Test]
        public void ConvertPixelFormatAsyncBlockingWaitSucceedsTest()
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            // when using a predefined quantizer, no sync callback is used (only for disposing but that is not awaited)
            Task<WriteableBitmap?> task = ref32bpp.ConvertPixelFormatAsync(PixelFormats.Indexed8, PredefinedColorsQuantizer.BlackAndWhite(), OrderedDitherer.Bayer4x4);
            Assert.IsFalse(task.IsCompleted);
            task.Wait();
            WriteableBitmap? result = task.Result;
            Assert.IsTrue(task.IsCompleted);
            Assert.IsNotNull(result);
            SaveBitmap(null, result!);
        }

        [Test]
        public void ConvertPixelFormatAsyncBlockingWaitDeadlockTest()
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            // a non-predefined quantizer causes that the result bitmap is created by a callback, which causes a deadlock with blocking wait
            Task<WriteableBitmap?> task = ref32bpp.ConvertPixelFormatAsync(PixelFormats.Indexed8, OptimizedPaletteQuantizer.Wu());
            Assert.IsFalse(task.IsCompleted);
            var ex = Assert.Throws<AggregateException>(() => task.Wait());
            Assert.IsInstanceOf<InvalidOperationException>(ex!.InnerExceptions[0]);
            Assert.AreEqual(Res.BitmapSourceExtensionsDeadlock, ex.InnerExceptions[0].Message);
        }

        [Test]
        public void ConvertPixelFormatActiveWaitingTest()
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            Task<WriteableBitmap?> task = ref32bpp.ConvertPixelFormatAsync(PixelFormats.Indexed8, OptimizedPaletteQuantizer.Wu(16));
            Assert.IsFalse(task.IsCompleted);
            do
            {
                // A very non-recommended way (it's like Application.DoEvents in WinForms)
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
            } while (!task.Wait(1));
            Assert.IsTrue(task.IsCompleted);
            Assert.IsNotNull(task.Result);
            SaveBitmap(null, task.Result!);
        }

        [Test]
        public void ConvertPixelFormatAsyncWithContinuationTest() => ExecuteAsyncTestWithDispatcher(finished =>
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            var context = SynchronizationContext.Current!;
            WriteableBitmap? result = null;
            Task task = ref32bpp.ConvertPixelFormatAsync(PixelFormats.Indexed8, OptimizedPaletteQuantizer.Wu())
                .ContinueWith(Continuation);

            // 1.) This part executes immediately
            Assert.IsFalse(task.IsCompleted);
            Assert.IsNull(result);

            // 2.) Here this method returns but the caller ExecuteAsyncTest starts the dispatcher and prevents the thread from exiting or blocking

            // 3.) This is executed when the async operation finishes
            Task Continuation(Task<WriteableBitmap?> completedTask)
            {
                Assert.IsTrue(completedTask.IsCompleted);
                result = completedTask.Result;
                Assert.IsNotNull(result);
                context.Post(_ => SaveBitmap(null, result!), null);

                // to let the dispatcher shut down and the test end
                finished.Set();
                return Task.CompletedTask;
            }
        });

        [Test]
        public void ConvertPixelFormatAsyncImmediateCancelTest()
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            Task<WriteableBitmap?> task = ref32bpp.ConvertPixelFormatAsync(PixelFormats.Indexed8,
                asyncConfig: new TaskConfig(new CancellationToken(true)));
            Assert.IsTrue(task.IsCanceled);
            var ex = Assert.Throws<AggregateException>(() => { var _ = task.Result; });
            Assert.IsInstanceOf<OperationCanceledException>(ex!.InnerExceptions[0]);
        }

        [Test]
        public void ConvertPixelFormatAsyncReturnDefaultIfCanceledTest()
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            Task<WriteableBitmap?> task = ref32bpp.ConvertPixelFormatAsync(PixelFormats.Indexed8,
                asyncConfig: new TaskConfig(new CancellationToken(true)) { ThrowIfCanceled = false });
            Assert.IsTrue(task.IsCompleted);
            Assert.IsNull(task.Result);
        }
#endif

#if !(NET35 || NET40)
        [Test]
        [SuppressMessage("ReSharper", "AsyncVoidLambda", Justification = "No problem, that's why there is the finished event")]
        public void ConvertPixelFormatAsyncWithAwaitTest() => ExecuteAsyncTestWithDispatcher(async finished =>
        {
            var ref32bpp = GetInfoIcon256();
            Assert.AreEqual(32, ref32bpp.Format.BitsPerPixel);

            WriteableBitmap? result = await ref32bpp.ConvertPixelFormatAsync(PixelFormats.Indexed8, OptimizedPaletteQuantizer.Wu());
            Assert.IsNotNull(result);
            SaveBitmap(null, result!);
            finished.Set();
        });
#endif

        #endregion

        #endregion
    }
}
