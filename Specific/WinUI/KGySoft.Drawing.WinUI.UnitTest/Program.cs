#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Program.cs
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.WinUI.Controls;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

using WinRT;

#endregion

#region Suppressions

// ReSharper disable LocalizableElement - Unit test strings are not localized

#endregion

namespace KGySoft.Drawing.WinUI
{
    public class Program : Application
    {
        #region Nested Classes

        private class ConsoleTestReporter : ITestListener
        {
            #region Methods

            public void TestStarted(ITest test)
            {
                if (test.HasChildren)
                    return;

                console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"{test.Name}...");
                console.ForegroundColor = ConsoleColor.DarkGray;
            }

            public void TestFinished(ITestResult result)
            {
                if (result.HasChildren)
                    return;

                ResultState state = result.ResultState;
                TestStatus status = state.Status;
                if (status == TestStatus.Skipped && state.Site == FailureSite.Parent)
                    return;

                string? message = result.Message;
                ConsoleColor origColor = console.ForegroundColor;
                console.ForegroundColor = status switch
                {
                    TestStatus.Failed => ConsoleColor.Red,
                    TestStatus.Passed => ConsoleColor.Green,
                    TestStatus.Skipped => ConsoleColor.DarkCyan,
                    _ => ConsoleColor.Yellow
                };

                Console.WriteLine(status);
                if (!String.IsNullOrEmpty(message))
                    Console.WriteLine($"Message: {message}");

                console.ForegroundColor = origColor;
            }

            public void TestOutput(TestOutput output)
            {
            }

            public void SendMessage(TestMessage message)
            {
            }

            #endregion
        }

        #endregion

        #region Fields

        private static ConsoleRenderer console = null!; // Initialized in OnLaunched

        #endregion

        #region Properties

        #region Internal Properties

        internal static DispatcherQueue Dispatcher { get; private set; } = null!; // Initialized in OnLaunched
        internal static TextWriter ConsoleWriter { get; private set; } = null!; // Initialized in OnLaunched
        internal static XamlRoot XamlRoot => console.XamlRoot;

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

                console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("====================================");
                console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{child.Name}: {child.Message}");
                Console.WriteLine(child.StackTrace);
                if (!child.Output.IsNullOrEmpty())
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
            // (e.g. even creating a WriteableBitmap would throw a COMException with RPC_E_WRONG_THREAD otherwise).
            base.OnLaunched(args);

            console = new ConsoleRenderer();
            ConsoleWriter = console.Writer;
            var window = new Window { Content = console };
            window.Activate();

            Task.Factory.StartNew(() =>
            {
                // Filtering can be done by reflecting NUnit.Framework.Internal.Filters.TestNameFilter,
                // or just calling the method to debug directly
                console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(FrameworkVersion);

                TestFilter filter = TestFilter.Empty; // (TestFilter)Reflection.Reflector.CreateInstance(Reflection.Reflector.ResolveType("NUnit.Framework.Internal.Filters.TestNameFilter")!, "PathToGeometryTest");
                var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
                runner.Load(typeof(Program).Assembly, new Dictionary<string, object>());
                Console.WriteLine("Executing tests...");
                ConsoleWriter = Console.Out;
                ITestResult result = runner.Run(new ConsoleTestReporter(), filter);
                console.ForegroundColor = result.FailCount > 0 ? ConsoleColor.Red
                    : result.InconclusiveCount > 0 ? ConsoleColor.Yellow
                    : ConsoleColor.Green;

                Console.WriteLine($"Passed: {result.PassCount}; Failed: {result.FailCount}; Inconclusive: {result.InconclusiveCount}; Skipped: {result.SkipCount}");
                if (!String.IsNullOrEmpty(result.Message))
                    Console.WriteLine($"Message: {result.Message}");
                ProcessChildren(result.Children);
            }, TaskCreationOptions.LongRunning);
        }

        #endregion

        #endregion
    }
}
