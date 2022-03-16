﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessTest.cs
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

using KGySoft.Collections;
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
            new object[] { "32 bit RGB Alpha 50%", KnownPixelFormat.Format32bppRgb, Color.FromArgb(128, Color.Blue), Color.FromArgb(0, 0, 128), 0xFF_00_00_80 },
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

        private static unsafe long GetRawValueNative(int bpp, IntPtr ptr)
        {
            switch (bpp)
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
                    throw new InvalidOperationException($"Unexpected pixel size: {bpp}bpp");
            }
        }

        #endregion

        #region Instance Methods

        [TestCaseSource(nameof(setGetPixelTestSource))]
        public unsafe void SetGetPixelTest(string testName, KnownPixelFormat pixelFormat, Color testColor, Color expectedResult, long expectedRawValue)
        {
            Color actualColor;
            long actualRawValue;

            Console.WriteLine($"{testName}: {pixelFormat} + {testColor}{Environment.NewLine}");

            bool AreEqual(Color c1, Color c2) => c1.ToArgb() == c2.ToArgb()
                || pixelFormat.ToInfo().HasPremultipliedAlpha && c1.A == 0 && c2.A == 0;

            using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(new Size(1, 1), pixelFormat))
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
                managedBitmapData.DoGetRow(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.DoGetRow(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, managedBitmapData));
            }

            long[] bufManaged = new long[1];
            using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(new Array2D<long>(bufManaged, 1, 1), 1, pixelFormat))
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
                managedBitmapData.DoGetRow(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.DoGetRow(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, managedBitmapData));
            }

            long[,] bufManaged2D = new long[1, 1];
            using (IBitmapDataInternal managedBitmapData = BitmapDataFactory.CreateManagedBitmapData(bufManaged2D, 1, pixelFormat))
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
                managedBitmapData.DoGetRow(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, managedBitmapData.DoGetRow(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, managedBitmapData));
            }

            long bufUnmanaged = 0L;
            using (IBitmapDataInternal unmanagedBitmapData = BitmapDataFactory.CreateUnmanagedBitmapData((IntPtr)(&bufUnmanaged), new Size(1, 1), sizeof(long), pixelFormat))
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
                unmanagedBitmapData.DoGetRow(0)[0] = new Color32(testColor);
                Assert.IsTrue(AreEqual(expectedResult, unmanagedBitmapData.DoGetRow(0)[0].ToColor()));
                Assert.AreEqual(expectedRawValue, GetRawValue(pixelFormat, unmanagedBitmapData));
            }
        }

        #endregion

        #endregion
    }
}