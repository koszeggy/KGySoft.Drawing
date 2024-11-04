#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ThinPathDrawer.cs
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
using System.Runtime.CompilerServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// This class is used to draw thin paths with solid colors. These drawing algorithms are actually duplicated in the classes
    /// derived from DrawThinPathSession in <see cref="Brush"/>. These are used in special cases, and are optimized for performance.
    /// </summary>
    internal static class ThinPathDrawer
    {
        #region Nested classes

        internal static class GenericDrawer<TAccessor, TColor, TArg>
            where TAccessor : struct, IBitmapDataAccessor<TColor, TArg>
            where TColor : unmanaged
        {
            #region Constants

            private const float roundingUnit = 1f / 32f;

            #endregion

            #region Methods

            #region Internal Methods 

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal static void DrawLine(IBitmapDataInternal bitmapData, PointF start, PointF end, TColor c, Rectangle bounds, bool doOffset, TArg arg = default!)
            {
                (Point p1, Point p2) = Round(start, end, doOffset);
                DrawLine(bitmapData, p1, p2, c, bounds, arg);
            }

            internal static void DrawLine(IBitmapDataInternal bitmapData, Point p1, Point p2, TColor c, Rectangle bounds, TArg arg = default!)
            {
                var accessor = new TAccessor();

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(bounds.Bottom))
                        return;

                    accessor.InitRow(bitmapData.GetRowCached(p1.Y), arg);
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, bounds.Right - 1);
                    for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                        accessor.SetColor(x, c);

                    return;
                }

                accessor.InitBitmapData(bitmapData, arg);

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(bounds.Right))
                        return;

                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, bounds.Bottom - 1);
                    for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                        accessor.SetColor(p1.X, y, c);

                    return;
                }

                // general line
                int width = (p2.X - p1.X).Abs();
                int height = (p2.Y - p1.Y).Abs();

                if (width > height)
                {
                    int numerator = width >> 1;
                    if (p1.X > p2.X)
                        (p1, p2) = (p2, p1);
                    int step = p2.Y > p1.Y ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible X coordinates
                    if (x < bounds.Left)
                    {
                        int diff = bounds.Left - x;
                        numerator = (int)((numerator + ((long)height * diff)) % width);
                        x = bounds.Left;
                        y += diff * step;
                    }

                    int endX = Math.Min(p2.X, bounds.Right - 1);
                    int offY = step > 0 ? Math.Min(p2.Y, bounds.Bottom - 1) + 1 : Math.Max(p2.Y, bounds.Top) - 1;
                    for (; x <= endX; x++)
                    {
                        // Drawing only if Y is visible
                        if ((uint)y < (uint)bounds.Bottom)
                            accessor.SetColor(x, y, c);
                        numerator += height;
                        if (numerator < width)
                            continue;

                        y += step;
                        if (y == offY)
                            return;
                        numerator -= width;
                    }
                }
                else
                {
                    int numerator = height >> 1;
                    if (p1.Y > p2.Y)
                        (p1, p2) = (p2, p1);
                    int step = p2.X > p1.X ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible Y coordinates
                    if (y < bounds.Top)
                    {
                        int diff = bounds.Top - y;
                        numerator = (int)((numerator + ((long)width * diff)) % height);
                        x += diff * step;
                        y = bounds.Top;
                    }

                    int endY = Math.Min(p2.Y, bounds.Bottom - 1);
                    int offX = step > 0 ? Math.Min(p2.X, bounds.Right - 1) + 1 : Math.Max(p2.X, bounds.Left) - 1;
                    for (; y <= endY; y++)
                    {
                        // Drawing only if X is visible
                        if ((uint)x < (uint)bounds.Right)
                            accessor.SetColor(x, y, c);
                        numerator += width;
                        if (numerator < height)
                            continue;

                        x += step;
                        if (x == offX)
                            return;
                        numerator -= height;
                    }
                }
            }

            #endregion

            #region Private Methods

            #region Static Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            private static (Point P1, Point P2) Round(PointF p1, PointF p2, bool doOffset)
            {
                float offset = doOffset ? 0.5f : 0f;
                return (new Point((int)(p1.X.RoundTo(roundingUnit) + offset), (int)(p1.Y.RoundTo(roundingUnit) + offset)),
                    (new Point((int)(p2.X.RoundTo(roundingUnit) + offset), (int)(p2.Y.RoundTo(roundingUnit) + offset))));
            }

            #endregion

            #endregion

            #endregion
        }

        #endregion

        #region Methods

        internal static void DrawLine(IReadWriteBitmapData bitmapData, Point p1, Point p2, Color32 color)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawLine(bitmap, p1, p2, color.ToPColorF(), new Rectangle(Point.Empty, bitmapData.Size));
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawLine(bitmap, p1, p2, color.ToColorF(), new Rectangle(Point.Empty, bitmapData.Size));
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawLine(bitmap, p1, p2, color.ToPColor64(), new Rectangle(Point.Empty, bitmapData.Size));
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawLine(bitmap, p1, p2, color.ToColor64(), new Rectangle(Point.Empty, bitmapData.Size));
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawLine(bitmap, p1, p2, color.ToPColor32(), new Rectangle(Point.Empty, bitmapData.Size));
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawLine(bitmap, p1, p2, bitmapData.Palette!.GetNearestColorIndex(color), new Rectangle(Point.Empty, bitmapData.Size));
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawLine(bitmap, p1, p2, color, new Rectangle(Point.Empty, bitmapData.Size));
        }

        internal static void DrawLine(IReadWriteBitmapData bitmapData, PointF p1, PointF p2, Color32 color, bool doOffset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawLine(bitmap, p1, p2, color.ToPColorF(), new Rectangle(Point.Empty, bitmapData.Size), doOffset);
                else
                    GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawLine(bitmap, p1, p2, color.ToColorF(), new Rectangle(Point.Empty, bitmapData.Size), doOffset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawLine(bitmap, p1, p2, color.ToPColor64(), new Rectangle(Point.Empty, bitmapData.Size), doOffset);
                else
                    GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawLine(bitmap, p1, p2, color.ToColor64(), new Rectangle(Point.Empty, bitmapData.Size), doOffset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawLine(bitmap, p1, p2, color.ToPColor32(), new Rectangle(Point.Empty, bitmapData.Size), doOffset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawLine(bitmap, p1, p2, bitmapData.Palette!.GetNearestColorIndex(color), new Rectangle(Point.Empty, bitmapData.Size), doOffset);
                return;
            }

            GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawLine(bitmap, p1, p2, color, new Rectangle(Point.Empty, bitmapData.Size), doOffset);
        }

        #endregion
    }
}
