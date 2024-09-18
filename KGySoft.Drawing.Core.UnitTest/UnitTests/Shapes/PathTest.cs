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

#nullable enable

#region Usings

using System;

using KGySoft.Diagnostics;
using KGySoft.Threading;

#region Used Namespaces

using System.Drawing;
using System.Drawing.Imaging;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;

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
        #region Properties

        private static object?[][] FillPathTestSource =>
        [
            //// string name, KnownPixelFormat pixelFormat, ShapeFillMode fillMode, WorkingColorSpace colorSpace, Color backColor /*Empty: AlphaGradient*/, Color fillColor, bool antiAliasing, bool alphaBlending, IQuantizer? quantizer, IDitherer? ditherer, bool singleThread
            ["32bppArgb_Alternate_Srgb_NQ_NA_NB", KnownPixelFormat.Format32bppArgb, ShapeFillMode.Alternate, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, false, false, null, null],
            //["32bppArgb_NonZero_Srgb_NQ_NA_NB", KnownPixelFormat.Format32bppArgb, ShapeFillMode.NonZero, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, false, false, null, null],
            ////["32bppArgb_Alternate_Srgb_NQ_NA_AB", KnownPixelFormat.Format32bppArgb, ShapeFillMode.Alternate, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, false, true, null, null],
            //["32bppArgb_Alternate_Srgb_NQ_AA_NB", KnownPixelFormat.Format32bppArgb, ShapeFillMode.Alternate, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, true, false, null, null],
            //["32bppArgb_Alternate_Srgb_NQ_AA_AB", KnownPixelFormat.Format32bppArgb, ShapeFillMode.Alternate, WorkingColorSpace.Srgb, Color.Cyan, Color.Blue, true, true, null, null],
            ////["32bppArgb_Alternate_Linear_NQ_NA_NB", KnownPixelFormat.Format32bppArgb, ShapeFillMode.Alternate, WorkingColorSpace.Linear, Color.Cyan, Color.Blue, false, false, null, null],
            ////["32bppArgb_Alternate_Linear_NQ_NA_AB", KnownPixelFormat.Format32bppArgb, ShapeFillMode.Alternate, WorkingColorSpace.Linear, Color.Cyan, Color.Blue, false, true, null, null],
            ////["32bppArgb_Alternate_Linear_NQ_AA_NB", KnownPixelFormat.Format32bppArgb, ShapeFillMode.Alternate, WorkingColorSpace.Linear, Color.Cyan, Color.Blue, true, false, null, null],
            //["32bppArgb_Alternate_Linear_NQ_AA_AB", KnownPixelFormat.Format32bppArgb, ShapeFillMode.Alternate, WorkingColorSpace.Linear, Color.Cyan, Color.Blue, true, true, null, null],
        ];

        #endregion

        #region Methods

        [TestCaseSource(nameof(FillPathTestSource))]
        public void FillPathTest(string name, KnownPixelFormat pixelFormat, ShapeFillMode fillMode, WorkingColorSpace colorSpace, Color backColor, Color fillColor, bool antiAliasing, bool alphaBlending, IQuantizer? quantizer, IDitherer? ditherer)
        {
            var options = new DrawingOptions
            {
                FillMode = fillMode,
                AntiAliasing = antiAliasing,
                AlphaBlending = alphaBlending,
                Quantizer = quantizer,
                Ditherer = ditherer,
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

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData(path.Bounds.Size + new Size(path.Bounds.Location) + new Size(path.Bounds.Location), pixelFormat, colorSpace);
            if (backColor != Color.Empty)
                bitmapDataBackground.Clear(backColor);
            else
                GenerateAlphaGradient(bitmapDataBackground);

            //var singleThreadContext = new SimpleContext(1);
            //var twoThreadContext = new CustomContext(2);

            //using var bitmapData = bitmapDataBackground.Clone();
            //new PerformanceTest { TestName = $"{path.Bounds.Size}" /*Iterations = 10_000*/ }
            //    .AddCase(() =>
            //    {
            //        //bitmapDataBackground.CopyTo(bitmapData);
            //        bitmapData.FillPath(null, path, Brush.CreateSolid(fillColor), options, false);
            //    }, "NoCache")
            //    .AddCase(() =>
            //    {
            //        //bitmapDataBackground.CopyTo(bitmapData);
            //        bitmapData.FillPath(null, path, Brush.CreateSolid(fillColor), options, true);
            //    }, "Cache")
            //    //.AddCase(() =>
            //    //{
            //    //    //bitmapData.Clear(default);
            //    //    bitmapData.FillPath(AsyncHelper.DefaultContext, path, new SolidBrush(Color.Blue), options);
            //    //}, "MultiThread")
            //    .DoTest()
            //    .DumpResults(Console.Out);

            // non-cached region
            var bitmapData1 = bitmapDataBackground.Clone();
            bitmapData1.FillPath(null, path, Brush.CreateSolid(fillColor), options, false);
            SaveBitmapData(name, bitmapData1);

            // generating cached region
            var bitmapData2 = bitmapDataBackground.Clone();
            bitmapData2.FillPath(null, path, Brush.CreateSolid(fillColor), options, true);
            AssertAreEqual(bitmapData1, bitmapData2);

            // re-using region from cache
            var bitmapData3 = bitmapDataBackground.Clone();
            bitmapData3.FillPath(null, path, Brush.CreateSolid(fillColor), options, true);
            AssertAreEqual(bitmapData1, bitmapData3);
        }

        [TestCaseSource(nameof(FillPathTestSource))]
        public void ClippedFillPathTest(string name, KnownPixelFormat pixelFormat, ShapeFillMode fillMode, WorkingColorSpace colorSpace, Color backColor, Color fillColor, bool antiAliasing, bool alphaBlending, IQuantizer? quantizer, IDitherer? ditherer)
        {
            var options = new DrawingOptions
            {
                FillMode = fillMode,
                AntiAliasing = antiAliasing,
                AlphaBlending = alphaBlending,
                Quantizer = quantizer,
                Ditherer = ditherer,
            };

            var offset = new SizeF(-10, -10);
            var path = new Path();
            path.AddLines(new PointF(50, 0) + offset, new PointF(90, 100) + offset, new PointF(0, 40) + offset, new PointF(100, 40) + offset, new PointF(10, 100) + offset);

            using var bitmapDataBackground = BitmapDataFactory.CreateBitmapData((path.Bounds.Size + offset * 2).ToSize(), pixelFormat, colorSpace);
            if (backColor != Color.Empty)
                bitmapDataBackground.Clear(backColor);
            else
                GenerateAlphaGradient(bitmapDataBackground);

            // non-cached region
            var bitmapData1 = bitmapDataBackground.Clone();
            bitmapData1.FillPath(null, path, Brush.CreateSolid(fillColor), options, false);
            SaveBitmapData(name, bitmapData1);

            // generating cached region
            var bitmapData2 = bitmapDataBackground.Clone();
            bitmapData2.FillPath(null, path, Brush.CreateSolid(fillColor), options, true);
            AssertAreEqual(bitmapData1, bitmapData2);

            // re-using region from cache
            var bitmapData3 = bitmapDataBackground.Clone();
            bitmapData3.FillPath(null, path, Brush.CreateSolid(fillColor), options, true);
            AssertAreEqual(bitmapData1, bitmapData3);
        }

        #endregion
    }
}
