#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedPaletteQuantizerTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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
    public class OptimizedPaletteQuantizerTest
    {
        #region Fields

        private static readonly object[][] fullyTransparentImageTestSource =
        {
            new object[] { "Octree TR", OptimizedPaletteQuantizer.Octree() },
            new object[] { "Octree Solid", OptimizedPaletteQuantizer.Octree(alphaThreshold: 0) },
            new object[] { "MedianCut TR", OptimizedPaletteQuantizer.MedianCut(),  },
            new object[] { "MedianCut Solid", OptimizedPaletteQuantizer.MedianCut(alphaThreshold: 0),  },
            new object[] { "Wu TR", OptimizedPaletteQuantizer.Wu(),  },
            new object[] { "Wu Solid", OptimizedPaletteQuantizer.Wu(alphaThreshold: 0),  },
        };

        #endregion

        #region Methods

        [TestCaseSource(nameof(fullyTransparentImageTestSource))]
        public void FullyTransparentImageTest(string testName, IQuantizer quantizer)
        {
            Console.WriteLine(testName);
            using var bmp = new Bitmap(1, 1);
            Assert.DoesNotThrow(() => bmp.Quantize(quantizer));
        }

        #endregion
    }
}