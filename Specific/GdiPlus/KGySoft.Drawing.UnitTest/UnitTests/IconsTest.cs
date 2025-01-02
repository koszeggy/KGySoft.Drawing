﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IconsTest.cs
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

using System.Drawing;

using KGySoft.Drawing.WinApi;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class IconsTest : TestBase
    {
        #region Properties

        private static object[][] SourceKnownIconsTest => new[]
        {
            new object[] { nameof(Icons.SystemInformation), Icons.SystemInformation },
            new object[] { nameof(Icons.SystemWarning), Icons.SystemWarning },
            new object[] { nameof(Icons.SystemError), Icons.SystemError },
            new object[] { nameof(Icons.SystemApplication), Icons.SystemApplication },
            new object[] { nameof(Icons.SystemShield), Icons.SystemShield },
            new object[] { nameof(Icons.Information), Icons.Information },
            new object[] { nameof(Icons.Warning), Icons.Warning },
            new object[] { nameof(Icons.Question), Icons.Question },
            new object[] { nameof(Icons.Error), Icons.Error },
            new object[] { nameof(Icons.Shield), Icons.Shield },
            new object[] { nameof(Icons.SecurityShield), Icons.SecurityShield },
            new object[] { nameof(Icons.SecuritySuccess), Icons.SecuritySuccess },
            new object[] { nameof(Icons.SecurityWarning), Icons.SecurityWarning },
            new object[] { nameof(Icons.SecurityQuestion), Icons.SecurityQuestion },
            new object[] { nameof(Icons.SecurityError), Icons.SecurityError },
            new object[] { nameof(Icons.Application), Icons.Application },
        };

        #endregion

        #region Methods

        [TestCaseSource(nameof(SourceKnownIconsTest))]
        public void KnownIconsTest(string iconName, Icon icon)
        {
            Assert.IsNotNull(icon);
            SaveIcon(iconName, icon);
        }

        [TestCase(StockIcon.Drive35)]
        public void StockIconsTest(StockIcon stockIcon)
        {
            var icon = Icons.GetStockIcon(stockIcon);
            Assert.IsTrue(!OSUtils.IsWindows || icon != null);
            SaveIcon(stockIcon.ToString(), icon);
        }

        [Test, Explicit]
        public void SaveSystemIconsInCurrentOS()
        {
            var icons = new[]
            {
                (nameof(Icons.SystemApplication), Icons.SystemApplication),
                (nameof(Icons.SystemError), Icons.SystemError),
                (nameof(Icons.SystemWarning), Icons.SystemWarning),
                (nameof(Icons.SystemInformation), Icons.SystemInformation),
                (nameof(Icons.SystemQuestion), Icons.SystemQuestion),
                (nameof(Icons.SystemShield), Icons.SystemShield),
            };
            foreach (var (name, icon) in icons)
            {
                foreach (Bitmap bitmap in icon.ExtractBitmaps())
                {
                    SaveImage($"{name}{bitmap.Width}W11", bitmap);
                }
            }
        }

        #endregion
    }
}
