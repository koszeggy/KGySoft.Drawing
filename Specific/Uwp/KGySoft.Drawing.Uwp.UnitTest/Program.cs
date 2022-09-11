#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Program.cs
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using KGySoft.Drawing.Uwp.UnitTest.Controls;

using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;

#endregion

namespace KGySoft.Drawing.Uwp.UnitTest
{
    public class Program : Application
    {
        #region Properties

        #region Internal Properties

        internal static CoreDispatcher Dispatcher { get; private set; }
        internal static TextWriter ConsoleWriter { get; private set; }

        #endregion

        #region Private Properties

        private static string FrameworkVersion => $"UWP ({RuntimeInformation.FrameworkDescription})";

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        // ReSharper disable once ObjectCreationAsStatement
        static void Main() => Start(_ => new Program());

        private static void ProcessChildren(IEnumerable<ITestResult> children)
        {
            foreach (ITestResult child in children)
            {
                if (child.HasChildren)
                {
                    ProcessChildren(child.Children);
                    continue;
                }

                if (child.FailCount == 0)
                    continue;

                Console.WriteLine("====================================");
                Console.WriteLine($"{child.Name}: {child.Message}");
                Console.WriteLine(child.StackTrace);
                if (!String.IsNullOrEmpty(child.Output))
                    Console.WriteLine($"Output: {child.Output}");

                for (int i = 0; i < child.AssertionResults.Count; i++)
                    Console.WriteLine($"Assertion #{i}: {child.AssertionResults[i].Message}{Environment.NewLine}{child.AssertionResults[i].StackTrace}{Environment.NewLine}");
            }
        }

        #endregion

        #region Instance Methods

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // This executes all tests in a real UWP application using a mocked UWP console.
            // It is needed for test cases that require a regular dispatcher thread
            // (eg. even creating a WriteableBitmap would throw an Exception with RPC_E_WRONG_THREAD otherwise).
            base.OnLaunched(args);
            Dispatcher = Window.Current.Dispatcher;

            var console = new ConsoleRenderer();
            ConsoleWriter = console.Writer;
            Window.Current.Content = console;
            Window.Current.Activate();

            Task.Factory.StartNew(() =>
            {
                // Filtering can be done by reflecting NUnit.Framework.Internal.Filters.TestNameFilter,
                // or just calling the method to debug directly
                Console.WriteLine(FrameworkVersion);

                var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
                runner.Load(typeof(Program).Assembly, new Dictionary<string, object>());
                Console.WriteLine("Executing tests...");
                ITestResult result = runner.Run(null, TestFilter.Empty);
                console.ForegroundColor = result.FailCount > 0 ? ConsoleColor.Red
                    : result.SkipCount > 0 ? ConsoleColor.Yellow
                    : ConsoleColor.Green;
                Console.WriteLine($"Passed: {result.PassCount}; Failed: {result.FailCount}; Skipped: {result.SkipCount}");
                Console.WriteLine($"Message: {result.Message}");
                ProcessChildren(result.Children);
            }, TaskCreationOptions.LongRunning);
        }

        #endregion

        #endregion
    }
}
