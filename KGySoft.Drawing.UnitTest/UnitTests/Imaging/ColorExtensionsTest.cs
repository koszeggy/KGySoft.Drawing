#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensionsTest.cs
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
            Color32 p32 = s32.ToPremultiplied();
            Assert.AreEqual(s32, p32.ToStraight());

            // ====== From Straight 32 =====
            // S32 -> S64M -> S32
            Color64 s64m = new Color64(s32);
            Assert.AreEqual(s32, s64m.ToColor32());

            // S32 -> S64N -> S32
            Color64 s64n = s32.ToColor64PlatformDependent();
            Assert.AreEqual(s32, s64n.ToColor32PlatformDependent());

            // S32 -> (S64M->) P64M -> (S64M->) S32
            Color64 p64m = s64m.ToPremultiplied();
            Assert.AreEqual(s64m, p64m.ToStraight());
            Assert.AreEqual(s32, p64m.ToStraight().ToColor32());

            // S32 -> P64N -> S32
            Color64 p64n = s32.ToPremultiplied64PlatformDependent();
            Assert.AreEqual(s32, p64n.ToStraight32PlatformDependent());

            // ====== From Premultiplied =====
            // P32 -> (S32->) S64M -> (S32->) P32
            Assert.AreEqual(s64m, new Color64(p32.ToStraight()));
            Assert.AreEqual(p32, s64m.ToColor32().ToPremultiplied());

            // P32 -> (S32->) S64N -> (S32->) P32
            Assert.AreEqual(s64n, p32.ToStraight().ToColor64PlatformDependent());
            Assert.AreEqual(p32, s64n.ToColor32PlatformDependent().ToPremultiplied());

            // P32 -> P64M -> P32
            Assert.AreEqual(p64m, new Color64(p32));
            Assert.AreEqual(p32, p64m.ToColor32());

            // P32 -> (S32->) P64N -> (S32->) P32
            // note: unfortunately p32.ToColor64PlatformDependent != p64n and p64n.ToColor32PlatformDependent != p32
            Assert.AreEqual(p64n, p32.ToStraight().ToPremultiplied64PlatformDependent());
            Assert.AreEqual(p32, p64n.ToStraight32PlatformDependent().ToPremultiplied());
        }

        #endregion
    }
}
