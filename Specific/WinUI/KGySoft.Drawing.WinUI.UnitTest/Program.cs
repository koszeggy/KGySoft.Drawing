#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Program.cs
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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using KGySoft.Drawing.WinUI.Controls;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

using WinRT;

#endregion

namespace KGySoft.Drawing.WinUI
{
    public class Program : Application
    {
        #region Properties

        #region Internal Properties

        internal static DispatcherQueue Dispatcher { get; private set; }
        internal static TextWriter ConsoleWriter { get; private set; }

        #endregion

        #region Private Properties

        private static string FrameworkVersion => $"WinUI ({RuntimeInformation.FrameworkDescription})";

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        [STAThread]
        static void Main()
        {
            ComWrappersSupport.InitializeComWrappers();
            Start(_ =>
            {
                Dispatcher = DispatcherQueue.GetForCurrentThread();
                SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(Dispatcher));
                var __ = new Program();
            });
        }

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
            // This executes all tests in a real WinUI application using a mocked WinUI console.
            // It is needed for test cases that require a regular dispatcher thread
            // (eg. even creating a WriteableBitmap would throw a COMException with RPC_E_WRONG_THREAD otherwise).
            base.OnLaunched(args);

            var console = new ConsoleRenderer();
            ConsoleWriter = console.Writer;
            var window = new Window { Content = console };
            window.Activate();

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
                if (!String.IsNullOrEmpty(result.Message))
                    Console.WriteLine($"Message: {result.Message}");
                ProcessChildren(result.Children);
            }, TaskCreationOptions.LongRunning);
        }

        #endregion

        #endregion
    }
}
