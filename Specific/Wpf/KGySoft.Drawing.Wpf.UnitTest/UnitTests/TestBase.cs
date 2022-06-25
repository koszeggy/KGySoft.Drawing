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

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Wpf.UnitTests
{
    public abstract class TestBase
    {
        #region Properties

        protected static bool SaveToFile => true;

        #endregion

        #region Methods

        protected static void SaveStream(string streamName, MemoryStream ms, string extension = "gif", [CallerMemberName] string testName = null)
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

        #endregion
    }
}