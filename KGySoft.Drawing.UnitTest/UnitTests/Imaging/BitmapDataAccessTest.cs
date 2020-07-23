#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
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
using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;
using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class BitmapDataAccessTest
    {
        #region Fields

        private static readonly object[][] setGetPixelTestSource =
        {
            new object[] { "32 bit ARGB Blue", PixelFormat.Format32bppArgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit ARGB Alpha 50%", PixelFormat.Format32bppArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x80_00_00_FF },
            new object[] { "32 bit ARGB Transparent", PixelFormat.Format32bppArgb, Color.Transparent, Color.Transparent, 0x00_FF_FF_FF },
            new object[] { "32 bit PARGB Blue", PixelFormat.Format32bppPArgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit PARGB Alpha 50%", PixelFormat.Format32bppPArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x80_00_00_80 },
            new object[] { "32 bit PARGB Alpha 1", PixelFormat.Format32bppPArgb, Color.FromArgb(1, Color.Blue), Color.FromArgb(1, Color.Blue), 0x01_00_00_01 },
            new object[] { "32 bit PARGB Alpha 254", PixelFormat.Format32bppPArgb, Color.FromArgb(254, Color.Blue), Color.FromArgb(254, Color.Blue), 0xFE_00_00_FE },
            new object[] { "32 bit PARGB Transparent", PixelFormat.Format32bppPArgb, Color.Transparent, Color.Empty, 0x00_00_00_00 },
            new object[] { "32 bit RGB Blue", PixelFormat.Format32bppRgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit RGB Alpha 50%", PixelFormat.Format32bppRgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 128), 0xFF_00_00_80 },
            new object[] { "32 bit RGB Transparent", PixelFormat.Format32bppRgb, Color.Transparent, Color.Black, 0xFF_00_00_00 },
            new object[] { "24 bit RGB Blue", PixelFormat.Format24bppRgb, Color.Blue, Color.Blue, 0x00_00_FF },
            new object[] { "24 bit RGB Transparent", PixelFormat.Format24bppRgb, Color.Transparent, Color.Black, 0x00_00_00 },
            new object[] { "64 bit ARGB Blue", PixelFormat.Format64bppArgb, Color.Blue, Color.Blue, 0x2000_0000_0000_2000 },
            new object[] { "64 bit ARGB Alpha 50%", PixelFormat.Format64bppArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x1010_0000_0000_2000 },
            new object[] { "64 bit ARGB Transparent", PixelFormat.Format64bppArgb, Color.Transparent, Color.Transparent, 0x0000_2000_2000_2000 },
            new object[] { "64 bit PARGB Blue", PixelFormat.Format64bppPArgb, Color.Blue, Color.Blue, 0x2000_0000_0000_2000 },
            new object[] { "64 bit PARGB Alpha Blue 50%", PixelFormat.Format64bppPArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x1010_0000_0000_1010 },
            new object[] { "64 bit PARGB Alpha Green 50%", PixelFormat.Format64bppPArgb, Color.FromArgb(128, Color.Green), Color.FromArgb(128, Color.Green), 0x1010_0000_0377_0000 },
            new object[] { "64 bit PARGB Alpha 1", PixelFormat.Format64bppPArgb, Color.FromArgb(1, Color.Blue), Color.FromArgb(0, Color.Blue), 0x0020_0000_0000_0020 },
            new object[] { "64 bit PARGB Alpha 254", PixelFormat.Format64bppPArgb, Color.FromArgb(254, Color.Blue), Color.FromArgb(254, Color.Blue), 0x1FDF_0000_0000_1FDF },
            new object[] { "64 bit PARGB Transparent", PixelFormat.Format64bppPArgb, Color.Transparent, Color.Empty, 0x0000_0000_0000_0000 },
            new object[] { "48 bit RGB Blue", PixelFormat.Format48bppRgb, Color.Blue, Color.Blue, 0x0000_0000_2000 },
            new object[] { "48 bit RGB White", PixelFormat.Format48bppRgb, Color.White, Color.White, 0x2000_2000_2000 },
            new object[] { "48 bit RGB Transparent", PixelFormat.Format48bppRgb, Color.Transparent, Color.Black, 0x0000_0000_0000 },
            new object[] { "16 bit GrayScale White", PixelFormat.Format16bppGrayScale, Color.White, Color.White, 0xFFFF },
            new object[] { "16 bit GrayScale Blue", PixelFormat.Format16bppGrayScale, Color.Blue, Color.FromArgb(0x1D, 0x1D, 0x1D), 0x1D2E },
            new object[] { "16 bit RGB565 Blue", PixelFormat.Format16bppRgb565, Color.Blue, Color.Blue, 0x001F },
            new object[] { "16 bit RGB565 Green", PixelFormat.Format16bppRgb565, Color.Green, Color.FromArgb(0, 130, 0), 0x0400 },
            new object[] { "16 bit RGB565 Transparent", PixelFormat.Format16bppRgb565, Color.Transparent, Color.Black, 0x0000 },
            new object[] { "16 bit RGB565 Empty", PixelFormat.Format16bppRgb565, Color.Empty, Color.Black, 0x0000 },
            new object[] { "16 bit RGB555 Blue", PixelFormat.Format16bppRgb555, Color.Blue, Color.Blue, 0x001F },
            new object[] { "16 bit RGB555 Green", PixelFormat.Format16bppRgb555, Color.Green, Color.FromArgb(0, 132, 0), 0x0200 },
            new object[] { "16 bit RGB555 Transparent", PixelFormat.Format16bppRgb555, Color.Transparent, Color.Black, 0x0000 },
            new object[] { "16 bit RGB555 Empty", PixelFormat.Format16bppRgb555, Color.Empty, Color.Black, 0x0000 },
            new object[] { "16 bit ARGB1555 Blue", PixelFormat.Format16bppArgb1555, Color.Blue, Color.Blue, 0x801F },
            new object[] { "16 bit ARGB1555 Green", PixelFormat.Format16bppArgb1555, Color.Green, Color.FromArgb(0, 132, 0), 0x8200 },
            new object[] { "16 bit ARGB1555 Transparent", PixelFormat.Format16bppArgb1555, Color.Transparent, Color.Transparent, 0x7FFF },
            new object[] { "16 bit ARGB1555 Empty", PixelFormat.Format16bppArgb1555, Color.Empty, Color.Empty, 0x0000 },
            new object[] { "8 bit Indexed Blue", PixelFormat.Format8bppIndexed, Color.Blue, Color.Blue, 12 },
            new object[] { "8 bit Indexed Blue 254", PixelFormat.Format8bppIndexed, Color.FromArgb(0, 0, 254), Color.Blue, 12 },
            new object[] { "8 bit Indexed Transparent", PixelFormat.Format8bppIndexed, Color.Transparent, Color.Empty, 16 },
            new object[] { "4 bit Indexed Blue", PixelFormat.Format4bppIndexed, Color.Blue, Color.Blue, 12 },
            new object[] { "4 bit Indexed Blue 254", PixelFormat.Format4bppIndexed, Color.FromArgb(0, 0, 254), Color.Blue, 12 },
            new object[] { "4 bit Indexed Transparent", PixelFormat.Format4bppIndexed, Color.Transparent, Color.Black, 0 },
            new object[] { "1 bit Indexed Blue", PixelFormat.Format1bppIndexed, Color.Blue, Color.Black, 0 },
            new object[] { "1 bit Indexed Blue 254", PixelFormat.Format1bppIndexed, Color.FromArgb(0, 0, 254), Color.Black, 0 },
            new object[] { "1 bit Indexed Lime", PixelFormat.Format1bppIndexed, Color.Lime, Color.White, 1 },
            new object[] { "1 bit Indexed Transparent", PixelFormat.Format1bppIndexed, Color.Transparent, Color.Black, 0 },
        };

        #endregion

        #region Methods

        #region Static Methods

        private static long GetRawValue(PixelFormat pixelFormat, IBitmapDataInternal bitmapData)
        {
            if (bitmapData is NativeBitmapDataBase nativeBitmapData)
                return GetRawValueNative(pixelFormat, nativeBitmapData.Scan0);

            var buffer = Reflector.GetField(bitmapData, "Buffer");
            switch (pixelFormat.ToBitsPerPixel())
            {
                case 64:
                    return ((Array2D<Color64>)buffer)[0, 0].ToArgb();
                case 48:
                    return ((Array2D<Color48>)buffer)[0, 0].ToRgb();
                case 32:
                    return (uint)((Array2D<Color32>)buffer)[0, 0].ToArgb();
                case 24:
                    return (uint)((Array2D<Color24>)buffer)[0, 0].ToRgb();
                case 16:
                    return pixelFormat switch
                    {
                        PixelFormat.Format16bppRgb565 => ((Array2D<Color16Rgb565>)buffer)[0, 0].Value,
                        PixelFormat.Format16bppRgb555 => ((Array2D<Color16Rgb555>)buffer)[0, 0].Value,
                        PixelFormat.Format16bppArgb1555 => ((Array2D<Color16Argb1555>)buffer)[0, 0].Value,
                        _ => ((Array2D<Color16Gray>)buffer)[0, 0].Value,
                    };
                case 8:
                    return ((Array2D<byte>)buffer)[0, 0];
                case 4:
                    return ((Array2D<byte>)buffer)[0, 0] >> 4;
                case 1:
                    return ((Array2D<byte>)buffer)[0, 0] >> 7;
                default:
                    throw new InvalidOperationException($"Unexpected pixel format: {pixelFormat}");
            }
        }

        private static unsafe long GetRawValueNative(PixelFormat pixelFormat, IntPtr ptr)
        {
            switch (pixelFormat.ToBitsPerPixel())
            {
                case 64:
                    return *(long*)ptr;
                case 48:
                    return *(uint*)ptr | ((long)(((ushort*)ptr)[2]) << 32);
                case 32:
                    return *(uint*)ptr;
                case 24:
                    return *(ushort*)ptr | (long)(((byte*)ptr)[2] << 16);
                case 16:
                    return *(ushort*)ptr;
                case 8:
                    return *(byte*)ptr;
                case 4:
                    return *(byte*)ptr >> 4;
                case 1:
                    return *(byte*)ptr >> 7;
                default:
                    throw new InvalidOperationException($"Unexpected pixel format: {pixelFormat}");
            }
        }

        #endregion

        #region Instance Methods

        [TestCaseSource(nameof(setGetPixelTestSource))]
        public void SetGetPixelTest(string testName, PixelFormat pixelFormat, Color testColor, Color expectedResult, long expectedRawValueNative)
        {
            Color actualColor;
            long actualRawValue;

            Console.WriteLine($"{testName}: {pixelFormat} + {testColor}{Environment.NewLine}");

            if (pixelFormat.IsSupportedNatively())
            {
                using Bitmap bmp = new Bitmap(1, 1, pixelFormat);

                // Reference test by Set/GetPixel
                try
                {
                    Console.Write("Bitmap.SetPixel/GetPixel: ");
                    bmp.SetPixel(0, 0, testColor);
                    actualColor = bmp.GetPixel(0, 0);
                    Console.WriteLine($"{expectedResult} vs. {actualColor} ({(expectedResult.ToArgb() == actualColor.ToArgb() ? "OK" : "Fail")})");
                    BitmapData data = bmp.LockBits(new Rectangle(0, 0, 1, 1), ImageLockMode.ReadOnly, pixelFormat);
                    try
                    {
                        actualRawValue = GetRawValueNative(pixelFormat, data.Scan0);
                        Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValueNative:X8} vs. {actualRawValue:X8} ({(expectedRawValueNative == actualRawValue ? "OK" : "Fail")}){Environment.NewLine}");
                    }
                    finally
                    {
                        bmp.UnlockBits(data);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                using (IBitmapDataInternal nativeBitmapData = BitmapDataFactory.CreateBitmapData(bmp, ImageLockMode.ReadWrite))
                {
                    // by Accessor Set/GetPixel
                    Console.Write("SetPixel/GetPixel native accessor: ");
                    nativeBitmapData.SetPixel(0, 0, testColor);
                    actualColor = nativeBitmapData.GetPixel(0, 0);
                    Console.WriteLine($"{expectedResult} vs. {actualColor} ({(expectedResult.ToArgb() == actualColor.ToArgb() ? "OK" : "Fail")})");
                    Assert.AreEqual(expectedResult.ToArgb(), actualColor.ToArgb());

                    actualRawValue = GetRawValue(pixelFormat, nativeBitmapData);
                    Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValueNative:X8} vs. {actualRawValue:X8} ({(expectedRawValueNative == actualRawValue ? "OK" : "Fail")})");
                    if (pixelFormat == nativeBitmapData.PixelFormat) // can differ in Linux for 16 bpp formats
                        Assert.AreEqual(expectedRawValueNative, actualRawValue);

                    // by indexer
                    nativeBitmapData.DoGetRow(0)[0] = new Color32(testColor);
                    Assert.AreEqual(expectedResult.ToArgb(), nativeBitmapData.DoGetRow(0)[0].ToArgb());
                    if (pixelFormat == nativeBitmapData.PixelFormat) // can differ in Linux for 16 bpp formats
                        Assert.AreEqual(expectedRawValueNative, GetRawValue(pixelFormat, nativeBitmapData));
                }
            }

            using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(new Size(1, 1), pixelFormat))
            {
                bool AreEqual(Color c1, Color c2) => c1.ToArgb() == c2.ToArgb()
                    || pixelFormat.HasFlag(PixelFormat.PAlpha) && c1.A == 0 && c2.A == 0;

                long expectedRawValueManaged = expectedRawValueNative;
                if (pixelFormat.ToBitsPerPixel() > 32)
                {
                    expectedRawValueManaged = pixelFormat == PixelFormat.Format64bppArgb ? new Color64(new Color32(testColor)).ToArgb()
                        : pixelFormat == PixelFormat.Format64bppPArgb ? new Color64(new Color32(testColor)).ToPremultiplied().ToArgb()
                        : pixelFormat == PixelFormat.Format48bppRgb ? testColor.A == 0 ? default : new Color48(new Color32(testColor)).ToRgb()
                        : expectedRawValueNative;

                    expectedResult = pixelFormat == PixelFormat.Format64bppArgb ? Color64.FromArgb(expectedRawValueManaged).ToColor32().ToColor()
                        : pixelFormat == PixelFormat.Format64bppPArgb ? new Color64(new Color32(testColor)).ToPremultiplied().ToStraight().ToColor32().ToColor()
                        : pixelFormat == PixelFormat.Format48bppRgb ? Color48.FromRgb(expectedRawValueManaged).ToColor32().ToColor()
                        : expectedResult;
                }

                // by Accessor Set/GetPixel
                Console.Write("SetPixel/GetPixel managed accessor: ");
                managedBitmapData.SetPixel(0, 0, testColor);
                actualColor = managedBitmapData.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(AreEqual(expectedResult, actualColor) ? "OK" : "Fail")})");
                Assert.IsTrue(AreEqual(expectedResult, actualColor));

                actualRawValue = GetRawValue(pixelFormat, managedBitmapData);
                Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValueManaged:X8} vs. {actualRawValue:X8} ({(expectedRawValueManaged == actualRawValue ? "OK" : "Fail")})");
                Assert.AreEqual(expectedRawValueManaged, actualRawValue);

                // by indexer
                managedBitmapData.DoGetRow(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.DoGetRow(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValueManaged, GetRawValue(pixelFormat, managedBitmapData));
            }
        }

        [Test]
        public void PremultipliedConversionTest()
        {
            Color32 s32 = new Color32(128, 255, 255, 255);

            // ====== 32 vs. 32 =====
            // S32 -> P32 -> S32
            Color32 p32 = s32.ToPremultiplied();
            Assert.AreEqual(s32, p32.ToStraight());

            // ====== From Straight 32 =====
            // S32 -> S64M -> S32
            Color64 s64m = new Color64(s32);
            Assert.AreEqual(s32, s64m.ToColor32());

            // S32 -> S64N -> S32
            Color64 s64n = s32.ToColor64PlatformDependent();
            Assert.AreEqual(s32, s64n.ToColor32PlatformDependent());

            // S32 -> (S64M->) P64M -> (S64M->) S32
            Color64 p64m = s64m.ToPremultiplied();
            Assert.AreEqual(s64m, p64m.ToStraight());
            Assert.AreEqual(s32, p64m.ToStraight().ToColor32());

            // S32 -> P64N -> S32
            Color64 p64n = s32.ToPremultiplied64PlatformDependent();
            Assert.AreEqual(s32, p64n.ToStraight32PlatformDependent());

            // ====== From Premultiplied =====
            // P32 -> (S32->) S64M -> (S32->) P32
            Assert.AreEqual(s64m, new Color64(p32.ToStraight()));
            Assert.AreEqual(p32, s64m.ToColor32().ToPremultiplied());

            // P32 -> (S32->) S64N -> (S32->) P32
            Assert.AreEqual(s64n, p32.ToStraight().ToColor64PlatformDependent());
            Assert.AreEqual(p32, s64n.ToColor32PlatformDependent().ToPremultiplied());

            // P32 -> P64M -> P32
            Assert.AreEqual(p64m, new Color64(p32));
            Assert.AreEqual(p32, p64m.ToColor32());

            // P32 -> (S32->) P64N -> (S32->) P32
            // note: unfortuantely p32.ToColor64PlatformDependent != p64n and p64n.ToColor32PlatformDependent != p32
            Assert.AreEqual(p64n, p32.ToStraight().ToPremultiplied64PlatformDependent());
            Assert.AreEqual(p32, p64n.ToStraight32PlatformDependent().ToPremultiplied());
        }

        #endregion

        #endregion
    }
}
