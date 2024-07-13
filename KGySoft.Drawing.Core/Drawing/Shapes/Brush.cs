#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Brush.cs
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
using System.Numerics;
using System.Runtime.CompilerServices;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents a brush shape filling operations.
    /// </summary>
    internal abstract class Brush // TODO: public when it can fill any path, IEquatable, ICloneable
    {
        #region Nested Types

        #region Enumerations

        private enum EdgeKind
        {
            Ascending,
            Descending,
            HorizontalRight,
            HorizontalLeft
        }

        // TODO: move to ActiveEdgeTableNonZero
        private enum IntersectionType : byte
        {
            Ascending,
            Descending,
            Corner,
            CornerPlaceholder
        }

        #endregion

        #region Nested Classes

        #region FillPathSession class

        private protected abstract class FillPathSession
        {
            #region Methods

            internal abstract void ApplyScanlineAntiAliasing(in RegionScanlineAntiAliasing scanline);

            #endregion
        }

        #endregion

        #region RegionScanner class

        private abstract class RegionScanner
        {
            #region Fields

            private Rectangle bounds;

            #endregion

            #region Properties

            protected int Top => bounds.Top;
            protected int Bottom => bounds.Bottom;
            protected int Left => bounds.Left;
            protected RawPath Path { get; }
            protected DrawingOptions DrawingOptions { get; }

            #endregion

            #region Constructors

            protected RegionScanner(RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
            {
                Path = path;
                this.bounds = bounds;
                DrawingOptions = drawingOptions;
            }

            #endregion

            #region Methods

            //internal abstract void ProcessScanline(int y, FillPathSession session) // TODO - parallelize
            internal abstract void ProcessNextScanline(FillPathSession session);

            #endregion
        }

        #endregion

        #region SolidRegionScanner class

        private class SolidRegionScanner : RegionScanner
        {
            #region Constructors

            public SolidRegionScanner(RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
                : base(path, bounds, drawingOptions)
            {
            }

            #endregion

            #region Methods

            internal override void ProcessNextScanline(FillPathSession session)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion

        #region AntiAliasingRegionScanner class

        private class AntiAliasingRegionScanner : RegionScanner
        {
            #region Constants

            private const float subpixelCount = 16f;

            #endregion
            
            #region Fields

            private readonly ActiveEdgeTable activeEdges;
            private readonly float subpixelSize;
            private readonly float subpixelArea;
            private readonly float[] intersections; // todo: into ActiveEdgeTable?
            private readonly int[] sortedIndexYStart; // todo: into ActiveEdgeTable/base?
            private readonly int[] sortedIndexYEnd; // todo: into ActiveEdgeTable/base?
            private readonly IntersectionType[]? intersectionTypes; // todo: into ActiveEdgeTableNonZero? (along with IntersectionType enum)
            private readonly ArraySection<float> scanlineBuffer;

            private ArraySection<EdgeEntry> edges; // todo: into ActiveEdgeTable?
            private int yStartIndex;
            private int yEndIndex;
            private float nextY;
            private int currentY;
            private float currentSubpixelY;
            private bool isScanlineDirty;

            #endregion

            #region Constructors

            public AntiAliasingRegionScanner(RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
                : base(path, bounds, drawingOptions)
            {
                subpixelSize = 1f / subpixelCount;
                subpixelArea = 1f / (subpixelCount * subpixelCount);
                var maxIntersectionCount = path.MaxVertices << 1;

                edges = new EdgeTable(path, subpixelCount).Edges;
                int edgeCount = edges.Length;
                activeEdges = ActiveEdgeTable.Create(drawingOptions.FillMode, edgeCount /*TODO: path.MaxVertices*/);
                intersections = new float[maxIntersectionCount];
                if (drawingOptions.FillMode == ShapeFillMode.NonZero)
                    intersectionTypes = new IntersectionType[maxIntersectionCount];

                currentY = bounds.Top - 1;

                // TODO perf: stackalloc, Span.Sort
                sortedIndexYStart = new int[edgeCount];
                sortedIndexYEnd = new int[edgeCount];
                var sortedIndexYStartKeys = new float[edgeCount];
                var sortedIndexYEndKeys = new float[edgeCount];
                for (int i = 0; i < edgeCount; i++)
                {
                    ref EdgeEntry edge = ref edges.GetElementReference(i);
                    sortedIndexYStartKeys[i] = edge.YStart;
                    sortedIndexYEndKeys[i] = edge.YEnd;
                    sortedIndexYStart[i] = sortedIndexYEnd[i] = i;
                }

                Array.Sort(sortedIndexYStartKeys, sortedIndexYStart);
                Array.Sort(sortedIndexYEndKeys, sortedIndexYEnd);

                SkipEdgesAboveTop();
                scanlineBuffer = new float[bounds.Width];
            }

            #endregion

            #region Methods

            #region Internal Methods
            
            //internal override void ProcessScanline(int y, FillPathSession session) // TODO - parallelize
            internal override void ProcessNextScanline(FillPathSession session)
            {
                // TODO: parallelize - MoveToRow(y)
                if (!MoveNextRow())
                    return;

                // TODO: handle this in MoveToRow, including when region is cached so it just returns a row from an Array2D, which is already clean
                if (isScanlineDirty)
                    Array.Clear(scanlineBuffer.UnderlyingArray!, scanlineBuffer.Offset, scanlineBuffer.Length);

                isScanlineDirty = false;

                int left = Left;
                float minX = left;
                int startMin = Int32.MaxValue;
                int endMax = Int32.MinValue;
                while (MoveNextSubpixelRow())
                {
                    ArraySection<float> points = activeEdges.ScanLine(currentSubpixelY, edges, intersections, intersectionTypes);
                    if (points.Length == 0)
                        continue;

                    for (int point = 0; point < points.Length - 1; point += 2)
                    {
                        float scanStart = points[point] - minX;
                        float scanEnd = points[point + 1] - minX;
                        int startX = (int)MathF.Floor(scanStart);
                        int endX = (int)MathF.Floor(scanEnd);

                        if (startX >= 0 && startX < scanlineBuffer.Length)
                        {
                            float subpixelWidth = (startX + 1 - scanStart) / subpixelSize;
                            scanlineBuffer[startX] += subpixelWidth * subpixelArea;
                            isScanlineDirty |= subpixelWidth > 0;
                        }

                        if (endX >= 0 && endX < scanlineBuffer.Length)
                        {
                            float subpixelWidth = (scanEnd - endX) / subpixelSize;
                            scanlineBuffer[endX] += subpixelWidth * subpixelArea;
                            isScanlineDirty |= subpixelWidth > 0;
                        }

                        int nextX = startX + 1;
                        endX = Math.Min(endX, scanlineBuffer.Length);
                        nextX = Math.Max(nextX, 0);

                        if (endX > nextX)
                        {
                            // TODO: vectorization if possible
                            for (int i = nextX; i < endX; i++)
                                scanlineBuffer[i] += subpixelSize;
                            isScanlineDirty = true;
                        }

                        startMin = Math.Min(startMin, startX);
                        endMax = Math.Max(endMax, endX);
                    }
                }

                if (isScanlineDirty)
                    session.ApplyScanlineAntiAliasing(new RegionScanlineAntiAliasing(currentY, left, scanlineBuffer, Math.Max(0, startMin), Math.Min(scanlineBuffer.Length - 1, endMax)));
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Skips the edges above <see cref="RegionScanner.Top"/>. It does not perform full scans, just at start/end Y positions.
            /// </summary>
            private void SkipEdgesAboveTop()
            {
                if (edges.Length == 0)
                    return;

                currentSubpixelY = edges[sortedIndexYStart[0]].YStart;

                int startIndex = 1;
                int endIndex = 0;

                float top = Top;
                while (currentSubpixelY < top)
                {
                    VisitEdges();
                    activeEdges.RemoveLeavingEdges();

                    bool startFinished = startIndex >= sortedIndexYStart.Length;
                    bool endFinished = endIndex >= sortedIndexYEnd.Length;
                    if (startFinished && endFinished)
                        return;

                    float startY = startFinished ? Single.MaxValue : edges[sortedIndexYStart[startIndex]].YStart;
                    float endY = endFinished ? Single.MaxValue : edges[sortedIndexYEnd[endIndex]].YEnd;

                    if (startY < endY)
                    {
                        currentSubpixelY = startY;
                        startIndex += 1;
                        continue;
                    }

                    currentSubpixelY = endY;
                    endIndex++;
                }
            }

            private void VisitEdges()
            {
                while (yStartIndex < sortedIndexYStart.Length)
                {
                    int i = sortedIndexYStart[yStartIndex];
                    if (edges[i].YStart > currentSubpixelY)
                        break;

                    activeEdges.EnterEdge(i);
                    yStartIndex += 1;
                }

                while (yEndIndex < sortedIndexYEnd.Length)
                {
                    int i = sortedIndexYEnd[yEndIndex];
                    if (edges[i].YEnd > currentSubpixelY)
                        break;

                    activeEdges.LeaveEdge(i);
                    yEndIndex += 1;
                }
            }

            private bool MoveNextRow()
            {
                currentY += 1;
                nextY = currentY + 1;
                currentSubpixelY = currentY - subpixelSize;
                return currentY < Bottom;
            }

            private bool MoveNextSubpixelRow()
            {
                currentSubpixelY += subpixelSize;
                VisitEdges();
                return currentSubpixelY < nextY;
            }

            #endregion

            #endregion
        }

        #endregion

        #region ActiveEdgeTable class

        private abstract class ActiveEdgeTable
        {
            #region Nested Types

            [Flags]
            private protected enum Flags : byte
            {
                None,
                Entering = 1,
                Leaving = 1 << 1
            }

            #endregion

            #region Fields

            #region Private Fields
            
            // TODO: to section?
            private readonly (int Index, Flags Flags)[] edgeTable;

            #endregion

            #region Private Protected Fields
            
            private protected int Count;

            #endregion

            #endregion

            #region Properties

            private protected ArraySection<(int Index, Flags Flags)> ActiveEdges => edgeTable.AsSection(0, Count);

            #endregion

            #region Constructors

            protected ActiveEdgeTable(int edgeCount)
            {
                edgeTable = new (int, Flags)[edgeCount];
                Count = 0;
            }

            #endregion

            #region Methods

            #region Static Methods

            internal static ActiveEdgeTable Create(ShapeFillMode fillMode, int edgeCount) => fillMode switch
            {
                ShapeFillMode.Alternate => new ActiveEdgeTableAlternate(edgeCount),
                ShapeFillMode.NonZero => new ActiveEdgeTableNonZero(edgeCount),
                _ => throw new ArgumentOutOfRangeException(nameof(fillMode), PublicResources.EnumOutOfRange(fillMode))
            };

            #endregion

            #region Instance Methods

            internal void EnterEdge(int enterIndex)
            {
                ref var entry = ref edgeTable[Count];
                entry.Index = enterIndex;
                entry.Flags = Flags.Entering;
                Count += 1;
            }

            internal void LeaveEdge(int leaveIndex)
            {
                var table = edgeTable;
                int len = Count;
                for (int i = 0; i < len; i++)
                {
                    if (table[i].Index == leaveIndex)
                    {
                        table[i].Flags |= Flags.Leaving;
                        return;
                    }
                }
            }

            internal void RemoveLeavingEdges()
            {
                int removed = 0;
                var table = edgeTable;
                int len = Count;
                for (int i = 0; i < len; i++)
                {
                    ref var entry = ref table[i];
                    if ((entry.Flags & Flags.Leaving) != Flags.None)
                        removed += 1;
                    else
                        table[i - removed] = (entry.Index, Flags.None);
                }

                Count -= removed;
            }

            internal abstract ArraySection<float> ScanLine(float y, ArraySection<EdgeEntry> edges, ArraySection<float> intersections, ArraySection<IntersectionType> intersectionTypes);

            #endregion

            #endregion
        }

        #endregion

        #region ActiveEdgeTableAlternate class

        private sealed class ActiveEdgeTableAlternate : ActiveEdgeTable
        {
            #region Constructors
            
            internal ActiveEdgeTableAlternate(int edgeCount)
                : base(edgeCount)
            {
            }

            #endregion

            #region Methods

            internal override ArraySection<float> ScanLine(float y, ArraySection<EdgeEntry> edges, ArraySection<float> intersections, ArraySection<IntersectionType> intersectionTypes)
            {
                #region Local Methods

                static void AddIntersection(float x, int emitCount, ArraySection<float> intersections, ref int count)
                {
                    switch (emitCount)
                    {
                        case 2:
                            intersections[count] = x;
                            intersections[count + 1] = x;
                            count += 2;
                            break;
                        case 1:
                            intersections[count] = x;
                            count += 1;
                            break;
                    }
                }

                #endregion

                int intersectionsCount = 0;
                int removed = 0;
                var activeEdges = ActiveEdges;
                int len = activeEdges.Length;
                for (int i = 0; i < len; i++)
                {
                    ref var entry = ref activeEdges.GetElementReference(i);
                    ref var edge = ref edges.GetElementReference(entry.Index);
                    float x = edge.GetX(y);
                    if ((entry.Flags & Flags.Entering) != Flags.None)
                        AddIntersection(x, edge.StartEmitCount, intersections, ref intersectionsCount);
                    else if ((entry.Flags & Flags.Leaving) != Flags.None)
                    {
                        AddIntersection(x, edge.EndEmitCount, intersections, ref intersectionsCount);
                        removed += 1;
                        continue;
                    }
                    else
                    {
                        intersections[intersectionsCount] = x;
                        intersectionsCount += 1;
                    }

                    activeEdges[i - removed] = (entry.Index, Flags.None);
                }

                Count -= removed;
                Array.Sort(intersections.UnderlyingArray!, 0, intersectionsCount);
                return intersections.Slice(0, intersectionsCount);
            }

            #endregion
        }

        #endregion

        #region ActiveEdgeTableNonZero class

        private sealed class ActiveEdgeTableNonZero : ActiveEdgeTable
        {
            #region Constants

            /// <summary>
            /// A constant, that is smaller than snapping factor for subpixel size, used to prevent sorting from mixing up elements with the same key.
            /// Its value is smaller than subpixel snapping distance (actually the same as equality tolerance for interpolated resizing).
            /// </summary>
            private const float sortStabilizerDelta = 1e-4f;

            #endregion

            #region Constructors

            internal ActiveEdgeTableNonZero(int edgeCount)
                : base(edgeCount)
            {
            }

            #endregion

            #region Methods

            #region Static Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            private static void AddIntersection(float x, int emitCount, bool isAscending, ArraySection<float> intersections, ArraySection<IntersectionType> intersectionTypes, ref int count)
            {
                if (emitCount == 2)
                {
                    // for corners emitting two points, making sure that the placeholder is a bit smaller than the actual corner to prevent sorting from replace them
                    intersectionTypes[count] = IntersectionType.CornerPlaceholder;
                    intersections[count] = x - sortStabilizerDelta;
                    count += 1;
                    intersectionTypes[count] = IntersectionType.Corner;
                    intersections[count] = x;
                    count += 1;
                    return;
                }

                if (emitCount != 1)
                    return;

                if (isAscending)
                {
                    intersectionTypes[count] = IntersectionType.Ascending;
                    intersections[count] = x + sortStabilizerDelta;
                    count += 1;
                    return;
                }

                intersectionTypes[count] = IntersectionType.Descending;
                intersections[count] = x - sortStabilizerDelta;
                count += 1;
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            private static void AddIntersectionWhenToggles(ArraySection<float> intersections, int i, int diff, float value, ref int windingNumber, ref int removed)
            {
                bool add = (windingNumber == 0 && diff != 0) || windingNumber * diff == -1;
                windingNumber += diff;

                if (add)
                    intersections[i - removed] = value;
                else
                    removed += 1;
            }

            #endregion

            #region Instance Methods

            internal override ArraySection<float> ScanLine(float y, ArraySection<EdgeEntry> edges, ArraySection<float> intersections, ArraySection<IntersectionType> intersectionTypes)
            {
                int intersectionsCount = 0;
                int removed = 0;
                var activeEdges = ActiveEdges;
                int len = activeEdges.Length;
                for (int i = 0; i < len; i++)
                {
                    ref var entry = ref activeEdges.GetElementReference(i);
                    ref var edge = ref edges.GetElementReference(entry.Index);
                    float x = edge.GetX(y);
                    if ((entry.Flags & Flags.Entering) != Flags.None)
                        AddIntersection(x, edge.StartEmitCount, edge.IsAscending, intersections, intersectionTypes!, ref intersectionsCount);
                    else if ((entry.Flags & Flags.Leaving) != Flags.None)
                    {
                        AddIntersection(x, edge.EndEmitCount, edge.IsAscending, intersections, intersectionTypes!, ref intersectionsCount);
                        removed += 1;
                        continue;
                    }
                    else
                    {
                        if (edge.IsAscending)
                        {
                            intersectionTypes![intersectionsCount] = IntersectionType.Ascending;
                            intersections[intersectionsCount] = x + sortStabilizerDelta;
                        }
                        else
                        {
                            intersectionTypes![intersectionsCount] = IntersectionType.Descending;
                            intersections[intersectionsCount] = x - sortStabilizerDelta;
                        }

                        intersectionsCount += 1;
                    }

                    activeEdges[i - removed] = (entry.Index, Flags.None);
                }

                Count -= removed;
                Array.Sort(intersections.UnderlyingArray!, intersectionTypes.UnderlyingArray, 0, intersectionsCount);
                intersections = intersections.Slice(0, intersectionsCount);
                intersectionTypes = intersectionTypes.Slice(0, intersectionsCount);

                // Counting the winding number and applying the nonzero winding rule. See details here: https://en.wikipedia.org/wiki/Point_in_polygon#Winding_number_algorithm
                removed = 0;
                int windingNumber = 0;
                len = intersectionTypes.Length;
                for (int i = 0; i < len; i++)
                {
                    IntersectionType type = intersectionTypes[i];
                    switch (type)
                    {
                        // it's always followed by a Corner, where it's processed
                        case IntersectionType.CornerPlaceholder:
                            removed += 1;
                            break;

                        case IntersectionType.Corner:
                            AddIntersectionWhenToggles(intersections, i, -1, intersections[i], ref windingNumber, ref removed);
                            removed -= 1;
                            AddIntersectionWhenToggles(intersections, i, 1, intersections[i], ref windingNumber, ref removed);
                            break;
                        default:
                            {
                                int diff = type == IntersectionType.Ascending ? 1 : -1;
                                float emitVal = intersections[i] + -(sortStabilizerDelta * diff);
                                AddIntersectionWhenToggles(intersections, i, diff, emitVal, ref windingNumber, ref removed);
                                break;
                            }
                    }
                }

                return intersections.Slice(0, intersections.Length - removed);
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #region Nested Structs

        #region EdgeTable struct

        private ref struct EdgeTable
        {
            #region Properties

            internal ArraySection<EdgeEntry> Edges { get; }

            #endregion

            #region Constructors

            public EdgeTable(RawPath path, float snappingFactor)
            {
                var edges = new EdgeEntry[path.TotalVertices];
                var snappedYCoords = new float[path.MaxVertices + 1];
                var enumerator = new EdgeEnumerator(edges);
                foreach (RawFigure figure in path.Figures)
                {
                    PointF[] vertices = figure.Vertices;
                    if (vertices.Length <= 3)
                        continue;

                    for (int i = 0; i < vertices.Length; i++)
                        snappedYCoords[i] = MathF.Round(vertices[i].Y * snappingFactor, MidpointRounding.AwayFromZero) / snappingFactor;

                    enumerator.StartNextFigure(new EdgeInfo(vertices, snappedYCoords, vertices.Length - 2),
                        new EdgeInfo(vertices, snappedYCoords, 0),
                        new EdgeInfo(vertices, snappedYCoords, 1));

                    enumerator.MoveNextEdge(false);

                    for (int i = 1; i < vertices.Length - 2; i++)
                    {
                        enumerator.Next = new EdgeInfo(vertices, snappedYCoords, i + 1);
                        enumerator.MoveNextEdge(true);
                    }

                    // 1st edge
                    enumerator.Next = new EdgeInfo(vertices, snappedYCoords, 0);
                    enumerator.MoveNextEdge(true);

                    // 2nd edge
                    enumerator.Next = new EdgeInfo(vertices, snappedYCoords, 1);
                    enumerator.MoveNextEdge(true);
                }

                Edges = edges.AsSection(0, enumerator.Count);
            }

            #endregion
        }

        #endregion

        #region EdgeInfo struct

        /// <summary>
        /// Represents an edge during the traversal of the raw vertices. The fields can be mutated during processing.
        /// </summary>
        private ref struct EdgeInfo
        {
            #region Fields

            internal PointF Start;
            internal PointF End;
            internal EdgeKind Kind;
            internal int StartEmitCount;
            internal int EndEmitCount;

            #endregion

            #region Constructors

            internal EdgeInfo(PointF[] vertices, float[] snappedYCoords, int index)
            {
                Start = new PointF(vertices[index].X, snappedYCoords[index]);
                End = new PointF(vertices[index + 1].X, snappedYCoords[index + 1]);
                Kind = Start.Y == End.Y
                    ? Start.X < End.X ? EdgeKind.HorizontalRight : EdgeKind.HorizontalLeft
                    : Start.Y < End.Y ? EdgeKind.Descending : EdgeKind.Ascending;
                StartEmitCount = 0;
                EndEmitCount = 0;
            }

            #endregion

            #region Methods

            #region Static Methods

            internal static void InitEmitCount(ref EdgeInfo edge1, ref EdgeInfo edge2)
            {
                // A vertex can be emitted multiple times (0 to 2), depending on the relation of its edges.
                // For example, emitting a vertex 0 times if the edges are collinear, but emitting 2 times
                // if one of the edges are horizontal, or the corner is concave.
                switch ((edge1.Kind, edge2.Kind))
                {
                    case (EdgeKind.Ascending, EdgeKind.Ascending):
                        edge2.StartEmitCount = 1;
                        break;
                    case (EdgeKind.Ascending, EdgeKind.Descending):
                        edge1.EndEmitCount = 1;
                        edge2.StartEmitCount = 1;
                        break;
                    case (EdgeKind.Ascending, EdgeKind.HorizontalLeft):
                        edge1.EndEmitCount = 2;
                        break;
                    case (EdgeKind.Ascending, EdgeKind.HorizontalRight):
                        edge1.EndEmitCount = 1;
                        break;

                    case (EdgeKind.Descending, EdgeKind.Ascending):
                        edge1.EndEmitCount = 1;
                        edge2.StartEmitCount = 1;
                        break;
                    case (EdgeKind.Descending, EdgeKind.Descending):
                        edge2.StartEmitCount = 1;
                        break;
                    case (EdgeKind.Descending, EdgeKind.HorizontalLeft):
                        edge1.EndEmitCount = 1;
                        break;
                    case (EdgeKind.Descending, EdgeKind.HorizontalRight):
                        edge1.EndEmitCount = 2;
                        break;

                    case (EdgeKind.HorizontalLeft, EdgeKind.Ascending):
                        edge2.StartEmitCount = 1;
                        break;
                    case (EdgeKind.HorizontalLeft, EdgeKind.Descending):
                        edge2.StartEmitCount = 2;
                        break;

                    case (EdgeKind.HorizontalRight, EdgeKind.Ascending):
                        edge2.StartEmitCount = 2;
                        break;
                    case (EdgeKind.HorizontalRight, EdgeKind.Descending):
                        edge2.StartEmitCount = 1;
                        break;
                }
            }

            #endregion

            #region Instance Methods

            internal EdgeEntry ToEdge()
            {
                bool isAscending = Kind == EdgeKind.Ascending;
                if (isAscending)
                {
                    (Start, End) = (End, Start);
                    (StartEmitCount, EndEmitCount) = (EndEmitCount, StartEmitCount);
                }

                return new EdgeEntry(this);
            }

            #endregion

            #endregion
        }

        #endregion

        #region EdgeEntry struct

        /// <summary>
        /// Represents an entry in the global edge table. Unlike <see cref="EdgeInfo"/>, this struct is not mutable.
        /// The basic idea is taken from here: https://www.cs.rit.edu/~icss571/filling/how_to.html
        /// The slope of the edge can be defined as y = mx + b, where m is the slope and b = y - intercept.
        /// From this, the reciprocal slope can be defined as x = py + q where p and q are analogue to m and b.
        /// </summary>
        
        private readonly struct EdgeEntry
        {
            #region Fields

            #region Internal Fields

            internal readonly float YStart;
            internal readonly float YEnd;
            internal readonly byte StartEmitCount;
            internal readonly byte EndEmitCount;
            internal readonly bool IsAscending;

            #endregion

            #region Private Fields
            
            private readonly float p, q;

            #endregion

            #endregion

            #region Constructors

            internal EdgeEntry(EdgeInfo edgeInfo)
            {
                YStart = edgeInfo.Start.Y;
                YEnd = edgeInfo.End.Y;
                float height = YEnd - YStart;
                IsAscending = edgeInfo.Kind is EdgeKind.Ascending;
                StartEmitCount = (byte)edgeInfo.StartEmitCount;
                EndEmitCount = (byte)edgeInfo.EndEmitCount;

                // calculating p and q adjusted to the origin for better accuracy
                Vector2 pStart = edgeInfo.Start.ToVector2();
                Vector2 pEnd = edgeInfo.End.ToVector2();
                Vector2 center = (pStart + pEnd) * 0.5f;
                pStart -= center;
                pEnd -= center;
                p = (pEnd.X - pStart.X) / height;
                q = ((pStart.X * pEnd.Y) - (pEnd.X * pStart.Y)) / height + (center.X - p * center.Y);
            }

            #endregion

            #region Methods

            internal float GetX(float y) => p * y + q;

            #endregion
        }

        #endregion

        #region EdgeEnumerator struct

        private ref struct EdgeEnumerator
        {
            #region Fields

            internal readonly EdgeEntry[] Edges;

            internal int Count;
            internal EdgeInfo Previous;
            internal EdgeInfo Current;
            internal EdgeInfo Next;

            #endregion

            #region Constructors

            internal EdgeEnumerator(EdgeEntry[] edges)
            {
                Edges = edges;
            }

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal void StartNextFigure(EdgeInfo previous, EdgeInfo current, EdgeInfo next)
            {
                Previous = previous;
                Current = current;
                Next = next;
            }

            internal void MoveNextEdge(bool addPrevious)
            {
                EdgeInfo.InitEmitCount(ref Previous, ref Current);
                EdgeInfo.InitEmitCount(ref Current, ref Next);
                if (addPrevious && Previous.Kind is EdgeKind.Ascending or EdgeKind.Descending)
                {
                    Edges[Count] = Previous.ToEdge();
                    Count += 1;
                }

                Previous = Current;
                Current = Next;
            }

            #endregion
        }

        #endregion

        #region RegionScanlineAntiAliasing struct

        private protected ref struct RegionScanlineAntiAliasing
        {
            #region Fields

            internal readonly int RowIndex;
            internal readonly int Left;
            internal readonly int MinIndex;
            internal readonly int MaxIndex;
            internal ArraySection<float> Scanline;

            #endregion

            #region Constructors

            internal RegionScanlineAntiAliasing(int y, int left, ArraySection<float> scanline, int startX, int endX)
            {
                RowIndex = y;
                Left = left;
                Scanline = scanline;
                MinIndex = startX;
                MaxIndex = endX;
            }

            #endregion
        }

        #endregion

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        #region Public Methods

        public static Brush CreateSolid(Color32 color) => new SolidBrush(color);
        public static Brush CreateSolid(Color64 color) => new SolidBrush(color);
        public static Brush CreateSolid(ColorF color) => new SolidBrush(color);

        #endregion

        #region Private Methods

        private static RegionScanner CreateScanner(RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
        {
            // TODO from cache if possible
            return drawingOptions.AntiAliasing ? new AntiAliasingRegionScanner(path, bounds, drawingOptions) : new SolidRegionScanner(path, bounds, drawingOptions);
        }

        #endregion

        #endregion

        #region Instance Methods

        #region Internal Methods

        internal void ApplyPath(IAsyncContext context, IReadWriteBitmapData bitmapData, Path path, DrawingOptions drawingOptions)
        {
            RawPath rawPath = path/*TODO .AsClosed() - if needed, cache it, too */.RawPath;
            // TODO: if rawPath.TryGetRegion(this, bitmapData, drawingOptions, out region) -> ApplyRegion... - important: if [Row]Path is not disposable, cached regions should not use ArrayPool or unmanaged buffers
            Rectangle bounds = Rectangle.Intersect(rawPath.Bounds, new Rectangle(Point.Empty, bitmapData.Size));

            if (bounds.IsEmpty || context.IsCancellationRequested)
                return;

            FillPathSession session = CreateSession(bitmapData, bounds, drawingOptions);
            RegionScanner scanner = CreateScanner(rawPath, bounds, drawingOptions);

            for (int y = bounds.Top; y < bounds.Bottom; y++)
            {
                if (context.IsCancellationRequested)
                    return;
                scanner.ProcessNextScanline(session);
            }

            // TODO
            //ParallelHelper.For(context, DrawingOperation.ProcessingPixels, bounds.Top, bounds.Bottom, y =>
            //{
            //    if (scanner.TryGetRowScanline(y, out var scanline))
            //        session.ApplyScanline(scanline);
            //});
        }

        internal abstract void ApplyRegion(IAsyncContext context, IReadWriteBitmapData bitmapData, IReadableBitmapData region, Path path, DrawingOptions drawingOptions);

        #endregion

        #region Private Protected Methods

        private protected abstract FillPathSession CreateSession(IReadWriteBitmapData bitmapData, Rectangle bounds, DrawingOptions drawingOptions);

        #endregion

        #endregion

        #endregion
    }
}