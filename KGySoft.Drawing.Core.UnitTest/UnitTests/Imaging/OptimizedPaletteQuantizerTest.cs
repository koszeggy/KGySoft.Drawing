#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedPaletteQuantizerTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class OptimizedPaletteQuantizerTest : TestBase
    {
        #region Fields

        private static readonly object[][] fullyTransparentImageTestSource =
        {
            new object[] { "Octree TR", OptimizedPaletteQuantizer.Octree() },
            new object[] { "Octree Solid", OptimizedPaletteQuantizer.Octree(alphaThreshold: 0) },
            new object[] { "MedianCut TR", OptimizedPaletteQuantizer.MedianCut(), },
            new object[] { "MedianCut Solid", OptimizedPaletteQuantizer.MedianCut(alphaThreshold: 0), },
            new object[] { "Wu TR", OptimizedPaletteQuantizer.Wu(), },
            new object[] { "Wu Solid", OptimizedPaletteQuantizer.Wu(alphaThreshold: 0), },
        };

        #endregion

        #region Methods

        [TestCaseSource(nameof(fullyTransparentImageTestSource))]
        public void FullyTransparentImageTest(string testName, IQuantizer quantizer)
        {
            Console.WriteLine(testName);
            using var bitmapData = BitmapDataFactory.CreateBitmapData(new Size(1, 1));
            Assert.DoesNotThrow(() => bitmapData.Quantize(quantizer));
        }

        [TestCase(2)]
        [TestCase(4)]
        [TestCase(16)]
        [TestCase(256)]
        [TestCase(512)]
        [TestCase(1024)]
        [TestCase(65536)]
        public void MaxColorsTest(int colorCount)
        {
            IReadWriteBitmapData source = GenerateAlphaGradientBitmapData(new Size(256, 128));

            foreach (var getQuantizer in new Func<int, Color, byte, OptimizedPaletteQuantizer>[]
                {
                    OptimizedPaletteQuantizer.Octree,
                    OptimizedPaletteQuantizer.MedianCut,
                    OptimizedPaletteQuantizer.Wu
                })
            {
                OptimizedPaletteQuantizer quantizer = getQuantizer.Invoke(colorCount, Color.Silver, 0);
                using IReadWriteBitmapData clone = source.Clone(quantizer.PixelFormatHint, quantizer);
                Assert.AreEqual(quantizer.PixelFormatHint, clone.PixelFormat.ToKnownPixelFormat());
                SaveBitmapData($"{getQuantizer.Method.Name} {colorCount}", clone);
                clone.Dispose();
            }
        }

        #endregion
    }
}