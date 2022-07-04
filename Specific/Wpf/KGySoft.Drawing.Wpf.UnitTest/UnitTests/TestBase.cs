#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TestBase.cs
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

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.Wpf.UnitTests
{
    public abstract class TestBase
    {
        #region Properties

        protected static bool SaveToFile => true;

        #endregion

        #region Methods

        protected static void SaveBitmap(string? imageName, BitmapSource bitmap, [CallerMemberName]string testName = null!)
        {
            if (!SaveToFile)
                return;
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            var stream = new MemoryStream();
            encoder.Save(stream);
            SaveStream(imageName, stream, "png", testName);

        }

        protected static void SaveStream(string? streamName, MemoryStream ms, string extension, [CallerMemberName]string testName = null!)
        {
            if (!SaveToFile)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fileName = Path.Combine(dir, $"{testName}{(streamName == null ? null : $"_{streamName}")}.{DateTime.Now:yyyyMMddHHmmssffff}.{extension}");
            using (var fs = File.Create(fileName))
                ms.WriteTo(fs);
        }

        protected static BitmapSource GetInfoIcon256() => GetBitmap(@"..\..\..\..\..\..\Help\Images\Information256.png");

        protected static BitmapSource GetBitmap(string fileName) => new BitmapImage(new Uri(fileName, UriKind.Relative));

        protected static void AssertAreEqual(BitmapSource reference, BitmapSource check, bool allowDifferentPixelFormats = false)
        {
            if (!allowDifferentPixelFormats)
                Assert.AreEqual(reference.Format, check.Format);

            using var source = reference.GetReadableBitmapData();
            using var target = check.GetReadableBitmapData();
            Assert.AreEqual(source.Width, target.Width);
            Assert.AreEqual(source.Height, target.Height);

            IReadableBitmapDataRow rowSrc = source.FirstRow;
            IReadableBitmapDataRow rowDst = target.FirstRow;

            do
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color32 c1 = rowSrc[x];
                    Color32 c2 = rowDst[x];
                    if (!(c1.A == 0 && c2.A == 0) && c1.ToArgb() != c2.ToArgb())
                        Assert.Fail($"Diff at {x}; {rowSrc.Index}: {c1} vs. {c2}");
                    //Assert.AreEqual(rowSrc[x + sourceRectangle.X], rowDst[x + targetLocation.X], $"Diff at {x}; {rowSrc.Index}");
                }
            } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());
        }

        #endregion
    }
}