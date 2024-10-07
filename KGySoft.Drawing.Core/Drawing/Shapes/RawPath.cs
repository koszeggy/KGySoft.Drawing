#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RawPath.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// The raw version of <see cref="Path"/> where everything is represented by simple points (straight line segments).
    /// </summary>
    internal sealed partial class RawPath
    {
        #region Nested Types

        #region Nested Enumerations
        
        [Flags]
        private enum RegionsCacheKey
        {
            None,
            NonZeroFillMode = 1,
            AntiAliasing = 1 << 1,
        }

        #endregion

        #region Nested Structs

        /// <summary>
        /// Needed because Pen has mutable properties. Could be replaced by ValueTuple if there was no DashPattern
        /// </summary>
        private readonly struct PenOptions : IEquatable<PenOptions>
        {
            #region Fields

            // TODO: LineStart/End, DashPattern
            internal readonly float Width;
            internal readonly float MiterLimit;
            internal readonly LineJoinStyle LineJoin;

            #endregion

            #region Constructors

            internal PenOptions(Pen pen)
            {
                Width = pen.Width;
                LineJoin = pen.LineJoin;
                MiterLimit = pen.MiterLimit;
            }

            #endregion

            #region Methods

            public bool Equals(PenOptions other) => Width.Equals(other.Width)
                && LineJoin == other.LineJoin
                && MiterLimit.Equals(other.MiterLimit);

            public override bool Equals(object? obj) => obj is PenOptions other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Width + MiterLimit * (1f / 256f), LineJoin);

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly List<RawFigure> figures;

        private Rectangle bounds;
        private int totalVertices;
        private int maxVertices;
        private IThreadSafeCacheAccessor<int, Region>? regionsCache;
        private IThreadSafeCacheAccessor<PenOptions, RawPath>? widePathsCache;

        #endregion

        #region Properties

        internal Rectangle Bounds => bounds;
        internal int TotalVertices => totalVertices;
        internal int MaxVertices => maxVertices;
        internal List<RawFigure> Figures => figures;
        internal bool IsEmpty => figures.Count == 0;

        #endregion

        #region Constructors

        internal RawPath(int capacity) => figures = new List<RawFigure>(capacity);

        #endregion

        #region Methods

        #region Static Methods

        private static void WidenClosedFigure(RawFigure figure, in PenOptions penOptions, RawPath widePath)
        {
            // Getting the open vertices so the first and last points are different but handling them as being in a ring
            IList<PointF> origPoints = figure.OpenVertices;
            Debug.Assert(origPoints.Count > 2);

            var widePoints = new List<PointF>(origPoints.Count << 1);
            int end = origPoints.Count - 1;

            // left outline
            WidenJoint(origPoints[end], origPoints[0], origPoints[1], penOptions, widePoints);
            for (int i = 1; i < end; i++)
                WidenJoint(origPoints[i - 1], origPoints[i], origPoints[i + 1], penOptions, widePoints);
            WidenJoint(origPoints[end - 1], origPoints[end], origPoints[0], penOptions, widePoints);
            widePath.AddRawFigure(widePoints, true);
            widePoints.Clear();

            // right outline
            WidenJoint(origPoints[0], origPoints[end], origPoints[end - 1], penOptions, widePoints);
            for (int i = end - 1; i > 0; i--)
                WidenJoint(origPoints[i + 1], origPoints[i], origPoints[i - 1], penOptions, widePoints);
            WidenJoint(origPoints[1], origPoints[0], origPoints[end], penOptions, widePoints);
            widePath.AddRawFigure(widePoints, true);
        }

        private static void WidenOpenFigure(RawFigure figure, in PenOptions penOptions, RawPath widePath)
        {
            throw new NotImplementedException();
        }

        // The original method was taken from ReactOS (MIT license): https://github.com/reactos/reactos/blob/764881a94b4129538d62fda2c99cfcd1ad518ce5/dll/win32/gdiplus/graphicspath.c#L1837
        // Main changes: converting to C#, more descriptive variable names, more styles (TODO), using vectors when possible
        private static void WidenJoint(PointF previousPoint, PointF currentPoint, PointF nextPoint, in PenOptions penOptions, List<PointF> widePoints)
        {
            switch (penOptions.LineJoin)
            {
                case LineJoinStyle.Miter:
                    float diffCurrentPrevX = currentPoint.X - previousPoint.X;
                    float diffCurrentPrevY = currentPoint.Y - previousPoint.Y;

                    // Checking if the current point is on the left side of the line segment
                    if (diffCurrentPrevX * (nextPoint.Y - previousPoint.Y) > diffCurrentPrevY * (nextPoint.X - previousPoint.X))
                    {
                        float distance = penOptions.Width / 2f;
                        float diffNextCurrentX = nextPoint.X - currentPoint.X;
                        float diffNextCurrentY = nextPoint.Y - currentPoint.Y;
                        float lenPrevCurrent = MathF.Sqrt(diffCurrentPrevX * diffCurrentPrevX + diffCurrentPrevY * diffCurrentPrevY);
                        float lenCurrentNext = MathF.Sqrt(diffNextCurrentX * diffNextCurrentX + diffNextCurrentY * diffNextCurrentY);

                        // The direction vectors for the previous-to-current and current-to-next segments
                        float dirPrevCurrentX = distance * diffCurrentPrevX / lenPrevCurrent;
                        float dirPrevCurrentY = distance * diffCurrentPrevY / lenPrevCurrent;
                        float dirCurrentNextX = distance * diffNextCurrentX / lenCurrentNext;
                        float dirCurrentNextY = distance * diffNextCurrentY / lenCurrentNext;

                        // The determinant for miter calculation and the actual miter offset.
                        float determinant = (dirPrevCurrentY * dirCurrentNextX - dirPrevCurrentX * dirCurrentNextY);
                        float miterOffsetX = (dirPrevCurrentX * dirCurrentNextX * (dirPrevCurrentX - dirCurrentNextX)
                                + dirPrevCurrentY * dirPrevCurrentY * dirCurrentNextX
                                - dirCurrentNextY * dirCurrentNextY * dirPrevCurrentX) / determinant;
                        float miterOffsetY = (dirPrevCurrentY * dirCurrentNextY * (dirPrevCurrentY - dirCurrentNextY)
                                + dirPrevCurrentX * dirPrevCurrentX * dirCurrentNextY
                                - dirCurrentNextX * dirCurrentNextX * dirPrevCurrentY) / determinant;

                        // Applying only if the miter offset is within the miter limit; otherwise, adding a Bevel join instead
                        if (miterOffsetX * miterOffsetX + miterOffsetY * miterOffsetY < penOptions.MiterLimit * penOptions.MiterLimit * distance * distance)
                        {
                            widePoints.Add(new PointF(currentPoint.X + miterOffsetX, currentPoint.Y + miterOffsetY));
                            return;
                        }
                    }

                    // fallback to Bevel style
                    goto default;

                default:
                    AddBevelPoint(currentPoint, previousPoint, penOptions, true, widePoints);
                    AddBevelPoint(currentPoint, nextPoint, penOptions, false, widePoints);
                    break;
            }
        }

        private static void AddBevelPoint(PointF endPoint, PointF nextPoint,
            in PenOptions penOptions, bool isRightSide, List<PointF> result)
        {
            float diffSegmentY = nextPoint.Y - endPoint.Y;
            float diffSegmentX = nextPoint.X - endPoint.X;
            float segmentLength = MathF.Sqrt(diffSegmentY * diffSegmentY + diffSegmentX * diffSegmentX);
            float distance = penOptions.Width / 2f;

            if (segmentLength == 0f)
            {
                result.Add(new PointF(endPoint.X, endPoint.Y));
                return;
            }

            float distBevelX, distBevelY;
            if (isRightSide)
            {
                distBevelX = -distance * diffSegmentY / segmentLength;
                distBevelY = distance * diffSegmentX / segmentLength;
            }
            else
            {
                distBevelX = distance * diffSegmentY / segmentLength;
                distBevelY = -distance * diffSegmentX / segmentLength;
            }

            result.Add(new PointF(endPoint.X + distBevelX, endPoint.Y + distBevelY));
        }

        #endregion

        #region Instance Methods

        #region Internal Methods

        internal void AddRawFigure(IList<PointF> points, bool isClosed)
        {
            if (points.Count == 0)
                return;
            var figure = new RawFigure(points, isClosed);
            bounds = IsEmpty ? figure.Bounds : Rectangle.Union(bounds, figure.Bounds);
            figures.Add(figure);
            totalVertices += figure.VertexCount;
            maxVertices = Math.Max(maxVertices, figure.VertexCount);
            regionsCache = null;
            widePathsCache = null;
        }

        internal Region GetCreateCachedRegion(DrawingOptions drawingOptions)
        {
            #region Local Methods

            static RegionsCacheKey GetHashKey(DrawingOptions options)
            {
                var result = RegionsCacheKey.None;
                if (options.FillMode == ShapeFillMode.NonZero)
                    result |= RegionsCacheKey.NonZeroFillMode;
                if (options.AntiAliasing)
                    result |= RegionsCacheKey.AntiAliasing;
                return result;
            }

            #endregion

            if (regionsCache == null)
            {
                var options = new LockFreeCacheOptions { InitialCapacity = 4, ThresholdCapacity = 4, HashingStrategy = HashingStrategy.And, MergeInterval = TimeSpan.FromMilliseconds(100) };
                Interlocked.CompareExchange(ref regionsCache, ThreadSafeCacheFactory.Create<int, Region>(CreateRegion, options), null);
            }

            return regionsCache[(int)GetHashKey(drawingOptions)];
        }

        internal RawPath GetCreateWidePath(Pen pen)
        {
            if (widePathsCache == null)
            {
                var options = new LockFreeCacheOptions { InitialCapacity = 2, ThresholdCapacity = 2, HashingStrategy = HashingStrategy.Modulo, MergeInterval = TimeSpan.FromMilliseconds(100) };
                Interlocked.CompareExchange(ref widePathsCache, ThreadSafeCacheFactory.Create<PenOptions, RawPath>(DoWidenPath, options), null);
            }

            return widePathsCache[new PenOptions(pen)];
        }

        internal RawPath WidenPath(Pen pen) => DoWidenPath(new PenOptions(pen));

        #endregion

        #region Private Methods

        private Region CreateRegion(int key) => new Region(bounds, ((RegionsCacheKey)key & RegionsCacheKey.AntiAliasing) != 0);

        private RawPath DoWidenPath(PenOptions penOptions)
        {
            var result = new RawPath(figures.Capacity);
            foreach (RawFigure figure in figures)
            {
                if (figure.VertexCount == 0)
                    continue;

                // TODO
                //if (key.Dash != null)
                //    WidenDashedFigure(result, figure);
                //else
                if (figure.IsClosed)
                    WidenClosedFigure(figure, penOptions, result);
                else
                    WidenOpenFigure(figure, penOptions, result);
            }

            return result;
        }

        #endregion

        #endregion

        #endregion
    }
}