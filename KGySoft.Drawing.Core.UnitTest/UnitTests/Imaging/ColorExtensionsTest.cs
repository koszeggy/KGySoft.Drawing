#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensionsTest.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
            ColorF sFSrgb = s32.ToColorF(false);
            Assert.AreEqual(s32, sFSrgb.ToColor32(false));
            
            sFSrgb = p32.ToColorF(false);
            Assert.IsTrue(p32.TolerantEquals(sFSrgb.ToPColor32(false), 1), $"{p32} vs. {sFSrgb.ToPColor32(false)}");
            
            sFSrgb = s64.ToColorF(false);
            Assert.AreEqual(s64, sFSrgb.ToColor64(false));

            sFSrgb = p64.ToColorF(false);
            Assert.IsTrue(p64.TolerantEquals(sFSrgb.ToPColor64(false), 1), $"{p64} vs. {sFSrgb.ToPColor64(false)}");

            PColorF pFSrgb = p32.ToPColorF(false);
            Assert.AreEqual(p32, pFSrgb.ToPColor32(false));

            pFSrgb = s32.ToPColorF(false);
            Assert.IsTrue(s32.TolerantEquals(pFSrgb.ToColor32(false), 1, 0), $"{s32} vs. {pFSrgb.ToColor32(false)}");

            pFSrgb = p64.ToPColorF(false);
            Assert.AreEqual(p64, pFSrgb.ToPColor64(false));

            pFSrgb = s64.ToPColorF(false);
            Assert.AreEqual(s32, pFSrgb.ToColor64(false).ToColor32());
        }

        [Test]
        public void IsValidTest()
        {
            var pc32 = new PColor32(128, 255, 128, 64);
            Assert.IsFalse(pc32.IsValid);

            PColor32 valid = pc32.Clip();
            Assert.IsTrue(valid.IsValid);
            Assert.AreEqual(new PColor32(128, 128, 128, 64), valid);

            var pc64 = new PColor64(32768, 65535, 32768, 16384);
            Assert.IsFalse(pc64.IsValid);

            PColor64 valid64 = pc64.Clip();
            Assert.IsTrue(valid64.IsValid);
            Assert.AreEqual(new PColor64(32768, 32768, 32768, 16384), valid64);

            PColorF pF = new PColorF(0.5f, 1f, 0.5f, 0.25f);
            Assert.IsFalse(pF.IsValid);

            PColorF pValidF = pF.Clip();
            Assert.IsTrue(pValidF.IsValid);
            Assert.AreEqual(new PColorF(0.5f, 0.5f, 0.5f, 0.25f), pValidF);

            pF = new PColorF(0.5f, 1.1f, -0.5f, Single.NaN);
            Assert.IsFalse(pF.IsValid);

            pValidF = pF.Clip();
            Assert.IsTrue(pValidF.IsValid);
            Assert.AreEqual(new PColorF(0.5f, 0.5f, 0f, 0f), pValidF);

            var cF = new ColorF(0.5f, 1.1f, -0.5f, Single.NaN);
            Assert.IsFalse(pF.IsValid);

            ColorF validF = cF.Clip();
            Assert.IsTrue(validF.IsValid);
#if !(NETCOREAPP3_0 || NET5_0) // known issue: .NET Core 3.0 and .NET 5 Release build return max instead of min for NaN (which is still valid, btw.)
            Assert.AreEqual(new ColorF(0.5f, 1f, 0f, 0f), validF); 
#endif
        }

        [Test]
        public void TolerantEqualsTest()
        {
            Assert.IsTrue(new Color32(1, 2, 3, 4).TolerantEquals(new Color32(2, 3, 4, 5), 1, 1));
            Assert.IsFalse(new Color32(1, 2, 3, 4).TolerantEquals(new Color32(2, 3, 4, 5), 1, 2));
            Assert.IsTrue(new Color64(1, 2, 3, 4).TolerantEquals(new Color64(2, 3, 4, 5), 1, 1));
            Assert.IsFalse(new Color64(1, 2, 3, 4).TolerantEquals(new Color64(2, 3, 4, 5), 1, 2));
            Assert.IsTrue(new ColorF(0.1f, 0.2f, 0.3f, 0.4f).TolerantEquals(new ColorF(0.2f, 0.3f, 0.4f, 0.5f), 0.15f, 0.1f));
            Assert.IsFalse(new ColorF(0.1f, 0.2f, 0.3f, 0.4f).TolerantEquals(new ColorF(0.2f, 0.3f, 0.4f, 0.5f), 0.15f, 0.2f));

            Assert.IsTrue(new PColor32(4, 3, 2, 1).TolerantEquals(new PColor32(5, 4, 3, 2), 1, 4));
            Assert.IsFalse(new PColor32(4, 3, 2, 1).TolerantEquals(new PColor32(5, 4, 3, 2), 1, 5));
            Assert.IsTrue(new PColor64(4, 3, 2, 1).TolerantEquals(new PColor64(5, 4, 3, 2), 1, 4));
            Assert.IsFalse(new PColor64(4, 3, 2, 1).TolerantEquals(new PColor64(5, 4, 3, 2), 1, 5));
            Assert.IsTrue(new PColorF(0.4f, 0.3f, 0.2f, 0.1f).TolerantEquals(new PColorF(0.5f, 0.4f, 0.3f, 0.2f), 0.15f, 0.4f));
            Assert.IsFalse(new PColorF(0.4f, 0.3f, 0.2f, 0.1f).TolerantEquals(new PColorF(0.5f, 0.4f, 0.3f, 0.2f), 0.15f, 0.5f));
        }

        #endregion
    }
}
