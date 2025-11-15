#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TestBase.cs
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

using System;
using System.IO;
using System.Runtime.CompilerServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.ExceptionServices;
#endif
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.Wpf.UnitTests
{
    public abstract class TestBase
    {
        #region Nested Classes

        private sealed class AsyncTestState
        {
            #region Properties

            internal Action<ManualResetEvent> Callback { get; set; } = default!;
            internal ManualResetEvent WaitHandle { get; set; } = default!;
            internal Exception? Error { get; set; }
            internal Dispatcher? Dispatcher { get; set; }

            #endregion
        }

        #endregion

        #region Properties

        protected static bool SaveToFile => Program.SaveTestImages;
        protected static bool AddTimestamp => Program.AddFileTimestamps;

        #endregion

        #region Methods

        #region Protected Methods

        protected static void SaveBitmap(string? imageName, BitmapSource bitmap, [CallerMemberName] string testName = null!)
        {
            if (!SaveToFile)
                return;

            // for potentially async results with blocking or without a running dispatcher: forcing processing pending events with higher priority
            bitmap.Dispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            var stream = new MemoryStream();
            encoder.Save(stream);
            SaveStream(imageName, stream, "png", testName);
        }

        protected static void SaveStream(string? streamName, MemoryStream ms, string extension, [CallerMemberName] string testName = null!)
        {
            if (!SaveToFile)
                return;

            string dir = Path.Combine(Files.GetExecutingPath(), "TestResults");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string fileName = Path.Combine(dir, $"{testName}{(streamName == null ? null : $"_{streamName}")}{GetTimestamp()}.{extension}");
            using (var fs = File.Create(fileName))
                ms.WriteTo(fs);
        }

        protected static BitmapSource GetInfoIcon256() => GetBitmap(@"..\..\..\..\..\..\Help\Images\Information256.png");

        protected static BitmapSource GetBitmap(string fileName) => new BitmapImage(new Uri(Path.Combine(Files.GetExecutingPath(), fileName), UriKind.Absolute));

        protected static void AssertAreEqual(BitmapSource reference, BitmapSource check, bool allowDifferentPixelFormats = false)
        {
            if (!allowDifferentPixelFormats)
                Assert.AreEqual(reference.Format, check.Format);

            using var source = reference.GetReadableBitmapData();
            using var target = check.GetReadableBitmapData();
            Assert.AreEqual(source.Size, target.Size);

            IReadableBitmapDataRowMovable rowSrc = source.FirstRow;
            IReadableBitmapDataRowMovable rowDst = target.FirstRow;

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

        /// <summary>
        /// Executes <paramref name="test"/> on a dedicated STA thread that starts the dispatcher so
        /// the thread will neither exit nor be blocked until the test completes.
        /// Without this even a simple test containing await would be blocked if contains sync callbacks.
        /// It also provides a <see cref="SynchronizationContext"/> so async continuations can be posted back to the test thread,
        /// helping to avoid <see cref="InvalidOperationException"/> due to accessing thread-affine WPF objects from a non-UI thread.
        /// </summary>
        protected static void ExecuteTestWithDispatcher(Action<ManualResetEvent> test)
        {
            #region Local Methods

            // This will be executed on a new thread
            static void Execute(object? state)
            {
                var asyncState = (AsyncTestState)state!;
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

                // Does not always work when debugging, calling Dispatcher.InvokeShutdown() from the invoking thread is more reliable.
                // Assuring that the dispatcher (and thus this thread) exits when the test finishes
                ThreadPool.RegisterWaitForSingleObject(asyncState.WaitHandle, (_, _) => Dispatcher.CurrentDispatcher.InvokeShutdown(), null, Timeout.Infinite, true);
                try
                {
                    // Invoking the callback that will set the wait handle when finishes
                    asyncState.Callback.Invoke(asyncState.WaitHandle);
                }
                catch (Exception e)
                {
                    // In case of error we save the exception so it can be thrown by the test case
                    // and manually set the wait handle (assuming the callback did not set it due to the error)
                    asyncState.Error = e;
                    asyncState.WaitHandle.Set();
                    return;
                }

                // Starting the dispatcher that prevents the thread from exiting and processes callbacks
                asyncState.Dispatcher = Dispatcher.CurrentDispatcher;
                Dispatcher.Run();
            }

            #endregion

            var waitHandle = new ManualResetEvent(false);
            var state = new AsyncTestState
            {
                Callback = test,
                WaitHandle = waitHandle
            };

            var thread = new Thread(Execute);
            thread.SetApartmentState(ApartmentState.STA);

            thread.Start(state);
            waitHandle.WaitOne();
            state.Dispatcher?.InvokeShutdown();
            if (state.Error != null)
            {
#if NET35 || NET40
                throw state.Error;
#else
                ExceptionDispatchInfo.Capture(state.Error).Throw();
#endif
            }
        }

        #endregion

        #region Private Methods

        private static string GetTimestamp() => AddTimestamp ? $".{DateTime.Now:yyyyMMddHHmmssffff}" : String.Empty;

        #endregion

        #endregion
    }
}