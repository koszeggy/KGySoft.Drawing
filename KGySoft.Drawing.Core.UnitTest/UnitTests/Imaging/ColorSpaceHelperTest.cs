#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorSpaceHelperTest.cs
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

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class ColorSpaceHelperTest
    {
        #region Methods

        [TestCase(0.5f, 0.5f, 0.25f, 0.125f)] // Pow x3
        [TestCase(0.5f, 0.001f, 0.002f, 0.003f)] // Linear conversion, 0 pow
        [TestCase(0.5f, 1f, 0.5f, 0.25f)] // Different channels, Pow x2
        [TestCase(0.5f, 0f, 1f, 0.001f)] // Different channels, 0 pow
        [TestCase(0.5f, 1.5f, -1f, Single.NaN)] // Out-of-range values
        public void LinearToSrgbTest(float a, float r, float g, float b)
        {
            var color = new ColorF(a, r, g, b);

            var expected = new ColorF(a.ClipF(),
                ColorSpaceHelper.LinearToSrgb(r),
                ColorSpaceHelper.LinearToSrgb(g),
                ColorSpaceHelper.LinearToSrgb(b));

            var actual = color.ToSrgb();
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs. {actual}");
        }

        [TestCase(0.5f, 0.5f, 0.25f, 0.125f)] // Pow x3
        [TestCase(0.5f, 0.001f, 0.002f, 0.003f)] // Linear conversion, 0 pow
        [TestCase(0.5f, 1f, 0.5f, 0.25f)] // Different channels, Pow x2
        [TestCase(0.5f, 0f, 1f, 0.001f)] // Different channels, 0 pow
        [TestCase(0.5f, 1.5f, -1f, Single.NaN)] // Out-of-range values
        public void SrgbToLinearTest(float a, float r, float g, float b)
        {
            var color = new ColorF(a, r, g, b);

            var expected = new ColorF(a.ClipF(),
                ColorSpaceHelper.SrgbToLinear(r),
                ColorSpaceHelper.SrgbToLinear(g),
                ColorSpaceHelper.SrgbToLinear(b));

            var actual = color.ToLinear();
            Assert.IsTrue(expected.TolerantEquals(actual), $"{expected} vs. {actual}");
        }

        #endregion
    }
}