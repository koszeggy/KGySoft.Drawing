﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PerformanceTest.cs
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

#if NETFRAMEWORK
using System; 
#endif
#if NETCOREAPP
using System.IO; 
#endif
#if !(NETFRAMEWORK || NETCOREAPP)
using System.Runtime.InteropServices; 
#endif
using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    internal class PerformanceTest : KGySoft.Diagnostics.PerformanceTest
    {
        #region Properties

        #region Static Properties

        internal static string FrameworkVersion =>
#if NETFRAMEWORK
            $".NET Framework Runtime {typeof(object).Assembly.ImageRuntimeVersion}";
#elif NETCOREAPP
            $".NET Core {Path.GetFileName(Path.GetDirectoryName(typeof(object).Assembly.Location))}";
#else
            $"{RuntimeInformation.FrameworkDescription})";
#endif

        #endregion

        #region Instance Properties

        public new string TestName
        {
            get => base.TestName;
            set => base.TestName = $"{value} ({FrameworkVersion})";
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        internal static void CheckTestingFramework()
        {
#if NET35
            if (typeof(object).Assembly.GetName().Version != new Version(2, 0, 0, 0))
                Assert.Inconclusive($"mscorlib version does not match to .NET 3.5: {typeof(object).Assembly.GetName().Version}. Try to run the tests as a console application");
#elif NET40_OR_GREATER
            if (typeof(object).Assembly.GetName().Version != new Version(4, 0, 0, 0))
                Assert.Inconclusive($"mscorlib version does not match to .NET 4.x: {typeof(object).Assembly.GetName().Version}. Check executing framework version.");
#endif
        }

        #endregion

        #region Instance Methods

        protected override void OnInitialize()
        {
#if DEBUG
            Assert.Inconclusive("Run the performance test in Release Build");
#endif
            base.OnInitialize();
            CheckTestingFramework();
        }

        #endregion

        #endregion
    }

    internal class PerformanceTest<TResult> : KGySoft.Diagnostics.PerformanceTest<TResult>
    {
        #region Properties

        public new string TestName
        {
            get => base.TestName;
            set => base.TestName = $"{value} ({PerformanceTest.FrameworkVersion})";
        }

        #endregion

        #region Methods

        protected override void OnInitialize()
        {
#if DEBUG
            Assert.Inconclusive("Run the performance test in Release Build");
#endif
            base.OnInitialize();
            PerformanceTest.CheckTestingFramework();
        }

        #endregion
    }
}
