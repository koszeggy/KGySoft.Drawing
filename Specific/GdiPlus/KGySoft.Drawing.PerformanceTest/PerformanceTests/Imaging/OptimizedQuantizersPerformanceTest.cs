#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedQuantizersPerformanceTest.cs
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

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests.Imaging
{
    [TestFixture]
    public class OptimizedQuantizersPerformanceTest
    {
        #region Methods

        [TestCase(256, null)]
        [TestCase(2, 1)]
        [TestCase(16, 4)]
        [TestCase(256, 8)]
        [TestCase(65536, 8)]
        public void OptimizedQuantizersTest(int maxColors, int? bitLevel)
        {
            //Bitmap bmp = Icons.Information.ExtractBitmap(new Size(256, 256));
            Bitmap bmp = new Bitmap(@"..\..\..\..\KGySoft.Drawing\Help\Images\AlphaGradient.png");
            IReadableBitmapData source = bmp!.GetReadableBitmapData();

            new PerformanceTest { TestName = $"Colors: {maxColors}, BitLevel:{bitLevel?.ToString() ?? "Default"}" }
                .AddCase(() =>
                {
                    using var _ = source.Clone(KnownPixelFormat.Format32bppArgb, OptimizedPaletteQuantizer.Octree(maxColors).ConfigureBitLevel(bitLevel));
                }, nameof(OptimizedPaletteQuantizer.Octree))
                .AddCase(() =>
                {
                    using var _ = source.Clone(KnownPixelFormat.Format32bppArgb, OptimizedPaletteQuantizer.MedianCut(maxColors).ConfigureBitLevel(bitLevel));
                }, nameof(OptimizedPaletteQuantizer.MedianCut))
                .AddCase(() =>
                {
                    using var _ = source.Clone(KnownPixelFormat.Format32bppArgb, OptimizedPaletteQuantizer.Wu(maxColors).ConfigureBitLevel(bitLevel));
                }, nameof(OptimizedPaletteQuantizer.Wu))
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }
}
