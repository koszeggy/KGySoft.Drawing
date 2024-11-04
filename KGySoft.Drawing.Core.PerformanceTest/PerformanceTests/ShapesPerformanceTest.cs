#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ShapesPerformanceTest.cs
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
using System.Drawing;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;
using KGySoft.Threading;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class ShapesPerformanceTest
    {
        #region Properties

        private static object?[][] FillPathTestSource =>
        [
            // string name, KnownPixelFormat pixelFormat, WorkingColorSpace colorSpace, Color backColor /*Empty: AlphaGradient*/, Color fillColor, DrawingOptions options
            ["32bppArgb_Alternate_NQ_Srgb_NA_NB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false }],
        ];


        #endregion

        #region Methods

        [TestCaseSource(nameof(FillPathTestSource))]
        public void FillPathCachingTest(string name, KnownPixelFormat pixelFormat, WorkingColorSpace colorSpace, DrawingOptions options)
        {
            var path = new Path(false)
                .AddPolygon(new(300, 300), new(260, 200), new(350, 260), new(250, 260), new(340, 200))
                .AddPolygon(new(50, 50), new(90, 150), new(0, 90), new(100, 90), new(10, 150))
                .AddPolygon(new(300, 50), new(260, 150), new(350, 90), new(250, 90), new(340, 150))
                .AddPolygon(new(50, 300), new(90, 200), new(0, 260), new(100, 260), new(10, 200));

            var pathCached = new Path(path) { PreferCaching = true };

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(path.Bounds.Size + new Size(path.Bounds.Location) + new Size(Math.Abs(path.Bounds.X), Math.Abs(path.Bounds.Y) /*path.Bounds.Location*/), pixelFormat, colorSpace);
            bitmapDataBackground.Clear(Color.Cyan, options.Ditherer);

            using var bitmapData = bitmapDataBackground.Clone();
            var brush = new SolidBrush(Color.Blue);
            new PerformanceTest { TestName = $"{name}_{path.Bounds.Size}", TestTime = 5000, /*Iterations = 10_000,*/ Repeat = 3 }
                .AddCase(() =>
                {
                    //bitmapDataBackground.CopyTo(bitmapData);
                    bitmapData.FillPath(null, brush, path, options);
                }, "No cache multi-thread")
                .AddCase(() =>
                {
                    //bitmapDataBackground.CopyTo(bitmapData);
                    bitmapData.FillPath(null, brush, pathCached, options);
                }, "Cache multi-thread")
                .AddCase(() =>
                {
                    //bitmapData.Clear(default);
                    bitmapData.FillPath(AsyncHelper.SingleThreadContext, brush, path, options);
                }, "No cache single-thread")
                .AddCase(() =>
                {
                    //bitmapData.Clear(default);
                    bitmapData.FillPath(AsyncHelper.SingleThreadContext, brush, pathCached, options);
                }, "Cache single-thread")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Explicit]
        [Test]
        public void DrawThinLinesPerfTest()
        {
            var path = new Path(false)
                .AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).CloseFigure()
                .AddEllipse(new RectangleF(0, 0, 100, 100))
                .AddRectangle(new RectangleF(0, 0, 100, 100));
            var bounds = path.RawPath.DrawOutlineBounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(size, KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Linear);
            bitmapDataBackground.Clear(Color32.White);

            using IReadWriteBitmapData texture = BitmapDataFactory.CreateBitmapData(2, 2);
            texture.SetPixel(0, 0, Color32.FromArgb(64, Color32.White));
            texture.SetPixel(1, 0, Color32.FromArgb(64, Color.Red));
            texture.SetPixel(0, 1, Color32.FromArgb(64, Color.Lime));
            texture.SetPixel(1, 1, Color32.FromArgb(64, Color.Blue));
            //var pen = new Pen(Color.Blue);
            var pen = new Pen(Brush.CreateTexture(texture));

            //DrawingOptions options1 = new DrawingOptions { TestBehavior = -1, AlphaBlending = false }; // Forcing caching in Region
            //using var bitmapData1 = bitmapDataBackground.Clone();
            //bitmapData1.DrawPath(context, path, pen, options1);
            //SaveBitmapData(null, bitmapData1);

            //DrawingOptions options2 = new DrawingOptions { TestBehavior = 0, AlphaBlending = false }; // Direct SetColor32
            //using var bitmapData2 = bitmapDataBackground.Clone();
            //bitmapData2.DrawPath(context, path, pen, options2);
            //AssertAreEqual(bitmapData1, bitmapData2);

            //var options3 = new DrawingOptions { TestBehavior = 1 }; // Delegate
            //using var bitmapData3 = bitmapDataBackground.Clone();
            //bitmapData3.DrawPath(context, path, pen, options3);
            //AssertAreEqual(bitmapData1, bitmapData3);

            //var options4 = new DrawingOptions { TestBehavior = 2 }; // FuncPtr
            //using var bitmapData4 = bitmapDataBackground.Clone();
            //bitmapData4.DrawPath(context, path, pen, options4);
            //AssertAreEqual(bitmapData1, bitmapData4);

            //var options5 = new DrawingOptions { TestBehavior = 3, AlphaBlending = false }; // Generic accessor
            //using var bitmapData5 = bitmapDataBackground.Clone();
            //bitmapData5.DrawPath(context, path, pen, options5);
            //AssertAreEqual(bitmapData1, bitmapData5);

            //using var bitmapData1 = bitmapDataBackground.Clone();
            //bitmapData1.DrawPath(context, path, pen); 
            //SaveBitmapData(null, bitmapData1);

            new PerformanceTest { Repeat = 3, TestTime = 5000, TestName = $"Draw thin path - Size: {size}; Vertices: {path.RawPath.TotalVertices}" }
                //.AddCase(() => bitmapData1.DrawPath(context, path, pen, options1), "Cached region")
                //.AddCase(() => bitmapData2.DrawPath(context, path, pen, options2), "Direct SetColor32")
                //.AddCase(() => bitmapData3.DrawPath(context, path, pen, options3), "Delegate")
                //.AddCase(() => bitmapData4.DrawPath(context, path, pen, options4), "Function pointer")
                //.AddCase(() => bitmapData5.DrawPath(context, path, pen, options5), "Generic accessor")
                .DoTest()
                .DumpResults(Console.Out);
        }

        [Test]
        public void DrawLineShortcutTest()
        {
            var p1 = new Point(1, 1);
            var p2 = new Point(10, 1);

            PointF p1f = p1;
            PointF p2f = p2;

            var path = new Path()
                .AddLine(p1, p2);

            Color32 color = Color.Blue;
            var pen = new Pen(color, 1f);

            using var bitmapData = BitmapDataFactory.CreateBitmapData(13, 3, KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb);
            bitmapData.Clear(Color.Cyan);

            new PerformanceTest { TestName = nameof(DrawLineShortcutTest), TestTime = 2000, Repeat = 3 }
                .AddCase(() => bitmapData.DrawPath(null, pen, path, null), "DrawPath")
                .AddCase(() => bitmapData.DrawLine(color, p1, p2), "DrawLine Color32, Point, DefaultContext")
                .AddCase(() => bitmapData.DrawLine(color, p1, p2, null, null), "DrawLine Color32, Point, ParallelConfig")
                .AddCase(() => bitmapData.DrawLine(null, color, p1, p2), "DrawLine Color32, Point, IAsyncContext")
                .AddCase(() => bitmapData.DrawLine(pen, p1, p2), "DrawLine Pen, Point")
                .AddCase(() => bitmapData.DrawLine(color, p1f, p2f), "DrawLine Color32, PointF")
                .AddCase(() => bitmapData.DrawLine(pen, p1f, p2f), "DrawLine Pen, PointF")
                .DoTest()
                .DumpResults(Console.Out);

            // ==[DrawLineShortcutTest (.NET Core 9.0.0-rc.2.24473.5) Results]================================================
            // Test Time: 2 000 ms
            // Warming up: Yes
            // Test cases: 7
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. DrawLine Color32, Point, DefaultContext: 132 037 546 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 44 012 515,33
            //   #1  43 853 049 iterations in 2 000,00 ms. Adjusted: 43 853 049,00	 <---- Worst
            //   #2  44 162 355 iterations in 2 000,00 ms. Adjusted: 44 162 355,00	 <---- Best
            //   #3  44 022 142 iterations in 2 000,00 ms. Adjusted: 44 022 142,00
            //   Worst-Best difference: 309 306,00 (0,71%)
            // 2. DrawLine Color32, Point, IAsyncContext: 128 103 198 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 42 701 065,28 (-1 311 450,05 / 97,02%)
            //   #1  43 066 323 iterations in 2 000,00 ms. Adjusted: 43 066 323,00	 <---- Best
            //   #2  42 107 380 iterations in 2 000,00 ms. Adjusted: 42 107 380,00	 <---- Worst
            //   #3  42 929 495 iterations in 2 000,00 ms. Adjusted: 42 929 492,85
            //   Worst-Best difference: 958 943,00 (2,28%)
            // 3. DrawLine Color32, Point, ParallelConfig: 101 907 614 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 33 969 200,70 (-10 043 314,63 / 77,18%)
            //   #1  33 906 594 iterations in 2 000,00 ms. Adjusted: 33 906 594,00	 <---- Worst
            //   #2  33 992 212 iterations in 2 000,00 ms. Adjusted: 33 992 212,00
            //   #3  34 008 808 iterations in 2 000,00 ms. Adjusted: 34 008 796,10	 <---- Best
            //   Worst-Best difference: 102 202,10 (0,30%)
            // 4. DrawLine Pen, Point: 98 803 932 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 32 934 644,00 (-11 077 871,33 / 74,83%)
            //   #1  32 845 705 iterations in 2 000,00 ms. Adjusted: 32 845 705,00	 <---- Worst
            //   #2  32 912 302 iterations in 2 000,00 ms. Adjusted: 32 912 302,00
            //   #3  33 045 925 iterations in 2 000,00 ms. Adjusted: 33 045 925,00	 <---- Best
            //   Worst-Best difference: 200 220,00 (0,61%)
            // 5. DrawLine Color32, PointF: 78 625 814 iterations in 6 000,08 ms. Adjusted for 2 000 ms: 26 208 263,17 (-17 804 252,16 / 59,55%)
            //   #1  26 135 630 iterations in 2 000,08 ms. Adjusted: 26 134 605,52	 <---- Worst
            //   #2  26 188 171 iterations in 2 000,00 ms. Adjusted: 26 188 171,00
            //   #3  26 302 013 iterations in 2 000,00 ms. Adjusted: 26 302 013,00	 <---- Best
            //   Worst-Best difference: 167 407,48 (0,64%)
            // 6. DrawLine Pen, PointF: 77 624 539 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 25 874 846,33 (-18 137 669,00 / 58,79%)
            //   #1  25 622 425 iterations in 2 000,00 ms. Adjusted: 25 622 425,00	 <---- Worst
            //   #2  26 071 052 iterations in 2 000,00 ms. Adjusted: 26 071 052,00	 <---- Best
            //   #3  25 931 062 iterations in 2 000,00 ms. Adjusted: 25 931 062,00
            //   Worst-Best difference: 448 627,00 (1,75%)
            // 7. DrawPath: 55 511 871 iterations in 6 000,01 ms. Adjusted for 2 000 ms: 18 503 913,61 (-25 508 601,72 / 42,04%)
            //   #1  18 506 689 iterations in 2 000,00 ms. Adjusted: 18 506 685,30
            //   #2  18 544 956 iterations in 2 000,00 ms. Adjusted: 18 544 955,07	 <---- Best
            //   #3  18 460 226 iterations in 2 000,01 ms. Adjusted: 18 460 100,47	 <---- Worst
            //   Worst-Best difference: 84 854,60 (0,46%)
        }

        [Test, Explicit]
        public void TestBehaviorTest()
        {
            var p1 = new Point(1, 1);
            var p2 = new Point(10, 1);

            PointF p1f = p1;
            PointF p2f = p2;

            var path = new Path()
                .TransformTranslation(1, 1)
                .AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90))
                .AddEllipse(new RectangleF(0, 0, 100, 100))
                .AddRoundedRectangle(new RectangleF(0, 0, 100, 100), 10);

            Color32 color = Color.Blue;
            var pen = new Pen(color, 1f);

            using var bitmapData1 = BitmapDataFactory.CreateBitmapData(path.RawPath.DrawOutlineBounds.Size + path.Bounds.Location.AsSize() * 2);
            using var bitmapData2 = BitmapDataFactory.CreateBitmapData(path.RawPath.DrawOutlineBounds.Size + path.Bounds.Location.AsSize() * 2);

            bitmapData1.Clear(Color.Cyan);
            bitmapData2.Clear(Color.Cyan);

            DrawingOptions options1 = null;//new DrawingOptions { TestBehavior = 1 };
            DrawingOptions options2 = null;//new DrawingOptions { TestBehavior = 2 };

            bitmapData1.DrawPath(null, pen, path, options1);
            bitmapData2.DrawPath(null, pen, path, options2);

            ;

            new PerformanceTest { TestName = nameof(DrawLineShortcutTest), TestTime = 5000, Repeat = 3 }
                .AddCase(() => bitmapData1.DrawPath(null, pen, path, options1), "SolidDrawSession<IndexedAccessor, int>")
                .AddCase(() => bitmapData2.DrawPath(null, pen, path, options2), "SolidDrawSessionWithQuantizing")
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }
}