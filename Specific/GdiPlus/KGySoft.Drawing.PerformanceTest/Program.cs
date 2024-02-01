#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Program.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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

using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

#endregion

namespace KGySoft.Drawing
{
    internal static class Program
    {
        #region Methods

        internal static void Main()
        {
            // This executes all tests. Can be useful for .NET 3.5, which is execute on .NET 4.x otherwise.
            // Filtering can be done by reflecting NUnit.Framework.Internal.Filters.TestNameFilter,
            // or just calling the method to debug directly
            var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
            runner.Load(typeof(Program).Assembly, new Dictionary<string, object>());
            ITestResult result = runner.Run(null, TestFilter.Empty);
            Console.WriteLine($"Passed: {result.PassCount}; Failed: {result.FailCount}; Skipped: {result.SkipCount}");
            Console.WriteLine($"Message: {result.Message}");
            Console.WriteLine($"Output: {result.Output}");
            if (result.AssertionResults.Count > 0)
            {
                for (int i = 0; i < result.AssertionResults.Count; i++)
                {
                    Console.WriteLine($"Assertion #{i}:");
                    Console.WriteLine(result.AssertionResults[i].Message);
                    Console.WriteLine(result.AssertionResults[i].StackTrace);
                }
            }
        }

        #endregion
    }
}
