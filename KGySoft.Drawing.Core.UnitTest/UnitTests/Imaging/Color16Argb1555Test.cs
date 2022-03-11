#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color16Argb555Test.cs
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

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class Color16Argb555Test
    {
        #region Methods

        [Test]
        public unsafe void SizeAndAlignmentTest()
        {
            Assert.AreEqual(2, sizeof(Color16Argb1555));

            Color16Argb1555* p = stackalloc Color16Argb1555[2];
            Assert.AreEqual(2, (byte*)&p[1] - (byte*)&p[0]);
        }

        #endregion
    }
}