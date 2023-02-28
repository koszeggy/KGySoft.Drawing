#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensionsTest.cs
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

using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests.Imaging
{
    [TestFixture]
    public class ColorExtensionsTest
    {
        #region Methods

        [Test]
        public void PremultipliedConversionTest()
        {
            Color32 s32 = new Color32(128, 255, 255, 255);

            // ====== 32 vs. 32 =====
            // S32 -> P32 -> S32
            PColor32 p32 = s32.ToPremultiplied();
            Assert.AreEqual(s32, p32.ToStraight());

            // ====== From Straight 32 =====
            // S32 -> S64M -> S32
            Color64 s64 = new Color64(s32);
            Assert.AreEqual(s32, s64.ToColor32());

            // S32 -> (S64->) P64 -> (S64->) S32
            PColor64 p64m = s64.ToPremultiplied();
            Assert.AreEqual(s64, p64m.ToStraight());
            Assert.AreEqual(s32, p64m.ToStraight().ToColor32());

            // ====== From Premultiplied =====
            // P32 -> (S32->) S64 -> (S32->) P32
            Assert.AreEqual(s64, new Color64(p32.ToStraight()));
            Assert.AreEqual(p32, s64.ToColor32().ToPremultiplied());

            // P32 -> P64 -> P32
            Assert.AreEqual(p64m, new Color64(p32.ToColor32()));
            Assert.AreEqual(p32, p64m.ToColor32());
        }

        #endregion
    }
}
