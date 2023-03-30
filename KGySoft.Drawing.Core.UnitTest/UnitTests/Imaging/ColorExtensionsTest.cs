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
            Color32 s32 = new Color32(128, 255, 128, 64);

            // S32 -> P32 -> S32
            PColor32 p32 = s32.ToPremultiplied();
            Assert.IsTrue(s32.TolerantEquals(p32.ToStraight(), 1, 0), $"{s32} vs. {p32.ToStraight()}");

            // S32 -> S64 -> S32
            Color64 s64 = new Color64(s32);
            Assert.AreEqual(s32, s64.ToColor32());

            // S32 -> (S64->) P64 -> (S64->) S32
            PColor64 p64 = s64.ToPremultiplied();
            Assert.IsTrue(s64.TolerantEquals(p64.ToStraight(), 1), $"{s64} vs. {p64.ToStraight()}");
            Assert.AreEqual(p64, s32.ToPColor64());
            Assert.AreEqual(s32, p64.ToStraight().ToColor32());
            Assert.AreEqual(s32, p64.ToColor32());

            // S32 -> SF -> S32
            ColorF sF = new ColorF(s32);
            Assert.AreEqual(s32, sF.ToColor32());

            // S32 -> (SF->) PF -> (SF->) S32
            PColorF pF = sF.ToPremultiplied();
            Assert.AreEqual(sF, pF.ToStraight());
            Assert.AreEqual(pF, s32.ToPColorF());
            Assert.AreEqual(s32, pF.ToStraight().ToColor32());
            Assert.AreEqual(s32, pF.ToColor32());

            // S64 -> (S32->) P32
            Assert.AreEqual(p32, s64.ToColor32().ToPremultiplied());
            Assert.AreEqual(p32, s64.ToPColor32());

            // P32 -> P64 -> P32
            Assert.AreEqual(p32, p64.ToPColor32());
            Assert.AreEqual(p32,  p32.ToPColor64().ToPColor32());

            // No gamma adjust
            ColorF sFSrgb = new ColorF(s32, false);
            Assert.AreEqual(s32, sFSrgb.ToColor32(false));

            PColorF pFSrgb = new PColorF(p32);
            Assert.AreEqual(p32, pFSrgb.ToPColor32(false));
        }

        #endregion
    }
}
