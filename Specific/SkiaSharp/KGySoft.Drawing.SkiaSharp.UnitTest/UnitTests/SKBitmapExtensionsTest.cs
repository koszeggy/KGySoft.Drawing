#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKBitmapExtensionsTest.cs
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
using System.Runtime.CompilerServices;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp.UnitTests
{
    [TestFixture]
    public class SKBitmapExtensionsTest
    {
        #region Fields

        private static readonly Color testColor = Color.FromArgb(128, Color.Blue);

        #endregion

        #region Methods

        [Test]
        public void SetGetPixelTest()
        {
            foreach (SKColorType colorType in Enum<SKColorType>.GetValues())
            {
                if (colorType == SKColorType.Unknown)
                    continue;

                foreach (SKAlphaType alphaType in Enum<SKAlphaType>.GetValues())
                {
                    if (alphaType == SKAlphaType.Unknown)
                        continue;

                    try
                    {
                        using var bitmap = new SKBitmap(new SKImageInfo(10, 10, colorType, alphaType));
                        bitmap.SetPixel(2, 3, new Color32(testColor).ToSKColor());
                        Color32 expected = bitmap.GetPixel(2, 3).ToColor32();
                        Console.Write($"{colorType}, {alphaType}: {bitmap.Info.AlphaType}: ");
                        
                        using IReadWriteBitmapData readWriteBitmapData = bitmap.GetReadWriteBitmapData();
                        readWriteBitmapData.SetPixel(2, 3, testColor);
                        Color32 actual = readWriteBitmapData.GetPixel(2, 3).ToColor32();
                        Assert.AreEqual(expected, actual);
                        Console.WriteLine(actual);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create bitmap with {colorType} and {alphaType}: {ex.Message}");
                    }
                }
            }
        }

        [Test]
        public void Test()
        {
            using (SKSurface surface = SKSurface.Create(new SKImageInfo(10, 10)))
            {
                using (IReadWriteBitmapData readWriteBitmapData = surface.GetReadWriteBitmapData())
                    readWriteBitmapData.SetPixel(2, 3, testColor);
            }
        }

        #endregion
    }
}
