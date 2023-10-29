#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessTest.cs
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
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

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
            new object[] { "32 bit ARGB Blue", KnownPixelFormat.Format32bppArgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit ARGB Alpha 50%", KnownPixelFormat.Format32bppArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x80_00_00_FF },
            new object[] { "32 bit ARGB Transparent", KnownPixelFormat.Format32bppArgb, Color.Transparent, Color.Transparent, 0x00_FF_FF_FF },
            new object[] { "32 bit PARGB Blue", KnownPixelFormat.Format32bppPArgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit PARGB Alpha 50%", KnownPixelFormat.Format32bppPArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), 0x80_00_00_80 },
            new object[] { "32 bit PARGB Alpha 1", KnownPixelFormat.Format32bppPArgb, Color.FromArgb(1, Color.Blue), Color.FromArgb(1, Color.Blue), 0x01_00_00_01 },
            new object[] { "32 bit PARGB Alpha 254", KnownPixelFormat.Format32bppPArgb, Color.FromArgb(254, Color.Blue), Color.FromArgb(254, Color.Blue), 0xFE_00_00_FE },
            new object[] { "32 bit PARGB Transparent", KnownPixelFormat.Format32bppPArgb, Color.Transparent, Color.Empty, 0x00_00_00_00 },
            new object[] { "32 bit RGB Blue", KnownPixelFormat.Format32bppRgb, Color.Blue, Color.Blue, 0xFF_00_00_FF },
            new object[] { "32 bit RGB Transparent", KnownPixelFormat.Format32bppRgb, Color.Transparent, Color.Black, 0xFF_00_00_00 },
            new object[] { "24 bit RGB Blue", KnownPixelFormat.Format24bppRgb, Color.Blue, Color.Blue, 0x00_00_FF },
            new object[] { "24 bit RGB Transparent", KnownPixelFormat.Format24bppRgb, Color.Transparent, Color.Black, 0x00_00_00 },
            new object[] { "64 bit ARGB Blue", KnownPixelFormat.Format64bppArgb, Color.Blue, Color.Blue, unchecked((long)0xFFFF_0000_0000_FFFF) },
            new object[] { "64 bit ARGB Alpha 50%", KnownPixelFormat.Format64bppArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), unchecked((long)0x8080_0000_0000_FFFF) },
            new object[] { "64 bit ARGB Transparent", KnownPixelFormat.Format64bppArgb, Color.Transparent, Color.Transparent, 0xFFFF_FFFF_FFFF },
            new object[] { "64 bit PARGB Blue", KnownPixelFormat.Format64bppPArgb, Color.Blue, Color.Blue, unchecked((long)0xFFFF_0000_0000_FFFF) },
            new object[] { "64 bit PARGB Alpha Blue 50%", KnownPixelFormat.Format64bppPArgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(128, Color.Blue), unchecked((long)0x8080_0000_0000_8080) },
            new object[] { "64 bit PARGB Alpha Green 50%", KnownPixelFormat.Format64bppPArgb, Color.FromArgb(128, Color.Green), Color.FromArgb(128, Color.Green), unchecked((long)0x8080_0000_4080_0000) },
            new object[] { "64 bit PARGB Alpha 1", KnownPixelFormat.Format64bppPArgb, Color.FromArgb(1, Color.Blue), Color.FromArgb(1, Color.Blue), 0x101_0000_0000_0101 },
            new object[] { "64 bit PARGB Alpha 254", KnownPixelFormat.Format64bppPArgb, Color.FromArgb(254, Color.Blue), Color.FromArgb(254, Color.Blue), unchecked((long)0xFEFE_0000_0000_FEFE) },
            new object[] { "64 bit PARGB Transparent", KnownPixelFormat.Format64bppPArgb, Color.Transparent, Color.Empty, 0x0000_0000_0000_0000 },
            new object[] { "48 bit RGB Blue", KnownPixelFormat.Format48bppRgb, Color.Blue, Color.Blue, 0x0000_0000_FFFF },
            new object[] { "48 bit RGB White", KnownPixelFormat.Format48bppRgb, Color.White, Color.White, 0xFFFF_FFFF_FFFF },
            new object[] { "48 bit RGB Transparent", KnownPixelFormat.Format48bppRgb, Color.Transparent, Color.Black, 0x0000_0000_0000 },
            new object[] { "16 bit GrayScale White", KnownPixelFormat.Format16bppGrayScale, Color.White, Color.White, 0xFFFF },
            new object[] { "16 bit GrayScale Blue", KnownPixelFormat.Format16bppGrayScale, Color.Blue, Color.FromArgb(0x1D, 0x1D, 0x1D), 0x1D2E },
            new object[] { "16 bit RGB565 Blue", KnownPixelFormat.Format16bppRgb565, Color.Blue, Color.Blue, 0x001F },
            new object[] { "16 bit RGB565 Green", KnownPixelFormat.Format16bppRgb565, Color.Green, Color.FromArgb(0, 130, 0), 0x0400 },
            new object[] { "16 bit RGB565 Transparent", KnownPixelFormat.Format16bppRgb565, Color.Transparent, Color.Black, 0x0000 },
            new object[] { "16 bit RGB565 Empty", KnownPixelFormat.Format16bppRgb565, Color.Empty, Color.Black, 0x0000 },
            new object[] { "16 bit RGB555 Blue", KnownPixelFormat.Format16bppRgb555, Color.Blue, Color.Blue, 0x001F },
            new object[] { "16 bit RGB555 Green", KnownPixelFormat.Format16bppRgb555, Color.Green, Color.FromArgb(0, 132, 0), 0x0200 },
            new object[] { "16 bit RGB555 Transparent", KnownPixelFormat.Format16bppRgb555, Color.Transparent, Color.Black, 0x0000 },
            new object[] { "16 bit RGB555 Empty", KnownPixelFormat.Format16bppRgb555, Color.Empty, Color.Black, 0x0000 },
            new object[] { "16 bit ARGB1555 Blue", KnownPixelFormat.Format16bppArgb1555, Color.Blue, Color.Blue, 0x801F },
            new object[] { "16 bit ARGB1555 Green", KnownPixelFormat.Format16bppArgb1555, Color.Green, Color.FromArgb(0, 132, 0), 0x8200 },
            new object[] { "16 bit ARGB1555 Transparent", KnownPixelFormat.Format16bppArgb1555, Color.Transparent, Color.Transparent, 0x7FFF },
            new object[] { "16 bit ARGB1555 Empty", KnownPixelFormat.Format16bppArgb1555, Color.Empty, Color.Empty, 0x0000 },
            new object[] { "8 bit Indexed Blue", KnownPixelFormat.Format8bppIndexed, Color.Blue, Color.Blue, 12 },
            new object[] { "8 bit Indexed Blue 254", KnownPixelFormat.Format8bppIndexed, Color.FromArgb(0, 0, 254), Color.Blue, 12 },
            new object[] { "8 bit Indexed Transparent", KnownPixelFormat.Format8bppIndexed, Color.Transparent, Color.Empty, 16 },
            new object[] { "4 bit Indexed Blue", KnownPixelFormat.Format4bppIndexed, Color.Blue, Color.Blue, 12 },
            new object[] { "4 bit Indexed Blue 254", KnownPixelFormat.Format4bppIndexed, Color.FromArgb(0, 0, 254), Color.Blue, 12 },
            new object[] { "4 bit Indexed Transparent", KnownPixelFormat.Format4bppIndexed, Color.Transparent, Color.Black, 0 },
            new object[] { "1 bit Indexed Blue", KnownPixelFormat.Format1bppIndexed, Color.Blue, Color.Black, 0 },
            new object[] { "1 bit Indexed Blue 254", KnownPixelFormat.Format1bppIndexed, Color.FromArgb(0, 0, 254), Color.Black, 0 },
            new object[] { "1 bit Indexed Lime", KnownPixelFormat.Format1bppIndexed, Color.Lime, Color.White, 1 },
            new object[] { "1 bit Indexed Transparent", KnownPixelFormat.Format1bppIndexed, Color.Transparent, Color.Black, 0 },
            new object[] { "128 bit RGBA Lime", KnownPixelFormat.Format128bppRgba, Color.Lime, Color.Lime, 0x3F800000_00000000 /* only R and G as float */ },
            new object[] { "128 bit RGBA Alpha 50%", KnownPixelFormat.Format128bppRgba, Color.FromArgb(128, Color.Lime), Color.FromArgb(128, Color.Lime), 0x3F800000_00000000 /* only R and G as float */ },
            new object[] { "128 bit RGBA Transparent", KnownPixelFormat.Format128bppRgba, Color.Transparent, Color.Transparent, 0x3F800000_3F800000 /* only R and G as float */ },
            new object[] { "128 bit PRGBA Lime", KnownPixelFormat.Format128bppPRgba, Color.Lime, Color.Lime, 0x3F800000_00000000 /* only R and G as float */ },
            new object[] { "128 bit PRGBA Alpha Lime 50%", KnownPixelFormat.Format128bppPRgba, Color.FromArgb(128, Color.Lime), Color.FromArgb(128, Color.Lime), 0x3F008081_00000000 },
            new object[] { "128 bit PRGBA Alpha Green 50%", KnownPixelFormat.Format128bppPRgba, Color.FromArgb(128, Color.Green), Color.FromArgb(128, Color.Green), 0x3DDDE874_00000000 },
            new object[] { "128 bit PRGBA Alpha 1", KnownPixelFormat.Format128bppPRgba, Color.FromArgb(1, Color.Lime), Color.FromArgb(1, Color.Lime), 0x3B808081_00000000 },
            new object[] { "128 bit PRGBA Alpha 254", KnownPixelFormat.Format128bppPRgba, Color.FromArgb(254, Color.Lime), Color.FromArgb(254, Color.Lime), 0x3F7EFEFF_00000000 },
            new object[] { "128 bit PRGBA Transparent", KnownPixelFormat.Format128bppPRgba, Color.Transparent, Color.Empty, 0x00000000_00000000 },
            new object[] { "96 bit RGB Blue", KnownPixelFormat.Format96bppRgb, Color.Lime, Color.Lime, 0x3F800000_00000000 },
            new object[] { "96 bit RGB White", KnownPixelFormat.Format96bppRgb, Color.White, Color.White, 0x3F800000_3F800000 },
            new object[] { "96 bit RGB Transparent", KnownPixelFormat.Format96bppRgb, Color.Transparent, Color.Black, 0x00000000_00000000 },
        };

        private static readonly object[][] blendingSetGetPixelTestSource =
        {
            new object[] { "96 bit RGB sRGB", KnownPixelFormat.Format96bppRgb, Color.FromArgb(128, Color.Red), Color.FromArgb(128, 0, 0), WorkingColorSpace.Srgb, 0x3E5D0A8B },
            new object[] { "96 bit RGB Linear", KnownPixelFormat.Format96bppRgb, Color.FromArgb(128, Color.Red), Color.FromArgb(188, 0, 0), WorkingColorSpace.Linear, 0x3F008081 },
            new object[] { "48 bit RGB sRGB", KnownPixelFormat.Format48bppRgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 128), WorkingColorSpace.Srgb, 0x0000_0000_807F },
            new object[] { "48 bit RGB Linear", KnownPixelFormat.Format48bppRgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 188), WorkingColorSpace.Linear, 0x0000_0000_BC94 },
            new object[] { "32 bit RGB sRGB", KnownPixelFormat.Format32bppRgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 127), WorkingColorSpace.Srgb, 0xFF_00_00_7F },
            new object[] { "32 bit RGB Linear", KnownPixelFormat.Format32bppRgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 188), WorkingColorSpace.Linear, 0xFF_00_00_BC },
            new object[] { "24 bit RGB sRGB", KnownPixelFormat.Format24bppRgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 127), WorkingColorSpace.Srgb, 0x00_00_7F },
            new object[] { "24 bit RGB Linear", KnownPixelFormat.Format24bppRgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 188), WorkingColorSpace.Linear, 0x00_00_BC },
            new object[] { "16 bit RGB555 sRGB", KnownPixelFormat.Format16bppRgb555, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 123), WorkingColorSpace.Srgb, 0b00000_00000_01111 },
            new object[] { "16 bit RGB555 Linear", KnownPixelFormat.Format16bppRgb555, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 189), WorkingColorSpace.Linear, 0b00000_00000_10111 },
            new object[] { "16 bit RGB565 sRGB", KnownPixelFormat.Format16bppRgb565, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 123), WorkingColorSpace.Srgb, 0b00000_000000_01111 },
            new object[] { "16 bit RGB565 Linear", KnownPixelFormat.Format16bppRgb565, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 189), WorkingColorSpace.Linear, 0b00000_000000_10111 },
            new object[] { "16 bit ARGB1555 sRGB", KnownPixelFormat.Format16bppArgb1555, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 123), WorkingColorSpace.Srgb, 0b1_00000_00000_01111 },
            new object[] { "16 bit ARGB1555 Linear", KnownPixelFormat.Format16bppArgb1555, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 189), WorkingColorSpace.Linear, 0b1_00000_00000_10111 },
            new object[] { "16 bit Gray sRGB", KnownPixelFormat.Format16bppGrayScale, Color.FromArgb(128, Color.Blue), Color.FromArgb(14, 14, 14), WorkingColorSpace.Srgb, 0x0EA6 },
            new object[] { "16 bit Gray Linear", KnownPixelFormat.Format16bppGrayScale, Color.FromArgb(128, Color.Blue), Color.FromArgb(53, 53, 53), WorkingColorSpace.Linear, 0x35B5 },
            new object[] { "8 bit Indexed sRGB", KnownPixelFormat.Format8bppIndexed, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 128), WorkingColorSpace.Srgb, 4 },
            new object[] { "8 bit Indexed Linear", KnownPixelFormat.Format8bppIndexed, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 204), WorkingColorSpace.Linear, 44 },
            new object[] { "4 bit Indexed sRGB", KnownPixelFormat.Format4bppIndexed, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 128), WorkingColorSpace.Srgb, 4 },
            new object[] { "4 bit Indexed Linear", KnownPixelFormat.Format4bppIndexed, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 128), WorkingColorSpace.Linear, 4 },
            new object[] { "1 bit Indexed sRGB", KnownPixelFormat.Format1bppIndexed, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 0), WorkingColorSpace.Srgb, 0 },
            new object[] { "1 bit Indexed Linear", KnownPixelFormat.Format1bppIndexed, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 0), WorkingColorSpace.Linear, 0 },
        };

        #endregion

        #region Methods

        #region Static Methods

        private static unsafe long GetRawValue(KnownPixelFormat pixelFormat, IBitmapDataInternal bitmapData)
        {
            if (bitmapData is UnmanagedBitmapDataBase unmanagedBitmapData)
                return GetRawValueNative(pixelFormat.ToBitsPerPixel(), unmanagedBitmapData.Scan0);

            if (bitmapData is not ManagedBitmapDataBase managedBitmapDataBase)
                throw new InvalidOperationException();

            fixed (byte* ptr = managedBitmapDataBase)
                return GetRawValueNative(pixelFormat.ToBitsPerPixel(), (IntPtr)ptr);
        }

        private static unsafe long GetRawValueNative(int bpp, IntPtr ptr) => bpp switch
        {
            64 or 96 or 128 => *(long*)ptr,
            48 => *(uint*)ptr | ((long)(((ushort*)ptr)[2]) << 32),
            32 => *(uint*)ptr,
            24 => *(ushort*)ptr | (long)(((byte*)ptr)[2] << 16),
            16 => *(ushort*)ptr,
            8 => *(byte*)ptr,
            4 => *(byte*)ptr >> 4,
            1 => *(byte*)ptr >> 7,
            _ => throw new InvalidOperationException($"Unexpected pixel size: {bpp}bpp")
        };

        #endregion

        #region Instance Methods

        [TestCaseSource(nameof(setGetPixelTestSource))]
        public void SetGetPixelTest(string testName, KnownPixelFormat pixelFormat, Color testColor, Color expectedResult, long expectedRawValue)
        {
            Size size = new Size(3, 2);
            Color actualColor;
            long actualRawValue;

            Console.WriteLine($"{testName}: {pixelFormat} + {testColor}{Environment.NewLine}");

            bool AreEqual(Color c1, Color c2) => c1.ToArgb() == c2.ToArgb()
                || pixelFormat.ToInfoInternal().HasPremultipliedAlpha && c1.A == 0 && c2.A == 0;

            using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                default, 128, WorkingColorSpace.Default, null))
            {
                // by Accessor Set/GetPixel
                Console.Write("SetPixel/GetPixel allocating managed accessor: ");
                managedBitmapData.SetPixel(0, 0, testColor);
                actualColor = managedBitmapData.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(AreEqual(expectedResult, actualColor) ? "OK" : "Fail")})");
                Assert.IsTrue(AreEqual(expectedResult, actualColor));

                actualRawValue = GetRawValue(pixelFormat, managedBitmapData);
                Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")})");
                Assert.AreEqual(expectedRawValue, actualRawValue);

                // by indexer
                managedBitmapData.GetRowCached(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetRowCached(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, managedBitmapData));

                // nonzero coordinates
                managedBitmapData.SetPixel(2, 1, testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetPixel(2, 1)));

                // different color formats
                managedBitmapData.SetColor32(0, 0, testColor.ToColor32());
                Assert.AreEqual(expectedResult.ToColor32(), managedBitmapData.GetColor32(0, 0));
                managedBitmapData.SetPColor32(0, 0, testColor.ToPColor32());
                Assert.AreEqual(expectedResult.ToPColor32(), managedBitmapData.GetPColor32(0, 0));
                managedBitmapData.SetColor64(0, 0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), managedBitmapData.GetColor64(0, 0));
                managedBitmapData.SetPColor64(0, 0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), managedBitmapData.GetPColor64(0, 0));
                managedBitmapData.SetColorF(0, 0, testColor.ToColorF());
                Assert.AreEqual(expectedResult.ToColorF(), managedBitmapData.GetColorF(0, 0));
                managedBitmapData.SetPColorF(0, 0, testColor.ToPColorF());
                Assert.AreEqual(expectedResult.ToPColorF(), managedBitmapData.GetPColorF(0, 0));

                IReadWriteBitmapDataRow row = managedBitmapData[0];
                row.SetColor32(0, testColor.ToColor32());
                Assert.AreEqual(expectedResult.ToColor32(), row.GetColor32(0));
                row.SetPColor32(0, testColor.ToPColor32());
                Assert.AreEqual(expectedResult.ToPColor32(), row.GetPColor32(0));
                row.SetColor64(0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), row.GetColor64(0));
                row.SetPColor64(0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), row.GetPColor64(0));
                row.SetColor64(0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), row.GetColor64(0));
                row.SetPColor64(0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), row.GetPColor64(0));
            }

            int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
            long[] bufManaged = new long[size.Height * longWidth];
            using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(
                new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat))
            {
                // by Accessor Set/GetPixel
                Console.Write("SetPixel/GetPixel wrapping managed accessor: ");
                managedBitmapData.SetPixel(0, 0, testColor);
                actualColor = managedBitmapData.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(AreEqual(expectedResult, actualColor) ? "OK" : "Fail")})");
                Assert.IsTrue(AreEqual(expectedResult, actualColor));

                actualRawValue = GetRawValue(pixelFormat, managedBitmapData);
                Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")})");
                Assert.AreEqual(expectedRawValue, actualRawValue);

                // by indexer
                managedBitmapData.GetRowCached(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetRowCached(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, managedBitmapData));

                // nonzero coordinates
                managedBitmapData.SetPixel(2, 1, testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetPixel(2, 1)));

                // different color formats
                managedBitmapData.SetColor32(0, 0, testColor.ToColor32());
                Assert.AreEqual(expectedResult.ToColor32(), managedBitmapData.GetColor32(0, 0));
                managedBitmapData.SetPColor32(0, 0, testColor.ToPColor32());
                Assert.AreEqual(expectedResult.ToPColor32(), managedBitmapData.GetPColor32(0, 0));
                managedBitmapData.SetColor64(0, 0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), managedBitmapData.GetColor64(0, 0));
                managedBitmapData.SetPColor64(0, 0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), managedBitmapData.GetPColor64(0, 0));
                managedBitmapData.SetColorF(0, 0, testColor.ToColorF());
                Assert.AreEqual(expectedResult.ToColorF(), managedBitmapData.GetColorF(0, 0));
                managedBitmapData.SetPColorF(0, 0, testColor.ToPColorF());
                Assert.AreEqual(expectedResult.ToPColorF(), managedBitmapData.GetPColorF(0, 0));

                IReadWriteBitmapDataRow row = managedBitmapData[0];
                row.SetColor32(0, testColor.ToColor32());
                Assert.AreEqual(expectedResult.ToColor32(), row.GetColor32(0));
                row.SetPColor32(0, testColor.ToPColor32());
                Assert.AreEqual(expectedResult.ToPColor32(), row.GetPColor32(0));
                row.SetColor64(0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), row.GetColor64(0));
                row.SetPColor64(0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), row.GetPColor64(0));
                row.SetColor64(0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), row.GetColor64(0));
                row.SetPColor64(0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), row.GetPColor64(0));
            }

            long[,] bufManaged2D = new long[size.Height, longWidth];
            using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufManaged2D, size.Width, pixelFormat))
            {
                // by Accessor Set/GetPixel
                Console.Write("SetPixel/GetPixel wrapping managed accessor 2D: ");
                managedBitmapData.SetPixel(0, 0, testColor);
                actualColor = managedBitmapData.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(AreEqual(expectedResult, actualColor) ? "OK" : "Fail")})");
                Assert.IsTrue(AreEqual(expectedResult, actualColor));

                actualRawValue = GetRawValue(pixelFormat, managedBitmapData);
                Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")})");
                Assert.AreEqual(expectedRawValue, actualRawValue);

                // by indexer
                managedBitmapData.GetRowCached(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetRowCached(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, managedBitmapData));

                // nonzero coordinates
                managedBitmapData.SetPixel(2, 1, testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetPixel(2, 1)));

                // different color formats
                managedBitmapData.SetColor32(0, 0, testColor.ToColor32());
                Assert.AreEqual(expectedResult.ToColor32(), managedBitmapData.GetColor32(0, 0));
                managedBitmapData.SetPColor32(0, 0, testColor.ToPColor32());
                Assert.AreEqual(expectedResult.ToPColor32(), managedBitmapData.GetPColor32(0, 0));
                managedBitmapData.SetColor64(0, 0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), managedBitmapData.GetColor64(0, 0));
                managedBitmapData.SetPColor64(0, 0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), managedBitmapData.GetPColor64(0, 0));
                managedBitmapData.SetColorF(0, 0, testColor.ToColorF());
                Assert.AreEqual(expectedResult.ToColorF(), managedBitmapData.GetColorF(0, 0));
                managedBitmapData.SetPColorF(0, 0, testColor.ToPColorF());
                Assert.AreEqual(expectedResult.ToPColorF(), managedBitmapData.GetPColorF(0, 0));

                IReadWriteBitmapDataRow row = managedBitmapData[0];
                row.SetColor32(0, testColor.ToColor32());
                Assert.AreEqual(expectedResult.ToColor32(), row.GetColor32(0));
                row.SetPColor32(0, testColor.ToPColor32());
                Assert.AreEqual(expectedResult.ToPColor32(), row.GetPColor32(0));
                row.SetColor64(0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), row.GetColor64(0));
                row.SetPColor64(0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), row.GetPColor64(0));
                row.SetColor64(0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), row.GetColor64(0));
                row.SetPColor64(0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), row.GetPColor64(0));
            }

            int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
            IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
            using (IBitmapDataInternal unmanagedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufUnmanaged, size, stride, pixelFormat, 
                       disposeCallback: () => Marshal.FreeHGlobal(bufUnmanaged)))
            {
                // by Accessor Set/GetPixel
                Console.Write("SetPixel/GetPixel unmanaged accessor: ");
                unmanagedBitmapData.SetPixel(0, 0, testColor);
                actualColor = unmanagedBitmapData.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(AreEqual(expectedResult, actualColor) ? "OK" : "Fail")})");
                Assert.IsTrue(AreEqual(expectedResult, actualColor));

                actualRawValue = GetRawValue(pixelFormat, unmanagedBitmapData);
                Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")})");
                Assert.AreEqual(expectedRawValue, actualRawValue);

                // by indexer
                unmanagedBitmapData.GetRowCached(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, unmanagedBitmapData.GetRowCached(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, unmanagedBitmapData));

                // nonzero coordinates
                unmanagedBitmapData.SetPixel(2, 1, testColor);
                Assert.IsTrue(AreEqual(expectedResult, unmanagedBitmapData.GetPixel(2, 1)));

                // different color formats
                unmanagedBitmapData.SetColor32(0, 0, testColor.ToColor32());
                Assert.AreEqual(expectedResult.ToColor32(), unmanagedBitmapData.GetColor32(0, 0));
                unmanagedBitmapData.SetPColor32(0, 0, testColor.ToPColor32());
                Assert.AreEqual(expectedResult.ToPColor32(), unmanagedBitmapData.GetPColor32(0, 0));
                unmanagedBitmapData.SetColor64(0, 0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), unmanagedBitmapData.GetColor64(0, 0));
                unmanagedBitmapData.SetPColor64(0, 0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), unmanagedBitmapData.GetPColor64(0, 0));
                unmanagedBitmapData.SetColorF(0, 0, testColor.ToColorF());
                Assert.AreEqual(expectedResult.ToColorF(), unmanagedBitmapData.GetColorF(0, 0));
                unmanagedBitmapData.SetPColorF(0, 0, testColor.ToPColorF());
                Assert.AreEqual(expectedResult.ToPColorF(), unmanagedBitmapData.GetPColorF(0, 0));

                IReadWriteBitmapDataRow row = unmanagedBitmapData[0];
                row.SetColor32(0, testColor.ToColor32());
                Assert.AreEqual(expectedResult.ToColor32(), row.GetColor32(0));
                row.SetPColor32(0, testColor.ToPColor32());
                Assert.AreEqual(expectedResult.ToPColor32(), row.GetPColor32(0));
                row.SetColor64(0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), row.GetColor64(0));
                row.SetPColor64(0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), row.GetPColor64(0));
                row.SetColor64(0, testColor.ToColor64());
                Assert.AreEqual(expectedResult.ToColor64(), row.GetColor64(0));
                row.SetPColor64(0, testColor.ToPColor64());
                Assert.AreEqual(expectedResult.ToPColor64(), row.GetPColor64(0));
            }
        }

        [TestCaseSource(nameof(blendingSetGetPixelTestSource))]
        public void BlendingSetGetPixelTest(string testName, KnownPixelFormat pixelFormat, Color testColor, Color expectedResult, WorkingColorSpace workingColorSpace, long expectedRawValue)
        {
            Size size = new Size(3, 2);
            Color actualColor;
            long actualRawValue;

            Console.WriteLine($"{testName}: {pixelFormat} + {testColor}{Environment.NewLine}");

            bool AreEqual(Color c1, Color c2) => c1.ToArgb() == c2.ToArgb()
                || pixelFormat.ToInfoInternal().HasPremultipliedAlpha && c1.A == 0 && c2.A == 0;

            using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                default, 128, workingColorSpace, null))
            {
                // by Accessor Set/GetPixel
                Console.Write("SetPixel/GetPixel allocating managed accessor: ");
                managedBitmapData.SetPixel(0, 0, testColor);
                actualColor = managedBitmapData.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(AreEqual(expectedResult, actualColor) ? "OK" : "Fail")})");
                Assert.IsTrue(AreEqual(expectedResult, actualColor));

                actualRawValue = GetRawValue(pixelFormat, managedBitmapData);
                Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")})");
                Assert.AreEqual(expectedRawValue, actualRawValue);

                // by indexer
                managedBitmapData.GetRowCached(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetRowCached(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, managedBitmapData));

                // nonzero coordinates
                managedBitmapData.SetPixel(2, 1, testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetPixel(2, 1)));
            }

            int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
            long[] bufManaged = new long[size.Height * longWidth];
            using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(
                new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat,
                default, 128, workingColorSpace, null, null, null))
            {
                // by Accessor Set/GetPixel
                Console.Write("SetPixel/GetPixel wrapping managed accessor: ");
                managedBitmapData.SetPixel(0, 0, testColor);
                actualColor = managedBitmapData.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(AreEqual(expectedResult, actualColor) ? "OK" : "Fail")})");
                Assert.IsTrue(AreEqual(expectedResult, actualColor));

                actualRawValue = GetRawValue(pixelFormat, managedBitmapData);
                Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")})");
                Assert.AreEqual(expectedRawValue, actualRawValue);

                // by indexer
                managedBitmapData.GetRowCached(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetRowCached(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, managedBitmapData));

                // nonzero coordinates
                managedBitmapData.SetPixel(2, 1, testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetPixel(2, 1)));
            }

            long[,] bufManaged2D = new long[size.Height, longWidth];
            using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(bufManaged2D, size.Width, pixelFormat,
                default, 128, workingColorSpace, null, null, null))
            {
                // by Accessor Set/GetPixel
                Console.Write("SetPixel/GetPixel wrapping managed accessor 2D: ");
                managedBitmapData.SetPixel(0, 0, testColor);
                actualColor = managedBitmapData.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(AreEqual(expectedResult, actualColor) ? "OK" : "Fail")})");
                Assert.IsTrue(AreEqual(expectedResult, actualColor));

                actualRawValue = GetRawValue(pixelFormat, managedBitmapData);
                Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")})");
                Assert.AreEqual(expectedRawValue, actualRawValue);

                // by indexer
                managedBitmapData.GetRowCached(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetRowCached(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, managedBitmapData));

                // nonzero coordinates
                managedBitmapData.SetPixel(2, 1, testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.GetPixel(2, 1)));
            }

            int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
            IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
            using (IBitmapDataInternal unmanagedBitmapData = BitmapDataFactory.CreateUnmanagedBitmapData(bufUnmanaged, size, stride, pixelFormat,
               default, 128, workingColorSpace, null, null, () => Marshal.FreeHGlobal(bufUnmanaged)))
            {
                // by Accessor Set/GetPixel
                Console.Write("SetPixel/GetPixel unmanaged accessor: ");
                unmanagedBitmapData.SetPixel(0, 0, testColor);
                actualColor = unmanagedBitmapData.GetPixel(0, 0);
                Console.WriteLine($"{expectedResult} vs. {actualColor} ({(AreEqual(expectedResult, actualColor) ? "OK" : "Fail")})");
                Assert.IsTrue(AreEqual(expectedResult, actualColor));

                actualRawValue = GetRawValue(pixelFormat, unmanagedBitmapData);
                Console.WriteLine($"  Expected vs. actual raw value: {expectedRawValue:X8} vs. {actualRawValue:X8} ({(expectedRawValue == actualRawValue ? "OK" : "Fail")})");
                Assert.AreEqual(expectedRawValue, actualRawValue);

                // by indexer
                unmanagedBitmapData.GetRowCached(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, unmanagedBitmapData.GetRowCached(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, unmanagedBitmapData));

                // nonzero coordinates
                unmanagedBitmapData.SetPixel(2, 1, testColor);
                Assert.IsTrue(AreEqual(expectedResult, unmanagedBitmapData.GetPixel(2, 1)));
            }
        }

        [TestCase(KnownPixelFormat.Format24bppRgb)] // no alpha, after blending no loss is expected
        [TestCase(KnownPixelFormat.Format32bppRgb)] // no alpha, after blending no loss is expected
        [TestCase(KnownPixelFormat.Format32bppPArgb)] // direct format, no loss is expected
        [TestCase(KnownPixelFormat.Format64bppPArgb)] // should work without loss of information
        [TestCase(KnownPixelFormat.Format128bppRgba)] // should work without loss of information
        [TestCase(KnownPixelFormat.Format128bppPRgba)] // actually incompatible color space
        public void SetGetPixelP32KnownTest(KnownPixelFormat pixelFormat)
        {
            Size size = new Size(1, 1);
            var baseColor = Color.FromArgb(0x80, 0xFF, 0x40);
            
            Console.WriteLine(pixelFormat);

            foreach (byte a in new[] { 0, 1, 127, 128, 129, 254, 255 })
            {
                PColor32 testColor = PColor32.FromArgb(a, baseColor);
                if (!pixelFormat.HasAlpha())
                    testColor = testColor.Blend(Color.Black.ToPColor32());

                Console.WriteLine($"Test {testColor.GetType().Name} Color: {testColor}");
                
                using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                           default, 128, WorkingColorSpace.Default, null))
                {
                    managedBitmapData.SetPColor32(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetPColor32(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetPColor32(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColor32(0));
                }

                int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
                long[] bufManaged = new long[size.Height * longWidth];
                using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(
                           new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat))
                {
                    managedBitmapData.SetPColor32(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetPColor32(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetPColor32(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColor32(0));
                }

                long[,] bufManaged2D = new long[size.Height, longWidth];
                using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufManaged2D, size.Width, pixelFormat))
                {
                    managedBitmapData.SetPColor32(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetPColor32(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetPColor32(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColor32(0));
                }

                int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
                IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
                using (IBitmapDataInternal unmanagedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufUnmanaged, size, stride, pixelFormat,
                           disposeCallback: () => Marshal.FreeHGlobal(bufUnmanaged)))
                {
                    unmanagedBitmapData.SetPColor32(0, 0, testColor);
                    Assert.AreEqual(testColor, unmanagedBitmapData.GetPColor32(0, 0));

                    IReadWriteBitmapDataRow row = unmanagedBitmapData[0];
                    row.SetPColor32(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColor32(0));
                }
            }
        }

        [Test]
        public void SetGetPixelP32CustomTest()
        {
            var pixelFormatInfo = new PixelFormatInfo(32) { HasPremultipliedAlpha = true };
            Size size = new Size(1, 1);
            var baseColor = Color.FromArgb(0x80, 0xFF, 0x40);

            Console.WriteLine(pixelFormatInfo);

            foreach (byte a in new[] { 0, 1, 127, 128, 129, 254, 255 })
            {
                PColor32 testColor = PColor32.FromArgb(a, baseColor);
                Console.WriteLine($"Test {testColor.GetType().Name} Color: {testColor}");

                throw new NotImplementedException("TODO: custom P32 setter/getter");
                //using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                //           default, 128, WorkingColorSpace.Default, null))
                //{
                //    managedBitmapData.SetPColor32(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetPColor32(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetPColor32(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColor32(0));
                //}

                //int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
                //long[] bufManaged = new long[size.Height * longWidth];
                //using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(
                //           new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat))
                //{
                //    managedBitmapData.SetPColor32(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetPColor32(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetPColor32(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColor32(0));
                //}

                //long[,] bufManaged2D = new long[size.Height, longWidth];
                //using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufManaged2D, size.Width, pixelFormat))
                //{
                //    managedBitmapData.SetPColor32(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetPColor32(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetPColor32(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColor32(0));
                //}

                //int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
                //IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
                //using (IBitmapDataInternal unmanagedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufUnmanaged, size, stride, pixelFormat,
                //           disposeCallback: () => Marshal.FreeHGlobal(bufUnmanaged)))
                //{
                //    unmanagedBitmapData.SetPColor32(0, 0, testColor);
                //    Assert.AreEqual(testColor, unmanagedBitmapData.GetPColor32(0, 0));

                //    IReadWriteBitmapDataRow row = unmanagedBitmapData[0];
                //    row.SetPColor32(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColor32(0));
                //}
            }
        }

        [TestCase(KnownPixelFormat.Format48bppRgb)] // no alpha, after blending no loss is expected
        [TestCase(KnownPixelFormat.Format64bppArgb)] // direct format, no loss is expected
        [TestCase(KnownPixelFormat.Format96bppRgb)] // no alpha, after blending no loss is expected
        [TestCase(KnownPixelFormat.Format128bppRgba)] // should work without loss of information
        [TestCase(KnownPixelFormat.Format128bppPRgba)] // should not work
        public void SetGetPixelS64KnownTest(KnownPixelFormat pixelFormat)
        {
            Size size = new Size(1, 1);
            var baseColor = Color.FromArgb(0x80, 0xFF, 0x40);

            Console.WriteLine(pixelFormat);

            foreach (ushort a in new[] { 0, 1, 32767, 32768, 32769, 65534, 65535 })
            {
                Color64 testColor = Color64.FromArgb(a, baseColor.ToColor64());
                if (!pixelFormat.HasAlpha())
                    testColor = testColor.Blend(Color.Black.ToColor64());

                Console.WriteLine($"Test {testColor.GetType().Name} Color: {testColor}");

                using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                           default, 128, WorkingColorSpace.Default, null))
                {
                    managedBitmapData.SetColor64(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetColor64(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetColor64(0, testColor);
                    Assert.AreEqual(testColor, row.GetColor64(0));
                }

                int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
                long[] bufManaged = new long[size.Height * longWidth];
                using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(
                           new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat))
                {
                    managedBitmapData.SetColor64(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetColor64(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetColor64(0, testColor);
                    Assert.AreEqual(testColor, row.GetColor64(0));
                }

                long[,] bufManaged2D = new long[size.Height, longWidth];
                using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufManaged2D, size.Width, pixelFormat))
                {
                    managedBitmapData.SetColor64(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetColor64(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetColor64(0, testColor);
                    Assert.AreEqual(testColor, row.GetColor64(0));
                }

                int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
                IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
                using (IBitmapDataInternal unmanagedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufUnmanaged, size, stride, pixelFormat,
                           disposeCallback: () => Marshal.FreeHGlobal(bufUnmanaged)))
                {
                    unmanagedBitmapData.SetColor64(0, 0, testColor);
                    Assert.AreEqual(testColor, unmanagedBitmapData.GetColor64(0, 0));

                    IReadWriteBitmapDataRow row = unmanagedBitmapData[0];
                    row.SetColor64(0, testColor);
                    Assert.AreEqual(testColor, row.GetColor64(0));
                }
            }
        }

        [Test]
        public void SetGetPixelS64CustomTest()
        {
            var pixelFormatInfo = new PixelFormatInfo(64) { HasAlpha = true, /*PrefersColor64 = true*/ };
            Size size = new Size(1, 1);
            var baseColor = Color.FromArgb(0x80, 0xFF, 0x40);

            Console.WriteLine(pixelFormatInfo);

            foreach (ushort a in new[] { 0, 1, 32767, 32768, 32769, 65534, 65535 })
            {
                Color64 testColor = Color64.FromArgb(a, baseColor.ToColor64());
                Console.WriteLine($"Test {testColor.GetType().Name} Color: {testColor}");

                throw new NotImplementedException("TODO: custom P32 setter/getter");
                //using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                //           default, 128, WorkingColorSpace.Default, null))
                //{
                //    managedBitmapData.SetColor64(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetColor64(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetColor64(0, testColor);
                //    Assert.AreEqual(testColor, row.GetColor64(0));
                //}

                //int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
                //long[] bufManaged = new long[size.Height * longWidth];
                //using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(
                //           new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat))
                //{
                //    managedBitmapData.SetColor64(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetColor64(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetColor64(0, testColor);
                //    Assert.AreEqual(testColor, row.GetColor64(0));
                //}

                //long[,] bufManaged2D = new long[size.Height, longWidth];
                //using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufManaged2D, size.Width, pixelFormat))
                //{
                //    managedBitmapData.SetColor64(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetColor64(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetColor64(0, testColor);
                //    Assert.AreEqual(testColor, row.GetColor64(0));
                //}

                //int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
                //IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
                //using (IBitmapDataInternal unmanagedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufUnmanaged, size, stride, pixelFormat,
                //           disposeCallback: () => Marshal.FreeHGlobal(bufUnmanaged)))
                //{
                //    unmanagedBitmapData.SetColor64(0, 0, testColor);
                //    Assert.AreEqual(testColor, unmanagedBitmapData.GetColor64(0, 0));

                //    IReadWriteBitmapDataRow row = unmanagedBitmapData[0];
                //    row.SetColor64(0, testColor);
                //    Assert.AreEqual(testColor, row.GetColor64(0));
                //}
            }
        }

        [TestCase(KnownPixelFormat.Format32bppArgb)] // TODO: should not work, remove this after implementation is done
        [TestCase(KnownPixelFormat.Format48bppRgb)] // no alpha, after blending no loss is expected
        [TestCase(KnownPixelFormat.Format64bppArgb)] // actually does not work for all possible values but in most cases it works
        [TestCase(KnownPixelFormat.Format64bppPArgb)] // direct format, no loss is expected
        [TestCase(KnownPixelFormat.Format96bppRgb)] // no alpha, after blending no loss is expected
        [TestCase(KnownPixelFormat.Format128bppRgba)] // just for curiosity: does the conversion back preserve the information?
        [TestCase(KnownPixelFormat.Format128bppPRgba)] // actually incompatible color space
        public void SetGetPixelP64KnownTest(KnownPixelFormat pixelFormat)
        {
            Size size = new Size(1, 1);
            var baseColor = Color.FromArgb(0x80, 0xFF, 0x40);

            Console.WriteLine(pixelFormat);

            foreach (ushort a in new[] { 0, 1, 32767, 32768, 32769, 65534, 65535 })
            {
                PColor64 testColor = PColor64.FromArgb(a, baseColor.ToColor64());
                if (!pixelFormat.HasAlpha())
                    testColor = testColor.Blend(Color.Black.ToPColor64());

                Console.WriteLine($"Test {testColor.GetType().Name} Color: {testColor}");

                using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                           default, 128, WorkingColorSpace.Default, null))
                {
                    managedBitmapData.SetPColor64(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetPColor64(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetPColor64(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColor64(0));
                }

                int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
                long[] bufManaged = new long[size.Height * longWidth];
                using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(
                           new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat))
                {
                    managedBitmapData.SetPColor64(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetPColor64(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetPColor64(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColor64(0));
                }

                long[,] bufManaged2D = new long[size.Height, longWidth];
                using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufManaged2D, size.Width, pixelFormat))
                {
                    managedBitmapData.SetPColor64(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetPColor64(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetPColor64(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColor64(0));
                }

                int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
                IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
                using (IBitmapDataInternal unmanagedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufUnmanaged, size, stride, pixelFormat,
                           disposeCallback: () => Marshal.FreeHGlobal(bufUnmanaged)))
                {
                    unmanagedBitmapData.SetPColor64(0, 0, testColor);
                    Assert.AreEqual(testColor, unmanagedBitmapData.GetPColor64(0, 0));

                    IReadWriteBitmapDataRow row = unmanagedBitmapData[0];
                    row.SetPColor64(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColor64(0));
                }
            }
        }

        [Test]
        public void SetGetPixelP64CustomTest()
        {
            var pixelFormatInfo = new PixelFormatInfo(64) { HasPremultipliedAlpha = true, /*PrefersColor64 = true*/ };
            Size size = new Size(1, 1);
            var baseColor = Color.FromArgb(0x80, 0xFF, 0x40);

            Console.WriteLine(pixelFormatInfo);

            foreach (ushort a in new[] { 0, 1, 32767, 32768, 32769, 65534, 65535 })
            {
                PColor64 testColor = PColor64.FromArgb(a, baseColor.ToColor64());
                Console.WriteLine($"Test {testColor.GetType().Name} Color: {testColor}");

                throw new NotImplementedException("TODO: custom P64 setter/getter");
                //using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                //           default, 128, WorkingColorSpace.Default, null))
                //{
                //    managedBitmapData.SetPColor64(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetPColor64(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetPColor64(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColor64(0));
                //}

                //int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
                //long[] bufManaged = new long[size.Height * longWidth];
                //using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(
                //           new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat))
                //{
                //    managedBitmapData.SetPColor64(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetPColor64(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetPColor64(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColor64(0));
                //}

                //long[,] bufManaged2D = new long[size.Height, longWidth];
                //using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufManaged2D, size.Width, pixelFormat))
                //{
                //    managedBitmapData.SetPColor64(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetPColor64(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetPColor64(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColor64(0));
                //}

                //int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
                //IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
                //using (IBitmapDataInternal unmanagedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufUnmanaged, size, stride, pixelFormat,
                //           disposeCallback: () => Marshal.FreeHGlobal(bufUnmanaged)))
                //{
                //    unmanagedBitmapData.SetPColor64(0, 0, testColor);
                //    Assert.AreEqual(testColor, unmanagedBitmapData.GetPColor64(0, 0));

                //    IReadWriteBitmapDataRow row = unmanagedBitmapData[0];
                //    row.SetPColor64(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColor64(0));
                //}
            }
        }

        [TestCase(KnownPixelFormat.Format96bppRgb)] // no alpha, after blending no loss is expected
        [TestCase(KnownPixelFormat.Format128bppRgba)] // direct format, no loss is expected
        public void SetGetPixelS128KnownTest(KnownPixelFormat pixelFormat)
        {
            Size size = new Size(1, 1);
            var baseColor = Color.FromArgb(0x80, 0xFF, 0x40);

            Console.WriteLine(pixelFormat);

            foreach (float a in new[] { 0f, 0f.Inc(), 0.5f.Dec(), 0.5f, 0.5f.Inc(), 1f.Dec(), 1f })
            {
                ColorF testColor = ColorF.FromArgb(a, baseColor.ToColorF());
                if (!pixelFormat.HasAlpha())
                    testColor = testColor.Blend(Color.Black.ToColorF());

                Console.WriteLine($"Test {testColor.GetType().Name} Color: {testColor}");

                using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                           default, 128, WorkingColorSpace.Default, null))
                {
                    managedBitmapData.SetColorF(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetColorF(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetColorF(0, testColor);
                    Assert.AreEqual(testColor, row.GetColorF(0));
                }

                int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
                long[] bufManaged = new long[size.Height * longWidth];
                using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(
                           new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat))
                {
                    managedBitmapData.SetColorF(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetColorF(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetColorF(0, testColor);
                    Assert.AreEqual(testColor, row.GetColorF(0));
                }

                long[,] bufManaged2D = new long[size.Height, longWidth];
                using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufManaged2D, size.Width, pixelFormat))
                {
                    managedBitmapData.SetColorF(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetColorF(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetColorF(0, testColor);
                    Assert.AreEqual(testColor, row.GetColorF(0));
                }

                int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
                IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
                using (IBitmapDataInternal unmanagedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufUnmanaged, size, stride, pixelFormat,
                           disposeCallback: () => Marshal.FreeHGlobal(bufUnmanaged)))
                {
                    unmanagedBitmapData.SetColorF(0, 0, testColor);
                    Assert.AreEqual(testColor, unmanagedBitmapData.GetColorF(0, 0));

                    IReadWriteBitmapDataRow row = unmanagedBitmapData[0];
                    row.SetColorF(0, testColor);
                    Assert.AreEqual(testColor, row.GetColorF(0));
                }
            }
        }

        [Test]
        public void SetGetPixelS128CustomTest()
        {
            var pixelFormatInfo = new PixelFormatInfo(128) { HasAlpha = true, /*PrefersColorF = true*/ };
            Size size = new Size(1, 1);
            var baseColor = Color.FromArgb(0x80, 0xFF, 0x40);

            Console.WriteLine(pixelFormatInfo);

            foreach (float a in new[] { 0f, 0f.Inc(), 0.5f.Dec(), 0.5f, 0.5f.Inc(), 1f.Dec(), 1f })
            {
                ColorF testColor = ColorF.FromArgb(a, baseColor.ToColorF());
                Console.WriteLine($"Test {testColor.GetType().Name} Color: {testColor}");

                throw new NotImplementedException("TODO: custom ColorF setter/getter");
                //using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                //           default, 128, WorkingColorSpace.Default, null))
                //{
                //    managedBitmapData.SetColorF(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetColorF(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetColorF(0, testColor);
                //    Assert.AreEqual(testColor, row.GetColorF(0));
                //}

                //int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
                //long[] bufManaged = new long[size.Height * longWidth];
                //using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(
                //           new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat))
                //{
                //    managedBitmapData.SetColorF(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetColorF(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetColorF(0, testColor);
                //    Assert.AreEqual(testColor, row.GetColorF(0));
                //}

                //long[,] bufManaged2D = new long[size.Height, longWidth];
                //using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufManaged2D, size.Width, pixelFormat))
                //{
                //    managedBitmapData.SetColorF(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetColorF(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetColorF(0, testColor);
                //    Assert.AreEqual(testColor, row.GetColorF(0));
                //}

                //int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
                //IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
                //using (IBitmapDataInternal unmanagedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufUnmanaged, size, stride, pixelFormat,
                //           disposeCallback: () => Marshal.FreeHGlobal(bufUnmanaged)))
                //{
                //    unmanagedBitmapData.SetColorF(0, 0, testColor);
                //    Assert.AreEqual(testColor, unmanagedBitmapData.GetColorF(0, 0));

                //    IReadWriteBitmapDataRow row = unmanagedBitmapData[0];
                //    row.SetColorF(0, testColor);
                //    Assert.AreEqual(testColor, row.GetColorF(0));
                //}
            }
        }

        [TestCase(KnownPixelFormat.Format96bppRgb)] // no alpha, after blending no loss is expected
        [TestCase(KnownPixelFormat.Format128bppRgba)] // should not work - TODO: but NaNs are not good...
        [TestCase(KnownPixelFormat.Format128bppPRgba)] // direct format, no loss is expected
        public void SetGetPixelP128KnownTest(KnownPixelFormat pixelFormat)
        {
            Size size = new Size(1, 1);
            var baseColor = Color.FromArgb(0x80, 0xFF, 0x40);

            Console.WriteLine(pixelFormat);

            foreach (float a in new[] { 0f, 0f.Inc(), 0.5f.Dec(), 0.5f, 0.5f.Inc(), 1f.Dec(), 1f })
            {
                PColorF testColor = PColorF.FromArgb(a, baseColor.ToColorF());
                if (!pixelFormat.HasAlpha())
                    testColor = testColor.Blend(Color.Black.ToPColorF());

                Console.WriteLine($"Test {testColor.GetType().Name} Color: {testColor}");

                using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                           default, 128, WorkingColorSpace.Default, null))
                {
                    managedBitmapData.SetPColorF(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetPColorF(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetPColorF(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColorF(0));
                }

                int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
                long[] bufManaged = new long[size.Height * longWidth];
                using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(
                           new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat))
                {
                    managedBitmapData.SetPColorF(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetPColorF(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetPColorF(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColorF(0));
                }

                long[,] bufManaged2D = new long[size.Height, longWidth];
                using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufManaged2D, size.Width, pixelFormat))
                {
                    managedBitmapData.SetPColorF(0, 0, testColor);
                    Assert.AreEqual(testColor, managedBitmapData.GetPColorF(0, 0));

                    IReadWriteBitmapDataRow row = managedBitmapData[0];
                    row.SetPColorF(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColorF(0));
                }

                int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
                IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
                using (IBitmapDataInternal unmanagedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufUnmanaged, size, stride, pixelFormat,
                           disposeCallback: () => Marshal.FreeHGlobal(bufUnmanaged)))
                {
                    unmanagedBitmapData.SetPColorF(0, 0, testColor);
                    Assert.AreEqual(testColor, unmanagedBitmapData.GetPColorF(0, 0));

                    IReadWriteBitmapDataRow row = unmanagedBitmapData[0];
                    row.SetPColorF(0, testColor);
                    Assert.AreEqual(testColor, row.GetPColorF(0));
                }
            }
        }

        [Test]
        public void SetGetPixelP128CustomTest()
        {
            var pixelFormatInfo = new PixelFormatInfo(128) { HasPremultipliedAlpha = true, /*PrefersColorF = true*/ };
            Size size = new Size(1, 1);
            var baseColor = Color.FromArgb(0x80, 0xFF, 0x40);

            Console.WriteLine(pixelFormatInfo);

            foreach (float a in new[] { 0f, 0f.Inc(), 0.5f.Dec(), 0.5f, 0.5f.Inc(), 1f.Dec(), 1f })
            {
                PColorF testColor = PColorF.FromArgb(a, baseColor.ToColorF());
                Console.WriteLine($"Test {testColor.GetType().Name} Color: {testColor}");

                throw new NotImplementedException("TODO: custom PColorF setter/getter");
                //using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(size, pixelFormat,
                //           default, 128, WorkingColorSpace.Default, null))
                //{
                //    managedBitmapData.SetPColorF(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetPColorF(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetPColorF(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColorF(0));
                //}

                //int longWidth = pixelFormat.ToBitsPerPixel() > 64 ? size.Width * 2 : size.Width;
                //long[] bufManaged = new long[size.Height * longWidth];
                //using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(
                //           new Array2D<long>(bufManaged, size.Height, longWidth), size.Width, pixelFormat))
                //{
                //    managedBitmapData.SetPColorF(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetPColorF(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetPColorF(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColorF(0));
                //}

                //long[,] bufManaged2D = new long[size.Height, longWidth];
                //using (IBitmapDataInternal managedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufManaged2D, size.Width, pixelFormat))
                //{
                //    managedBitmapData.SetPColorF(0, 0, testColor);
                //    Assert.AreEqual(testColor, managedBitmapData.GetPColorF(0, 0));

                //    IReadWriteBitmapDataRow row = managedBitmapData[0];
                //    row.SetPColorF(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColorF(0));
                //}

                //int stride = Math.Max(8, pixelFormat.GetByteWidth(size.Width));
                //IntPtr bufUnmanaged = Marshal.AllocHGlobal(stride * size.Height);
                //using (IBitmapDataInternal unmanagedBitmapData = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bufUnmanaged, size, stride, pixelFormat,
                //           disposeCallback: () => Marshal.FreeHGlobal(bufUnmanaged)))
                //{
                //    unmanagedBitmapData.SetPColorF(0, 0, testColor);
                //    Assert.AreEqual(testColor, unmanagedBitmapData.GetPColorF(0, 0));

                //    IReadWriteBitmapDataRow row = unmanagedBitmapData[0];
                //    row.SetPColorF(0, testColor);
                //    Assert.AreEqual(testColor, row.GetPColorF(0));
                //}
            }
        }

        #endregion

        #endregion
    }
}
