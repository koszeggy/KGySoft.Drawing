#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ShapesPerformanceTest.cs
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

#region Used Namespaces

using System;
using System.Drawing;
using System.Runtime.InteropServices;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;
using KGySoft.Threading;

using NUnit.Framework;

#endregion

#region Used Aliases

#if NETFRAMEWORK
using Brush = KGySoft.Drawing.Shapes.Brush;
using Pen = KGySoft.Drawing.Shapes.Pen;
using SolidBrush = KGySoft.Drawing.Shapes.SolidBrush;
#endif

#endregion

#endregion

#nullable enable

namespace KGySoft.Drawing.PerformanceTests
{
    [TestFixture]
    public class ShapesPerformanceTest
    {
        #region Properties

        private static object?[][] FillPathTestSource =>
        [
            // string name, KnownPixelFormat pixelFormat, WorkingColorSpace colorSpace, DrawingOptions options
            ["32bppArgb_Alternate_NQ_Srgb_NA_NB", KnownPixelFormat.Format32bppArgb, WorkingColorSpace.Srgb, new DrawingOptions { FillMode = ShapeFillMode.Alternate, AlphaBlending = false, AntiAliasing = false }],
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
            var context2Threads = new SimpleContext(2);
            new PerformanceTest { TestName = $"{name}_{path.Bounds.Size}", TestTime = 5000, /*Iterations = 10_000,*/ Repeat = 3 }
                .AddCase(() => bitmapData.FillPath(null, brush, path, options), "No cache multi-thread")
                .AddCase(() => bitmapData.FillPath(null, brush, pathCached, options), "Cache multi-thread")
                .AddCase(() => bitmapData.FillPath(context2Threads, brush, path, options), "No cache 2 threads")
                .AddCase(() => bitmapData.FillPath(context2Threads, brush, pathCached, options), "Cache 2 threads")
                .AddCase(() => bitmapData.FillPath(AsyncHelper.SingleThreadContext, brush, path, options), "No cache single-thread")
                .AddCase(() => bitmapData.FillPath(AsyncHelper.SingleThreadContext, brush, pathCached, options), "Cache single-thread")
                .DoTest()
                .DumpResults(Console.Out);

            // ==[32bppArgb_Alternate_NQ_Srgb_NA_NB_{Width=350, Height=250} (.NET Core 10.0.0-rc.2.25502.107) Results]================================================
            // Test Time: 5 000 ms
            // Warming up: Yes
            // Test cases: 6
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. Cache multi-thread: 922 320 iterations in 15 000,01 ms. Adjusted for 5 000 ms: 307 439,84
            //   #1  310 375 iterations in 5 000,00 ms. Adjusted: 310 374,74	 <---- Best
            //   #2  308 526 iterations in 5 000,00 ms. Adjusted: 308 525,94
            //   #3  303 419 iterations in 5 000,00 ms. Adjusted: 303 418,82	 <---- Worst
            //   Worst-Best difference: 6 955,92 (2,29%)
            // 2. No cache multi-thread: 693 052 iterations in 15 000,06 ms. Adjusted for 5 000 ms: 231 016,46 (-76 423,38 / 75,14%)
            //   #1  231 755 iterations in 5 000,01 ms. Adjusted: 231 754,49
            //   #2  228 886 iterations in 5 000,03 ms. Adjusted: 228 884,64	 <---- Worst
            //   #3  232 411 iterations in 5 000,02 ms. Adjusted: 232 410,25	 <---- Best
            //   Worst-Best difference: 3 525,62 (1,54%)
            // 3. Cache 2 threads: 541 909 iterations in 15 000,06 ms. Adjusted for 5 000 ms: 180 635,59 (-126 804,25 / 58,75%)
            //   #1  184 450 iterations in 5 000,00 ms. Adjusted: 184 449,93	 <---- Best
            //   #2  178 143 iterations in 5 000,06 ms. Adjusted: 178 140,85	 <---- Worst
            //   #3  179 316 iterations in 5 000,00 ms. Adjusted: 179 315,98
            //   Worst-Best difference: 6 309,07 (3,54%)
            // 4. No cache 2 threads: 384 857 iterations in 15 000,07 ms. Adjusted for 5 000 ms: 128 285,10 (-179 154,73 / 41,73%)
            //   #1  130 075 iterations in 5 000,01 ms. Adjusted: 130 074,63
            //   #2  130 784 iterations in 5 000,02 ms. Adjusted: 130 783,54	 <---- Best
            //   #3  123 998 iterations in 5 000,03 ms. Adjusted: 123 997,14	 <---- Worst
            //   Worst-Best difference: 6 786,41 (5,47%)
            // 5. Cache single-thread: 293 177 iterations in 15 000,11 ms. Adjusted for 5 000 ms: 97 724,96 (-209 714,88 / 31,79%)
            //   #1  97 600 iterations in 5 000,02 ms. Adjusted: 97 599,61
            //   #2  97 518 iterations in 5 000,08 ms. Adjusted: 97 516,49	 <---- Worst
            //   #3  98 059 iterations in 5 000,01 ms. Adjusted: 98 058,78	 <---- Best
            //   Worst-Best difference: 542,28 (0,56%)
            // 6. No cache single-thread: 221 150 iterations in 15 000,22 ms. Adjusted for 5 000 ms: 73 715,57 (-233 724,27 / 23,98%)
            //   #1  73 584 iterations in 5 000,13 ms. Adjusted: 73 582,06	 <---- Worst
            //   #2  73 923 iterations in 5 000,06 ms. Adjusted: 73 922,09	 <---- Best
            //   #3  73 643 iterations in 5 000,03 ms. Adjusted: 73 642,55
            //   Worst-Best difference: 340,02 (0,46%)
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
        public void DrawIntoCustomBitmapDataPerfTest()
        {
            var path = new Path(false)
                .AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90))
                .AddEllipse(new RectangleF(0, 0, 100, 100))
                .AddRectangle(new RectangleF(0, 0, 100, 100));
            var bounds = path.RawPath.DrawOutlineBounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));

            using IReadWriteBitmapData texture = BitmapDataFactory.CreateBitmapData(2, 2);
            texture.SetPixel(0, 0, Color32.FromArgb(64, Color32.White));
            texture.SetPixel(1, 0, Color32.FromArgb(64, Color.Red));
            texture.SetPixel(0, 1, Color32.FromArgb(64, Color.Lime));
            texture.SetPixel(1, 1, Color32.FromArgb(64, Color.Blue));
            var pen = new Pen(Color.Blue);
            //var pen = new Pen(Brush.CreateTexture(texture));

            using var bitmapDataNative = BitmapDataFactory.CreateBitmapData(size);
            bitmapDataNative.DrawPath(pen, path);

            using var bitmapDataCustom = BitmapDataFactory.CreateBitmapData(new Color32[size.Width * size.Height], size, size.Width * 4, new PixelFormatInfo(KnownPixelFormat.Format32bppArgb),
                (row, x) => row[x], (row, x, c) => row[x] = c);
            bitmapDataCustom.DrawPath(pen, path);

            new PerformanceTest { Repeat = 3, TestTime = 5000, TestName = $"Draw thin path - Size: {size}; Vertices: {path.RawPath.TotalVertices}" }
                .AddCase(() => bitmapDataNative.DrawPath(null, pen, path), "Native")
                .AddCase(() => bitmapDataCustom.DrawPath(null, pen, path), "Custom")
                .DoTest()
                .DumpResults(Console.Out);

            // Before custom drawing optimization:
            // ==[Draw thin path - Size: {Width=101, Height=101}; Vertices: 73 (.NET Core 9.0.0) Results]================================================
            // Test Time: 5 000 ms
            // Warming up: Yes
            // Test cases: 2
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. Native: 5 185 637 iterations in 15 000,06 ms. Adjusted for 5 000 ms: 1 728 539,01
            //   #1  1 650 747 iterations in 5 000,00 ms. Adjusted: 1 650 746,14	 <---- Worst
            //   #2  1 779 226 iterations in 5 000,05 ms. Adjusted: 1 779 207,07	 <---- Best
            //   #3  1 755 664 iterations in 5 000,00 ms. Adjusted: 1 755 663,82
            //   Worst-Best difference: 128 460,93 (7,78%)
            // 2. Custom: 747 659 iterations in 15 000,03 ms. Adjusted for 5 000 ms: 249 219,23 (-1 479 319,78 / 14,42%)
            //   #1  253 095 iterations in 5 000,00 ms. Adjusted: 253 094,75	 <---- Best
            //   #2  245 772 iterations in 5 000,02 ms. Adjusted: 245 771,15	 <---- Worst
            //   #3  248 792 iterations in 5 000,00 ms. Adjusted: 248 791,79
            //   Worst-Best difference: 7 323,60 (2,98%)

            // After custom drawing optimization:
            // ==[Draw thin path - Size: {Width=101, Height=101}; Vertices: 73 (.NET Core 9.0.0) Results]================================================
            // Test Time: 5 000 ms
            // Warming up: Yes
            // Test cases: 2
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. Native: 5 219 015 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 1 739 671,14
            //   #1  1 712 635 iterations in 5 000,00 ms. Adjusted: 1 712 634,62	 <---- Worst
            //   #2  1 765 012 iterations in 5 000,00 ms. Adjusted: 1 765 011,44	 <---- Best
            //   #3  1 741 368 iterations in 5 000,00 ms. Adjusted: 1 741 367,37
            //   Worst-Best difference: 52 376,81 (3,06%)
            // 2. Custom: 1 951 245 iterations in 15 000,01 ms. Adjusted for 5 000 ms: 650 414,36 (-1 089 256,79 / 37,39%)
            //   #1  652 911 iterations in 5 000,01 ms. Adjusted: 652 910,28	 <---- Best
            //   #2  651 140 iterations in 5 000,01 ms. Adjusted: 651 139,10
            //   #3  647 194 iterations in 5 000,00 ms. Adjusted: 647 193,69	 <---- Worst
            //   Worst-Best difference: 5 716,59 (0,88%)
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

        [Test]
        public void DrawRectangleShortcutTest()
        {
            var rect = new Rectangle(1, 1, 10, 10);
            RectangleF rectF = rect;
            var pixelFormat = KnownPixelFormat.Format32bppArgb;

            var path = new Path()
                .AddRectangle(rectF);

            Color32 color = Color.Blue;
            var pen = new Pen(color);

            var bounds = path.Bounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));

            using var bitmapData = BitmapDataFactory.CreateBitmapData(size, pixelFormat);
            bitmapData.Clear(Color.Cyan);

            new PerformanceTest { TestName = nameof(DrawRectangleShortcutTest), TestTime = 2000, Repeat = 3 }
                .AddCase(() => bitmapData.DrawPath(pen, path, null), "DrawPath")
                .AddCase(() => bitmapData.DrawPath(null, pen, path, null), "DrawPath IAsyncContext")
                .AddCase(() => bitmapData.DrawRectangle(color, rect), "DrawRectangle Color32, Rectangle, DefaultContext")
                .AddCase(() => bitmapData.DrawRectangle(color, rect, null, null), "DrawRectangle Color32, Rectangle, ParallelConfig")
                .AddCase(() => bitmapData.DrawRectangle(null, color, rect), "DrawRectangle Color32, Rectangle, IAsyncContext")
                .AddCase(() => bitmapData.DrawRectangle(pen, rect), "DrawRectangle Pen, Rectangle")
                .AddCase(() => bitmapData.DrawRectangle(color, rectF), "DrawRectangle Color32, RectangleF")
                .AddCase(() => bitmapData.DrawRectangle(pen, rectF), "DrawRectangle Pen, RectangleF")
                .DoTest()
                .DumpResults(Console.Out);

            // ==[DrawRectangleShortcutTest (.NET Core 9.0.0-rc.2.24473.5) Results]================================================
            // Test Time: 2 000 ms
            // Warming up: Yes
            // Test cases: 8
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. DrawRectangle Color32, Rectangle, IAsyncContext: 38 314 532 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 12 771 510,24
            //   #1  12 730 277 iterations in 2 000,00 ms. Adjusted: 12 730 277,00	 <---- Worst
            //   #2  12 812 766 iterations in 2 000,00 ms. Adjusted: 12 812 765,36	 <---- Best
            //   #3  12 771 489 iterations in 2 000,00 ms. Adjusted: 12 771 488,36
            //   Worst-Best difference: 82 488,36 (0,65%)
            // 2. DrawRectangle Color32, Rectangle, DefaultContext: 38 232 719 iterations in 6 000,01 ms. Adjusted for 2 000 ms: 12 744 211,71 (-27 298,53 / 99,79%)
            //   #1  12 805 752 iterations in 2 000,01 ms. Adjusted: 12 805 669,40	 <---- Best
            //   #2  12 719 466 iterations in 2 000,00 ms. Adjusted: 12 719 465,36
            //   #3  12 707 501 iterations in 2 000,00 ms. Adjusted: 12 707 500,36	 <---- Worst
            //   Worst-Best difference: 98 169,04 (0,77%)
            // 3. DrawRectangle Color32, Rectangle, ParallelConfig: 35 459 038 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 11 819 678,55 (-951 831,70 / 92,55%)
            //   #1  11 833 134 iterations in 2 000,00 ms. Adjusted: 11 833 132,82
            //   #2  11 838 926 iterations in 2 000,00 ms. Adjusted: 11 838 925,41	 <---- Best
            //   #3  11 786 978 iterations in 2 000,00 ms. Adjusted: 11 786 977,41	 <---- Worst
            //   Worst-Best difference: 51 948,00 (0,44%)
            // 4. DrawRectangle Pen, Rectangle: 34 824 288 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 11 608 095,23 (-1 163 415,01 / 90,89%)
            //   #1  11 486 446 iterations in 2 000,00 ms. Adjusted: 11 486 445,43	 <---- Worst
            //   #2  11 585 116 iterations in 2 000,00 ms. Adjusted: 11 585 114,84
            //   #3  11 752 726 iterations in 2 000,00 ms. Adjusted: 11 752 725,41	 <---- Best
            //   Worst-Best difference: 266 279,99 (2,32%)
            // 5. DrawRectangle Color32, RectangleF: 29 448 677 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 9 816 225,34 (-2 955 284,90 / 76,86%)
            //   #1  9 761 810 iterations in 2 000,00 ms. Adjusted: 9 761 809,51	 <---- Worst
            //   #2  9 810 111 iterations in 2 000,00 ms. Adjusted: 9 810 110,51
            //   #3  9 876 756 iterations in 2 000,00 ms. Adjusted: 9 876 756,00	 <---- Best
            //   Worst-Best difference: 114 946,49 (1,18%)
            // 6. DrawRectangle Pen, RectangleF: 26 669 052 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 8 889 682,81 (-3 881 827,43 / 69,61%)
            //   #1  8 891 501 iterations in 2 000,00 ms. Adjusted: 8 891 500,56
            //   #2  8 875 616 iterations in 2 000,00 ms. Adjusted: 8 875 615,11	 <---- Worst
            //   #3  8 901 935 iterations in 2 000,00 ms. Adjusted: 8 901 932,77	 <---- Best
            //   Worst-Best difference: 26 317,66 (0,30%)
            // 7. DrawPath IAsyncContext: 18 478 297 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 6 159 431,82 (-6 612 078,42 / 48,23%)
            //   #1  6 185 476 iterations in 2 000,00 ms. Adjusted: 6 185 475,38	 <---- Best
            //   #2  6 184 433 iterations in 2 000,00 ms. Adjusted: 6 184 432,38
            //   #3  6 108 388 iterations in 2 000,00 ms. Adjusted: 6 108 387,69	 <---- Worst
            //   Worst-Best difference: 77 087,69 (1,26%)
            // 8. DrawPath: 16 536 922 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 5 512 306,91 (-7 259 203,33 / 43,16%)
            //   #1  4 856 357 iterations in 2 000,00 ms. Adjusted: 4 856 356,03	 <---- Worst
            //   #2  5 866 668 iterations in 2 000,00 ms. Adjusted: 5 866 667,71	 <---- Best
            //   #3  5 813 897 iterations in 2 000,00 ms. Adjusted: 5 813 897,00
            //   Worst-Best difference: 1 010 311,68 (20,80%)
        }

        [Test]
        public void DrawEllipseShortcutTest()
        {
            var rect = new Rectangle(1, 1, 10, 5);
            RectangleF rectF = rect;
            var pixelFormat = KnownPixelFormat.Format32bppArgb;

            var path = new Path()
                .AddEllipse(rectF);

            Color32 color = Color.Blue;
            var pen = new Pen(color);

            var bounds = path.Bounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));

            using var bitmapData = BitmapDataFactory.CreateBitmapData(size, pixelFormat);
            bitmapData.Clear(Color.Cyan);

            new PerformanceTest { TestName = nameof(DrawEllipseShortcutTest), TestTime = 2000, Repeat = 3 }
                .AddCase(() => bitmapData.DrawPath(pen, path, null), "DrawPath")
                .AddCase(() => bitmapData.DrawPath(null, pen, path, null), "DrawPath IAsyncContext")
                .AddCase(() => bitmapData.DrawEllipse(color, rect), "DrawEllipse Color32, Rectangle, DefaultContext")
                .AddCase(() => bitmapData.DrawEllipse(color, rect, null, null), "DrawEllipse Color32, Rectangle, ParallelConfig")
                .AddCase(() => bitmapData.DrawEllipse(null, color, rect), "DrawEllipse Color32, Rectangle, IAsyncContext")
                .AddCase(() => bitmapData.DrawEllipse(pen, rect), "DrawEllipse Pen, Rectangle")
                .AddCase(() => bitmapData.DrawEllipse(color, rectF), "DrawEllipse Color32, RectangleF")
                .AddCase(() => bitmapData.DrawEllipse(pen, rectF), "DrawEllipse Pen, RectangleF")
                .DoTest()
                .DumpResults(Console.Out);

             // ==[DrawEllipseShortcutTest (.NET Core 9.0.0-rc.2.24473.5) Results]================================================
             //   Test Time: 2 000 ms
             //   Warming up: Yes
             //   Test cases: 8
             //   Repeats: 3
             //   Calling GC.Collect: Yes
             //   Forced CPU Affinity: No
             //   Cases are sorted by fulfilled iterations (the most first)
             //   --------------------------------------------------
             //   1. DrawEllipse Color32, Rectangle, IAsyncContext: 77 913 014 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 25 971 004,67
             //     #1  26 058 791 iterations in 2 000,00 ms. Adjusted: 26 058 791,00	 <---- Best
             //     #2  26 023 982 iterations in 2 000,00 ms. Adjusted: 26 023 982,00
             //     #3  25 830 241 iterations in 2 000,00 ms. Adjusted: 25 830 241,00	 <---- Worst
             //     Worst-Best difference: 228 550,00 (0,88%)
             //   2. DrawEllipse Color32, Rectangle, DefaultContext: 77 162 276 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 25 720 758,24 (-250 246,43 / 99,04%)
             //     #1  25 650 374 iterations in 2 000,00 ms. Adjusted: 25 650 374,00
             //     #2  25 581 781 iterations in 2 000,00 ms. Adjusted: 25 581 779,72	 <---- Worst
             //     #3  25 930 121 iterations in 2 000,00 ms. Adjusted: 25 930 121,00	 <---- Best
             //     Worst-Best difference: 348 341,28 (1,36%)
             //   3. DrawEllipse Color32, RectangleF: 69 196 205 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 23 065 401,28 (-2 905 603,38 / 88,81%)
             //     #1  23 016 779 iterations in 2 000,00 ms. Adjusted: 23 016 777,85	 <---- Worst
             //     #2  23 059 062 iterations in 2 000,00 ms. Adjusted: 23 059 062,00
             //     #3  23 120 364 iterations in 2 000,00 ms. Adjusted: 23 120 364,00	 <---- Best
             //     Worst-Best difference: 103 586,15 (0,45%)
             //   4. DrawEllipse Color32, Rectangle, ParallelConfig: 65 499 201 iterations in 6 000,07 ms. Adjusted for 2 000 ms: 21 832 792,59 (-4 138 212,08 / 84,07%)
             //     #1  21 664 244 iterations in 2 000,00 ms. Adjusted: 21 664 244,00	 <---- Worst
             //     #2  21 881 239 iterations in 2 000,00 ms. Adjusted: 21 881 239,00
             //     #3  21 953 718 iterations in 2 000,08 ms. Adjusted: 21 952 894,77	 <---- Best
             //     Worst-Best difference: 288 650,77 (1,33%)
             //   5. DrawEllipse Pen, Rectangle: 63 987 491 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 21 329 163,67 (-4 641 841,00 / 82,13%)
             //     #1  21 378 263 iterations in 2 000,00 ms. Adjusted: 21 378 263,00	 <---- Best
             //     #2  21 257 819 iterations in 2 000,00 ms. Adjusted: 21 257 819,00	 <---- Worst
             //     #3  21 351 409 iterations in 2 000,00 ms. Adjusted: 21 351 409,00
             //     Worst-Best difference: 120 444,00 (0,57%)
             //   6. DrawEllipse Pen, RectangleF: 57 370 147 iterations in 6 000,19 ms. Adjusted for 2 000 ms: 19 122 774,68 (-6 848 229,98 / 73,63%)
             //     #1  18 886 191 iterations in 2 000,00 ms. Adjusted: 18 886 191,00	 <---- Worst
             //     #2  19 190 666 iterations in 2 000,19 ms. Adjusted: 19 188 844,02
             //     #3  19 293 290 iterations in 2 000,00 ms. Adjusted: 19 293 289,04	 <---- Best
             //     Worst-Best difference: 407 098,04 (2,16%)
             //   7. DrawPath IAsyncContext: 18 079 540 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 6 026 512,23 (-19 944 492,44 / 23,20%)
             //     #1  5 989 138 iterations in 2 000,00 ms. Adjusted: 5 989 135,90	 <---- Worst
             //     #2  6 037 763 iterations in 2 000,00 ms. Adjusted: 6 037 763,00
             //     #3  6 052 639 iterations in 2 000,00 ms. Adjusted: 6 052 637,79	 <---- Best
             //     Worst-Best difference: 63 501,89 (1,06%)
             //   8. DrawPath: 16 558 139 iterations in 6 000,04 ms. Adjusted for 2 000 ms: 5 519 346,00 (-20 451 658,67 / 21,25%)
             //     #1  5 488 626 iterations in 2 000,04 ms. Adjusted: 5 488 526,66	 <---- Worst
             //     #2  5 540 330 iterations in 2 000,00 ms. Adjusted: 5 540 329,45	 <---- Best
             //     #3  5 529 183 iterations in 2 000,00 ms. Adjusted: 5 529 181,89
             //     Worst-Best difference: 51 802,79 (0,94%)
        }

        [Test]
        public void FillRectangleShortcutTest()
        {
            var rect = new Rectangle(1, 1, 10, 10);
            RectangleF rectF = rect;
            var pixelFormat = KnownPixelFormat.Format32bppArgb;

            var pathNoCache = new Path(false)
                .AddRectangle(rectF);
            var pathCache = new Path(pathNoCache) { PreferCaching = true };

            Color32 color = Color.Blue;
            var brush = Brush.CreateSolid(color);

            var bounds = pathNoCache.Bounds;
            Size size = bounds.Size + new Size(bounds.Location) + new Size(Math.Abs(bounds.X), Math.Abs(bounds.Y));

            using var bitmapData = BitmapDataFactory.CreateBitmapData(size, pixelFormat);
            bitmapData.Clear(Color.Cyan);

            new PerformanceTest { TestName = nameof(FillRectangleShortcutTest), TestTime = 2000, Repeat = 3 }
                .AddCase(() => bitmapData.FillPath(brush, pathNoCache, null), "FillPath, no cache")
                .AddCase(() => bitmapData.FillPath(brush, pathCache, null), "FillPath, cache")
                .AddCase(() => bitmapData.FillPath(null, brush, pathNoCache, null), "FillPath IAsyncContext, no cache")
                .AddCase(() => bitmapData.FillPath(null, brush, pathCache, null), "FillPath IAsyncContext, cache")
                .AddCase(() => bitmapData.FillRectangle(color, rect), "FillRectangle Color32, Rectangle, DefaultContext")
                .AddCase(() => bitmapData.FillRectangle(color, rect, null, null), "FillRectangle Color32, Rectangle, ParallelConfig")
                .AddCase(() => bitmapData.FillRectangle(null, color, rect), "FillRectangle Color32, Rectangle, IAsyncContext")
                .AddCase(() => bitmapData.FillRectangle(brush, rect), "FillRectangle Brush, Rectangle")
                .AddCase(() => bitmapData.FillRectangle(color, rectF), "FillRectangle Color32, RectangleF")
                .AddCase(() => bitmapData.FillRectangle(brush, rectF), "FillRectangle Brush, RectangleF")
                .AddCase(() => bitmapData.Clip(rect).Clear(color), "Clip(Rectangle).Clear Color32")
                .DoTest()
                .DumpResults(Console.Out);

            // ==[FillRectangleShortcutTest (.NET Core 9.0.0-rc.2.24473.5) Results]================================================
            // Test Time: 2 000 ms
            // Warming up: Yes
            // Test cases: 11
            // Repeats: 3
            // Calling GC.Collect: Yes
            // Forced CPU Affinity: No
            // Cases are sorted by fulfilled iterations (the most first)
            // --------------------------------------------------
            // 1. FillRectangle Color32, Rectangle, DefaultContext: 23 827 608 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 7 942 535,34
            //   #1  7 975 557 iterations in 2 000,00 ms. Adjusted: 7 975 556,20	 <---- Best
            //   #2  7 937 310 iterations in 2 000,00 ms. Adjusted: 7 937 309,21
            //   #3  7 914 741 iterations in 2 000,00 ms. Adjusted: 7 914 740,60	 <---- Worst
            //   Worst-Best difference: 60 815,60 (0,77%)
            // 2. FillRectangle Color32, Rectangle, IAsyncContext: 23 704 142 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 7 901 380,14 (-41 155,20 / 99,48%)
            //   #1  7 898 774 iterations in 2 000,00 ms. Adjusted: 7 898 773,61
            //   #2  7 957 571 iterations in 2 000,00 ms. Adjusted: 7 957 570,60	 <---- Best
            //   #3  7 847 797 iterations in 2 000,00 ms. Adjusted: 7 847 796,22	 <---- Worst
            //   Worst-Best difference: 109 774,39 (1,40%)
            // 3. FillRectangle Color32, RectangleF: 22 344 304 iterations in 6 000,02 ms. Adjusted for 2 000 ms: 7 448 082,57 (-494 452,77 / 93,77%)
            //   #1  7 456 345 iterations in 2 000,01 ms. Adjusted: 7 456 289,82	 <---- Best
            //   #2  7 439 342 iterations in 2 000,00 ms. Adjusted: 7 439 341,26	 <---- Worst
            //   #3  7 448 617 iterations in 2 000,00 ms. Adjusted: 7 448 616,63
            //   Worst-Best difference: 16 948,57 (0,23%)
            // 4. FillRectangle Color32, Rectangle, ParallelConfig: 21 441 194 iterations in 6 000,05 ms. Adjusted for 2 000 ms: 7 147 003,26 (-795 532,08 / 89,98%)
            //   #1  7 168 551 iterations in 2 000,05 ms. Adjusted: 7 168 367,13	 <---- Best
            //   #2  7 164 666 iterations in 2 000,00 ms. Adjusted: 7 164 665,64
            //   #3  7 107 977 iterations in 2 000,00 ms. Adjusted: 7 107 977,00	 <---- Worst
            //   Worst-Best difference: 60 390,13 (0,85%)
            // 5. FillRectangle Brush, Rectangle: 20 975 821 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 6 991 939,40 (-950 595,94 / 88,03%)
            //   #1  7 018 750 iterations in 2 000,00 ms. Adjusted: 7 018 748,60	 <---- Best
            //   #2  6 949 962 iterations in 2 000,00 ms. Adjusted: 6 949 961,65	 <---- Worst
            //   #3  7 007 109 iterations in 2 000,00 ms. Adjusted: 7 007 107,95
            //   Worst-Best difference: 68 786,94 (0,99%)
            // 6. FillRectangle Brush, RectangleF: 19 025 249 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 6 341 748,60 (-1 600 786,73 / 79,85%)
            //   #1  6 405 195 iterations in 2 000,00 ms. Adjusted: 6 405 192,76	 <---- Best
            //   #2  6 340 604 iterations in 2 000,00 ms. Adjusted: 6 340 603,68
            //   #3  6 279 450 iterations in 2 000,00 ms. Adjusted: 6 279 449,37	 <---- Worst
            //   Worst-Best difference: 125 743,39 (2,00%)
            // 7. FillPath IAsyncContext, cache: 11 479 282 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 3 826 425,99 (-4 116 109,35 / 48,18%)
            //   #1  3 847 939 iterations in 2 000,00 ms. Adjusted: 3 847 937,46
            //   #2  3 773 115 iterations in 2 000,00 ms. Adjusted: 3 773 114,43	 <---- Worst
            //   #3  3 858 228 iterations in 2 000,00 ms. Adjusted: 3 858 226,07	 <---- Best
            //   Worst-Best difference: 85 111,64 (2,26%)
            // 8. FillPath, cache: 10 843 896 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 3 614 631,46 (-4 327 903,88 / 45,51%)
            //   #1  3 633 296 iterations in 2 000,00 ms. Adjusted: 3 633 295,27
            //   #2  3 566 364 iterations in 2 000,00 ms. Adjusted: 3 566 363,64	 <---- Worst
            //   #3  3 644 236 iterations in 2 000,00 ms. Adjusted: 3 644 235,45	 <---- Best
            //   Worst-Best difference: 77 871,81 (2,18%)
            // 9. Clip(Rectangle).Clear Color32: 5 918 889 iterations in 6 010,38 ms. Adjusted for 2 000 ms: 1 969 571,07 (-5 972 964,27 / 24,80%)
            //   #1  1 966 246 iterations in 2 005,47 ms. Adjusted: 1 960 883,87	 <---- Worst
            //   #2  1 986 397 iterations in 2 000,00 ms. Adjusted: 1 986 396,80	 <---- Best
            //   #3  1 966 246 iterations in 2 004,91 ms. Adjusted: 1 961 432,55
            //   Worst-Best difference: 25 512,94 (1,30%)
            // 10. FillPath IAsyncContext, no cache: 3 779 633 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 1 259 877,18 (-6 682 658,15 / 15,86%)
            //   #1  1 261 110 iterations in 2 000,00 ms. Adjusted: 1 261 109,37
            //   #2  1 263 575 iterations in 2 000,00 ms. Adjusted: 1 263 574,62	 <---- Best
            //   #3  1 254 948 iterations in 2 000,00 ms. Adjusted: 1 254 947,56	 <---- Worst
            //   Worst-Best difference: 8 627,06 (0,69%)
            // 11. FillPath, no cache: 3 511 102 iterations in 6 000,00 ms. Adjusted for 2 000 ms: 1 170 366,46 (-6 772 168,88 / 14,74%)
            //   #1  1 069 638 iterations in 2 000,00 ms. Adjusted: 1 069 636,66	 <---- Worst
            //   #2  1 206 642 iterations in 2 000,00 ms. Adjusted: 1 206 641,58
            //   #3  1 234 822 iterations in 2 000,00 ms. Adjusted: 1 234 821,14	 <---- Best
            //   Worst-Best difference: 165 184,47 (15,44%)             
        }

        [TestCase(KnownPixelFormat.Format32bppArgb)]
        [TestCase(KnownPixelFormat.Format8bppIndexed)]
        public void FillRectangleVsClearTest(KnownPixelFormat pixelFormat)
        {
            #region Local Methods

            static void CheckPixels(IReadableBitmapData bitmapData, Color32 color)
            {
                Color32 expectedColor = new SolidBitmapData(new Size(1, 1), color)
                    .Clone(bitmapData.PixelFormat.AsKnownPixelFormatInternal, PredefinedColorsQuantizer.FromBitmapData(bitmapData))
                    .GetColor32(0, 0);
                var row = bitmapData.FirstRow;
                do
                {
                    for (int x = 0; x < row.Width; x++)
                        Assert.AreEqual(expectedColor, row[x]);
                } while (row.MoveNextRow());
            }

            static int GetAutoCoreCount(IBitmapData bitmapData)
            {
                int rowSize = bitmapData.RowSize;
                int pixelSize = bitmapData.PixelFormat.GetByteWidth(1);
                int pixelWidth = rowSize / pixelSize;
                int effectiveRowSize = pixelWidth * pixelSize;
                long byteLength = (long)effectiveRowSize * bitmapData.Height;
#if NETFRAMEWORK
                // On .NET Framework allowing only a single core below 2048^2 bytes (both aligned and unaligned). Parallelizing aligned bitmap is just slower below that threshold,
                // while for unaligned bitmaps allowing more cores may gain better results for too much cost. But above the limit we allow full parallelization.
                int autoCoreCount = byteLength < (2048 * 2048) ? 1 : Environment.ProcessorCount;
#elif NET9_0_OR_GREATER
                // On .NET 9+ we start to allow 2 cores above 512^2 bytes (unaligned) or 640^2 bytes (aligned), and 3+ cores only above 2048^2 bytes (both aligned and unaligned).
                // This means that on .NET Core we practically never allow more than 6 cores due to practical bitmap sizes.
                int autoCoreCount = rowSize == effectiveRowSize && byteLength < (640 * 640) ? 1 // aligned: a 1-shot clear is faster below around 640^2 bytes
                    : byteLength >= (1024 * 1024) ? Math.Min(Environment.ProcessorCount, (byteLength.Log2() >> 1) - 8) // 1024^2: 2; 2048^2: 3; 4096^2: 4; etc.
                    : byteLength >= (512 * 512) ? Math.Min(Environment.ProcessorCount, 2) // unaligned: 2 cores already above 512^2 bytes (the general formula above would allow 2 cores from 1024^2 bytes only)
                    : 1;
#elif NET6_0_OR_GREATER
                // On .NET 6/7/8 we start to allow 2 cores above 1024^2 bytes (unaligned) or 1280^2 bytes (aligned). The condition of 3+ cores is the same as in .NET 9+.
                int autoCoreCount = rowSize == effectiveRowSize && byteLength < (1280 * 1280) ? 1 // aligned: a 1-shot clear is faster below around 640^2 bytes
                    : Math.Min(Environment.ProcessorCount, (byteLength.Log2() >> 1) - 8); // 1024^2: 2; 2048^2: 3; 4096^2: 4; etc.
#else
                // .NET Core 2.x-.NET 5 and .NET Standard (note: .NET Standard is a complicated case as the actual runtime may vary)
                int autoCoreCount = rowSize == effectiveRowSize && byteLength < (800 * 800) ? 1 // aligned: a 1-shot clear is faster below around 800^2 bytes
                    : byteLength >= (1024 * 1024) ? Math.Min(Environment.ProcessorCount, (byteLength.Log2() >> 1) - 8) // 1024^2: 2; 2048^2: 3; 4096^2: 4; etc.
                    : byteLength >= (576 * 576) ? Math.Min(Environment.ProcessorCount, 2) : 1 ; // unaligned: 2 cores already above 576^2 bytes (the general formula above would allow 2 cores from 1024^2 bytes only)
#endif
                return autoCoreCount;
            }

            #endregion

            // The results in the comments below are for .NET 10.
            //Size size = new Size((100 << 2) / pixelFormat.GetByteWidth(1), (100 << 1)); // below threshold - .NET 10: A1: 100%, U1: 62%, U2: 47&, A2: 46%, 3: 29%, 4: 28%, 16: 10%
            Size size = new Size((128 << 2) / pixelFormat.GetByteWidth(1), (128 << 2)); // .NET 9/10 unaligned threshold - A1: 100%, A2: 99%, U2: 95%, U1: 77%, 3: 72%, 4: 65%, 16: 59%
            //Size size = new Size((144 << 2) / pixelFormat.GetByteWidth(1), (144 << 2)); // .NET Core 3/5 unaligned threshold - A1: 100%, 16: 96%, U2: 94%, 4: 85%, A2: 84%, U1: 70%
            //Size size = new Size((160 << 2) / pixelFormat.GetByteWidth(1), (160 << 2)); // .NET 9/10 aligned threshold - U2: 100%, A2: 99%, 3: 95%, A1: 92%, 4: 82%, 16: 72%, U1: 71%
            //Size size = new Size((200 << 2) / pixelFormat.GetByteWidth(1), (200 << 2)); // .NET Core 3/5 aligned threshold
            //Size size = new Size((256 << 2) / pixelFormat.GetByteWidth(1), (256 << 2)); // .NET 6/7/8: unaligned threshold; otherwise, below 3 cores threshold - A2: 100%, 3: 89%, 4: 89%, U2: 88%, 16: 81%, A1: 68%, U1: 56%
            //Size size = new Size((320 << 2) / pixelFormat.GetByteWidth(1), (320 << 2)); // .NET 6/7/8: aligned threshold
            //Size size = new Size((256 << 3) / pixelFormat.GetByteWidth(1), (256 << 3)); // 3 cores threshold - 3A: 100%, 3U: 94%, 4: 89%, 16: 83%, A2: 78%, U2: 65%, A1: 41%, U1: 35%
            //Size size = new Size((256 << 4) / pixelFormat.GetByteWidth(1), (256 << 4)); // 4 cores threshold - 4A: 100%, 4U: 94%, 16: 82%, 3: 77%, A2: 54%, U2: 52%, A1: 26%, U1: 25%
            //Size size = new Size((256 << 5) / pixelFormat.GetByteWidth(1), (256 << 5)); // 5 cores threshold - 5A: 100%, 16: 99%, 5U: 98%, 4: 90%, 3: 78%, U2: 64%, A2: 63%, A1: 41%, U1: 40%
            //Size size = new Size((256 << 6) / pixelFormat.GetByteWidth(1), (256 << 6)); // 6 cores threshold - 6: 100%, 16: 99%, 4: 93%, 3: 78%, U2: 63%, A2: 62%, A1: 42%, U1: 41%

            using var bitmapDataManaged = BitmapDataFactory.CreateBitmapData(size, pixelFormat);
            IntPtr bufAligned = Marshal.AllocHGlobal(size.Height * pixelFormat.GetByteWidth(size.Width));
            IntPtr bufUnaligned = Marshal.AllocHGlobal(size.Height * (pixelFormat.GetByteWidth(size.Width) + 1));
            using var bitmapDataAligned = BitmapDataFactory.CreateBitmapData(bufAligned, size, pixelFormat.GetByteWidth(size.Width), pixelFormat, disposeCallback: () => Marshal.FreeHGlobal(bufAligned));
            using var bitmapDataUnaligned = BitmapDataFactory.CreateBitmapData(bufUnaligned, size, pixelFormat.GetByteWidth(size.Width) + 1, pixelFormat, disposeCallback: () => Marshal.FreeHGlobal(bufUnaligned));
            //var context2Threads = new SimpleContext(2);
            //var context3Threads = new SimpleContext(3);
            //var context4Threads = new SimpleContext(4);
            //var contextMaxThreads = new SimpleContext(ParallelHelper.CoreCount);

            Console.WriteLine($"Core count: {ParallelHelper.CoreCount}");
            int autoCoreCountAligned = GetAutoCoreCount(bitmapDataAligned);
            int autoCoreCountUnaligned = GetAutoCoreCount(bitmapDataUnaligned);
            Console.WriteLine($"Auto core count aligned/unaligned: {autoCoreCountAligned}/{autoCoreCountUnaligned}");

            Color32 color = Color32.FromRgb(ThreadSafeRandom.Instance.Next());
            bitmapDataManaged.Clear(AsyncHelper.DefaultContext, color);
            CheckPixels(bitmapDataManaged, color);

            color = Color32.FromRgb(ThreadSafeRandom.Instance.Next());
            bitmapDataAligned.Clear(AsyncHelper.DefaultContext, color);
            CheckPixels(bitmapDataAligned, color);

            color = Color32.FromRgb(ThreadSafeRandom.Instance.Next());
            bitmapDataUnaligned.Clear(AsyncHelper.SingleThreadContext, color);
            CheckPixels(bitmapDataUnaligned, color);

            color = Color32.FromRgb(ThreadSafeRandom.Instance.Next());
            bitmapDataUnaligned.Clear(AsyncHelper.DefaultContext, color);
            CheckPixels(bitmapDataUnaligned, color);

            string prefix = pixelFormat.ToBitsPerPixel() <= 8 ? "extra padding" : "unaligned";

            color = Color.White;
            new PerformanceTest { TestName = $"{nameof(FillRectangleVsClearTest)} {pixelFormat} {size.Width}x{size.Height}", TestTime = 2000, Repeat = 3 }
                .AddCase(() => bitmapDataManaged.FillRectangle(AsyncHelper.DefaultContext, color, new Rectangle(Point.Empty, size)), "FillRectangle (managed)")
                .AddCase(() => bitmapDataAligned.FillRectangle(AsyncHelper.DefaultContext, color, new Rectangle(Point.Empty, size)), "FillRectangle (aligned)")
                .AddCase(() => bitmapDataUnaligned.FillRectangle(AsyncHelper.DefaultContext, color, new Rectangle(Point.Empty, size)), $"FillRectangle ({prefix})")
                .AddCase(() => bitmapDataManaged.Clear(AsyncHelper.DefaultContext, color), $"Clear (managed, auto ({autoCoreCountAligned}) threads)")
                .AddCase(() => bitmapDataAligned.Clear(AsyncHelper.DefaultContext, color), $"Clear (aligned, auto ({autoCoreCountAligned}) threads)")
                //.AddCase(() => bitmapDataAligned.Clear(AsyncHelper.SingleThreadContext, color), "Clear (aligned, single thread)")
                //.AddCase(() => bitmapDataAligned.Clear(context2Threads, color), "Clear (aligned, 2 threads)")
                .AddCase(() => bitmapDataUnaligned.Clear(AsyncHelper.DefaultContext, color), $"Clear ({prefix} auto ({autoCoreCountUnaligned}) threads)")
                //.AddCase(() => bitmapDataUnaligned.Clear(AsyncHelper.SingleThreadContext, color), $"Clear ({prefix}, single thread)")
                //.AddCase(() => bitmapDataUnaligned.Clear(context2Threads, color), $"Clear ({prefix} 2 threads)")
                //.AddCase(() => bitmapDataUnaligned.Clear(context3Threads, color), $"Clear ({prefix} 3 threads)")
                //.AddCase(() => bitmapDataUnaligned.Clear(context4Threads, color), $"Clear ({prefix} 4 threads)")
                //.AddCase(() => bitmapDataUnaligned.Clear(contextMaxThreads, color), $"Clear ({prefix} max ({ParallelHelper.CoreCount}) threads)")
                .DoTest()
                .DumpResults(Console.Out);
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

            Point location = path.Bounds.Location;
            using var bitmapData1 = BitmapDataFactory.CreateBitmapData(path.RawPath.DrawOutlineBounds.Size + new Size(location.X * 2, location.Y * 2));
            using var bitmapData2 = BitmapDataFactory.CreateBitmapData(path.RawPath.DrawOutlineBounds.Size + new Size(location.X * 2, location.Y * 2));

            bitmapData1.Clear(Color.Cyan);
            bitmapData2.Clear(Color.Cyan);

            DrawingOptions? options1 = null;//new DrawingOptions { TestBehavior = 1 };
            DrawingOptions? options2 = null;//new DrawingOptions { TestBehavior = 2 };

            bitmapData1.DrawPath(null, pen, path, options1);
            bitmapData2.DrawPath(null, pen, path, options2);

            new PerformanceTest { TestName = nameof(DrawLineShortcutTest), TestTime = 5000, Repeat = 3 }
                .AddCase(() => bitmapData1.DrawPath(null, pen, path, options1), "SolidDrawSession<IndexedAccessor, int>")
                .AddCase(() => bitmapData2.DrawPath(null, pen, path, options2), "SolidDrawSessionWithQuantizing")
                .DoTest()
                .DumpResults(Console.Out);
        }

        #endregion
    }
}