#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PixelAccessPerformanceTest.cs
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KGySoft.Collections;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class PixelAccessPerformanceTest
    {
        #region Methods

        [Test]
        public void TypedVsByteBufferTest()
        {
            var size = new Size(10, 10);
            using var bd1 = BitmapDataFactory.CreateBitmapData(size, KnownPixelFormat.Format32bppArgb);
            using var bd2 = BitmapDataFactory.CreateBitmapData(new Array2D<byte>(size.Height, size.Width * bd1.PixelFormat.GetByteWidth(1)), size.Width, KnownPixelFormat.Format32bppArgb);

            new PerformanceTest<Color32> { Iterations = 10_000_000, TestTime = 5000, Repeat = 3 }
                .AddCase(() => bd2.GetColor32(0, 0), "byte[] backed")
                .AddCase(() => bd1.GetColor32(0, 0), "Typed")
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }
}