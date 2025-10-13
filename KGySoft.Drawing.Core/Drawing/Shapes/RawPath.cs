#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RawPath.cs
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
using System.Collections.Generic;
using System.Drawing;
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif
using System.Runtime.CompilerServices;
using System.Threading;

using KGySoft.Collections;
using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// The raw version of <see cref="Path"/> where everything is represented by simple points (straight line segments).
    /// </summary>
    internal sealed class RawPath
    {
        #region Nested Types

        #region Nested Enumerations
        
        [Flags]
        private enum RegionsCacheKey
        {
            None,
            NonZeroFillMode = 1,
            AntiAliasing = 1 << 1,
            Outline = 1 << 2,
            Offset = 1 << 3
        }

        #endregion

        #region Nested Structs

        /// <summary>
        /// Needed because Pen has mutable properties.
        /// </summary>
        private readonly struct PenOptions : IEquatable<PenOptions>
        {
            #region Fields

            // TODO: DashPattern
            internal readonly float Width;
            internal readonly float MiterLimit;
            internal readonly LineJoinStyle LineJoin;
            internal readonly LineCapStyle StartCap;
            internal readonly LineCapStyle EndCap;
            internal readonly bool Offset;

            #endregion

            #region Constructors

            internal PenOptions(Pen pen, DrawingOptions options)
            {
                Width = pen.Width;
                Offset = options.DrawPathPixelOffset == PixelOffset.Half;
                LineJoin = pen.LineJoin;
                if (Width <= 1f)
                {
                    MiterLimit = 2f;
                    StartCap = EndCap = LineCapStyle.Flat;
                    return;
                }

                MiterLimit = pen.MiterLimit;
                StartCap = pen.StartCap;
                EndCap = pen.EndCap;
            }

            #endregion

            #region Methods

            public bool Equals(PenOptions other) => Width.Equals(other.Width)
                && LineJoin == other.LineJoin
                && MiterLimit.Equals(other.MiterLimit)
                && StartCap == other.StartCap
                && EndCap == other.EndCap
                && Offset == other.Offset;

            public override bool Equals(object? obj) => obj is PenOptions other && Equals(other);

            public override int GetHashCode() => (Width + MiterLimit * (1f / 256f), (int)LineJoin | ((int)StartCap << 4) | ((int)EndCap << 8) | (Offset ? (1 << 12) : 0)).GetHashCode();

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
        internal Rectangle DrawOutlineBounds => new Rectangle(bounds.Location, bounds.Size + new Size(1, 1));
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

        private static void WidenPoint(RawFigure figure, in PenOptions penOptions, RawPath widePath)
        {
            var point = figure.OpenVertices[0];
            float distance = penOptions.Width / 2f;

            // Round start/end cap: regular circle
            if (penOptions is { StartCap: LineCapStyle.Round, EndCap: LineCapStyle.Round })
            {
                widePath.AddRawFigure(BezierSegment.FromEllipse(point, distance, distance).GetFlattenedPointsInternal(), true, penOptions.Offset);
                return;
            }

            // A point has no direction so arbitrarily using a horizontal orientation when widening a single point.
            // Unlike in WidenOpenFigure we don't split the start cap into two sessions here. Also meaning, we start from bottom-left.
            var points = new List<PointF>(4);

            switch (penOptions.StartCap)
            {
                case LineCapStyle.Flat:
                    points.Add(new PointF(point.X - 0.5f, point.Y + distance));
                    points.Add(new PointF(point.X - 0.5f, point.Y - distance));
                    break;
                case LineCapStyle.Square:
                    points.Add(new PointF(point.X - distance, point.Y + distance));
                    points.Add(new PointF(point.X - distance, point.Y - distance));
                    break;
                case LineCapStyle.Triangle:
                    points.Add(new PointF(point.X, point.Y + distance));
                    points.Add(new PointF(point.X - distance, point.Y));
                    points.Add(new PointF(point.X, point.Y - distance));
                    break;
                case LineCapStyle.Round:
                    points.AddRange(BezierSegment.FromArc(point, distance, distance, 
                        MathF.PI / 2f, MathF.PI).GetFlattenedPointsInternal());
                    break;
                default:
                    throw new InvalidOperationException(Res.InternalError($"Unhandled cap style: {penOptions.StartCap}"));
            }

            switch (penOptions.EndCap)
            {
                case LineCapStyle.Flat:
                    points.Add(new PointF(point.X + 0.5f, point.Y - distance));
                    points.Add(new PointF(point.X + 0.5f, point.Y + distance));
                    break;
                case LineCapStyle.Square:
                    points.Add(new PointF(point.X + distance, point.Y - distance));
                    points.Add(new PointF(point.X + distance, point.Y + distance));
                    break;
                case LineCapStyle.Triangle:
                    points.Add(new PointF(point.X, point.Y - distance));
                    points.Add(new PointF(point.X + distance, point.Y));
                    points.Add(new PointF(point.X, point.Y + distance));
                    break;
                case LineCapStyle.Round:
                    points.AddRange(BezierSegment.FromArc(point, distance, distance,
                        -MathF.PI / 2f, MathF.PI).GetFlattenedPointsInternal());
                    break;
                default:
                    throw new InvalidOperationException(Res.InternalError($"Unhandled cap style: {penOptions.EndCap}"));

            }

            widePath.AddRawFigure(points, true, penOptions.Offset);
        }

        private static void WidenClosedFigure(RawFigure figure, in PenOptions penOptions, RawPath widePath)
        {
            // ClosedVertices contains the first point at the end again, which we ignore here.
            // Still, not using OpenVertices because that may be a wrapper, which is slower.
            IList<PointF> origPoints = figure.ClosedVertices;
            Debug.Assert(origPoints.Count > 3);

            var widePoints = new List<PointF>(origPoints.Count << 1);
            int end = origPoints.Count - 2;

            // left outline
            WidenJoint(origPoints[end], origPoints[0], origPoints[1], penOptions, widePoints);
            for (int i = 1; i < end; i++)
                WidenJoint(origPoints[i - 1], origPoints[i], origPoints[i + 1], penOptions, widePoints);
            WidenJoint(origPoints[end - 1], origPoints[end], origPoints[0], penOptions, widePoints);
            widePath.AddRawFigure(widePoints, true, penOptions.Offset);
            widePoints.Clear();

            // right outline
            WidenJoint(origPoints[0], origPoints[end], origPoints[end - 1], penOptions, widePoints);
            for (int i = end - 1; i > 0; i--)
                WidenJoint(origPoints[i + 1], origPoints[i], origPoints[i - 1], penOptions, widePoints);
            WidenJoint(origPoints[1], origPoints[0], origPoints[end], penOptions, widePoints);
            widePath.AddRawFigure(widePoints, true, penOptions.Offset);
        }

        private static void WidenOpenFigure(RawFigure figure, in PenOptions penOptions, RawPath widePath)
        {
            IList<PointF> origPoints = figure.OpenVertices;
            Debug.Assert(origPoints.Count > 1);

            var widePoints = new List<PointF>(origPoints.Count << 1);
            int end = origPoints.Count - 1;

            // start cap left side, left outline
            WidenCap(origPoints[0], origPoints[1], penOptions, penOptions.StartCap, false, true, widePoints);
            for (int i = 1; i < end; i++)
                WidenJoint(origPoints[i - 1], origPoints[i], origPoints[i + 1], penOptions, widePoints);

            // end cap
            WidenCap(origPoints[end], origPoints[end - 1], penOptions, penOptions.EndCap, true, true, widePoints);

            // right outline, start cap right side
            for (int i = end - 1; i > 0; i--)
                WidenJoint(origPoints[i + 1], origPoints[i], origPoints[i - 1], penOptions, widePoints);
            WidenCap(origPoints[0], origPoints[1], penOptions, penOptions.StartCap, true, false, widePoints);

            widePath.AddRawFigure(widePoints, true, penOptions.Offset);
        }

        // The original method was taken from ReactOS (MIT license): https://github.com/reactos/reactos/blob/764881a94b4129538d62fda2c99cfcd1ad518ce5/dll/win32/gdiplus/graphicspath.c#L1837
        // Main changes: converting to C#, more descriptive variable names, adding Round style, using vectors on platforms where available
        private static void WidenJoint(PointF previousPoint, PointF currentPoint, PointF nextPoint, in PenOptions penOptions, List<PointF> result)
        {
            float radius = penOptions.Width / 2f;

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            ref Vector2 previousVec = ref previousPoint.AsVector2();
            ref Vector2 currentVec = ref currentPoint.AsVector2();
            ref Vector2 nextVec = ref nextPoint.AsVector2();
#endif

            switch (penOptions.LineJoin)
            {
                case LineJoinStyle.Miter:
                case LineJoinStyle.Round:
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                    Vector2 diffCurrentPrev = currentVec - previousVec;

                    // Checking if the current point is on the left side of the line segment
                    if (diffCurrentPrev.X * (nextPoint.Y - previousPoint.Y) > diffCurrentPrev.Y * (nextPoint.X - previousPoint.X))
                    {
                        if (penOptions.LineJoin == LineJoinStyle.Miter)
                        {
                            Vector2 diffNextCurrent = nextVec - currentVec;
                            float lenPrevCurrent = diffCurrentPrev.Length();
                            float lenCurrentNext = diffNextCurrent.Length();

                            // The direction vectors for the previous-to-current and current-to-next segments
                            Vector2 dirPrevCurrent = (radius * diffCurrentPrev).Div(lenPrevCurrent);
                            Vector2 dirCurrentNext = (radius * diffNextCurrent).Div(lenCurrentNext);

                            // The determinant for miter calculation and the actual miter offset.
                            float determinant = (dirPrevCurrent.Y * dirCurrentNext.X - dirPrevCurrent.X * dirCurrentNext.Y);
                            Vector2 miterOffset = dirPrevCurrent * dirCurrentNext * (dirPrevCurrent - dirCurrentNext);
                            miterOffset += new Vector2(dirPrevCurrent.Y * dirPrevCurrent.Y * dirCurrentNext.X
                                - dirCurrentNext.Y * dirCurrentNext.Y * dirPrevCurrent.X,
                                dirPrevCurrent.X * dirPrevCurrent.X * dirCurrentNext.Y
                                - dirCurrentNext.X * dirCurrentNext.X * dirPrevCurrent.Y);
                            miterOffset = miterOffset.Div(determinant);

                            // Applying only if the miter offset is within the miter limit; otherwise, adding a Bevel join instead
                            if (miterOffset.LengthSquared() < penOptions.MiterLimit * penOptions.MiterLimit * radius * radius)
                            {
                                Vector2 vec = currentVec + miterOffset;
                                result.Add(vec.AsPointF());
                                return;
                            }
                        }
                        else // Round
                        {
                            PointF startPoint = GetBevelPoint(currentVec, previousVec, radius, true);
                            PointF endPoint = GetBevelPoint(currentVec, nextVec, radius, false);

                            Vector2 dist = endPoint.AsVector2() - startPoint.AsVector2();
                            float length = dist.Length();
                            if (penOptions.Width >= length && !length.TolerantIsZero(Constants.PointEqualityTolerance))
                            {
                                float halfSweepRad = MathF.Asin(length / penOptions.Width);
                                float startRad = MathF.Atan2(dist.Y, dist.X) - halfSweepRad - MathF.PI / 2f;
                                result.AddRange(BezierSegment.FromArc(currentPoint, radius, radius,
                                    startRad, 2f * halfSweepRad).GetFlattenedPointsInternal());
                            }
                            else
                            {
                                result.Add(startPoint);
                                result.Add(endPoint);
                            }

                            break;
                        }
                    }
#else
                    float diffCurrentPrevX = currentPoint.X - previousPoint.X;
                    float diffCurrentPrevY = currentPoint.Y - previousPoint.Y;

                    // Checking if the current point is on the left side of the line segment
                    if (diffCurrentPrevX * (nextPoint.Y - previousPoint.Y) > diffCurrentPrevY * (nextPoint.X - previousPoint.X))
                    {
                        if (penOptions.LineJoin == LineJoinStyle.Miter)
                        {
                            float diffNextCurrentX = nextPoint.X - currentPoint.X;
                            float diffNextCurrentY = nextPoint.Y - currentPoint.Y;
                            float lenPrevCurrent = MathF.Sqrt(diffCurrentPrevX * diffCurrentPrevX + diffCurrentPrevY * diffCurrentPrevY);
                            float lenCurrentNext = MathF.Sqrt(diffNextCurrentX * diffNextCurrentX + diffNextCurrentY * diffNextCurrentY);

                            // The direction vectors for the previous-to-current and current-to-next segments
                            float dirPrevCurrentX = radius * diffCurrentPrevX / lenPrevCurrent;
                            float dirPrevCurrentY = radius * diffCurrentPrevY / lenPrevCurrent;
                            float dirCurrentNextX = radius * diffNextCurrentX / lenCurrentNext;
                            float dirCurrentNextY = radius * diffNextCurrentY / lenCurrentNext;

                            // The determinant for miter calculation and the actual miter offset.
                            float determinant = (dirPrevCurrentY * dirCurrentNextX - dirPrevCurrentX * dirCurrentNextY);
                            float miterOffsetX = (dirPrevCurrentX * dirCurrentNextX * (dirPrevCurrentX - dirCurrentNextX)
                                + (dirPrevCurrentY * dirPrevCurrentY * dirCurrentNextX
                                - dirCurrentNextY * dirCurrentNextY * dirPrevCurrentX)) / determinant;
                            float miterOffsetY = (dirPrevCurrentY * dirCurrentNextY * (dirPrevCurrentY - dirCurrentNextY)
                                + (dirPrevCurrentX * dirPrevCurrentX * dirCurrentNextY
                                - dirCurrentNextX * dirCurrentNextX * dirPrevCurrentY)) / determinant;

                            // Applying only if the miter offset is within the miter limit; otherwise, adding a Bevel join instead
                            if (miterOffsetX * miterOffsetX + miterOffsetY * miterOffsetY < penOptions.MiterLimit * penOptions.MiterLimit * radius * radius)
                            {
                                result.Add(new PointF(currentPoint.X + miterOffsetX, currentPoint.Y + miterOffsetY));
                                return;
                            }
                        }
                        else // Round
                        {
                            PointF startPoint = GetBevelPoint(currentPoint, previousPoint, radius, true);
                            PointF endPoint = GetBevelPoint(currentPoint, nextPoint, radius, false);

                            float distX = endPoint.X - startPoint.X;
                            float distY = endPoint.Y - startPoint.Y;
                            float length = MathF.Sqrt(distX * distX + distY * distY);
                            if (penOptions.Width >= length && !length.TolerantIsZero(Constants.PointEqualityTolerance))
                            {
                                float halfSweepRad = MathF.Asin(length / penOptions.Width);
                                float startRad = MathF.Atan2(distY, distX) - halfSweepRad - MathF.PI / 2f;
                                result.AddRange(BezierSegment.FromArc(currentPoint, radius, radius,
                                    startRad, 2f * halfSweepRad).GetFlattenedPointsInternal());
                            }
                            else
                            {
                                result.Add(startPoint);
                                result.Add(endPoint);
                            }

                            break;
                        }
                    }
#endif

                    // fallback to Bevel style
                    goto case LineJoinStyle.Bevel;

                case LineJoinStyle.Bevel:
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                    result.Add(GetBevelPoint(currentVec, previousVec, radius, true));
                    result.Add(GetBevelPoint(currentVec, nextVec, radius, false));
#else
                    result.Add(GetBevelPoint(currentPoint, previousPoint, radius, true));
                    result.Add(GetBevelPoint(currentPoint, nextPoint, radius, false));
#endif
                    break;

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unhandled join type: {penOptions.LineJoin}"));
            }
        }

        // The original method was taken from ReactOS (MIT license): https://github.com/reactos/reactos/blob/764881a94b4129538d62fda2c99cfcd1ad518ce5/dll/win32/gdiplus/graphicspath.c#L1879
        // Main changes: converting to C#, more descriptive variable names, Flat style extends the bevel points by a fix 0.5 px cap, converting round cap to line segments, using vectors on platforms where available
        private static void WidenCap(PointF endPoint, PointF nextPoint, in PenOptions penOptions, LineCapStyle cap, bool addRightSide, bool addLeftSide, List<PointF> result)
        {
            if (!addRightSide && cap == LineCapStyle.Round)
                return;

            // When the cap style is Flat, the original code just added two bevel points at the original start/end points.
            // But that way lines are always 1 pixel shorter than needed (when drawing, right/bottom coordinates are inclusive).
            // This is also in sync with the DrawThinRawPath behavior.
            float distance = penOptions.Width / 2f;
            float extensionLength = cap == LineCapStyle.Flat ? 0.5f : distance;
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
            ref Vector2 endVec = ref endPoint.AsVector2();
            ref Vector2 nextVec = ref nextPoint.AsVector2();
            Vector2 diffSegment = nextVec - endVec;
            float segmentLength = diffSegment.Length();
            Vector2 extend = (extensionLength * diffSegment).Div(segmentLength);
#else
            float diffSegmentX = nextPoint.X - endPoint.X;
            float diffSegmentY = nextPoint.Y - endPoint.Y;
            float segmentLength = MathF.Sqrt(diffSegmentY * diffSegmentY + diffSegmentX * diffSegmentX);
            float extendX = extensionLength * diffSegmentX / segmentLength;
            float extendY = extensionLength * diffSegmentY / segmentLength;
#endif

            switch (cap)
            {
                case LineCapStyle.Flat:
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                    if (addRightSide)
                        result.Add(GetBevelPoint(endVec - extend, nextVec, distance, true));
                    if (addLeftSide)
                        result.Add(GetBevelPoint(endVec - extend, nextVec, distance, false));
#else
                    if (addRightSide)
                        result.Add(GetBevelPoint(new PointF(endPoint.X - extendX, endPoint.Y - extendY), nextPoint, distance, true));
                    if (addLeftSide)
                        result.Add(GetBevelPoint(new PointF(endPoint.X - extendX, endPoint.Y - extendY), nextPoint, distance, false));
#endif
                    break;

                case LineCapStyle.Square:
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                    if (addRightSide)
                    {
                        Vector2 vec = endVec - extend + new Vector2(-extend.Y, extend.X);
                        result.Add(vec.AsPointF());
                    }

                    if (addLeftSide)
                    {
                        Vector2 vec = endVec - extend + new Vector2(extend.Y, -extend.X);
                        result.Add(vec.AsPointF());
                    }
#else
                    if (addRightSide)
                        result.Add(new PointF(endPoint.X - extendX - extendY, endPoint.Y - extendY + extendX));
                    if (addLeftSide)
                        result.Add(new PointF(endPoint.X - extendX + extendY, endPoint.Y - extendY - extendX));
#endif
                    break;

                case LineCapStyle.Triangle:
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                    if (addRightSide)
                    {
                        result.Add(GetBevelPoint(endVec, nextVec, distance, true));
                        Vector2 vec = endVec - extend;
                        result.Add(vec.AsPointF());
                    }
                    if (addLeftSide)
                        result.Add(GetBevelPoint(endVec, nextVec, distance, false));
#else
                    if (addRightSide)
                    {
                        result.Add(GetBevelPoint(endPoint, nextPoint, distance, true));
                        result.Add(new PointF(endPoint.X - extendX, endPoint.Y - extendY));
                    }

                    if (addLeftSide)
                        result.Add(GetBevelPoint(endPoint, nextPoint, distance, false));
#endif
                    break;

                case LineCapStyle.Round:
                    Debug.Assert(addRightSide);
                    const float distControlPoint = 0.5522848f; // 4/3 * (sqrt(2) - 1)
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                    Vector2 ctrlPoint = extend * distControlPoint;
                    var bezierPoints = new[]
                    {
                        // first 90-degree arc
                        new PointF(endPoint.X - extend.Y, endPoint.Y + extend.X),
                        new PointF(endPoint.X - extend.Y - ctrlPoint.X, endPoint.Y + extend.X - ctrlPoint.Y),
                        new PointF(endPoint.X - extend.X - ctrlPoint.Y, endPoint.Y - extend.Y + ctrlPoint.X),

                        // midpoint
                        new PointF(endPoint.X - extend.X, endPoint.Y - extend.Y),
                        
                        // second 90-degree arc
                        new PointF(endPoint.X - extend.X + ctrlPoint.Y, endPoint.Y - extend.Y - ctrlPoint.X),
                        new PointF(endPoint.X + extend.Y - ctrlPoint.X, endPoint.Y - extend.X - ctrlPoint.Y),
                        new PointF(endPoint.X + extend.Y, endPoint.Y - extend.X)
                    };
#else
                    float ctrlPointX = extendX * distControlPoint;
                    float ctrlPointY = extendY * distControlPoint;
                    var bezierPoints = new[]
                    {
                        // first 90-degree arc
                        new PointF(endPoint.X - extendY, endPoint.Y + extendX),
                        new PointF(endPoint.X - extendY - ctrlPointX, endPoint.Y + extendX - ctrlPointY),
                        new PointF(endPoint.X - extendX - ctrlPointY, endPoint.Y - extendY + ctrlPointX),
                                
                        // midpoint
                        new PointF(endPoint.X - extendX, endPoint.Y - extendY),

                        // second 90-degree arc
                        new PointF(endPoint.X - extendX + ctrlPointY, endPoint.Y - extendY - ctrlPointX),
                        new PointF(endPoint.X + extendY - ctrlPointX, endPoint.Y - extendX - ctrlPointY),
                        new PointF(endPoint.X + extendY, endPoint.Y - extendX)
                    };
#endif

                    result.AddRange(new BezierSegment(bezierPoints, false).GetFlattenedPointsInternal());
                    break;

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unhandled cap style: {cap}"));
            }
        }

#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
        private static PointF GetBevelPoint(Vector2 endPoint, Vector2 nextPoint, float distance, bool isRightSide)
        {
            Vector2 diffSegment = nextPoint - endPoint;
            float segmentLength = diffSegment.Length();

            if (segmentLength == 0f)
                return endPoint.AsPointF();

            Vector2 distBevel = isRightSide
                ? (distance * new Vector2(-diffSegment.Y, diffSegment.X)).Div(segmentLength)
                : (distance * new Vector2(diffSegment.Y, -diffSegment.X)).Div(segmentLength);
            Vector2 result = endPoint + distBevel;
            return result.AsPointF();
        }
#else
        private static PointF GetBevelPoint(PointF endPoint, PointF nextPoint, float distance, bool isRightSide)
        {
            float diffSegmentX = nextPoint.X - endPoint.X;
            float diffSegmentY = nextPoint.Y - endPoint.Y;
            float segmentLength = MathF.Sqrt(diffSegmentY * diffSegmentY + diffSegmentX * diffSegmentX);

            if (segmentLength == 0f)
                return endPoint;

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

            return new PointF(endPoint.X + distBevelX, endPoint.Y + distBevelY);
        }
#endif

#endregion

        #region Instance Methods

        #region Internal Methods

        internal void AddRawFigure(IList<PointF> points, bool isClosed, bool offset)
        {
            if (points.Count == 0)
                return;
            var figure = new RawFigure(points, isClosed, offset);
            bounds = IsEmpty ? figure.Bounds : Rectangle.Union(bounds, figure.Bounds);
            figures.Add(figure);
            totalVertices += figure.VertexCount;
            maxVertices = Math.Max(maxVertices, figure.VertexCount);
            regionsCache = null;
            widePathsCache = null;
        }

        internal Region GetCreateCachedRegion(DrawingOptions drawingOptions, bool isOutline = false)
        {
            #region Local Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            static RegionsCacheKey GetHashKey(DrawingOptions options, bool outline)
            {
                var result = RegionsCacheKey.None;
                if (options.FillMode == ShapeFillMode.NonZero && !outline)
                    result |= RegionsCacheKey.NonZeroFillMode;
                if (options.AntiAliasing)
                    result |= RegionsCacheKey.AntiAliasing;
                if (outline)
                    result |= RegionsCacheKey.Outline;
                if (!outline && options.ScanPathPixelOffset == PixelOffset.Half || outline && options.DrawPathPixelOffset == PixelOffset.Half)
                    result |= RegionsCacheKey.Offset;
                return result;
            }

            #endregion

            if (regionsCache == null)
            {
                var options = new LockFreeCacheOptions { InitialCapacity = 4, ThresholdCapacity = 4, HashingStrategy = HashingStrategy.And, MergeInterval = TimeSpan.FromMilliseconds(100) };
                Interlocked.CompareExchange(ref regionsCache, ThreadSafeCacheFactory.Create<int, Region>(CreateRegion, options), null);
            }

            return regionsCache[(int)GetHashKey(drawingOptions, isOutline)];
        }

        internal RawPath GetCreateWidePath(Pen pen, DrawingOptions drawingOptions)
        {
            if (widePathsCache == null)
            {
                var options = new LockFreeCacheOptions { InitialCapacity = 2, ThresholdCapacity = 2, HashingStrategy = HashingStrategy.Modulo, MergeInterval = TimeSpan.FromMilliseconds(100) };
                Interlocked.CompareExchange(ref widePathsCache, ThreadSafeCacheFactory.Create<PenOptions, RawPath>(DoWidenPath, options), null);
            }

            return widePathsCache[new PenOptions(pen, drawingOptions)];
        }

        internal RawPath WidenPath(Pen pen, DrawingOptions drawingOptions) => DoWidenPath(new PenOptions(pen, drawingOptions));

        #endregion

        #region Private Methods

        private Region CreateRegion(int key)
        {
            var options = (RegionsCacheKey)key;
            return new Region((options & RegionsCacheKey.Outline) != 0 ? DrawOutlineBounds : bounds, (options & RegionsCacheKey.AntiAliasing) != 0);
        }

        private RawPath DoWidenPath(PenOptions penOptions)
        {
            var result = new RawPath(figures.Capacity);
            foreach (RawFigure figure in figures)
            {
                switch (figure.VertexCount)
                {
                    case > 1:
                        // TODO
                        //if (key.Dash != null)
                        //    WidenDashedFigure(result, figure);
                        //else
                        if (figure.IsClosed)
                            WidenClosedFigure(figure, penOptions, result);
                        else
                            WidenOpenFigure(figure, penOptions, result);
                        break;
                    case 1:
                        WidenPoint(figure, penOptions, result);
                        break;
                    default:
                        continue;
                }
            }

            return result;
        }

        #endregion

        #endregion

#endregion
    }
}