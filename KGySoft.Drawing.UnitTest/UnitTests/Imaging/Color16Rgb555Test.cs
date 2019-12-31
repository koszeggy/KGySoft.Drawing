#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color16Rgb555Test.cs
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

using System.Drawing;

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class Color16Rgb555Test
    {
        #region Methods

        [Test]
        public unsafe void SizeAndAlignmentTest()
        {
            Assert.AreEqual(2, sizeof(Color16Rgb555));

            Color16Rgb555* p = stackalloc Color16Rgb555[2];
            Assert.AreEqual(2, (byte*)&p[1] - (byte*)&p[0]);
        }

        #endregion
    }
}