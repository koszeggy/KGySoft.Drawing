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
using System.Drawing;
using System.Threading.Tasks;

using Windows.UI.Core;

using KGySoft.Drawing.Imaging;
using NUnit.Framework;

#endregion

namespace KGySoft.Drawing.Uwp.UnitTest
{
    public abstract class TestBase
    {
        #region Constructors

        protected TestBase() => Console.SetOut(Program.ConsoleWriter);

        #endregion

        #region Methods

        protected async Task ExecuteTest(DispatchedHandler callback) => await Program.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, callback);
        
        protected async Task ExecuteTestAsync(Func<Task> callback)
        {
            var result = new TaskCompletionSource<bool>();

            // ReSharper disable once AsyncVoidLambda - not a problem, finished state is signed in result
            await Program.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    await callback.Invoke();
                    result.SetResult(default);
                }
                catch (Exception e)
                {
                    result.SetException(e);
                }
            });

            await result.Task;
        }

        protected static IReadWriteBitmapData GenerateAlphaGradientBitmapData(Size size)
        {
            var result = BitmapDataFactory.CreateBitmapData(size, KnownPixelFormat.Format32bppPArgb);
            GenerateAlphaGradient(result);
            return result;
        }

        protected static void GenerateAlphaGradient(IReadWriteBitmapData bitmapData)
        {
            static byte ClipToByte(float value) => value < Byte.MinValue ? Byte.MinValue
                : value > Byte.MaxValue ? Byte.MaxValue
                : (byte)value;

            var firstRow = bitmapData.FirstRow;
            float ratio = 255f / (bitmapData.Width / 6f);
            float limit = bitmapData.Width / 6f;

            for (int x = 0; x < bitmapData.Width; x++)
            {
                // red -> yellow
                if (x < limit)
                    firstRow[x] = new Color32(255, ClipToByte(x * ratio), 0);
                // yellow -> green
                else if (x < limit * 2)
                    firstRow[x] = new Color32(ClipToByte(255 - (x - limit) * ratio), 255, 0);
                // green -> cyan
                else if (x < limit * 3)
                    firstRow[x] = new Color32(0, 255, ClipToByte((x - limit * 2) * ratio));
                // cyan -> blue
                else if (x < limit * 4)
                    firstRow[x] = new Color32(0, ClipToByte(255 - (x - limit * 3) * ratio), 255);
                // blue -> magenta
                else if (x < limit * 5)
                    firstRow[x] = new Color32(ClipToByte((x - limit * 4) * ratio), 0, 255);
                // magenta -> red
                else
                    firstRow[x] = new Color32(255, 0, ClipToByte(255 - (x - limit * 5) * ratio));
            }

            if (bitmapData.Height < 2)
                return;

            var row = bitmapData.GetMovableRow(1);
            ratio = 255f / bitmapData.Height;
            do
            {
                byte a = ClipToByte(255 - row.Index * ratio);
                for (int x = 0; x < bitmapData.Width; x++)
                    row[x] = Color32.FromArgb(a, firstRow[x]);

            } while (row.MoveNextRow());
        }

        protected static void AssertAreEqual(IReadableBitmapData source, IReadableBitmapData target, bool allowDifferentPixelFormats = false, Rectangle sourceRectangle = default, Point targetLocation = default, int tolerance = 0)
        {
            if (sourceRectangle == default)
                sourceRectangle = new Rectangle(Point.Empty, source.Size);

            Assert.AreEqual(sourceRectangle.Size, target.Size);
            if (!allowDifferentPixelFormats)
                Assert.AreEqual(source.PixelFormat, target.PixelFormat);

            IReadableBitmapDataRowMovable rowSrc = source.GetMovableRow(sourceRectangle.Y);
            IReadableBitmapDataRowMovable rowDst = target.GetMovableRow(targetLocation.Y);

            do
            {
                if (tolerance > 0)
                {
                    for (int x = 0; x < sourceRectangle.Width; x++)
                    {
                        Color32 c1 = rowSrc[x + sourceRectangle.X];
                        Color32 c2 = rowDst[x + targetLocation.X];

                        // this is faster than the asserts below
                        if (!(c1.A == 0 && c2.A == 0) &&
                            (c1.A != c2.A
                            || Math.Abs(c1.R - c2.R) > tolerance
                            || Math.Abs(c1.G - c2.G) > tolerance
                            || Math.Abs(c1.B - c2.B) > tolerance))
                            Assert.Fail($"Diff at {x}; {rowSrc.Index}: {c1} vs. {c2}");

                        //Assert.AreEqual(c1.A, c2.A, $"Diff at {x}; {rowSrc.Index}");
                        //Assert.That(() => Math.Abs(c1.R - c2.R), new LessThanOrEqualConstraint(tolerance), $"Diff at {x}; {rowSrc.Index}");
                        //Assert.That(() => Math.Abs(c1.G - c2.G), new LessThanOrEqualConstraint(tolerance), $"Diff at {x}; {rowSrc.Index}");
                        //Assert.That(() => Math.Abs(c1.B - c2.B), new LessThanOrEqualConstraint(tolerance), $"Diff at {x}; {rowSrc.Index}");
                    }

                    continue;
                }

                for (int x = 0; x < sourceRectangle.Width; x++)
                {
                    Color32 c1 = rowSrc[x + sourceRectangle.X];
                    Color32 c2 = rowDst[x + targetLocation.X];
                    if (!(c1.A == 0 && c2.A == 0) && c1.ToArgb() != c2.ToArgb())
                        Assert.Fail($"Diff at {x}; {rowSrc.Index}: {c1} vs. {c2}");
                    //Assert.AreEqual(rowSrc[x + sourceRectangle.X], rowDst[x + targetLocation.X], $"Diff at {x}; {rowSrc.Index}");
                }
            } while (rowSrc.MoveNextRow() && rowDst.MoveNextRow());
        }

        #endregion
    }
}