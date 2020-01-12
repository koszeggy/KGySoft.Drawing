#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OptimizedPaletteQuantizerTest.cs
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

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class OptimizedPaletteQuantizerTest
    {
        #region Fields

        private static object[][] fullyTransparentImageTestSource =
        {
            new object[] { "Octree TR", OptimizedPaletteQuantizer.MedianCut() },
            new object[] { "Octree Solid", OptimizedPaletteQuantizer.MedianCut(alphaThreshold: 0) },
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