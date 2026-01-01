#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GlobalInitialization.cs
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
#if NETCOREAPP
using System.IO;
#endif

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing
{
    [SetUpFixture]
    public class GlobalInitialization
    {
        #region Methods

        [OneTimeSetUp]
        public void Initialize()
        {
            if (Program.ConsoleWriter != null)
                Console.SetOut(Program.ConsoleWriter);
#if NET35
            if (typeof(object).Assembly.GetName().Version != new Version(2, 0, 0, 0))
                Assert.Inconclusive($"mscorlib version does not match to .NET 3.5: {typeof(object).Assembly.GetName().Version}. Change the executing framework to .NET 2.0 or execute the tests as a console application.");
#elif NETFRAMEWORK
            if (typeof(object).Assembly.GetName().Version != new Version(4, 0, 0, 0))
                Assert.Inconclusive($"mscorlib version does not match to .NET 4.x: {typeof(object).Assembly.GetName().Version}. Change the executing framework to .NET 4.x");
#elif !NETCOREAPP
#error unknown .NET version
#endif

#if NET7_0_OR_GREATER && !WINDOWS
            Assert.Inconclusive("When targeting .NET 7 or later, executing the tests require Windows. For Unix systems target .NET 6 or earlier.");
#endif

#if !WINDOWS && (NET5_0 || NET6_0)
            // To make sure that System.Drawing types can be used also on Unix systems
            // NOTE: With the latest test runners under VS Code it doesn't work from here so execute the tests as an application instead.
            DrawingModule.Initialize();
#endif
        }

        #endregion
    }
}
