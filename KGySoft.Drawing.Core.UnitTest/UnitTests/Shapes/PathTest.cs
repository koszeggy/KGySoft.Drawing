#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PathTest.cs
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

#region Used Namespaces

using System.Drawing;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;
using KGySoft.Threading;

using NUnit.Framework;

#endregion

#region Used Aliases

using Brush = KGySoft.Drawing.Shapes.Brush;

#endregion

#endregion

namespace KGySoft.Drawing.UnitTests.Shapes
{
    [TestFixture]
    public class PathTest : TestBase
    {
        #region Nested classes

        #region CustomContext class

        private sealed class CustomContext(int maxDegree) : IAsyncContext
        {
            #region Properties

            public int MaxDegreeOfParallelism => maxDegree;
            public bool IsCancellationRequested => false;
            public bool CanBeCanceled => false;
            public IAsyncProgress? Progress => (IAsyncProgress)null;
            public object State => null;

            #endregion

            #region Methods

            public void ThrowIfCancellationRequested()
            {
            }

            #endregion
        }

        #endregion

        #endregion

        #region Methods

        [Test]
        public void FillPathTest()
        {
            var options = new DrawingOptions
            {
                AntiAliasing = false,
                FillMode = ShapeFillMode.Alternate,
                AlphaBlending = false,
            };

            var path = new Path();

            // reference polygon from https://www.cs.rit.edu/~icss571/filling/example.html (appears with inverted Y coords)
            //path.AddLines(new(10, 10), new(10, 16), new(16, 20), new(28, 10), new(28, 16), new(22, 10));

            // star, small
            //path.AddLines(new(30, 20), new(26, 30), new(35, 24), new(25, 24), new(34, 30));

            // star, big
            //path.AddLines(new(300, 200), new(260, 300), new(350, 240), new(250, 240), new(340, 300));
            //path.CloseFigure(); // combine with the following to mix two closed figures - note: causes holes even with Alternate mode, but the same happens for GDI+, too

            // Multiple stars with all possible edge relations (to test EdgeInfo.ConfigureEdgeRelation)
            path.AddLines(new(300, 300), new(260, 200), new(350, 260), new(250, 260), new(340, 200));
            path.CloseFigure();
            path.AddLines(new(50, 50), new(90, 150), new(0, 90), new(100, 90), new(10, 150));
            path.CloseFigure();
            path.AddLines(new(300, 50), new(260, 150), new(350, 90), new(250, 90), new(340, 150));
            path.CloseFigure();
            path.AddLines(new(50, 300), new(90, 200), new(0, 260), new(100, 260), new(10, 200));

            using var bitmapData = BitmapDataFactory.CreateBitmapData(path.Bounds.Size + new Size(path.Bounds.Location) + new Size(path.Bounds.Location));

            var singleThreadContext = new CustomContext(1);
            //var twoThreadContext = new CustomContext(2);

            //new PerformanceTest { TestName = $"{path.Bounds.Size}" /*Iterations = 10_000*/ }
            //    .AddCase(() =>
            //    {
            //        //bitmapData.Clear(default);
            //        bitmapData.FillPath(singleThreadContext, path, new SolidBrush(Color.Blue), options);
            //    }, "SingleThread")
            //    .AddCase(() =>
            //    {
            //        //bitmapData.Clear(default);
            //        bitmapData.FillPath(twoThreadContext, path, new SolidBrush(Color.Blue), options);
            //    }, "TwoThread")
            //    .AddCase(() =>
            //    {
            //        //bitmapData.Clear(default);
            //        bitmapData.FillPath(AsyncHelper.DefaultContext, path, new SolidBrush(Color.Blue), options);
            //    }, "MultiThread")
            //    .DoTest()
            //    .DumpResults(Console.Out);

            bitmapData.Clear(Color.Cyan);
            bitmapData.FillPath(singleThreadContext, path, Brush.CreateSolid(Color.Blue), options);

            //AssertAreEqual(bitmapData, BitmapDataFactory.Load(File.OpenRead(@"D:\temp\Images\ref\ref.raw")));
        }

        #endregion
    }
}
