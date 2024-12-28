#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GraphicsPathExtensionsTest.cs
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

using System.Drawing;
using System.Drawing.Drawing2D;

using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;

using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.UnitTests
{
    [TestFixture]
    public class GraphicsPathExtensionsTest : TestBase
    {
        #region Nested classes

        #region Builder class

        private sealed class Builder
        {
            #region Fields

            private readonly GraphicsPath path = new GraphicsPath();

            #endregion

            #region Properties

            internal GraphicsPath Path => path;

            #endregion

            #region Methods

            internal Builder CloseFigure()
            {
                path.CloseFigure();
                return this;
            }

            internal Builder AddLines(params Point[] points)
            {
                path.AddLines(points);
                return this;
            }

            internal Builder AddArc(RectangleF rect, float startAngle, float sweepAngle)
            {
                path.AddArc(rect, startAngle, sweepAngle);
                return this;
            }

            internal Builder AddEllipse(RectangleF rect)
            {
                path.AddEllipse(rect);
                return this;
            }

            internal Builder AddPolygon(params Point[] points)
            {
                path.AddPolygon(points);
                return this;
            }

            internal Builder AddRoundedRectangle(RectangleF rect, float radius)
            {
                path.AddRoundedRectangle(rect, radius);
                return this;
            }

            #endregion
        }

        #endregion

        #endregion

        #region Properties

        private static object[][] GraphicsPathToPathTestSource => new[]
        {
            new object[] { "Empty", new GraphicsPath() },
            new object[] { "Single point", new Builder().AddLines(new Point(0, 0)).Path },
            new object[] { "Single point closed", new Builder().AddLines(new Point(0, 0)).CloseFigure().Path },
            new object[] { "Single line", new Builder().AddLines(new(0, 0), new(10, 10)).Path },
            new object[] { "Single bezier arc", new Builder().AddArc(new Rectangle(0, 0, 100, 100), 90, 90).Path },
            new object[] { "Multi bezier arc", new Builder().AddArc(new Rectangle(0, 0, 100, 100), 90, 300).Path },
            new object[] { "Point-bezier", new Builder().AddLines(new Point(50, 50)).AddArc(new Rectangle(0, 0, 100, 100), 90, 90).Path },
            new object[] { "Bezier-point", new Builder().AddArc(new Rectangle(0, 0, 100, 100), 90, 90).AddLines(new Point(50, 50)).Path },
            new object[] { "Open-close", new Builder().AddLines(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100)).Path },
            new object[] { "Closed figures", new Builder().AddPolygon(new(50, 0), new(79, 90), new(2, 35), new(97, 35), new(21, 90)).AddEllipse(new Rectangle(0, 0, 100, 100)).AddRoundedRectangle(new Rectangle(0, 0, 100, 100), 10).Path },
        };

        #endregion

        #region Methods

        [TestCaseSource(nameof(GraphicsPathToPathTestSource))]
        public void GraphicsPathToPathTest(string name, GraphicsPath graphicsPath)
        {
            using var bmpRef = new Bitmap(103, 103);
            using (var g = Graphics.FromImage(bmpRef))
            {
                g.Clear(Color.Cyan);
                g.TranslateTransform(1, 1);
                g.DrawPath(Pens.Blue, graphicsPath);
            }

            SaveImage($"{name}_orig", bmpRef);

            Path path = graphicsPath.ToPath();
            using var bmp = new Bitmap(103, 103);
            using var bmpData = bmp.GetReadWriteBitmapData();
            bmpData.Clear(Color.Cyan);
            bmpData.DrawPath(Color.Blue, path, new DrawingOptions { Transformation = TransformationMatrix.CreateTranslation(1, 1) });

            SaveImage($"{name}_converted", bmp);

            // The equality is not pixel perfect so it should be compared visually
            //using var refData = bmpRef.GetReadableBitmapData();
            //AssertAreEqual(refData, bmpData);

            graphicsPath.Dispose();
        }

        #endregion
    }
}
