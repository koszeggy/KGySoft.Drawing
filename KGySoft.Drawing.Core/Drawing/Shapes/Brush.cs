﻿#region Copyright

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Reflection;
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

        #endregion

        #region Nested Classes

        #region FillPathSession class

        private protected abstract class FillPathSession
        {
            #region Methods

            internal abstract void ApplyScanlineAntiAliasing(in RegionScanline<float> scanline);
            internal abstract void ApplyScanlineSolid(in RegionScanline<byte> scanline);

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

            #region Internal Properties

            internal abstract bool IsSingleThreaded { get; }

            #endregion

            #region Protected Properties

            protected int Top => bounds.Top;
            protected int Bottom => bounds.Bottom;
            protected int Left => bounds.Left;
            protected int Width => bounds.Width;

            // TODO: remove
            //protected RawPath Path { get; }
            //protected DrawingOptions DrawingOptions { get; }

            #endregion

            #endregion

            #region Constructors

            protected RegionScanner(RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
            {
                this.bounds = bounds;
                // TODO: remove
                //Path = path;
                //DrawingOptions = drawingOptions;
            }

            #endregion

            #region Methods

            internal abstract void ProcessScanline(int y, FillPathSession session);
            internal abstract void ProcessNextScanline(FillPathSession session);

            #endregion
        }

        #endregion

        #region SolidRegionScanner class

        private class SolidRegionScanner : RegionScanner
        {
            #region Nested Structs

            /// <summary>
            /// Contains thread-specific data for region scanning
            /// </summary>
            private struct RegionScannerContext
            {
                #region Fields

                #region Internal Fields

                internal int CurrentY;
                internal ArraySection<byte> ScanlineBuffer;
                internal bool IsScanlineDirty;

                #endregion

                #region Private Fields

                private readonly SolidRegionScanner scanner;
                private readonly ActiveEdgeTable activeEdges;
                private readonly int scanlinePixelWidth;

                private int yStartIndex;
                private int yEndIndex;
                private int rowStartMin;
                private int rowEndMax;
                private int currentVisitedY;

                #endregion

                #endregion

                #region Properties

                internal int StartX => Math.Max(0, rowStartMin);
                internal int EndX => Math.Min(scanner.Width - 1, rowEndMax);

                #endregion

                #region Constructors

                internal RegionScannerContext(SolidRegionScanner scanner, ActiveEdgeTable activeEdges)
                {
                    this.scanner = scanner;
                    this.activeEdges = activeEdges;
                    scanlinePixelWidth = scanner.Width;
                    ScanlineBuffer = new byte[KnownPixelFormat.Format1bppIndexed.GetByteWidth(scanlinePixelWidth)]; // TODO: allocate

                    if (scanner.edges.Length == 0)
                        return;

                    currentVisitedY = (int)scanner.edges[scanner.sortedIndexYStart[0]].YStart;
                }

                internal RegionScannerContext(in RegionScannerContext other, int top)
                {
                    Debug.Assert(top > other.CurrentY);
                    this = other;
                    activeEdges = other.activeEdges.Clone();
                    ScanlineBuffer = new byte[other.ScanlineBuffer.Length]; // TODO: allocate
                    IsScanlineDirty = false;
                    SkipEdgesAbove(top);
                }

                #endregion

                #region Methods

                #region Internal Methods

                internal void SkipEdgesAbove(int rowIndex)
                {
                    CurrentY = rowIndex - 1;
                    var edges = scanner.edges;
                    var sortedIndexYStart = scanner.sortedIndexYStart;
                    var sortedIndexYEnd = scanner.sortedIndexYEnd;

                    int startIndex = yStartIndex + 1;
                    int endIndex = yEndIndex;

                    while (currentVisitedY < rowIndex)
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
                            currentVisitedY = (int)startY;
                            startIndex += 1;
                            continue;
                        }

                        currentVisitedY = (int)endY;
                        endIndex += 1;
                    }
                }

                internal bool MoveNextRow()
                {
                    CurrentY += 1;
                    currentVisitedY = CurrentY;
                    if (CurrentY >= scanner.Bottom)
                        return false;

                    if (IsScanlineDirty)
                        Array.Clear(ScanlineBuffer.UnderlyingArray!, ScanlineBuffer.Offset, ScanlineBuffer.Length);
                    IsScanlineDirty = false;

                    rowStartMin = Int32.MaxValue;
                    rowEndMax = Int32.MinValue;
                    VisitEdges();

                    return true;
                }

                internal void ScanCurrentRow()
                {
                    ArraySection<float> points = activeEdges.ScanLine(currentVisitedY, scanner.edges);
                    if (points.Length == 0)
                        return;

                    float minX = scanner.Left;
                    for (int point = 0; point < points.Length - 1; point += 2)
                    {
                        float scanStart = points[point] - minX;
                        float scanEnd = points[point + 1] - minX;
                        int startX = (int)MathF.Floor(scanStart);
                        int endX = (int)MathF.Floor(scanEnd);

                        if (startX >= 0 && startX < scanlinePixelWidth)
                        {
                            if (startX + 1 - scanStart >= 0.5f)
                            {
                                ColorExtensions.Set1bppColorIndex(ref ScanlineBuffer.GetElementReference(startX >> 3), startX, 1);
                                IsScanlineDirty = true;
                            }
                        }

                        if (endX >= 0 && endX < scanlinePixelWidth)
                        {
                            if (scanEnd - endX >= 0.5f || endX == startX + 1 && scanEnd - scanStart >= 0.5f)
                            {
                                ColorExtensions.Set1bppColorIndex(ref ScanlineBuffer.GetElementReference(endX >> 3), endX, 1);
                                IsScanlineDirty = true;
                            }
                        }

                        int nextX = startX + 1;
                        endX = Math.Min(endX, scanlinePixelWidth);
                        nextX = Math.Max(nextX, 0);

                        if (endX > nextX)
                        {
                            // TODO: vectorization if possible - or at least combine byte changes
                            for (int i = nextX; i < endX; i++)
                                ColorExtensions.Set1bppColorIndex(ref ScanlineBuffer.GetElementReference(i >> 3), i, 1);
                            IsScanlineDirty = true;
                        }

                        rowStartMin = Math.Min(rowStartMin, startX);
                        rowEndMax = Math.Max(rowEndMax, endX);
                    }
                }

                #endregion

                #region Private Methods

                private void VisitEdges()
                {
                    var edges = scanner.edges;
                    var sortedIndexYStart = scanner.sortedIndexYStart;
                    var sortedIndexYEnd = scanner.sortedIndexYEnd;
                    while (yStartIndex < sortedIndexYStart.Length)
                    {
                        int i = sortedIndexYStart[yStartIndex];
                        if (edges[i].YStart > currentVisitedY)
                            break;

                        activeEdges.EnterEdge(i);
                        yStartIndex += 1;
                    }

                    while (yEndIndex < sortedIndexYEnd.Length)
                    {
                        int i = sortedIndexYEnd[yEndIndex];
                        if (edges[i].YEnd > currentVisitedY)
                            break;

                        activeEdges.LeaveEdge(i);
                        yEndIndex += 1;
                    }
                }

                #endregion

                #endregion
            }

            #endregion

            #region Constants

            private const float roundingUnit = 0.25f;
            private const int parallelThreshold = 256;

            #endregion

            #region Fields

            private readonly int[] sortedIndexYStart;
            private readonly int[] sortedIndexYEnd;
            private readonly StrongBox<(int ThreadId, RegionScannerContext Context)>?[]? threadContextCache;
            private readonly int hashMask;

            private RegionScannerContext mainContext;
            private ArraySection<EdgeEntry> edges;

            #endregion

            #region Properties

            internal override bool IsSingleThreaded => threadContextCache == null;

            #endregion

            #region Constructors

            public SolidRegionScanner(IAsyncContext context, RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
                : base(path, bounds, drawingOptions)
            {
                edges = new EdgeTable(path, roundingUnit).Edges;
                int edgeCount = edges.Length;
                var activeEdges = ActiveEdgeTable.Create(drawingOptions.FillMode, edgeCount, path.TotalVertices);

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
                mainContext = new RegionScannerContext(this, activeEdges);
                mainContext.SkipEdgesAbove(bounds.Top);

                if (bounds.Width < parallelThreshold || context.MaxDegreeOfParallelism == 1 || EnvironmentHelper.CoreCount == 1)
                    return;

                threadContextCache = new StrongBox<(int ThreadId, RegionScannerContext)>?[EnvironmentHelper.GetThreadBasedCacheSize(context.MaxDegreeOfParallelism)];
                hashMask = threadContextCache.Length - 1;
            }

            #endregion

            #region Methods
            
            #region Internal Methods

            internal override void ProcessNextScanline(FillPathSession session)
            {
                Debug.Assert(IsSingleThreaded);
                if (!mainContext.MoveNextRow())
                    return;

                mainContext.ScanCurrentRow();

                if (mainContext.IsScanlineDirty)
                    session.ApplyScanlineSolid(new RegionScanline<byte>(mainContext.CurrentY, Left, mainContext.ScanlineBuffer, mainContext.StartX, mainContext.EndX));
            }

            internal override void ProcessScanline(int y, FillPathSession session)
            {
                Debug.Assert(!IsSingleThreaded);

                ref RegionScannerContext context = ref GetThreadContext(y);
                context.MoveNextRow();
                context.ScanCurrentRow();

                if (context.IsScanlineDirty)
                    session.ApplyScanlineSolid(new RegionScanline<byte>(context.CurrentY, Left, context.ScanlineBuffer, context.StartX, context.EndX));
            }

            #endregion

            #region Private Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            private ref RegionScannerContext GetThreadContext(int y)
            {
                // Note that cache item access is not volatile, because it's even better if a thread sees its own last cached value.
                int threadId = EnvironmentHelper.CurrentThreadId;
                int hash = threadId & hashMask;
                StrongBox<(int ThreadId, RegionScannerContext Context)>? cacheEntry = threadContextCache![hash];

                if (cacheEntry != null)
                {
                    ref var value = ref cacheEntry.Value;
                    if (value.ThreadId == threadId)
                    {
                        ref RegionScannerContext context = ref value.Context;
                        if (context.CurrentY < y)
                        {
                            context.SkipEdgesAbove(y);
                            return ref context;
                        }
                    }

                    //value.Context.Dispose(); // TODO: release dropped entry's active edge table buffer and scanline buffer
                }

                var result = new StrongBox<(int ThreadId, RegionScannerContext Context)>((threadId, new RegionScannerContext(mainContext, y)));
                threadContextCache[hash] = result;
                return ref result.Value.Context;
            }

            #endregion

            #endregion
        }

        #endregion

        #region AntiAliasingRegionScanner class

        private class AntiAliasingRegionScanner : RegionScanner
        {
            #region Nested Structs

            /// <summary>
            /// Contains thread-specific data for region scanning
            /// </summary>
            private struct RegionScannerContext
            {
                #region Fields

                #region Internal Fields

                internal int CurrentY;
                internal ArraySection<float> ScanlineBuffer;
                internal bool IsScanlineDirty;

                #endregion

                #region Private Fields

                private readonly AntiAliasingRegionScanner scanner;
                private readonly ActiveEdgeTable activeEdges;

                private int yStartIndex;
                private int yEndIndex;
                private int rowStartMin;
                private int rowEndMax;
                private float nextY;
                private float currentSubpixelY;

                #endregion

                #endregion

                #region Properties

                internal int StartX => Math.Max(0, rowStartMin);
                internal int EndX => Math.Min(ScanlineBuffer.Length - 1, rowEndMax);

                #endregion

                #region Constructors

                internal RegionScannerContext(AntiAliasingRegionScanner scanner, ActiveEdgeTable activeEdges)
                {
                    this.scanner = scanner;
                    this.activeEdges = activeEdges;
                    ScanlineBuffer = new float[scanner.Width]; // TODO: allocate

                    if (scanner.edges.Length == 0)
                        return;

                    currentSubpixelY = scanner.edges[scanner.sortedIndexYStart[0]].YStart;
                }

                internal RegionScannerContext(in RegionScannerContext other, int top)
                {
                    Debug.Assert(top > other.CurrentY);
                    this = other;
                    activeEdges = other.activeEdges.Clone();
                    ScanlineBuffer = new float[other.ScanlineBuffer.Length]; // TODO: allocate
                    IsScanlineDirty = false;
                    SkipEdgesAbove(top);
                }

                #endregion

                #region Methods

                #region Internal Methods

                internal void SkipEdgesAbove(int rowIndex)
                {
                    CurrentY = rowIndex - 1;
                    float top = rowIndex;
                    var edges = scanner.edges;
                    var sortedIndexYStart = scanner.sortedIndexYStart;
                    var sortedIndexYEnd = scanner.sortedIndexYEnd;

                    int startIndex = yStartIndex + 1;
                    int endIndex = yEndIndex;

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
                        endIndex += 1;
                    }
                }

                internal bool MoveNextRow()
                {
                    CurrentY += 1;
                    nextY = CurrentY + 1;
                    currentSubpixelY = CurrentY - scanner.subpixelSize;
                    if (CurrentY >= scanner.Bottom)
                        return false;

                    if (IsScanlineDirty)
                        Array.Clear(ScanlineBuffer.UnderlyingArray!, ScanlineBuffer.Offset, ScanlineBuffer.Length);
                    IsScanlineDirty = false;

                    rowStartMin = Int32.MaxValue;
                    rowEndMax = Int32.MinValue;

                    return true;
                }

                internal bool MoveNextSubpixelRow()
                {
                    currentSubpixelY += scanner.subpixelSize;
                    VisitEdges();
                    return currentSubpixelY < nextY;
                }

                internal void ScanCurrentSubpixelRow()
                {
                    ArraySection<float> points = activeEdges.ScanLine(currentSubpixelY, scanner.edges);
                    if (points.Length == 0)
                        return;

                    float minX = scanner.Left;
                    float subpixelSize = scanner.subpixelSize;
                    float subpixelArea = scanner.subpixelArea;
                    for (int point = 0; point < points.Length - 1; point += 2)
                    {
                        float scanStart = points[point] - minX;
                        float scanEnd = points[point + 1] - minX;
                        int startX = (int)MathF.Floor(scanStart);
                        int endX = (int)MathF.Floor(scanEnd);

                        if (startX >= 0 && startX < ScanlineBuffer.Length)
                        {
                            float subpixelWidth = (startX + 1 - scanStart) / subpixelSize;
                            ScanlineBuffer[startX] += subpixelWidth * subpixelArea;
                            IsScanlineDirty |= subpixelWidth > 0;
                        }

                        if (endX >= 0 && endX < ScanlineBuffer.Length)
                        {
                            float subpixelWidth = (scanEnd - endX) / subpixelSize;
                            ScanlineBuffer[endX] += subpixelWidth * subpixelArea;
                            IsScanlineDirty |= subpixelWidth > 0;
                        }

                        int nextX = startX + 1;
                        endX = Math.Min(endX, ScanlineBuffer.Length);
                        nextX = Math.Max(nextX, 0);

                        if (endX > nextX)
                        {
                            // TODO: vectorization if possible
                            for (int i = nextX; i < endX; i++)
                                ScanlineBuffer[i] += subpixelSize;
                            IsScanlineDirty = true;
                        }

                        rowStartMin = Math.Min(rowStartMin, startX);
                        rowEndMax = Math.Max(rowEndMax, endX);
                    }
                }

                #endregion

                #region Private Methods

                private void VisitEdges()
                {
                    var edges = scanner.edges;
                    var sortedIndexYStart = scanner.sortedIndexYStart;
                    var sortedIndexYEnd = scanner.sortedIndexYEnd;
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

                #endregion

                #endregion
            }

            #endregion

            #region Constants

            private const float subpixelCount = 16f;
            private const int parallelThreshold = 64;

            #endregion

            #region Fields

            private readonly float subpixelSize;
            private readonly float subpixelArea;
            private readonly int[] sortedIndexYStart;
            private readonly int[] sortedIndexYEnd;
            private readonly StrongBox<(int ThreadId, RegionScannerContext Context)>?[]? threadContextCache;
            private readonly int hashMask;

            private RegionScannerContext mainContext;
            private ArraySection<EdgeEntry> edges;

            #endregion

            #region Properties

            internal override bool IsSingleThreaded => threadContextCache == null;

            #endregion

            #region Constructors

            public AntiAliasingRegionScanner(IAsyncContext context, RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
                : base(path, bounds, drawingOptions)
            {
                subpixelSize = 1f / subpixelCount;
                subpixelArea = 1f / (subpixelCount * subpixelCount);

                edges = new EdgeTable(path, subpixelSize).Edges;
                int edgeCount = edges.Length;
                var activeEdges = ActiveEdgeTable.Create(drawingOptions.FillMode, edgeCount, path.TotalVertices);

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
                mainContext = new RegionScannerContext(this, activeEdges);
                mainContext.SkipEdgesAbove(bounds.Top);

                if (bounds.Width < parallelThreshold || context.MaxDegreeOfParallelism == 1 || EnvironmentHelper.CoreCount == 1)
                    return;

                threadContextCache = new StrongBox<(int ThreadId, RegionScannerContext)>?[EnvironmentHelper.GetThreadBasedCacheSize(context.MaxDegreeOfParallelism)];
                hashMask = threadContextCache.Length - 1;
            }

            #endregion

            #region Methods

            #region Internal Methods

#if DEBUG
            // TODO: remove
            private SortedList<int, string> debugInfo = new();
            private static string Dump(object? o)
            {
                if (o == null)
                    return "<null>";

                if (o is IConvertible convertible)
                    return convertible.ToString(CultureInfo.InvariantCulture);

                if (o is ActiveEdgeTable t)
                    o = Reflector.GetProperty(t, "ActiveEdges");

                if (o is IEnumerable enumerable)
                    return $"[{enumerable.Cast<object>().Select(Dump).Join(", ")}]";

                return $"{{{o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => f.Name != "scanner")
                    .Select(f => $"{f.Name} = {Dump(f.GetValue(o))}")
                    .Join(", ")}}}";
            }
#endif

            internal override void ProcessNextScanline(FillPathSession session)
            {
                Debug.Assert(IsSingleThreaded);
                if (!mainContext.MoveNextRow())
                    return;

                while (mainContext.MoveNextSubpixelRow())
                    mainContext.ScanCurrentSubpixelRow();
#if DEBUG
                // TODO: remove
                debugInfo.Add(mainContext.CurrentY, Dump(mainContext));
                if (mainContext.CurrentY == Bottom - 1)
                    File.WriteAllLines(@"D:\temp\Images\ref\ref.txt", debugInfo.Select(l => $"{l.Key}:{l.Value}"));
#endif

                if (mainContext.IsScanlineDirty)
                    session.ApplyScanlineAntiAliasing(new RegionScanline<float>(mainContext.CurrentY, Left, mainContext.ScanlineBuffer, mainContext.StartX, mainContext.EndX));
            }

            internal override void ProcessScanline(int y, FillPathSession session)
            {
                Debug.Assert(!IsSingleThreaded);

                ref RegionScannerContext context = ref GetThreadContext(y);
                context.MoveNextRow();

                while (context.MoveNextSubpixelRow())
                    context.ScanCurrentSubpixelRow();

#if DEBUG
                // TODO: remove
                lock (debugInfo)
                {
                    if (debugInfo.Count == 0)
                    {
                        foreach (var line in File.ReadLines(@"D:\temp\Images\ref\ref.txt"))
                        {
                            int pos = line.IndexOf(':');
                            debugInfo.Add(Int32.Parse(line.Substring(0, pos), CultureInfo.InvariantCulture), line.Substring(pos + 1));
                        }
                    } 
                }

                string expected = debugInfo[context.CurrentY];
                string actual = Dump(context);
                Debug.Assert(expected == actual, $"Expected vs actual:{Environment.NewLine}{expected}{Environment.NewLine}{actual}");
#endif

                if (context.IsScanlineDirty)
                    session.ApplyScanlineAntiAliasing(new RegionScanline<float>(context.CurrentY, Left, context.ScanlineBuffer, context.StartX, context.EndX));
            }

            #endregion

            #region Private Methods
            
            [MethodImpl(MethodImpl.AggressiveInlining)]
            private ref RegionScannerContext GetThreadContext(int y)
            {
                // Note that cache item access is not volatile, because it's even better if a thread sees its own last cached value.
                int threadId = EnvironmentHelper.CurrentThreadId;
                int hash = threadId & hashMask;
                StrongBox<(int ThreadId, RegionScannerContext Context)>? cacheEntry = threadContextCache![hash];

                if (cacheEntry != null)
                {
                    ref var value = ref cacheEntry.Value;
                    if (value.ThreadId == threadId)
                    {
                        ref RegionScannerContext context = ref value.Context;
                        if (context.CurrentY < y)
                        {
                            context.SkipEdgesAbove(y);
                            return ref context;
                        }
                    }

                    //value.Context.Dispose(); // TODO: release dropped entry's active edge table buffer and scanline buffer
                }

                var result = new StrongBox<(int ThreadId, RegionScannerContext Context)>((threadId, new RegionScannerContext(mainContext, y)));
                threadContextCache[hash] = result;
                return ref result.Value.Context;
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

            private readonly (int Index, Flags Flags)[] activeEdges;

            #endregion

            #region Private Protected Fields

            #region Private Protected Fields

            private protected ArraySection<float> Intersections;
            private protected int Count;

            #endregion

            #endregion

            #endregion

            #region Properties

            private protected ArraySection<(int Index, Flags Flags)> ActiveEdges => activeEdges.AsSection(0, Count);

            #endregion

            #region Constructors

            protected ActiveEdgeTable(int edgeCount, int maxIntersectionCount)
            {
                activeEdges = new (int, Flags)[edgeCount];
                Count = 0;
                Intersections = new float[maxIntersectionCount];
            }

            protected ActiveEdgeTable(ActiveEdgeTable other)
                : this(other.activeEdges.Length, other.Intersections.UnderlyingArray.Length)
            {
                Count = other.Count;
                //Array.Copy(other.activeEdges, 0, activeEdges, 0, Count); // TODO
                other.activeEdges.CopyTo(activeEdges, 0);
                other.Intersections.CopyTo(Intersections);
            }

            #endregion

            #region Methods

            #region Static Methods

            internal static ActiveEdgeTable Create(ShapeFillMode fillMode, int edgeCount, int maxIntersectionCount) => fillMode switch
            {
                ShapeFillMode.Alternate => new ActiveEdgeTableAlternate(edgeCount, maxIntersectionCount),
                ShapeFillMode.NonZero => new ActiveEdgeTableNonZero(edgeCount, maxIntersectionCount),
                _ => throw new ArgumentOutOfRangeException(nameof(fillMode), PublicResources.EnumOutOfRange(fillMode))
            };

            #endregion

            #region Instance Methods

            internal void EnterEdge(int enterIndex)
            {
                ref var entry = ref activeEdges[Count];
                entry.Index = enterIndex;
                entry.Flags = Flags.Entering;
                Count += 1;
            }

            internal void LeaveEdge(int leaveIndex)
            {
                var table = activeEdges;
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
                var table = activeEdges;
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

            internal abstract ArraySection<float> ScanLine(float y, ArraySection<EdgeEntry> edges);

            internal abstract ActiveEdgeTable Clone();

            #endregion

            #endregion
        }

        #endregion

        #region ActiveEdgeTableAlternate class

        private sealed class ActiveEdgeTableAlternate : ActiveEdgeTable
        {
            #region Constructors

            #region Internal Constructors

            internal ActiveEdgeTableAlternate(int edgeCount, int maxIntersectionCount)
                : base(edgeCount, maxIntersectionCount)
            {
            }

            #endregion

            #region Private Constructors
            
            private ActiveEdgeTableAlternate(ActiveEdgeTableAlternate other)
                : base(other)
            {
            }

            #endregion

            #endregion

            #region Methods

            internal override ArraySection<float> ScanLine(float y, ArraySection<EdgeEntry> edges)
            {
                #region Local Methods

                static void AddIntersection(float x, bool emit, ArraySection<float> intersections, ref int count)
                {
                    if (!emit)
                        return;

                    intersections[count] = x;
                    count += 1;
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
                        AddIntersection(x, edge.EmitStart, Intersections, ref intersectionsCount);
                    else if ((entry.Flags & Flags.Leaving) != Flags.None)
                    {
                        AddIntersection(x, edge.EmitEnd, Intersections, ref intersectionsCount);
                        removed += 1;
                        continue;
                    }
                    else
                    {
                        Intersections[intersectionsCount] = x;
                        intersectionsCount += 1;
                    }

                    activeEdges[i - removed] = (entry.Index, Flags.None);
                }

                Count -= removed;
                Array.Sort(Intersections.UnderlyingArray!, 0, intersectionsCount);
                return Intersections.Slice(0, intersectionsCount);
            }

            internal override ActiveEdgeTable Clone() => new ActiveEdgeTableAlternate(this);

            #endregion
        }

        #endregion

        #region ActiveEdgeTableNonZero class

        private sealed class ActiveEdgeTableNonZero : ActiveEdgeTable
        {
            #region Nested Types

            private enum IntersectionType : byte
            {
                Ascending,
                Descending,
                Corner
            }

            #endregion
            
            #region Constants

            /// <summary>
            /// A constant, that is smaller than snapping factor for subpixel size, used to prevent sorting from mixing up elements with the same key.
            /// Its value is smaller than subpixel snapping distance (actually the same as equality tolerance for interpolated resizing).
            /// </summary>
            private const float sortStabilizerDelta = 1e-4f;

            #endregion

            #region Fields

            private ArraySection<IntersectionType> intersectionTypes;

            #endregion

            #region Constructors

            #region Internal Constructors

            internal ActiveEdgeTableNonZero(int edgeCount, int maxIntersectionCount)
                : base(edgeCount, maxIntersectionCount)
            {
                intersectionTypes = new IntersectionType[maxIntersectionCount];
            }

            #endregion

            #region Private Constructors

            private ActiveEdgeTableNonZero(ActiveEdgeTableNonZero other)
                : base(other)
            {
                intersectionTypes = new IntersectionType[other.intersectionTypes.Length];
                other.intersectionTypes.CopyTo(intersectionTypes);
            }

            #endregion

            #endregion

            #region Methods

            #region Static Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            private static void AddIntersection(float x, bool emit, bool isAscending, ArraySection<float> intersections, ArraySection<IntersectionType> intersectionTypes, ref int count)
            {
                if (!emit)
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

            internal override ArraySection<float> ScanLine(float y, ArraySection<EdgeEntry> edges)
            {
                int intersectionsCount = 0;
                int removed = 0;
                var activeEdges = ActiveEdges;
                var intersections = Intersections;
                var types = intersectionTypes;
                int len = activeEdges.Length;
                for (int i = 0; i < len; i++)
                {
                    ref var entry = ref activeEdges.GetElementReference(i);
                    ref var edge = ref edges.GetElementReference(entry.Index);
                    float x = edge.GetX(y);
                    if ((entry.Flags & Flags.Entering) != Flags.None)
                        AddIntersection(x, edge.EmitStart, edge.IsAscending, intersections, types, ref intersectionsCount);
                    else if ((entry.Flags & Flags.Leaving) != Flags.None)
                    {
                        AddIntersection(x, edge.EmitEnd, edge.IsAscending, intersections, types, ref intersectionsCount);
                        removed += 1;
                        continue;
                    }
                    else
                    {
                        if (edge.IsAscending)
                        {
                            types[intersectionsCount] = IntersectionType.Ascending;
                            intersections[intersectionsCount] = x + sortStabilizerDelta;
                        }
                        else
                        {
                            types[intersectionsCount] = IntersectionType.Descending;
                            intersections[intersectionsCount] = x - sortStabilizerDelta;
                        }

                        intersectionsCount += 1;
                    }

                    activeEdges[i - removed] = (entry.Index, Flags.None);
                }

                Count -= removed;
                Array.Sort(intersections.UnderlyingArray!, types.UnderlyingArray, 0, intersectionsCount);
                intersections = intersections.Slice(0, intersectionsCount);
                types = types.Slice(0, intersectionsCount);

                // Counting the winding number and applying the nonzero winding rule. See details here: https://en.wikipedia.org/wiki/Point_in_polygon#Winding_number_algorithm
                removed = 0;
                int windingNumber = 0;
                len = types.Length;
                for (int i = 0; i < len; i++)
                {
                    IntersectionType type = types[i];
                    switch (type)
                    {
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

            internal override ActiveEdgeTable Clone() => new ActiveEdgeTableNonZero(this);

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

            public EdgeTable(RawPath path, float roundingUnit)
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
                        snappedYCoords[i] = vertices[i].Y.RoundTo(roundingUnit);

                    enumerator.StartNextFigure(new EdgeInfo(vertices, snappedYCoords, vertices.Length - 2),
                        new EdgeInfo(vertices, snappedYCoords, 0));

                    enumerator.MoveNextEdge(false, new EdgeInfo(vertices, snappedYCoords, 1));

                    for (int i = 1; i < vertices.Length - 2; i++)
                        enumerator.MoveNextEdge(true, new EdgeInfo(vertices, snappedYCoords, i + 1));

                    // 1st edge
                    enumerator.MoveNextEdge(true, new EdgeInfo(vertices, snappedYCoords, 0));

                    // 2nd edge
                    enumerator.MoveNextEdge(true, default);
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

            internal readonly EdgeKind Kind;

            internal PointF Start;
            internal PointF End;
            internal bool EmitStart;
            internal bool EmitEnd;

            #endregion

            #region Constructors

            [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator",
                Justification = "False alarm, these coordinates are already snapped (rounded) to a fraction of 2.")]
            internal EdgeInfo(PointF[] vertices, float[] snappedYCoords, int index)
            {
                Start = new PointF(vertices[index].X, snappedYCoords[index]);
                End = new PointF(vertices[index + 1].X, snappedYCoords[index + 1]);
                Kind = Start.Y == End.Y
                    ? Start.X < End.X ? EdgeKind.HorizontalRight : EdgeKind.HorizontalLeft
                    : Start.Y < End.Y ? EdgeKind.Descending : EdgeKind.Ascending;
                EmitStart = false;
                EmitEnd = false;
            }

            #endregion

            #region Methods

            #region Static Methods

            internal static void ConfigureEdgeRelation(ref EdgeInfo edge1, ref EdgeInfo edge2)
            {
                // A vertex can be emitted at be beginning and/or at the end of the edge, depending on the relation of its edges.
                // For example, not emitting a vertex at all if the edges are collinear, or when the another end is already emitted.
                // This configuration ensures that relevant vertices are emitted exactly once in any configuration.
                switch ((edge1.Kind, edge2.Kind))
                {
                    case (EdgeKind.Ascending, EdgeKind.Ascending):
                        edge2.EmitStart = true;
                        break;
                    case (EdgeKind.Ascending, EdgeKind.Descending):
                        edge1.EmitEnd = true;
                        edge2.EmitStart = true;
                        break;
                    case (EdgeKind.Ascending, EdgeKind.HorizontalLeft):
                        edge1.EmitEnd = true;
                        break;
                    case (EdgeKind.Ascending, EdgeKind.HorizontalRight):
                        edge1.EmitEnd = true;
                        break;

                    case (EdgeKind.Descending, EdgeKind.Ascending):
                        edge1.EmitEnd = true;
                        edge2.EmitStart = true;
                        break;
                    case (EdgeKind.Descending, EdgeKind.Descending):
                        edge2.EmitStart = true;
                        break;

                    case (EdgeKind.HorizontalLeft, EdgeKind.Descending):
                        edge2.EmitStart = true;
                        break;
                    case (EdgeKind.HorizontalRight, EdgeKind.Descending):
                        edge2.EmitStart = true;
                        break;
                }
            }

            #endregion

            #region Instance Methods

            #region Public Methods

            public override string ToString() => $"{Start} -> {End} ({Kind}) - Emit start/end: [{EmitStart};{EmitEnd}]";

            #endregion

            #region Internal Methods

            internal EdgeEntry ToEdge()
            {
                bool isAscending = Kind == EdgeKind.Ascending;
                if (isAscending)
                {
                    (Start, End) = (End, Start);
                    (EmitStart, EmitEnd) = (EmitEnd, EmitStart);
                }

                return new EdgeEntry(this);
            }

            #endregion

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
            internal readonly bool EmitStart;
            internal readonly bool EmitEnd;
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
                EmitStart = edgeInfo.EmitStart;
                EmitEnd = edgeInfo.EmitEnd;

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

            #region Public Methods

            public override string ToString() => $"Y: {YStart}->{YEnd} ({(IsAscending ? "" : "non-")}ascending); Emit start/end: [{EmitStart}; {EmitEnd}]; p={p}; q={q}";

            #endregion

            #region Internal Methods

            internal float GetX(float y) => p * y + q;

            #endregion

            #endregion
        }

        #endregion

        #region EdgeEnumerator struct

        private ref struct EdgeEnumerator
        {
            #region Fields

            private readonly EdgeEntry[] edges;

            internal int Count;
            private EdgeInfo previous;
            private EdgeInfo current;

            #endregion

            #region Constructors

            internal EdgeEnumerator(EdgeEntry[] edges) => this.edges = edges;

            #endregion

            #region Methods

            internal void StartNextFigure(EdgeInfo previous, EdgeInfo current)
            {
                this.previous = previous;
                this.current = current;
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal void MoveNextEdge(bool addPrevious, EdgeInfo next)
            {
                EdgeInfo.ConfigureEdgeRelation(ref previous, ref current);
                if (addPrevious && previous.Kind is EdgeKind.Ascending or EdgeKind.Descending)
                {
                    edges[Count] = previous.ToEdge();
                    Count += 1;
                }

                previous = current;
                current = next;
            }

            #endregion
        }

        #endregion

        #region RegionScanline<T> struct

        private protected ref struct RegionScanline<T>
        {
            #region Fields

            internal readonly int RowIndex;
            internal readonly int Left;
            internal readonly int MinIndex;
            internal readonly int MaxIndex;
            internal ArraySection<T> Scanline;

            #endregion

            #region Constructors

            internal RegionScanline(int y, int left, ArraySection<T> scanline, int startX, int endX)
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

        private static RegionScanner CreateScanner(IAsyncContext context, RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
        {
            // TODO from cache if possible
            return drawingOptions.AntiAliasing ? new AntiAliasingRegionScanner(context, path, bounds, drawingOptions) : new SolidRegionScanner(context, path, bounds, drawingOptions);
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
            RegionScanner scanner = CreateScanner(context, rawPath, bounds, drawingOptions);

            if (scanner.IsSingleThreaded)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, bounds.Height);
                for (int y = bounds.Top; y < bounds.Bottom; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    scanner.ProcessNextScanline(session);
                    context.Progress?.Increment();
                }

                return;
            }

            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, bounds.Top, bounds.Bottom,
                y => scanner.ProcessScanline(y, session));
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