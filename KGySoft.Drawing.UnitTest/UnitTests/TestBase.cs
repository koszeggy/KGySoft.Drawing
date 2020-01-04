﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TestBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
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
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using KGySoft.CoreLibraries;
using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    public abstract class TestBase
    {
        #region Constants

        private const bool saveToFile = true;

        #endregion

        #region Methods

        protected static void AssertPlatformDependent(Action code, params PlatformID[] platforms)
        {
            try
            {
                code.Invoke();
            }
            catch (Exception e)
            {
                if (Environment.OSVersion.Platform.In(platforms))
                    throw;
                Assert.Inconclusive($"Test failed on platform {Environment.OSVersion.Platform}: {e.Message}");
            }
        }

        protected static void SaveIcon(string iconName, Icon icon, [CallerMemberName]string testName = null)
        {
            if (!saveToFile || icon == null)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fileName = Path.Combine(dir, $"{testName}_{iconName}.{DateTime.Now:yyyyMMddHHmmssffff}.ico");
            using (var fs = File.Create(fileName))
                icon.SaveHighQuality(fs);
        }

        protected static void SaveImage(string imageName, Image image, [CallerMemberName]string testName = null)
        {
            if (!saveToFile)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            bool toGif = image.GetBitsPerPixel() <= 8;
            string fileName = Path.Combine(dir, $"{testName}_{imageName}.{DateTime.Now:yyyyMMddHHmmssffff}.{(toGif ? "gif" : "png")}");
            using (var fs = File.Create(fileName))
            {
                if (image is Metafile metafile)
                {
                    metafile.Save(fs);
                    return;
                }

                if (toGif)
                    image.SaveAsGif(fs);
                else if (image.PixelFormat == PixelFormat.Format16bppGrayScale || image.PixelFormat.ToBitsPerPixel() > 32)
                {
                    using var toSave = image.ConvertPixelFormat(PixelFormat.Format32bppArgb);
                    toSave.Save(fs, ImageFormat.Png);
                }
                else
                    image.Save(fs, ImageFormat.Png);
            }
        }

        #endregion
    }
}