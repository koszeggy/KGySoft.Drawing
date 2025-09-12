#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Brush.cs
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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
using System.Numerics;
#endif
using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents a brush for shape filling operations. Use the static <c>Create...</c> methods to create an instance.
    /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_Shapes_Brush.htm">online help</a> for image examples.</div>
    /// </summary>
    /// <remarks>
    /// <para>Brushes are used for filling shapes. There are several shape-filling methods in the <see cref="BitmapDataExtensions"/> class, whose names start with <c>Fill</c> or <c>BeginFill</c>.
    /// The ones that have no <see cref="Brush"/> parameter use a solid brush implicitly, created from the specified <see cref="Color32"/> instance.</para>
    /// <para>The <see cref="Brush"/> class is abstract, and you can create instances by using the static <c>Create...</c> methods. The created instances are immutable.</para>
    /// <note>In fact, even a <see cref="Pen"/> uses a <see cref="Brush"/> internally for drawing the outlines of shapes.
    /// You can use the appropriate constructor, or the <see cref="Pen.Brush"/> property to set a brush for a <see cref="Pen"/>.</note>
    /// </remarks>
    /// <example>
    /// <para>The following table enlists the available brushes and their factory methods:
    /// <table class="table is-hoverable"><thead><tr><th width="80%">Description</th><th width="20%">Image Example</th></tr></thead><tbody>
    /// <tr><td><see cref="O:KGySoft.Drawing.Shapes.Brush.CreateSolid">CreateSolid</see>: Creates a solid brush with a single color. <see cref="Color32"/>, <see cref="Color64"/>
    /// and <see cref="ColorF"/> overloads are available, and you can also use the named <see cref="Color"/> members to cast them to <see cref="Color32"/> implicitly.
    /// For example, <c>Brush.CreateSolid(Color32.FromArgb(128, Color.Blue))</c> creates a blue solid brush with 50% transparency that can produce a similar result to the one in the image.</td>
    /// <td><img src="../Help/Images/DrawingOptionsAlphaBlendingEnabledNoAA.png" alt="Polygon filled with 50% transparency, AntiAliasing = false, AlphaBlending = true."/></td></tr>
    /// <tr><td><para><see cref="O:KGySoft.Drawing.Shapes.Brush.CreateLinearGradient">CreateLinearGradient</see>: Creates a linear gradient brush. There are two groups of overloads:
    /// one for specifying a pair of start/end points optionally with some <see cref="GradientWrapMode"/>, and another one with an angle, which automatically stretches the gradient for the paths to fill.</para>
    /// <para>The top image uses specific start/end points without repeating the gradient, whereas the bottom one just specified a zero angle to create a horizontal gradient.
    /// Note that in the bottom image the start/end points are automatically adjusted to the bounds of the shape in each fill sessions. Both examples filled a single elliptic path with three different translations.</para>
    /// <para>See the <strong>Examples</strong> section of the <see cref="GradientWrapMode"/> enumeration and the <see cref="CreateLinearGradient(float,Color32,Color32,WorkingColorSpace)"/> method for more image examples.</para></td>
    /// <td><img src="../Help/Images/BrushLinearGradientWrapModeStop.png" alt="Linear gradient brush with start/end points"/>
    /// <br/><img src="../Help/Images/BrushLinearGradientAngle.png" alt="Linear gradient brush with an angle"/></td></tr>
    /// <tr><td><para><see cref="O:KGySoft.Drawing.Shapes.Brush.CreateTexture">CreateTexture</see>: Creates a texture brush. A <see cref="TextureMapMode"/> can be specified, which allows tiling, stretching, centering and a few other modes.</para>
    /// <para>The top image uses a texture with a map mode of tiling with mirroring on both axes, whereas the bottom one centers the same texture and uses no alpha blending, so where there is no texture pixel to set,
    /// the path region cuts a transparent hole in the image. Both examples filled a single elliptic path with three different translations.</para>
    /// <para>See the <strong>Examples</strong> section of the <see cref="TextureMapMode"/> enumeration for more image examples.</para></td>
    /// <td><img src="../Help/Images/BrushTextureTileFlipXY.png" alt="Texture brush with TileFlipXY map mode"/>
    /// <br/><img src="../Help/Images/BrushTextureCenter.png" alt="Texture brush with Center map mode"/></td></tr>
    /// </tbody></table></para>
    /// </example>
    public abstract class Brush
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

        #region PathSessionBase class

        private protected abstract class PathSessionBase : IDisposable
        {
            #region Properties

            #region Internal Properties
            
            internal IAsyncContext Context { get; }
            internal DrawingOptions DrawingOptions { get; }
            internal Rectangle VisibleBounds { get; }
            internal Region? Region { get; }
            internal RawPath RawPath { get; }

            #endregion

            #region Protected Properties

            protected Brush Owner { get; }
            protected IBitmapDataInternal BitmapData { get; }
            protected WorkingColorSpace WorkingColorSpace { get; set; }
            protected bool Blend { get; }
            protected float PixelOffset { get; }

            #endregion

            #endregion

            #region Constructors

            protected PathSessionBase(Brush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions options, Region? region)
            {
                Owner = owner;
                Context = context;
                BitmapData = (bitmapData as IBitmapDataInternal) ?? new BitmapDataWrapper(bitmapData, true, true);
                RawPath = rawPath;
                DrawingOptions = options;
                VisibleBounds = bounds;
                Region = region;
                WorkingColorSpace = bitmapData.GetPreferredColorSpace();
                Blend = options.AlphaBlending && (owner.HasAlpha || options.AntiAliasing);
                PixelOffset = DrawingOptions.DrawPathPixelOffset == Shapes.PixelOffset.Half ? 0.5f : 0f;
            }

            #endregion

            #region Methods

            #region Public Methods

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Internal Methods

            /// <summary>
            /// Completing the session if it wasn't canceled and there were no errors. Unlike Dispose, this executes only on success.
            /// </summary>
            internal virtual void FinalizeSession()
            {
            }

            #endregion

            #region Protected Methods

            protected virtual void Dispose(bool disposing)
            {
            }

            #endregion

            #endregion
        }

        #endregion

        #region FillPathSession class

        private protected abstract class FillPathSession : PathSessionBase
        {
            #region Properties

            internal virtual bool IsSingleThreaded => false;

            #endregion

            #region Constructors

            protected FillPathSession(Brush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions options, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, options, region)
            {
            }

            #endregion

            #region Methods

            [SecuritySafeCritical]internal abstract void ApplyScanlineAntiAliasing(in RegionScanline scanline);
            [SecuritySafeCritical]internal abstract void ApplyScanlineSolid(in RegionScanline scanline);

            #endregion
        }

        #endregion

        #region RegionScanner class

        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local",
            Justification = "Rectangles: Preventing creating defensive copies on older platforms where the properties are not readonly.")]
        private abstract class RegionScanner : IDisposable
        {
            #region Fields

            private ArraySection<byte> primaryBuffer; // allocated by the known number of vertices
            private ArraySection<byte> secondaryBuffer; // allocated after determining the number of edges

            private Rectangle generateBounds;
            private Rectangle visibleBounds;

            #endregion

            #region Properties

            #region Internal Properties

            internal abstract bool IsSingleThreaded { get; }
            internal Region? Region => Session.Region;

            #endregion

            #region Protected Properties

            protected FillPathSession Session { get; }
            protected CastArray<byte, int> SortedIndexYStart { get; }
            protected CastArray<byte, int> SortedIndexYEnd { get; }
            protected int Top => generateBounds.Top;
            protected int Bottom => generateBounds.Bottom;
            protected int Left => generateBounds.Left;
            protected int Right => generateBounds.Right;
            protected int Width => generateBounds.Width;
            protected int VisibleTop => visibleBounds.Top;
            protected int VisibleBottom => visibleBounds.Bottom;
            protected int VisibleLeft => visibleBounds.Left;
            protected int VisibleWidth => visibleBounds.Width;
            protected CastArray<byte, EdgeEntry> Edges { get; }

            #endregion

            #endregion

            #region Constructors

            protected unsafe RegionScanner(FillPathSession session, float roundingUnit)
            {
                #region Local Methods

                static int GetActiveTableBufferSize(ShapeFillMode fillMode, int edgeCount, int maxIntersectionCount)
                    => sizeof((int, byte)) * edgeCount // active edges
                        + sizeof(float) * maxIntersectionCount // intersections
                        + (fillMode is ShapeFillMode.NonZero ? sizeof(byte) * maxIntersectionCount : 0); // intersection types

                #endregion

                Session = session;
                RawPath path = session.RawPath;
                visibleBounds = session.VisibleBounds;
                generateBounds = session.Region == null ? visibleBounds : path.Bounds;
                int len = sizeof(EdgeEntry) * path.TotalVertices
                    + sizeof(float) * (path.MaxVertices + 1);
                primaryBuffer = new ArraySection<byte>(len, false);

                Edges = new EdgeTable(primaryBuffer, path, roundingUnit).Edges;
                int edgeCount = Edges.Length;

                len = sizeof(int) * 2 * edgeCount
                    + Math.Max(sizeof(float) * 2 * edgeCount, GetActiveTableBufferSize(session.DrawingOptions.FillMode, edgeCount, path.TotalVertices)); // Max: the floats are needed locally only, then they can be re-used
                var buffer = secondaryBuffer = new ArraySection<byte>(len, false);
                var sortedIndexYStart = buffer.Allocate<int>(edgeCount);
                var sortedIndexYEnd = buffer.Allocate<int>(edgeCount);
                var sortedIndexYStartKeys = buffer.Allocate<float>(edgeCount);
                var sortedIndexYEndKeys = buffer.Allocate<float>(edgeCount);
                for (int i = 0; i < edgeCount; i++)
                {
                    ref EdgeEntry edge = ref Edges.GetElementReference(i);
                    sortedIndexYStartKeys[i] = edge.YStart;
                    sortedIndexYEndKeys[i] = edge.YEnd;
                    sortedIndexYStart[i] = sortedIndexYEnd[i] = i;
                }

                ParallelHelper.Sort(session.Context, sortedIndexYStartKeys, sortedIndexYStart);
                ParallelHelper.Sort(session.Context, sortedIndexYEndKeys, sortedIndexYEnd);
                SortedIndexYStart = sortedIndexYStart;
                SortedIndexYEnd = sortedIndexYEnd;
            }

            protected ArraySection<byte> GetActiveTableBuffer() => secondaryBuffer.Slice(sizeof(int) * 2 * Edges.Length);

            #endregion

            #region Methods

            #region Public Methods

            public virtual void Dispose()
            {
                // Not the usual dispose pattern to make it a bit more performant,
                // and because we know that we don't need protection against multiple disposals, etc.
                primaryBuffer.Release();
                secondaryBuffer.Release();
            }

            #endregion

            #region Internal Methods

            internal abstract void ProcessScanline(int y);
            internal abstract void ProcessNextScanline();

            #endregion

            #endregion
        }

        #endregion

        #region SolidRegionScanner class

        private sealed class SolidRegionScanner : RegionScanner
        {
            #region Nested Structs

            /// <summary>
            /// Contains thread-specific data for region scanning
            /// </summary>
            private struct SolidScannerContext : IDisposable
            {
                #region Fields

                #region Internal Fields

                internal int CurrentY;
                internal ArraySection<byte> ScanlineBuffer;

                #endregion

                #region Private Fields

                private readonly SolidRegionScanner scanner;
                private readonly ActiveEdgeTable activeEdges;
                private readonly int scanlinePixelWidth;
                private readonly float pixelOffset;

                private bool isScanlineDirty;
                private int yStartIndex;
                private int yEndIndex;
                private int rowStartMin;
                private int rowEndMax;
                private float currentYf;

                #endregion

                #endregion

                #region Properties

                internal int StartX => Math.Max(scanner.VisibleLeft, rowStartMin) - scanner.Left;
                internal int EndX => Math.Min(scanner.VisibleLeft + scanner.VisibleWidth - 1, rowEndMax) - scanner.Left;

                internal bool IsVisibleScanlineDirty
                {
                    get
                    {
                        if (scanner.Region != null)
                        {
                            if (CurrentY < scanner.VisibleTop || CurrentY >= scanner.VisibleBottom || StartX > EndX)
                                return false;
                        }

                        return isScanlineDirty;
                    }
                }

                #endregion

                #region Constructors

                internal SolidScannerContext(SolidRegionScanner scanner, ActiveEdgeTable activeEdges)
                {
                    this.scanner = scanner;
                    this.activeEdges = activeEdges;
                    scanlinePixelWidth = scanner.Width;
                    pixelOffset = scanner.pixelOffset;

                    // When there is a region, MoveNextRow will take a row from region mask. Otherwise, allocating one row.
                    if (scanner.Region == null)
                        ScanlineBuffer = new ArraySection<byte>(KnownPixelFormat.Format1bppIndexed.GetByteWidth(scanlinePixelWidth));

                    if (scanner.Edges.Length == 0)
                        return;

                    currentYf = scanner.Edges[scanner.SortedIndexYStart[0]].YStart;
                }

                internal SolidScannerContext(in SolidScannerContext other, int top)
                {
                    Debug.Assert(top > other.CurrentY);
                    this = other;
                    activeEdges = other.activeEdges.Clone();

                    // not pooling from here because colliding cache items might be overwritten while they are still in use
                    ScanlineBuffer = scanner.Region is Region region
                        ? region.Mask[top - scanner.Top]
                        : new byte[other.ScanlineBuffer.Length];
                    isScanlineDirty = false;
                    currentYf = other.currentYf;
                    SkipEdgesAbove(top + pixelOffset, true);
                }

                #endregion

                #region Methods

                #region Public Methods

                public void Dispose()
                {
                    // Not quite the usual Dispose pattern, which is intended because this class is used internally only.
                    // Skipping if this is a default instance or when we have a region so scanline is not self allocated.
                    if (scanner == null || scanner.Region != null)
                        return;

                    ScanlineBuffer.Release();
                }

                #endregion

                #region Internal Methods

                internal void SkipEdgesAbove(float top, bool isInitializing)
                {
                    if (isInitializing)
                        CurrentY = (int)MathF.Floor(top) - 1;
                    if (currentYf >= top)
                        return;

                    var edges = scanner.Edges;
                    var sortedIndexYStart = scanner.SortedIndexYStart;
                    var sortedIndexYEnd = scanner.SortedIndexYEnd;

                    int startIndex = yStartIndex + 1;
                    int endIndex = yEndIndex;
                    bool visitEdges = isInitializing;

                    do
                    {
                        if (visitEdges)
                        {
                            VisitEdges();
                            activeEdges.RemoveLeavingEdges();
                        }

                        bool startFinished = startIndex >= sortedIndexYStart.Length;
                        bool endFinished = endIndex >= sortedIndexYEnd.Length;
                        if (startFinished && endFinished)
                            return;

                        float startY = startFinished ? Single.MaxValue : edges[sortedIndexYStart[startIndex]].YStart;
                        float endY = endFinished ? Single.MaxValue : edges[sortedIndexYEnd[endIndex]].YEnd;

                        if (startY < endY)
                        {
                            visitEdges = startY > currentYf;
                            currentYf = startY;
                            startIndex += 1;
                            continue;
                        }

                        visitEdges = endY > currentYf;
                        currentYf = endY;
                        endIndex += 1;
                    } while (currentYf < top);
                }

                internal bool MoveNextRow()
                {
                    CurrentY += 1;
                    if (CurrentY >= scanner.Bottom)
                        return false;

                    if (scanner.Region is Region region)
                        ScanlineBuffer = region.Mask[CurrentY - scanner.Top];
                    else if (isScanlineDirty)
                        ScanlineBuffer.Clear();
                    isScanlineDirty = false;

                    rowStartMin = Int32.MaxValue;
                    rowEndMax = Int32.MinValue;

                    // Skipping edges is needed if roundingUnit < 1 because edges may dis/appear between two pixel lines
                    float nextY = CurrentY + pixelOffset;
                    if (currentYf < nextY)
                        SkipEdgesAbove(nextY, false);

                    currentYf = nextY;
                    VisitEdges();

                    return true;
                }

                internal void ScanCurrentRow()
                {
                    CastArray<byte, float> points = activeEdges.ScanLine(currentYf, scanner.Edges);
                    if (points.Length == 0)
                        return;

                    int left = scanner.Left;
                    int right = scanner.Right;
                    for (int point = 0; point < points.Length - 1; point += 2)
                    {
                        float scanStart = points.GetElementUnsafe(point);
                        float scanEnd = points.GetElementUnsafe(point + 1);
                        if (scanEnd - scanStart < 0.5f)
                            continue;

                        // the first (potentially partial) pixel
                        int startX = (int)MathF.Floor(scanStart);
                        int posX = startX - left;
                        if ((uint)posX < (uint)scanlinePixelWidth && startX + 1 - scanStart >= 0.5f)
                        {
                            ColorExtensions.Set1bppColorIndex(ref ScanlineBuffer.GetElementReferenceUnchecked(posX >> 3), posX, 1);
                            isScanlineDirty = true;
                        }

                        // the last (potentially partial) pixel
                        int endX = (int)MathF.Floor(scanEnd);
                        posX = endX - left;
                        if ((uint)posX < (uint)scanlinePixelWidth && scanEnd - endX >= 0.5f)
                        {
                            ColorExtensions.Set1bppColorIndex(ref ScanlineBuffer.GetElementReferenceUnchecked(posX >> 3), posX, 1);
                            isScanlineDirty = true;
                        }

                        // full pixels between first and last (within the scanned region)
                        int fullStartX = Math.Max(startX + 1, left) - left;
                        int fullEndX = Math.Min(endX - 1, right - 1) - left;
                        if (fullEndX >= fullStartX)
                        {
                            // TODO: vectorization if possible - or at least combine byte changes
                            Debug.Assert(fullStartX >= 0 && fullEndX < scanlinePixelWidth);
                            for (int i = fullStartX; i <= fullEndX; i++)
                                ColorExtensions.Set1bppColorIndex(ref ScanlineBuffer.GetElementReferenceUnchecked(i >> 3), i, 1);
                            isScanlineDirty = true;
                        }

                        if (!isScanlineDirty)
                            continue;

                        rowStartMin = Math.Min(rowStartMin, startX);
                        rowEndMax = Math.Max(rowEndMax, endX);
                    }
                }

                #endregion

                #region Private Methods

                private void VisitEdges()
                {
                    var edges = scanner.Edges;
                    var sortedIndexYStart = scanner.SortedIndexYStart;
                    var sortedIndexYEnd = scanner.SortedIndexYEnd;
                    while (yStartIndex < sortedIndexYStart.Length)
                    {
                        int i = sortedIndexYStart[yStartIndex];
                        if (edges[i].YStart > currentYf)
                            break;

                        activeEdges.EnterEdge(i);
                        yStartIndex += 1;
                    }

                    while (yEndIndex < sortedIndexYEnd.Length)
                    {
                        int i = sortedIndexYEnd[yEndIndex];
                        if (edges[i].YEnd > currentYf)
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

            private const int parallelThreshold = 256;
            private const float roundingUnit = 1f / 32f;

            #endregion

            #region Fields

            private readonly StrongBox<(int ThreadId, SolidScannerContext Context)>?[]? threadContextCache;
            private readonly int hashMask;
            private readonly float pixelOffset;

            private SolidScannerContext mainContext;

            #endregion

            #region Properties

            internal override bool IsSingleThreaded => threadContextCache == null;

            #endregion

            #region Constructors

            public SolidRegionScanner(FillPathSession session)
                : base(session, roundingUnit)
            {
                var drawingOptions = session.DrawingOptions;
                var activeEdges = ActiveEdgeTable.Create(GetActiveTableBuffer(), drawingOptions.FillMode, Edges.Length, session.RawPath.TotalVertices);
                var context = session.Context;

                pixelOffset = drawingOptions.ScanPathPixelOffset == PixelOffset.Half ? 0.5f : 0f;
                mainContext = new SolidScannerContext(this, activeEdges);
                mainContext.SkipEdgesAbove(Top + pixelOffset, true);

                int parallelFactor = drawingOptions.Ditherer != null ? 2 : drawingOptions.Quantizer != null ? 1 : 0;
                if (Width < (parallelThreshold >> parallelFactor) || context.MaxDegreeOfParallelism == 1 || ParallelHelper.CoreCount == 1)
                    return;

                threadContextCache = new StrongBox<(int ThreadId, SolidScannerContext)>?[EnvironmentHelper.GetThreadBasedCacheSize(context.MaxDegreeOfParallelism)];
                hashMask = threadContextCache.Length - 1;
            }

            #endregion

            #region Methods

            #region Public Methods

            public override void Dispose()
            {
                base.Dispose();
                mainContext.Dispose();
                if (threadContextCache == null)
                    return;

                // cache entries don't use pooling so their disposal just nullifies the backing array
                for (int i = 0; i < threadContextCache.Length; i++)
                    threadContextCache[i]?.Value.Context.Dispose();
            }

            #endregion

            #region Internal Methods

            internal override void ProcessNextScanline()
            {
                Debug.Assert(IsSingleThreaded || Session.IsSingleThreaded);
                if (!mainContext.MoveNextRow())
                    return;

                mainContext.ScanCurrentRow();

                if (mainContext.IsVisibleScanlineDirty)
                    Session.ApplyScanlineSolid(new RegionScanline(mainContext.CurrentY, Left, mainContext.ScanlineBuffer, mainContext.StartX, mainContext.EndX));
            }

            // TODO: remove (this helps testing the multi-threaded context on a single thread)
            //private int? currY;
            //internal override void ProcessNextScanline()
            //{
            //    Debug.Assert(IsSingleThreaded || Session.IsSingleThreaded);

            //    currY ??= mainContext.CurrentY;
            //    currY += 1;
            //    var context = new SolidScannerContext(mainContext, currY.Value);
            //    if (!context.MoveNextRow())
            //        return;

            //    context.ScanCurrentRow();

            //    if (context.IsVisibleScanlineDirty)
            //        Session.ApplyScanlineSolid(new RegionScanline(context.CurrentY, Left, context.ScanlineBuffer, context.StartX, context.EndX));
            //}

            internal override void ProcessScanline(int y)
            {
                Debug.Assert(!IsSingleThreaded);

                ref SolidScannerContext context = ref GetThreadContext(y);
                context.MoveNextRow();
                context.ScanCurrentRow();

                if (context.IsVisibleScanlineDirty)
                    Session.ApplyScanlineSolid(new RegionScanline(context.CurrentY, Left, context.ScanlineBuffer, context.StartX, context.EndX));
            }

            #endregion

            #region Private Methods

            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            private ref SolidScannerContext GetThreadContext(int y)
            {
                // Note that cache item access is not volatile, because it's even better if a thread sees its own last cached value.
                int threadId = EnvironmentHelper.CurrentThreadId;
                int hash = threadId & hashMask;
                StrongBox<(int ThreadId, SolidScannerContext Context)>? cacheEntry = threadContextCache![hash];

                if (cacheEntry != null)
                {
                    ref var value = ref cacheEntry.Value;
                    if (value.ThreadId == threadId)
                    {
                        ref SolidScannerContext context = ref value.Context;
                        if (context.CurrentY < y)
                        {
                            context.SkipEdgesAbove(y + pixelOffset, true);
                            return ref context;
                        }
                    }

                    // Here there is a collision so the old entry will be overwritten. Not disposing the old one though as it may still be used.
                }

                var result = new StrongBox<(int ThreadId, SolidScannerContext Context)>((threadId, new SolidScannerContext(mainContext, y)));
                threadContextCache[hash] = result;
                return ref result.Value.Context;
            }

            #endregion

            #endregion
        }

        #endregion

        #region AntiAliasingRegionScanner class

        private sealed class AntiAliasingRegionScanner : RegionScanner
        {
            #region Nested Structs

            /// <summary>
            /// Contains thread-specific data for region scanning
            /// </summary>
            private struct AntiAliasingScannerContext : IDisposable
            {
                #region Fields

                #region Internal Fields

                internal int CurrentY;
                internal ArraySection<byte> ScanlineBuffer;

                #endregion

                #region Private Fields

                private readonly AntiAliasingRegionScanner scanner;
                private readonly ActiveEdgeTable activeEdges;
                private readonly float subpixelOffset;

                private bool isScanlineDirty;
                private int yStartIndex;
                private int yEndIndex;
                private int rowStartMin;
                private int rowEndMax;
                private float nextY;
                private float currentSubpixelY;
                private float currentYf;

                #endregion

                #endregion

                #region Properties

                internal int StartX => Math.Max(scanner.VisibleLeft, rowStartMin) - scanner.Left;
                internal int EndX => Math.Min(scanner.VisibleLeft + scanner.VisibleWidth - 1, rowEndMax) - scanner.Left;

                internal bool IsVisibleScanlineDirty
                {
                    get
                    {
                        if (scanner.Region != null)
                        {
                            if (CurrentY < scanner.VisibleTop || CurrentY >= scanner.VisibleBottom || StartX > EndX)
                                return false;
                        }

                        return isScanlineDirty;
                    }
                }

                #endregion

                #region Constructors

                internal AntiAliasingScannerContext(AntiAliasingRegionScanner scanner, ActiveEdgeTable activeEdges)
                {
                    this.scanner = scanner;
                    this.activeEdges = activeEdges;
                    subpixelOffset = scanner.subpixelOffset;

                    // When there is a region, MoveNextRow will take a row from region mask. Otherwise, allocating one row.
                    if (scanner.Region == null)
                        ScanlineBuffer = new ArraySection<byte>(scanner.Width);

                    if (scanner.Edges.Length == 0)
                        return;

                    currentYf = scanner.Edges[scanner.SortedIndexYStart[0]].YStart;
                }

                internal AntiAliasingScannerContext(in AntiAliasingScannerContext other, int top)
                {
                    Debug.Assert(top > other.CurrentY);
                    this = other;
                    activeEdges = other.activeEdges.Clone();

                    if (scanner.Region is Region region)
                        ScanlineBuffer = region.Mask[top - scanner.Top];
                    else
                    {
                        // not pooling from here because colliding cache items might be overwritten while they are still in use
                        ScanlineBuffer = new byte[other.ScanlineBuffer.Length];
                    }

                    isScanlineDirty = false;
                    currentYf = other.currentYf;
                    SkipEdgesAbove(top + subpixelOffset, true);
                }

                #endregion

                #region Methods

                #region Public Methods

                public void Dispose()
                {
                    // Not quite the usual Dispose pattern, which is intended because this class is used internally only.
                    // Skipping if this is a default instance or when we have a region so scanline is not self allocated.
                    if (scanner == null || scanner.Region != null)
                        return;

                    ScanlineBuffer.Release();
                }

                #endregion

                #region Internal Methods

                internal void SkipEdgesAbove(float top, bool isInitializing)
                {
                    if (isInitializing)
                        CurrentY = (int)MathF.Floor(top) - 1;
                    if (currentYf >= top)
                        return;

                    var edges = scanner.Edges;
                    var sortedIndexYStart = scanner.SortedIndexYStart;
                    var sortedIndexYEnd = scanner.SortedIndexYEnd;

                    int startIndex = yStartIndex + 1;
                    int endIndex = yEndIndex;
                    bool visitEdges = isInitializing;

                    do
                    {
                        if (visitEdges)
                        {
                            VisitEdges();
                            activeEdges.RemoveLeavingEdges();
                        }

                        bool startFinished = startIndex >= sortedIndexYStart.Length;
                        bool endFinished = endIndex >= sortedIndexYEnd.Length;
                        if (startFinished && endFinished)
                            return;

                        float startY = startFinished ? Single.MaxValue : edges[sortedIndexYStart[startIndex]].YStart;
                        float endY = endFinished ? Single.MaxValue : edges[sortedIndexYEnd[endIndex]].YEnd;

                        if (startY < endY)
                        {
                            visitEdges = startY > currentYf;
                            currentYf = startY;
                            startIndex += 1;
                            continue;
                        }

                        visitEdges = endY > currentYf;
                        currentYf = endY;
                        endIndex += 1;
                    } while (currentYf < top);
                }

                internal bool MoveNextRow()
                {
                    CurrentY += 1;
                    if (CurrentY >= scanner.Bottom)
                        return false;

                    nextY = CurrentY + 1;
                    currentSubpixelY = CurrentY - subpixelSizeF;
                    if (scanner.Region is Region region)
                        ScanlineBuffer = region.Mask[CurrentY - scanner.Top];
                    else if (isScanlineDirty)
                        ScanlineBuffer.Clear();
                    isScanlineDirty = false;

                    rowStartMin = Int32.MaxValue;
                    rowEndMax = Int32.MinValue;

                    return true;
                }

                internal bool MoveNextSubpixelRow()
                {
                    currentSubpixelY += subpixelSizeF;
                    if (currentSubpixelY >= nextY)
                        return false;

                    // Skipping edges is needed if roundingUnit < subpixelSizeF because edges may dis/appear between two subpixel lines.
                    // The effect is not that apparent than during non-antialiasing scanning, but it also may cause artifacts.
                    // It would not be needed if we didn't use offset and roundingUnit would be the same as subpixelSizeF.
                    float nextScanY = currentSubpixelY + subpixelOffset;
                    if (currentYf < nextScanY)
                        SkipEdgesAbove(nextScanY, false);

                    currentYf = nextScanY;
                    VisitEdges();
                    return true;
                }

                internal void ScanCurrentSubpixelRow()
                {
                    CastArray<byte, float> points = activeEdges.ScanLine(currentYf, scanner.Edges);
                    if (points.Length == 0)
                        return;

                    int left = scanner.Left;
                    int right = scanner.Right;
                    int scanlinePixelWidth = ScanlineBuffer.Length;
                    for (int point = 0; point < points.Length - 1; point += 2)
                    {
                        float scanStart = points.GetElementUnsafe(point);
                        float scanEnd = points.GetElementUnsafe(point + 1);
                        if (scanEnd - scanStart < subpixelSizeF)
                            continue;

                        // the first (potentially partial) pixel
                        int startX = (int)MathF.Floor(scanStart);
                        int posX = startX - left;
                        if ((uint)posX < (uint)scanlinePixelWidth)
                        {
                            uint subpixelWidth = (uint)((startX + 1 - scanStart) * subpixelIntegerScale) >> subpixelSizeFactor;
                            if (subpixelWidth > 0u)
                            {
                                ref byte scanlinePixel = ref ScanlineBuffer.GetElementReferenceUnchecked(posX);
                                scanlinePixel = (scanlinePixel + subpixelWidth).ClipToByte();
                                isScanlineDirty = true;
                            }
                        }

                        // the last (potentially partial) pixel
                        int endX = (int)MathF.Floor(scanEnd);
                        posX = endX - left;
                        if ((uint)posX < (uint)scanlinePixelWidth)
                        {
                            uint subpixelWidth = (uint)((scanEnd - endX) * subpixelIntegerScale) >> subpixelSizeFactor;
                            if (subpixelWidth > 0u)
                            {
                                ref byte scanlinePixel = ref ScanlineBuffer.GetElementReferenceUnchecked(posX);
                                scanlinePixel = (scanlinePixel + subpixelWidth).ClipToByte();
                                isScanlineDirty = true;
                            }
                        }

                        // full pixels between first and last (within the scanned region)
                        int fullStartX = Math.Max(startX + 1, left) - left;
                        int fullEndX = Math.Min(endX - 1, right - 1) - left;
                        if (fullEndX >= fullStartX)
                        {
                            // TODO: vectorization if possible
                            Debug.Assert(fullStartX >= 0 && fullEndX < scanlinePixelWidth);
                            for (int i = fullStartX; i <= fullEndX; i++)
                            {
                                ref byte scanlinePixel = ref ScanlineBuffer.GetElementReferenceUnchecked(i);
                                scanlinePixel = (scanlinePixel + subpixelSize).ClipToByte();
                            }

                            isScanlineDirty = true;
                        }

                        if (!isScanlineDirty)
                            continue;

                        rowStartMin = Math.Min(rowStartMin, startX);
                        rowEndMax = Math.Max(rowEndMax, endX);
                    }
                }

                #endregion

                #region Private Methods

                private void VisitEdges()
                {
                    var edges = scanner.Edges;
                    var sortedIndexYStart = scanner.SortedIndexYStart;
                    var sortedIndexYEnd = scanner.SortedIndexYEnd;
                    while (yStartIndex < sortedIndexYStart.Length)
                    {
                        int i = sortedIndexYStart[yStartIndex];
                        if (edges[i].YStart > currentYf)
                            break;

                        activeEdges.EnterEdge(i);
                        yStartIndex += 1;
                    }

                    while (yEndIndex < sortedIndexYEnd.Length)
                    {
                        int i = sortedIndexYEnd[yEndIndex];
                        if (edges[i].YEnd > currentYf)
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

            private const int subpixelCount = 16;

            // We could use just floats but if subpixelCount is 16, then we have basically 256 different possible values (subpixel area is 1/16^2 = 1/256).
            // So using bytes instead of floats both in scanline and region mask buffers to spare 3 bytes per pixel, so going with integers where that's more optimal.
            private const uint subpixelIntegerScale = 256;
            private const uint subpixelSize = subpixelIntegerScale / subpixelCount;
            private const int subpixelSizeFactor = 4; // Math.Log2(subpixelSize);
            private const float subpixelSizeF = (float)subpixelSize / subpixelIntegerScale;

            private const float roundingUnit = subpixelSizeF / 2f;

            private const int parallelThreshold = 64;

            #endregion

            #region Fields

            private readonly StrongBox<(int ThreadId, AntiAliasingScannerContext Context)>?[]? threadContextCache;
            private readonly int hashMask;

            // Just like in SolidRegionScanner, we can have a nonzero offset to take samples from the middle of the subpixel rows.
            // It helps to create completely solid pixels symmetrically at the top/bottom of the shapes (e.g. ellipse).
            private readonly float subpixelOffset;

            private AntiAliasingScannerContext mainContext;

            #endregion

            #region Properties

            internal override bool IsSingleThreaded => threadContextCache == null;

            #endregion

            #region Constructors

            public AntiAliasingRegionScanner(FillPathSession session)
                : base(session, roundingUnit)
            {
                var drawingOptions = session.DrawingOptions;
                var context = session.Context;
                var activeEdges = ActiveEdgeTable.Create(GetActiveTableBuffer(), drawingOptions.FillMode, Edges.Length, session.RawPath.TotalVertices);

                subpixelOffset = drawingOptions.ScanPathPixelOffset == PixelOffset.Half ? subpixelSizeF / 2f : 0f;
                mainContext = new AntiAliasingScannerContext(this, activeEdges);
                mainContext.SkipEdgesAbove(Top + subpixelOffset, true);

                int parallelFactor = drawingOptions.Ditherer != null ? 2 : drawingOptions.Quantizer != null ? 1 : 0;
                if (Width < (parallelThreshold >> parallelFactor) || context.MaxDegreeOfParallelism == 1 || ParallelHelper.CoreCount == 1)
                    return;

                threadContextCache = new StrongBox<(int ThreadId, AntiAliasingScannerContext)>?[EnvironmentHelper.GetThreadBasedCacheSize(context.MaxDegreeOfParallelism)];
                hashMask = threadContextCache.Length - 1;
            }

            #endregion

            #region Methods

            #region Public Methods

            public override void Dispose()
            {
                base.Dispose();
                mainContext.Dispose();
                if (threadContextCache == null)
                    return;

                // cache entries don't use pooling so their disposal just nullifies the backing array
                for (int i = 0; i < threadContextCache.Length; i++)
                    threadContextCache[i]?.Value.Context.Dispose();
            }

            #endregion

            #region Internal Methods

            internal override void ProcessNextScanline()
            {
                Debug.Assert(IsSingleThreaded || Session.IsSingleThreaded);
                if (!mainContext.MoveNextRow())
                    return;

                while (mainContext.MoveNextSubpixelRow())
                    mainContext.ScanCurrentSubpixelRow();

                if (mainContext.IsVisibleScanlineDirty)
                    Session.ApplyScanlineAntiAliasing(new RegionScanline(mainContext.CurrentY, Left, mainContext.ScanlineBuffer, mainContext.StartX, mainContext.EndX));
            }

            // TODO: remove (this helps testing the multi-threaded context on a single thread)
            //private int? currY;
            //internal override void ProcessNextScanline()
            //{
            //    Debug.Assert(IsSingleThreaded || Session.IsSingleThreaded);
            //    currY ??= mainContext.CurrentY;
            //    currY += 1;
            //    var context = new AntiAliasingScannerContext(mainContext, currY.Value);
            //    if (!context.MoveNextRow())
            //        return;

            //    while (context.MoveNextSubpixelRow())
            //        context.ScanCurrentSubpixelRow();

            //    if (context.IsVisibleScanlineDirty)
            //        Session.ApplyScanlineAntiAliasing(new RegionScanline(context.CurrentY, Left, context.ScanlineBuffer, context.StartX, context.EndX));
            //}

            internal override void ProcessScanline(int y)
            {
                Debug.Assert(!IsSingleThreaded);

                ref AntiAliasingScannerContext context = ref GetThreadContext(y);
                context.MoveNextRow();

                while (context.MoveNextSubpixelRow())
                    context.ScanCurrentSubpixelRow();

                if (context.IsVisibleScanlineDirty)
                    Session.ApplyScanlineAntiAliasing(new RegionScanline(context.CurrentY, Left, context.ScanlineBuffer, context.StartX, context.EndX));
            }

            #endregion

            #region Private Methods

            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            private ref AntiAliasingScannerContext GetThreadContext(int y)
            {
                // Note that cache item access is not volatile, because it's even better if a thread sees its own last cached value.
                int threadId = EnvironmentHelper.CurrentThreadId;
                int hash = threadId & hashMask;
                StrongBox<(int ThreadId, AntiAliasingScannerContext Context)>? cacheEntry = threadContextCache![hash];

                if (cacheEntry != null)
                {
                    ref var value = ref cacheEntry.Value;
                    if (value.ThreadId == threadId)
                    {
                        ref AntiAliasingScannerContext context = ref value.Context;
                        if (context.CurrentY < y)
                        {
                            context.SkipEdgesAbove(y + subpixelOffset, true);
                            return ref context;
                        }
                    }

                    // Here there is a collision so the old entry will be overwritten. Not disposing the old one though as it may still be used.
                }

                var result = new StrongBox<(int ThreadId, AntiAliasingScannerContext Context)>((threadId, new AntiAliasingScannerContext(mainContext, y)));
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

            private readonly CastArray<byte, (int Index, Flags Flags)> activeEdges;

            #endregion

            #region Private Protected Fields

            #region Private Protected Fields

            private protected readonly CastArray<byte, float> Intersections;

            private protected int Count;

            #endregion

            #endregion

            #endregion

            #region Properties

            private protected CastArray<byte, (int Index, Flags Flags)> ActiveEdges => activeEdges.Slice(0, Count);

            #endregion

            #region Constructors

            protected ActiveEdgeTable(ref ArraySection<byte> buffer, int edgeCount, int maxIntersectionCount)
            {
                activeEdges = buffer.Allocate<(int, Flags)>(edgeCount);
                Count = 0;
                Intersections = buffer.Allocate<float>(maxIntersectionCount);
            }

            protected unsafe ActiveEdgeTable(ActiveEdgeTable other)
            {
                // not pooling from here because colliding cache items might be overwritten while they are still in use
#if NET5_0_OR_GREATER
                Intersections = GC.AllocateUninitializedArray<byte>(sizeof(float) * other.Intersections.Length);
                activeEdges = GC.AllocateUninitializedArray<byte>(sizeof((int, Flags)) * other.activeEdges.Length);
#else
                Intersections = new byte[sizeof(float) * other.Intersections.Length];
                activeEdges = new byte[sizeof((int, Flags)) * other.activeEdges.Length];
#endif
                Count = other.Count;
                other.activeEdges.Slice(0, Count).CopyTo(activeEdges);
                other.Intersections.CopyTo(Intersections);
            }

            #endregion

            #region Methods

            #region Static Methods

            internal static ActiveEdgeTable Create(ArraySection<byte> buffer, ShapeFillMode fillMode, int edgeCount, int maxIntersectionCount) => fillMode switch
            {
                ShapeFillMode.Alternate => new ActiveEdgeTableAlternate(ref buffer, edgeCount, maxIntersectionCount),
                ShapeFillMode.NonZero => new ActiveEdgeTableNonZero(ref buffer, edgeCount, maxIntersectionCount),
                _ => throw new InvalidOperationException(Res.InternalError($"Unhandled fill mode {fillMode}"))
            };

            #endregion

            #region Instance Methods

            internal void EnterEdge(int enterIndex)
            {
                Debug.Assert(activeEdges.Length > Count);
                ref var entry = ref activeEdges.GetElementReferenceUnsafe(Count);
                entry.Index = enterIndex;
                entry.Flags = Flags.Entering;
                Count += 1;
            }

            internal void LeaveEdge(int leaveIndex)
            {
                Debug.Assert(activeEdges.Length >= Count);
                var table = activeEdges;
                int len = Count;
                for (int i = 0; i < len; i++)
                {
                    ref var entry = ref table.GetElementReferenceUnsafe(i);
                    if (entry.Index == leaveIndex)
                    {
                        entry.Flags |= Flags.Leaving;
                        return;
                    }
                }
            }

            internal void RemoveLeavingEdges()
            {
                Debug.Assert(activeEdges.Length >= Count);
                int removed = 0;
                var table = activeEdges;
                int len = Count;
                for (int i = 0; i < len; i++)
                {
                    ref var entry = ref table.GetElementReferenceUnsafe(i);
                    if ((entry.Flags & Flags.Leaving) != Flags.None)
                        removed += 1;
                    else
                        table.GetElementReferenceUnsafe(i - removed) = (entry.Index, Flags.None);
                }

                Count -= removed;
            }

            internal abstract CastArray<byte, float> ScanLine(float y, in CastArray<byte, EdgeEntry> edges);

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

            internal ActiveEdgeTableAlternate(ref ArraySection<byte> buffer, int edgeCount, int maxIntersectionCount)
                : base(ref buffer, edgeCount, maxIntersectionCount)
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

            internal override CastArray<byte, float> ScanLine(float y, in CastArray<byte, EdgeEntry> edges)
            {
                #region Local Methods

                static void AddIntersection(float x, bool emit, in CastArray<byte, float> intersections, ref int count)
                {
                    if (!emit)
                        return;

                    Debug.Assert(intersections.Length >= count);
                    intersections.SetElementUnsafe(count, x);
                    count += 1;
                }

                #endregion

                int intersectionsCount = 0;
                int removed = 0;
                var activeEdges = ActiveEdges;
                int len = activeEdges.Length;
                for (int i = 0; i < len; i++)
                {
                    ref var entry = ref activeEdges.GetElementReferenceUnsafe(i);
                    ref var edge = ref edges.GetElementReference(entry.Index);
                    float x = edge.GetX(y);
                    if ((entry.Flags & Flags.Leaving) != Flags.None)
                    {
                        if ((entry.Flags & Flags.Entering) == Flags.None)
                            AddIntersection(x, edge.EmitEnd, Intersections, ref intersectionsCount);
                        removed += 1;
                        continue;
                    }

                    if ((entry.Flags & Flags.Entering) != Flags.None)
                        AddIntersection(x, edge.EmitStart, Intersections, ref intersectionsCount);
                    else
                    {
                        Debug.Assert(Intersections.Length > intersectionsCount);
                        Intersections.SetElementUnsafe(intersectionsCount, x);
                        intersectionsCount += 1;
                    }

                    activeEdges.GetElementReferenceUnsafe(i - removed) = (entry.Index, Flags.None);
                }

                Count -= removed;

                CastArray<byte, float> result = Intersections.Slice(0, intersectionsCount);
                result.Sort();
                return result;
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

            private CastArray<byte, IntersectionType> intersectionTypes;

            #endregion

            #region Constructors

            #region Internal Constructors

            internal ActiveEdgeTableNonZero(ref ArraySection<byte> buffer, int edgeCount, int maxIntersectionCount)
                : base(ref buffer, edgeCount, maxIntersectionCount)
            {
                intersectionTypes = buffer.Allocate<IntersectionType>(sizeof(IntersectionType) * maxIntersectionCount);
            }

            #endregion

            #region Private Constructors

            private ActiveEdgeTableNonZero(ActiveEdgeTableNonZero other)
                : base(other)
            {
                // not pooling from here because colliding cache items might be overwritten while they are still in use
#if NET5_0_OR_GREATER
                intersectionTypes = GC.AllocateArray<byte>(sizeof(IntersectionType) * other.intersectionTypes.Length);
#else
                intersectionTypes = new byte[sizeof(IntersectionType) * other.intersectionTypes.Length];
#endif
                other.intersectionTypes.CopyTo(intersectionTypes);
            }

            #endregion

            #endregion

            #region Methods

            #region Static Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            private static void AddIntersection(float x, bool emit, bool isAscending, in CastArray<byte, float> intersections, in CastArray<byte, IntersectionType> intersectionTypes, ref int count)
            {
                if (!emit)
                    return;

                Debug.Assert(intersectionTypes.Length > count);
                Debug.Assert(intersections.Length > count);
                if (isAscending)
                {
                    intersectionTypes.SetElementUnsafe(count, IntersectionType.Ascending);
                    intersections.SetElementUnsafe(count, x + sortStabilizerDelta);
                    count += 1;
                    return;
                }

                intersectionTypes.SetElementUnsafe(count, IntersectionType.Descending);
                intersections.SetElementUnsafe(count, x - sortStabilizerDelta);
                count += 1;
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            private static void AddIntersectionWhenToggles(in CastArray<byte, float> intersections, int i, int diff, float value, ref int windingNumber, ref int removed)
            {
                bool add = (windingNumber == 0 && diff != 0) || windingNumber * diff == -1;
                windingNumber += diff;

                if (add)
                {
                    Debug.Assert(intersections.Length > i - removed);
                    intersections.SetElementUnsafe(i - removed, value);
                }
                else
                    removed += 1;
            }

            #endregion

            #region Instance Methods

            internal override CastArray<byte, float> ScanLine(float y, in CastArray<byte, EdgeEntry> edges)
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
                    if ((entry.Flags & Flags.Leaving) != Flags.None)
                    {
                        if ((entry.Flags & Flags.Entering) == Flags.None)
                            AddIntersection(x, edge.EmitEnd, edge.IsAscending, intersections, types, ref intersectionsCount);
                        removed += 1;
                        continue;
                    }

                    if ((entry.Flags & Flags.Entering) != Flags.None)
                        AddIntersection(x, edge.EmitStart, edge.IsAscending, intersections, types, ref intersectionsCount);
                    else
                    {
                        if (edge.IsAscending)
                        {
                            types.SetElementUnsafe(intersectionsCount, IntersectionType.Ascending);
                            intersections.SetElementUnsafe(intersectionsCount, x + sortStabilizerDelta);
                        }
                        else
                        {
                            types.SetElementUnsafe(intersectionsCount, IntersectionType.Descending);
                            intersections.SetElementUnsafe(intersectionsCount, x - sortStabilizerDelta);
                        }

                        intersectionsCount += 1;
                    }

                    activeEdges.GetElementReferenceUnsafe(i - removed) = (entry.Index, Flags.None);
                }

                Count -= removed;
                intersections = intersections.Slice(0, intersectionsCount);
                types = types.Slice(0, intersectionsCount);
                intersections.Sort(types);

                // Counting the winding number and applying the nonzero winding rule. See details here: https://en.wikipedia.org/wiki/Point_in_polygon#Winding_number_algorithm
                removed = 0;
                int windingNumber = 0;
                len = types.Length;
                for (int i = 0; i < len; i++)
                {
                    IntersectionType type = types.GetElementUnsafe(i);
                    switch (type)
                    {
                        case IntersectionType.Corner:
                            AddIntersectionWhenToggles(intersections, i, -1, intersections.GetElementUnsafe(i), ref windingNumber, ref removed);
                            removed -= 1;
                            AddIntersectionWhenToggles(intersections, i, 1, intersections.GetElementUnsafe(i), ref windingNumber, ref removed);
                            break;
                        default:
                            {
                                int diff = type == IntersectionType.Ascending ? 1 : -1;
                                float emitVal = intersections.GetElementUnsafe(i) + -(sortStabilizerDelta * diff);
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

        #region DrawThinPathSession class

        private protected abstract class DrawThinPathSession : PathSessionBase
        {
            #region Constants

            private const float roundingUnit = 1f / 32f;

            #endregion

            #region Constructors

            protected DrawThinPathSession(Brush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions options, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, options, region)
            {
            }

            #endregion

            #region Methods

            #region Internal Methods

            [SecuritySafeCritical]internal abstract void DrawLine(PointF start, PointF end);
            internal abstract void DrawEllipse(RectangleF bounds);
            internal abstract void DrawArc(ArcSegment arc);

            #endregion

            #region Protected Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            protected (Point P1, Point P2) Round(PointF p1, PointF p2)
            {
                // NOTE: Unlike in DirectDrawer, rounding is not in a checked context here, because RawPath is already validated.
                return (new Point((int)MathF.Floor(p1.X.RoundTo(roundingUnit) + PixelOffset), (int)MathF.Floor(p1.Y.RoundTo(roundingUnit) + PixelOffset)),
                    new Point((int)MathF.Floor(p2.X.RoundTo(roundingUnit) + PixelOffset), (int)MathF.Floor(p2.Y.RoundTo(roundingUnit) + PixelOffset)));
            }

            #endregion

            #endregion
        }

        #endregion

        #region DrawIntoRegionSession class

        private sealed class DrawIntoRegionSession : DrawThinPathSession
        {
            #region Constructors

            internal DrawIntoRegionSession(Brush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions options, Region region)
                : base(owner, context, bitmapData, rawPath, bounds, options, region)
            {
            }

            #endregion

            #region Methods

            #region Internal Methods

            [SecuritySafeCritical]
            [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
                Justification = "Optimizations for special cases. Not extracting additional methods to prevent placing more frames on the call stack.")]
            internal override void DrawLine(PointF start, PointF end)
            {
                Debug.Assert(!Region!.IsAntiAliased);
                Array2D<byte> mask = Region!.Mask;
                (Point p1, Point p2) = Round(start, end);
                var offset = Region.Bounds.Location.AsSize();
                p1 -= offset;
                p2 -= offset;
                Size size = Region.Bounds.Size;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(size.Height))
                        return;

                    ArraySection<byte> row = mask[p1.Y];
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, size.Width - 1);
                    for (int x = Math.Max(p1.X, 0); x <= max; x++)
                        // TODO: optimize if possible
                        ColorExtensions.Set1bppColorIndex(ref row.GetElementReferenceUnchecked(x >> 3), x, 1);

                    return;
                }

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(size.Width))
                        return;

                    int maskPos = p1.X >> 3;
                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, size.Height - 1);
                    for (int y = Math.Max(p1.Y, 0); y <= max; y++)
                        ColorExtensions.Set1bppColorIndex(ref mask.GetElementReferenceUnchecked(y, maskPos), p1.X, 1);
                    return;
                }

                // general line
                long width = (p2.X - p1.X).Abs();
                long height = (p2.Y - p1.Y).Abs();

                if (width >= height)
                {
                    long numerator = width >> 1;
                    if (p1.X > p2.X)
                        (p1, p2) = (p2, p1);
                    int step = p2.Y > p1.Y ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible X coordinates
                    if (x < 0)
                    {
                        numerator = (numerator - height * x) % width;
                        y -= x * step;
                        x = 0;
                    }

                    int endX = Math.Min(p2.X, size.Width - 1);
                    int offY = step > 0 ? Math.Min(p2.Y, size.Height - 1) + 1 : Math.Max(p2.Y, 0) - 1;
                    for (; x <= endX; x++)
                    {
                        // Drawing only if Y is visible
                        if ((uint)y < (uint)size.Height)
                            ColorExtensions.Set1bppColorIndex(ref mask.GetElementReferenceUnchecked(y, x >> 3), x, 1);
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
                    long numerator = height >> 1;
                    if (p1.Y > p2.Y)
                        (p1, p2) = (p2, p1);
                    int step = p2.X > p1.X ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible Y coordinates
                    if (y < 0)
                    {
                        numerator = (numerator - width * y) % height;
                        x -= y * step;
                        y = 0;
                    }

                    int endY = Math.Min(p2.Y, size.Height - 1);
                    int offX = step > 0 ? Math.Min(p2.X, size.Width - 1) + 1 : Math.Max(p2.X, 0) - 1;
                    for (; y <= endY; y++)
                    {
                        // Drawing only if X is visible
                        if ((uint)x < (uint)size.Width)
                            ColorExtensions.Set1bppColorIndex(ref mask.GetElementReferenceUnchecked(y, x >> 3), x, 1);
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

            // Based on http://members.chello.at/~easyfilter/bresenham.c
            // Main changes: converting to C#, correcting types, more descriptive variable names, and using the ColorExtensions.Set1bppColorIndex method.
            internal override void DrawEllipse(RectangleF bounds)
            {
                Debug.Assert(!Region!.IsAntiAliased);
                Array2D<byte> mask = Region!.Mask;
                (Point p1, Point p2) = Round(bounds.Location, bounds.Location + bounds.Size);

                Size offset = Region.Bounds.Location.AsSize();
                p1 -= offset;
                p2 -= offset;
                Size size = Region.Bounds.Size;

                (int left, int right) = p2.X >= p1.X ? (p1.X, p2.X) : (p2.X, p1.X);
                (int top, int bottom) = p2.Y >= p1.Y ? (p1.Y, p2.Y) : (p2.Y, p1.Y);
                int width = right - left; // exclusive: the actual drawn width is width + 1
                int height = bottom - top; // exclusive: the actual drawn height is height + 1

                Debug.Assert(width <= ArcSegment.DrawAsLinesThreshold && height <= ArcSegment.DrawAsLinesThreshold);
                if (left >= size.Width || top >= size.Height || right < 0 || bottom < 0)
                    return;

                int oddHeightCorrection = height & 1;
                long widthSquared = (long)width * width;
                long heightSquared = (long)height * height;
                long stepX = 1L - width;
                stepX = (stepX * heightSquared) << 2; // should be checked(stepX * heightSquared * 4) if height could be larger than 916395
                long stepY = (oddHeightCorrection + 1L) * widthSquared;
                stepY <<= 2; // should be checked(stepY * 4) if width could be larger than 916396
                long err = oddHeightCorrection * widthSquared;
                err += stepX + stepY; //  should be checked(stepX + stepY + err) if size could be larger than 916396 x 916395

                bottom = top + ((height + 1) >> 1);
                top = bottom - oddHeightCorrection;
                long scaledWidth = widthSquared << 3;
                long scaledHeight = heightSquared << 3;

                do
                {
                    SetPixel(left, top);
                    SetPixel(right, top);
                    SetPixel(left, bottom);
                    SetPixel(right, bottom);

                    long err2 = err << 1; //should be checked(err * 2) if size could be larger than 916396 x 916395
                    if (err2 <= stepY)
                    {
                        top -= 1;
                        bottom += 1;
                        stepY += scaledWidth; //should be checked(stepY + scaledWidth) if width could be larger than 916396
                        err += stepY;
                    }

                    if (err2 >= stepX || err2 > stepY)
                    {
                        left += 1;
                        right -= 1;
                        stepX += scaledHeight; //should be checked(stepX + scaledHeight) if height could be larger than 916395
                        err += stepX;
                    }
                } while (left <= right);

                if (left > size.Width || right < -1 || top < 0 && bottom >= size.Height)
                    return;

                while (bottom - top <= height)
                {
                    SetPixel(left - 1, top);
                    SetPixel(right + 1, top);
                    top -= 1;
                    SetPixel(left - 1, bottom);
                    SetPixel(right + 1, bottom);
                    bottom += 1;
                }

                #region Local Methods

                [MethodImpl(MethodImpl.AggressiveInlining)]
                void SetPixel(int x, int y)
                {
                    if ((uint)x >= (uint)size.Width || (uint)y >= (uint)size.Height)
                        return;
                    Debug.Assert((uint)(x >> 3) < (uint)mask.Width && (uint)y < (uint)mask.Height);
                    ColorExtensions.Set1bppColorIndex(ref mask.GetElementReferenceUnchecked(y, x >> 3), x, 1);
                }

                #endregion
            }

            // Based on the combination of http://members.chello.at/~easyfilter/bresenham.c and https://www.scattergood.io/arc-drawing-algorithm/
            [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
                Justification = "False alarm, the new analyzer includes the complexity of local methods")]
            internal override void DrawArc(ArcSegment arc)
            {
                Debug.Assert(!Region!.IsAntiAliased);
                Debug.Assert(Math.Abs(arc.SweepAngle) < 360f, "Don't draw a full ellipse as an arc.");
                Array2D<byte> mask = Region!.Mask;
                RectangleF bounds = arc.Bounds;
                (Point p1, Point p2) = Round(bounds.Location, bounds.Location + bounds.Size);

                Size offset = Region.Bounds.Location.AsSize();
                p1 -= offset;
                p2 -= offset;
                Size size = Region.Bounds.Size;

                (int left, int right) = p2.X >= p1.X ? (p1.X, p2.X) : (p2.X, p1.X);
                (int top, int bottom) = p2.Y >= p1.Y ? (p1.Y, p2.Y) : (p2.Y, p1.Y);
                int width = right - left; // exclusive: the actual drawn width is width + 1
                int height = bottom - top; // exclusive: the actual drawn height is height + 1

                Debug.Assert(width <= ArcSegment.DrawAsLinesThreshold && height <= ArcSegment.DrawAsLinesThreshold);
                if (left >= size.Width || top >= size.Height || right < 0 || bottom < 0)
                    return;

                int oddHeightCorrection = height & 1;
                long widthSquared = (long)width * width;
                long heightSquared = (long)height * height;
                long stepX = 1L - width;
                stepX = (stepX * heightSquared) << 2; // should be checked(stepX * heightSquared * 4) if height could be larger than 916395
                long stepY = (oddHeightCorrection + 1L) * widthSquared;
                stepY <<= 2; // should be checked(stepY * 4) if width could be larger than 916396
                long err = oddHeightCorrection * widthSquared;
                err += stepX + stepY; //  should be checked(stepX + stepY + err) if size could be larger than 916396 x 916395

                bottom = top + ((height + 1) >> 1);
                top = bottom - oddHeightCorrection;
                long scaledWidth = widthSquared << 3;
                long scaledHeight = heightSquared << 3;

                // Not using arc.RadiusX/Y here because that is shorter by a half pixel (even if there is no rounding error)
                // because ArcSegment has no concept of line width, and here we draw a 1px wide path.
                float centerX = (left + right + 1) / 2f;
                float radiusX = (width + 1) / 2f;
                float radiusY = (height + 1) / 2f;
                (float startRad, float endRad) = arc.GetStartEndRadians();
                ArcSegment.AdjustAngles(ref startRad, ref endRad, radiusX, radiusY);

                // To prevent calculating Atan2 for each pixel, we just calculate a valid start/end range once, and apply it based on the current sector attributes.
                BitVector32 sectors = arc.GetSectors();
                int startX = (int)(centerX + radiusX * MathF.Cos(startRad));
                int endX = (int)(centerX + radiusX * MathF.Cos(endRad));

                do
                {
                    SetPixel(right, bottom, 0);
                    SetPixel(left, bottom, 1);
                    SetPixel(left, top, 2);
                    SetPixel(right, top, 3);

                    long err2 = err << 1; //should be checked(err * 2) if size could be larger than 916396 x 916395
                    if (err2 <= stepY)
                    {
                        top -= 1;
                        bottom += 1;
                        stepY += scaledWidth; //should be checked(stepY + scaledWidth) if width could be larger than 916396
                        err += stepY;
                    }

                    if (err2 >= stepX || err2 > stepY)
                    {
                        left += 1;
                        right -= 1;
                        stepX += scaledHeight; //should be checked(stepX + scaledHeight) if height could be larger than 916395
                        err += stepX;
                    }
                } while (left <= right);

                if (left > size.Width || right < -1 || top < 0 && bottom >= size.Height)
                    return;

                while (bottom - top <= height)
                {
                    SetPixel(right + 1, bottom, 0);
                    SetPixel(left - 1, bottom, 1);
                    bottom += 1;
                    SetPixel(left - 1, top, 2);
                    SetPixel(right + 1, top, 3);
                    top -= 1;
                }

                #region Local Methods

                void SetPixel(int x, int y, int sector)
                {
                    if ((uint)x >= (uint)size.Width || (uint)y >= (uint)size.Height)
                        return;

                    int sectorType = sectors[ArcSegment.Sectors[sector]];
                    if (sectorType == ArcSegment.SectorNotDrawn)
                        return;

                    if (sectorType == ArcSegment.SectorFullyDrawn
                        || sector > 1 // positive sector point
                        && (sectorType == ArcSegment.SectorStart && x >= startX
                            || sectorType == ArcSegment.SectorEnd && x <= endX
                            || sectorType == ArcSegment.SectorStartEnd && x >= startX && x <= endX)
                        || sector <= 1 // negative sector point
                        && (sectorType == ArcSegment.SectorStart && x <= startX
                            || sectorType == ArcSegment.SectorEnd && x >= endX
                            || sectorType == ArcSegment.SectorStartEnd && x <= startX && x >= endX))
                    {
                        Debug.Assert((uint)(x >> 3) < (uint)mask.Width && (uint)y < (uint)mask.Height);
                        ColorExtensions.Set1bppColorIndex(ref mask.GetElementReferenceUnchecked(y, x >> 3), x, 1);
                    }
                }

                #endregion
            }

            internal override void FinalizeSession() => Owner.ApplyRegion(Context, BitmapData, RawPath, VisibleBounds, DrawingOptions, Region!);

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

            internal CastArray<byte, EdgeEntry> Edges { get; }

            #endregion

            #region Constructors

            public EdgeTable(ArraySection<byte> buffer, RawPath path, float roundingUnit)
            {
                var edgesBuffer = buffer.Allocate<EdgeEntry>(path.TotalVertices);
                var snappedYCoords = buffer.Allocate<float>(path.MaxVertices + 1);
                var enumerator = new EdgeEnumerator(edgesBuffer);
                foreach (RawFigure figure in path.Figures)
                {
                    List<PointF> vertices = figure.ClosedVertices;
                    if (vertices.Count <= 3)
                        continue;

                    for (int i = 0; i < vertices.Count; i++)
                        snappedYCoords[i] = vertices[i].Y.RoundTo(roundingUnit);

                    enumerator.StartNextFigure(new EdgeInfo(vertices, snappedYCoords, vertices.Count - 2),
                        new EdgeInfo(vertices, snappedYCoords, 0));

                    enumerator.MoveNextEdge(false, new EdgeInfo(vertices, snappedYCoords, 1));

                    for (int i = 1; i < vertices.Count - 2; i++)
                        enumerator.MoveNextEdge(true, new EdgeInfo(vertices, snappedYCoords, i + 1));

                    // 1st edge
                    enumerator.MoveNextEdge(true, new EdgeInfo(vertices, snappedYCoords, 0));

                    // 2nd edge
                    enumerator.MoveNextEdge(true, default);
                }

                Edges = edgesBuffer.Slice(0, enumerator.Count);
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
            internal EdgeInfo(List<PointF> vertices, CastArray<byte, float> snappedYCoords, int index)
            {
                Debug.Assert(index < vertices.Count - 1 && index < snappedYCoords.Length - 1);
                Start = new PointF(vertices[index].X, snappedYCoords.GetElementUnsafe(index));
                End = new PointF(vertices[index + 1].X, snappedYCoords.GetElementUnsafe(index + 1));
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
#if NETCOREAPP || NET45_OR_GREATER || NETSTANDARD
                Vector2 pStart = edgeInfo.Start.AsVector2();
                Vector2 pEnd = edgeInfo.End.AsVector2();
                Vector2 center = (pStart + pEnd) * 0.5f;
                pStart -= center;
                pEnd -= center;
                p = (pEnd.X - pStart.X) / height;
                q = ((pStart.X * pEnd.Y) - (pEnd.X * pStart.Y)) / height + (center.X - p * center.Y);
#else
                PointF pStart = edgeInfo.Start;
                PointF pEnd = edgeInfo.End;
                PointF center = (pStart + new SizeF(pEnd));
                center.X *= 0.5f;
                center.Y *= 0.5f;
                pStart.X -= center.X;
                pStart.Y -= center.Y;
                pEnd.X -= center.X;
                pEnd.Y -= center.Y;
                p = (pEnd.X - pStart.X) / height;
                q = ((pStart.X * pEnd.Y) - (pEnd.X * pStart.Y)) / height + (center.X - p * center.Y);
#endif
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

            private readonly CastArray<byte, EdgeEntry> edges;

            internal int Count;
            private EdgeInfo previous;
            private EdgeInfo current;

            #endregion

            #region Constructors

            internal EdgeEnumerator(CastArray<byte, EdgeEntry> edges) => this.edges = edges;

            #endregion

            #region Methods

            [SuppressMessage("ReSharper", "ParameterHidesMember", Justification = "Initialization")]
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
                    Debug.Assert(edges.Length > Count);
                    edges.SetElementUnsafe(Count, previous.ToEdge());
                    Count += 1;
                }

                previous = current;
                current = next;
            }

            #endregion
        }

        #endregion

        #region RegionScanline struct

        private protected ref struct RegionScanline
        {
            #region Fields

            internal readonly int RowIndex;
            internal readonly int Left;
            internal readonly int MinIndex;
            internal readonly int MaxIndex;
            internal readonly ArraySection<byte> Scanline;

            #endregion

            #region Constructors

            internal RegionScanline(int y, int left, ArraySection<byte> scanline, int startX, int endX)
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

        #region RegionApplicator struct

        private struct RegionApplicator
        {
            #region Constants

            private const int parallelThreshold = 256;

            #endregion

            #region Fields

            private readonly FillPathSession session;
            private readonly bool isAntiAliased;
            private readonly Array2D<byte> mask;

            private Rectangle bounds;
            private Rectangle visibleBounds;

            #endregion

            #region Properties

            internal bool IsSingleThreaded
            {
                get
                {
                    int parallelFactor = session.DrawingOptions.Ditherer != null ? 2 : session.DrawingOptions.Quantizer != null ? 1 : 0;
                    return visibleBounds.Width < (parallelThreshold >> parallelFactor) || session.Context.MaxDegreeOfParallelism == 1 || ParallelHelper.CoreCount == 1;
                }
            }

            #endregion

            #region Constructors

            internal RegionApplicator(FillPathSession session)
            {
                this.session = session;
                var region = session.Region!;
                bounds = region.Bounds;
                visibleBounds = session.VisibleBounds;
                Debug.Assert(bounds.Contains(visibleBounds));

                isAntiAliased = region.IsAntiAliased;
                mask = region.Mask;
            }

            #endregion

            #region Methods

            #region Internal Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal void ApplyScanline(int y)
            {
                // A virtual method like RegionScanner.ProcessScanline would be more elegant but this is actually faster
                if (isAntiAliased)
                    ApplyScanlineAntiAliased(y);
                else
                    ApplyScanlineSolid(y);
            }

            #endregion

            #region Private Methods

            private void ApplyScanlineSolid(int y)
            {
                int startX = visibleBounds.Left - bounds.Left;
                int endX = startX + visibleBounds.Width - 1;
                ArraySection<byte> scanline = mask[y - bounds.Top];

                // visible bounds are intersected by target bitmap bounds (and this asserted in the constructor) so we are safe here
                if (scanline.GetElementUnchecked(startX >> 3) == 0)
                {
                    // adjusting to next coordinate divisible by 8
                    startX = (startX | 7) + 1;
                    while (startX <= endX && scanline.GetElementUnchecked(startX >> 3) == 0)
                        startX += 8;
                    if (startX > endX)
                        return;
                }

                if (scanline.GetElementUnchecked(endX >> 3) == 0)
                {
                    // adjusting to previous coordinate divisible by 8 minus 1
                    endX = (endX | 7) - 8;
                    while (startX <= endX && scanline.GetElementUnchecked(endX >> 3) == 0)
                        endX -= 8;
                    if (startX > endX)
                        return;
                }

                session.ApplyScanlineSolid(new RegionScanline(y, bounds.Left, scanline, startX, endX));
            }

            private void ApplyScanlineAntiAliased(int y)
            {
                int startX = visibleBounds.Left - bounds.Left;
                int endX = startX + visibleBounds.Width - 1;
                ArraySection<byte> scanline = mask[y - bounds.Top];

                while (startX <= endX && scanline.GetElementUnchecked(startX) == 0)
                    startX += 1;

                while (startX <= endX && scanline.GetElementUnchecked(endX) == 0)
                    endX -= 1;

                if (startX > endX)
                    return;

                session.ApplyScanlineAntiAliasing(new RegionScanline(y, bounds.Left, scanline, startX, endX));
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #endregion

        #region Properties

        internal abstract bool HasAlpha { get; }

        #endregion

        #region Constructors

        internal Brush()
        {
        }

        #endregion

        #region Methods

        #region Static Methods

        #region Public Methods

        /// <summary>
        /// Creates a solid <see cref="Brush"/> with the specified 32-bit color.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_Shapes_Brush_CreateSolid.htm">online help</a> for an image example.</div>
        /// </summary>
        /// <param name="color">A <see cref="Color32"/> value that represents the color of the brush.
        /// This overload can be used also for <see cref="Color"/> instances as there is an implicit cast from <see cref="Color"/> to <see cref="Color32"/>.</param>
        /// <returns>A <see cref="Brush"/> that uses the specified color.</returns>
        /// <remarks>
        /// <para>Though a solid brush generally uses a single color, filling a <see cref="Path"/> or a directly supported simple shape may use more colors
        /// if <see cref="DrawingOptions.AntiAliasing">DrawingOptions.AntiAliasing</see> is <see langword="true"/>,
        /// or when <paramref name="color"/> has alpha (transparency), and <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see> is <see langword="true"/>.</para>
        /// <para>If the <paramref name="color"/> has alpha and <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see> is <see langword="true"/>,
        /// the color space used for blending is determined by the specified <see cref="DrawingOptions.Quantizer"/>, or, when it is <see langword="null"/>, by the <see cref="IBitmapData.WorkingColorSpace"/>
        /// of the target <see cref="IReadWriteBitmapData"/>.</para>
        /// </remarks>
        /// <example>
        /// <para>The following image was created with a solid brush created from 50% transparent blue color, and <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see> was <see langword="true"/>,
        /// whereas the target bitmap data had <see cref="WorkingColorSpace.Linear"/> working color space:
        /// <br/><img src="../Help/Images/DrawingOptionsAlphaBlendingEnabledNoAA.png" alt="Polygon filled with 50% transparency, AntiAliasing = false, AlphaBlending = true."/></para>
        /// </example>
        public static Brush CreateSolid(Color32 color) => new SolidBrush(color);

        /// <summary>
        /// Creates a solid <see cref="Brush"/> with the specified 64-bit color.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateSolid(Color32)"/> overload for details.
        /// </summary>
        /// <param name="color">A <see cref="Color64"/> value that represents the color of the brush.</param>
        /// <returns>A <see cref="Brush"/> that uses the specified color.</returns>
        public static Brush CreateSolid(Color64 color) => new SolidBrush(color);

        /// <summary>
        /// Creates a solid <see cref="Brush"/> with the specified 128-bit color.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateSolid(Color32)"/> overload for details.
        /// </summary>
        /// <param name="color">A <see cref="ColorF"/> value that represents the color of the brush.</param>
        /// <returns>A <see cref="Brush"/> that uses the specified color.</returns>
        public static Brush CreateSolid(ColorF color) => new SolidBrush(color);

        /// <summary>
        /// Creates a texture <see cref="Brush"/> with the specified <paramref name="texture"/>.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="TextureMapMode"/> enumeration for image examples.
        /// </summary>
        /// <param name="texture">The texture to use for the <see cref="Brush"/>.</param>
        /// <param name="mapMode">Determines how the texture is mapped to the target area. This parameter is optional.
        /// <br/>Default value: <see cref="TextureMapMode.Tile"/>.</param>
        /// <param name="offset">Specifies an offset to apply when mapping the texture to the target area. This parameter is optional.
        /// <br/>Default value: <see cref="Point.Empty">Point.Empty</see>.</param>
        /// <param name="hasAlphaHint"><see langword="true"/> to indicate that the <paramref name="texture"/> uses the alpha channel (has transparency); otherwise, <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="true"/>.</param>
        /// <returns>A <see cref="Brush"/> that uses the specified <paramref name="texture"/>.</returns>
        /// <remarks>
        /// <para>This overload maps the entire <paramref name="texture"/> to the target <see cref="IReadWriteBitmapData"/>. To use only a smaller region of it,
        /// use the <see cref="CreateTexture(IReadableBitmapData,Rectangle,TextureMapMode,Point,bool)"/> overload instead.</para>
        /// <para>If the pixel format of the <paramref name="texture"/> has alpha channel, but the <paramref name="texture"/> does not use it,
        /// you can set the <paramref name="hasAlphaHint"/> parameter to <see langword="false"/> to improve performance.
        /// If the pixel format of the <paramref name="texture"/> does not have an alpha channel, then this parameter is ignored.</para>
        /// </remarks>
        public static Brush CreateTexture(IReadableBitmapData texture, TextureMapMode mapMode = TextureMapMode.Tile, Point offset = default, bool hasAlphaHint = true)
            => TextureBasedBrush.Create(texture, mapMode, hasAlphaHint, offset);

        /// <summary>
        /// Creates a texture <see cref="Brush"/>, using the specified <paramref name="textureBounds"/> for the <paramref name="texture"/> parameter.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="TextureMapMode"/> enumeration for image examples.
        /// </summary>
        /// <param name="texture">The texture to use for the <see cref="Brush"/>.</param>
        /// <param name="textureBounds">A <see cref="Rectangle"/> that specifies the actual bounds of the specified <paramref name="texture"/>.</param>
        /// <param name="mapMode">Determines how the texture is mapped to the target area. This parameter is optional.
        /// <br/>Default value: <see cref="TextureMapMode.Tile"/>.</param>
        /// <param name="offset">Specifies an offset to apply when mapping the texture to the target area. This parameter is optional.
        /// <br/>Default value: <see cref="Point.Empty">Point.Empty</see>.</param>
        /// <param name="hasAlphaHint"><see langword="true"/> to indicate that the <paramref name="texture"/> uses the alpha channel (has transparency); otherwise, <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="true"/>.</param>
        /// <returns>A <see cref="Brush"/> that uses the specified <paramref name="texture"/>.</returns>
        /// <remarks>
        /// <para>This overload allows you to map only a smaller region of the <paramref name="texture"/> bitmap for the brush. It can be useful when a single bitmap contains multiple textures.</para>
        /// <para>If the pixel format of the <paramref name="texture"/> has alpha channel, but the <paramref name="texture"/> does not use it,
        /// you can set the <paramref name="hasAlphaHint"/> parameter to <see langword="false"/> to improve performance.
        /// If the pixel format of the <paramref name="texture"/> does not have an alpha channel, then this parameter is ignored.</para>
        /// </remarks>
        public static Brush CreateTexture(IReadableBitmapData texture, Rectangle textureBounds, TextureMapMode mapMode = TextureMapMode.Tile, Point offset = default, bool hasAlphaHint = true)
            => CreateTexture(texture.Clip(textureBounds), mapMode, offset, hasAlphaHint);

        /// <summary>
        /// Creates a linear gradient <see cref="Brush"/> that blends from one color to another along a straight line specified by <paramref name="startPoint"/> and <paramref name="endPoint"/>.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="GradientWrapMode"/> enumeration for image examples.
        /// </summary>
        /// <param name="startPoint">The starting point of the gradient.</param>
        /// <param name="endPoint">The end point of the gradient.</param>
        /// <param name="startColor">The starting color of the gradient.</param>
        /// <param name="endColor">The end color of the gradient.</param>
        /// <param name="wrapMode">Determines how the gradient is treated outside the gradient area. This parameter is optional.
        /// <br/>Default value: <see cref="GradientWrapMode.Stop"/>.</param>
        /// <param name="workingColorSpace">Determines the color space in which the gradient is calculated. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>, which means <see cref="WorkingColorSpace.Srgb"/> for this overload.</param>
        /// <returns>A linear gradient <see cref="Brush"/> that uses the specified parameters.</returns>
        /// <remarks>
        /// <para>This overload creates a gradient with fix coordinates. To automatically apply the gradient to the bounds of each <see cref="Path"/>, use the overloads with an <c>angle</c> parameter,
        /// such as the <see cref="CreateLinearGradient(float,Color32,Color32,WorkingColorSpace)"/> overload.</para>
        /// <para>The <paramref name="workingColorSpace"/> parameter affects only the color interpolation, not the blending with the background,
        /// which is determined by the color space of the target bitmap data (or by <see cref="DrawingOptions.Quantizer">DrawingOptions.Quantizer</see>, if specified).</para>
        /// <para>In this overload the default value of <paramref name="workingColorSpace"/> maps to <see cref="WorkingColorSpace.Srgb">WorkingColorSpace.Srgb</see>, because
        /// the <see cref="Color32"/> type represents a color in the sRGB color space. When creating a monochromatic gradient, specifying <see cref="WorkingColorSpace.Srgb"/> can be better, because then the perceived
        /// mid-tone brightness will be at the middle of the gradient. Otherwise, using <see cref="WorkingColorSpace.Linear"/> may be more appropriate, so the transition between the colors will seem more "natural".</para>
        /// </remarks>
        public static Brush CreateLinearGradient(PointF startPoint, PointF endPoint, Color32 startColor, Color32 endColor,
            GradientWrapMode wrapMode = GradientWrapMode.Stop, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default)
            => CreateLinearGradient(startPoint, endPoint, startColor.ToColor64(), endColor.ToColor64(), wrapMode, workingColorSpace);

        /// <summary>
        /// Creates a linear gradient <see cref="Brush"/> that blends from one color to another along a straight line specified by <paramref name="startPoint"/> and <paramref name="endPoint"/>.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="GradientWrapMode"/> enumeration for image examples.
        /// </summary>
        /// <param name="startPoint">The starting point of the gradient.</param>
        /// <param name="endPoint">The end point of the gradient.</param>
        /// <param name="startColor">The starting color of the gradient.</param>
        /// <param name="endColor">The end color of the gradient.</param>
        /// <param name="wrapMode">Determines how the gradient is treated outside the gradient area. This parameter is optional.
        /// <br/>Default value: <see cref="GradientWrapMode.Stop"/>.</param>
        /// <param name="workingColorSpace">Determines the color space in which the gradient is calculated. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>, which means <see cref="WorkingColorSpace.Srgb"/> for this overload.</param>
        /// <returns>A linear gradient <see cref="Brush"/> that uses the specified parameters.</returns>
        /// <remarks>
        /// <para>This overload creates a gradient with fix coordinates. To automatically apply the gradient to the bounds of each <see cref="Path"/>, use the overloads with an <c>angle</c> parameter,
        /// such as the <see cref="CreateLinearGradient(float,Color32,Color32,WorkingColorSpace)"/> overload.</para>
        /// <para>The <paramref name="workingColorSpace"/> parameter affects only the color interpolation, not the blending with the background,
        /// which is determined by the color space of the target bitmap data (or by <see cref="DrawingOptions.Quantizer">DrawingOptions.Quantizer</see>, if specified).</para>
        /// <para>In this overload the default value of <paramref name="workingColorSpace"/> maps to <see cref="WorkingColorSpace.Srgb">WorkingColorSpace.Srgb</see>, because
        /// the <see cref="Color64"/> type represents a color in the sRGB color space. When creating a monochromatic gradient, specifying <see cref="WorkingColorSpace.Srgb"/> can be better, because then the perceived
        /// mid-tone brightness will be at the middle of the gradient. Otherwise, using <see cref="WorkingColorSpace.Linear"/> may be more appropriate, so the transition between the colors will seem more "natural".</para>
        /// </remarks>
        public static Brush CreateLinearGradient(PointF startPoint, PointF endPoint, Color64 startColor, Color64 endColor,
            GradientWrapMode wrapMode = GradientWrapMode.Stop, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default)
        {
            if (startPoint.HasNaNOrInfinity())
                throw new ArgumentOutOfRangeException(nameof(startPoint), PublicResources.ArgumentOutOfRange);
            if (endPoint.HasNaNOrInfinity())
                throw new ArgumentOutOfRangeException(nameof(endPoint), PublicResources.ArgumentOutOfRange);
            if (startPoint.TolerantEquals(endPoint, Constants.EqualityTolerance))
                throw new ArgumentException(Res.ShapesStartEndTooClose);
            if (!workingColorSpace.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
          
            if (startColor == endColor && wrapMode != GradientWrapMode.Clip)
                return CreateSolid(startColor);
            bool isLinear = workingColorSpace == WorkingColorSpace.Linear;
            return new LinearGradientBrush(startPoint, endPoint, startColor.ToColorF(isLinear), endColor.ToColorF(isLinear), wrapMode, isLinear);
        }

        /// <summary>
        /// Creates a linear gradient <see cref="Brush"/> that blends from one color to another along a straight line specified by <paramref name="startPoint"/> and <paramref name="endPoint"/>.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="GradientWrapMode"/> enumeration for image examples.
        /// </summary>
        /// <param name="startPoint">The starting point of the gradient.</param>
        /// <param name="endPoint">The end point of the gradient.</param>
        /// <param name="startColor">The starting color of the gradient.</param>
        /// <param name="endColor">The end color of the gradient.</param>
        /// <param name="wrapMode">Determines how the gradient is treated outside the gradient area. This parameter is optional.
        /// <br/>Default value: <see cref="GradientWrapMode.Stop"/>.</param>
        /// <param name="workingColorSpace">Determines the color space in which the gradient is calculated. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>, which means <see cref="WorkingColorSpace.Linear"/> for this overload.</param>
        /// <returns>A linear gradient <see cref="Brush"/> that uses the specified parameters.</returns>
        /// <remarks>
        /// <para>This overload creates a gradient with fix coordinates. To automatically apply the gradient to the bounds of each <see cref="Path"/>, use the overloads with an <c>angle</c> parameter,
        /// such as the <see cref="CreateLinearGradient(float,Color32,Color32,WorkingColorSpace)"/> overload.</para>
        /// <para>The <paramref name="workingColorSpace"/> parameter affects only the color interpolation, not the blending with the background,
        /// which is determined by the color space of the target bitmap data (or by <see cref="DrawingOptions.Quantizer">DrawingOptions.Quantizer</see>, if specified).</para>
        /// <para>In this overload the default value of <paramref name="workingColorSpace"/> maps to <see cref="WorkingColorSpace.Linear">WorkingColorSpace.Linear</see>, because
        /// the <see cref="ColorF"/> type represents a color in the linear color space. When creating a monochromatic gradient, specifying <see cref="WorkingColorSpace.Srgb"/> can be better, because then the perceived
        /// mid-tone brightness will be at the middle of the gradient. Otherwise, using <see cref="WorkingColorSpace.Linear"/> may be more appropriate, so the transition between the colors will seem more "natural".</para>
        /// </remarks>
        public static Brush CreateLinearGradient(PointF startPoint, PointF endPoint, ColorF startColor, ColorF endColor,
            GradientWrapMode wrapMode = GradientWrapMode.Stop, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default)
        {
            if (startPoint.HasNaNOrInfinity())
                throw new ArgumentOutOfRangeException(nameof(startPoint), PublicResources.ArgumentOutOfRange);
            if (endPoint.HasNaNOrInfinity())
                throw new ArgumentOutOfRangeException(nameof(endPoint), PublicResources.ArgumentOutOfRange);
            if (startPoint.TolerantEquals(endPoint, Constants.EqualityTolerance))
                throw new ArgumentException(Res.ShapesStartEndTooClose);
            if (!workingColorSpace.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));

            if (startColor == endColor && wrapMode != GradientWrapMode.Clip)
                return CreateSolid(startColor);
            bool isLinear = workingColorSpace != WorkingColorSpace.Srgb;
            return new LinearGradientBrush(startPoint, endPoint, startColor.ToColorF(isLinear), endColor.ToColorF(isLinear), wrapMode, isLinear);
        }

        /// <summary>
        /// Creates a linear gradient <see cref="Brush"/> that blends from one color to another along a straight line. The line is specified by an <paramref name="angle"/> only,
        /// whereas the actual start and end points are determined by the bounding rectangle of the current <see cref="Path"/> in each session.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_Shapes_Brush_CreateLinearGradient_3.htm">online help</a> for an example with image.</div>
        /// </summary>
        /// <param name="angle">The angle of the gradient line in degrees. The angle is measured clockwise from the positive x-axis.</param>
        /// <param name="startColor">The starting color of the gradient.</param>
        /// <param name="endColor">The end color of the gradient.</param>
        /// <param name="workingColorSpace">Determines the color space in which the gradient is calculated. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>, which means <see cref="WorkingColorSpace.Srgb"/> for this overload.</param>
        /// <returns>A linear gradient <see cref="Brush"/> that uses the specified parameters.</returns>
        /// <remarks>
        /// <para>This overload creates a gradient with dynamic coordinates, applying the gradient automatically to the bounds of each <see cref="Path"/>. To use fix coordinates instead,
        /// use the overloads with a <c>startPoint</c> and <c>endPoint</c> parameters, such as the <see cref="CreateLinearGradient(PointF,PointF,Color32,Color32,GradientWrapMode,WorkingColorSpace)"/> overload.</para>
        /// <para>The <paramref name="workingColorSpace"/> parameter affects only the color interpolation, not the blending with the background,
        /// which is determined by the color space of the target bitmap data (or by <see cref="DrawingOptions.Quantizer">DrawingOptions.Quantizer</see>, if specified).</para>
        /// <para>In this overload the default value of <paramref name="workingColorSpace"/> maps to <see cref="WorkingColorSpace.Srgb">WorkingColorSpace.Srgb</see>, because
        /// the <see cref="Color32"/> type represents a color in the sRGB color space. When creating a monochromatic gradient, specifying <see cref="WorkingColorSpace.Srgb"/> can be better, because then the perceived
        /// mid-tone brightness will be at the middle of the gradient. Otherwise, using <see cref="WorkingColorSpace.Linear"/> may be more appropriate, so the transition between the colors will seem more "natural".</para>
        /// </remarks>
        /// <example>
        /// <para>The following example demonstrates how to create a horizontal linear gradient brush that blends from white to black, adjusted always to the currently filled shape:
        /// <code lang="C#"><![CDATA[
        /// public IReadWriteBitmapData CreateBitmap()
        /// {
        ///     var bitmap = BitmapDataFactory.CreateBitmapData(100, 150);
        ///     bitmap.Clear(Color.Cyan);
        ///
        ///     // Zero angle means horizontal gradient
        ///     var brush = Brush.CreateLinearGradient(angle: 0f, Color.White, Color.Black);
        ///
        ///     // Filling the same ellipse three times with different offsets
        ///     foreach (int offset in new[] { -50, 0, 50 })
        ///     {
        ///         var options = new DrawingOptions
        ///         {
        ///             AntiAliasing = true,
        ///             Transformation = TransformationMatrix.CreateTranslation(offset, offset + 50)
        ///         };
        ///
        ///         bitmap.FillEllipse(brush, new Rectangle(0, 0, 100, 50), options);
        ///     }
        ///
        ///     return bitmap;
        /// }]]></code></para>
        /// <para>The example above produces the result below. Note that the gradient always starts from the left edge of the ellipse and ends at the right edge, regardless of the ellipse's location.
        /// As a contrast, see the examples at the <see cref="GradientWrapMode"/> enumeration, where the gradients always use specific start and end points.
        /// <br/><img src="../Help/Images/BrushLinearGradientAngle.png" alt="Three ellipses filled with horizontal linear gradient from white to black."/></para>
        /// </example>
        public static Brush CreateLinearGradient(float angle, Color32 startColor, Color32 endColor, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default)
            => CreateLinearGradient(angle, startColor.ToColor64(), endColor.ToColor64(), workingColorSpace);

        /// <summary>
        /// Creates a linear gradient <see cref="Brush"/> that blends from one color to another along a straight line. The line is specified by an <paramref name="angle"/> only,
        /// whereas the actual start and end points are determined by the bounding rectangle of the current <see cref="Path"/> in each session.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="CreateLinearGradient(float,Color32,Color32,WorkingColorSpace)"/> overload for an example with image.
        /// </summary>
        /// <param name="angle">The angle of the gradient line in degrees. The angle is measured clockwise from the positive x-axis.</param>
        /// <param name="startColor">The starting color of the gradient.</param>
        /// <param name="endColor">The end color of the gradient.</param>
        /// <param name="workingColorSpace">Determines the color space in which the gradient is calculated. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>, which means <see cref="WorkingColorSpace.Srgb"/> for this overload.</param>
        /// <returns>A linear gradient <see cref="Brush"/> that uses the specified parameters.</returns>
        /// <remarks>
        /// <para>This overload creates a gradient with dynamic coordinates, applying the gradient automatically to the bounds of each <see cref="Path"/>. To use fix coordinates instead,
        /// use the overloads with a <c>startPoint</c> and <c>endPoint</c> parameters, such as the <see cref="CreateLinearGradient(PointF,PointF,Color32,Color32,GradientWrapMode,WorkingColorSpace)"/> overload.</para>
        /// <para>The <paramref name="workingColorSpace"/> parameter affects only the color interpolation, not the blending with the background,
        /// which is determined by the color space of the target bitmap data (or by <see cref="DrawingOptions.Quantizer">DrawingOptions.Quantizer</see>, if specified).</para>
        /// <para>In this overload the default value of <paramref name="workingColorSpace"/> maps to <see cref="WorkingColorSpace.Srgb">WorkingColorSpace.Srgb</see>, because
        /// the <see cref="Color32"/> type represents a color in the sRGB color space. When creating a monochromatic gradient, specifying <see cref="WorkingColorSpace.Srgb"/> can be better, because then the perceived
        /// mid-tone brightness will be at the middle of the gradient. Otherwise, using <see cref="WorkingColorSpace.Linear"/> may be more appropriate, so the transition between the colors will seem more "natural".</para>
        /// </remarks>
        public static Brush CreateLinearGradient(float angle, Color64 startColor, Color64 endColor, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default)
        {
            if (Single.IsNaN(angle) || Single.IsInfinity(angle))
                throw new ArgumentOutOfRangeException(nameof(angle), PublicResources.ArgumentOutOfRange);
            if (!workingColorSpace.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));

            bool isLinear = workingColorSpace == WorkingColorSpace.Linear;
            return new LinearGradientBrush(angle, startColor.ToColorF(isLinear), endColor.ToColorF(isLinear), isLinear);
        }

        /// <summary>
        /// Creates a linear gradient <see cref="Brush"/> that blends from one color to another along a straight line. The line is specified by an <paramref name="angle"/> only,
        /// whereas the actual start and end points are determined by the bounding rectangle of the current <see cref="Path"/> in each session.
        /// <br/>See the <strong>Examples</strong> section of the <see cref="CreateLinearGradient(float,Color32,Color32,WorkingColorSpace)"/> overload for an example with image.
        /// </summary>
        /// <param name="angle">The angle of the gradient line in degrees. The angle is measured clockwise from the positive x-axis.</param>
        /// <param name="startColor">The starting color of the gradient.</param>
        /// <param name="endColor">The end color of the gradient.</param>
        /// <param name="workingColorSpace">Determines the color space in which the gradient is calculated. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>, which means <see cref="WorkingColorSpace.Srgb"/> for this overload.</param>
        /// <returns>A linear gradient <see cref="Brush"/> that uses the specified parameters.</returns>
        /// <remarks>
        /// <para>This overload creates a gradient with dynamic coordinates, applying the gradient automatically to the bounds of each <see cref="Path"/>. To use fix coordinates instead,
        /// use the overloads with a <c>startPoint</c> and <c>endPoint</c> parameters, such as the <see cref="CreateLinearGradient(PointF,PointF,Color32,Color32,GradientWrapMode,WorkingColorSpace)"/> overload.</para>
        /// <para>The <paramref name="workingColorSpace"/> parameter affects only the color interpolation, not the blending with the background,
        /// which is determined by the color space of the target bitmap data (or by <see cref="DrawingOptions.Quantizer">DrawingOptions.Quantizer</see>, if specified).</para>
        /// <para>In this overload the default value of <paramref name="workingColorSpace"/> maps to <see cref="WorkingColorSpace.Linear">WorkingColorSpace.Linear</see>, because
        /// the <see cref="ColorF"/> type represents a color in the linear color space. When creating a monochromatic gradient, specifying <see cref="WorkingColorSpace.Srgb"/> can be better, because then the perceived
        /// mid-tone brightness will be at the middle of the gradient. Otherwise, using <see cref="WorkingColorSpace.Linear"/> may be more appropriate, so the transition between the colors will seem more "natural".</para>
        /// </remarks>
        public static Brush CreateLinearGradient(float angle, ColorF startColor, ColorF endColor, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default)
        {
            if (Single.IsNaN(angle) || Single.IsInfinity(angle))
                throw new ArgumentOutOfRangeException(nameof(angle), PublicResources.ArgumentOutOfRange);
            if (!workingColorSpace.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));

            bool isLinear = workingColorSpace != WorkingColorSpace.Srgb;
            return new LinearGradientBrush(angle, startColor.ToColorF(isLinear), endColor.ToColorF(isLinear), isLinear);
        }

        // TODO
        //public static Brush CreateLinearGradient(PointF startPoint, PointF endPoint, IList<Color32> colors, IList<float> positions, float angle = 0f, GradientMapMode mapMode = GradientMapMode.Stop, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default) => throw new NotImplementedException();
        //public static Brush CreateLinearGradient(float angle, IList<Color32> colors, IList<float> positions, GradientMapMode mapMode = GradientMapMode.Stop, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default) => throw new NotImplementedException();

        #endregion

        #region Private Methods

        private static RegionScanner CreateScanner(FillPathSession session)
            => session.DrawingOptions.AntiAliasing
                ? new AntiAliasingRegionScanner(session)
                : new SolidRegionScanner(session);

        #endregion

        #endregion

        #region Instance Methods

        #region Internal Methods

        internal bool FillPath(IAsyncContext context, IReadWriteBitmapData bitmapData, Path path, DrawingOptions drawingOptions)
            => FillRawPath(context, bitmapData, path.RawPath, drawingOptions, path.PreferCaching);

        [SuppressMessage("ReSharper", "AccessToDisposedClosure",
            Justification = "False alarm, ParallelHelper.For does not use the delegate after returning.")]
        internal bool FillRawPath(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, DrawingOptions drawingOptions, bool cache)
        {
            Rectangle pathBounds = rawPath.Bounds;
            Rectangle visibleBounds = pathBounds.IntersectSafe(new Rectangle(Point.Empty, bitmapData.Size));

            if (visibleBounds.IsEmpty() || context.IsCancellationRequested)
                return !context.IsCancellationRequested;

            Region? region = null;
            if (cache && (long)pathBounds.Width * pathBounds.Height <= drawingOptions.CacheRegionLimit)
            {
                region = rawPath.GetCreateCachedRegion(drawingOptions);

                // If we already have a generated region, we just re-apply it on a much faster path.
                if (region.IsGenerated)
                    return ApplyRegion(context, bitmapData, rawPath, visibleBounds, drawingOptions, region);
            }

            try
            {
                using FillPathSession session = CreateFillSession(context, bitmapData, rawPath, visibleBounds, drawingOptions, region);
                using RegionScanner scanner = CreateScanner(session);
                Rectangle generateBounds = region == null ? visibleBounds : pathBounds;

                if (scanner.IsSingleThreaded || session.IsSingleThreaded)
                {
                    context.Progress?.New(DrawingOperation.ProcessingPixels, generateBounds.Height);
                    for (int y = generateBounds.Top; y < generateBounds.Bottom; y++)
                    {
                        if (context.IsCancellationRequested)
                            return false;

                        scanner.ProcessNextScanline();
                        context.Progress?.Increment();
                    }
                }
                else
                {
                    if (!ParallelHelper.For(context, DrawingOperation.ProcessingPixels, generateBounds.Top, generateBounds.Bottom,
                        y => scanner.ProcessScanline(y)))
                    {
                        return false;
                    }
                }

                region?.SetCompleted();
                session.FinalizeSession();
                return !context.IsCancellationRequested;
            }
            catch (Exception)
            {
                region?.Reset();
                throw;
            }
            finally
            {
                if (context.IsCancellationRequested)
                    region?.Reset();
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
            Justification = "False alarm, the new analyzer includes the complexity of local methods")]
        internal bool DrawThinPath(IAsyncContext context, IReadWriteBitmapData bitmapData, Path path, DrawingOptions drawingOptions, bool cache)
        {
            Debug.Assert(!drawingOptions.AntiAliasing);

            RawPath rawPath = path.RawPath;
            Rectangle pathBounds = rawPath.DrawOutlineBounds;
            Rectangle visibleBounds = pathBounds.IntersectSafe(new Rectangle(Point.Empty, bitmapData.Size));

            if (visibleBounds.IsEmpty() || context.IsCancellationRequested)
                return !context.IsCancellationRequested;

            Region? region = null;
            if (NeedsRegion())
            {
                region = cache && (long)pathBounds.Width * pathBounds.Height <= drawingOptions.CacheRegionLimit
                    ? rawPath.GetCreateCachedRegion(drawingOptions, true)
                    : new Region(visibleBounds, drawingOptions.AntiAliasing);

                // If we already have a generated region, we just re-apply it
                if (region.IsGenerated)
                    return ApplyRegion(context, bitmapData, rawPath, visibleBounds, drawingOptions, region);
            }

            try
            {
                using DrawThinPathSession? session = CreateDrawThinPathSession(context, bitmapData, rawPath, visibleBounds, drawingOptions, region);
                if (session == null)
                    return false;

                List<PathSegment> segments = path.GetSegments();
                context.Progress?.New(DrawingOperation.ProcessingPixels, segments.Count);
                foreach (PathSegment segment in segments)
                {
                    if (context.IsCancellationRequested)
                        return false;

                    // Special handling for arcs for nicer Bresenham-like results. Not needed for wide pens or anti-aliased paths.
                    if (segment is ArcSegment arc && arc.Width <= ArcSegment.DrawAsLinesThreshold && arc.Height <= ArcSegment.DrawAsLinesThreshold)
                    {
                        if (Math.Abs(arc.SweepAngle) >= 360f)
                            session.DrawEllipse(arc.Bounds);
                        else
                            session.DrawArc(arc);
                        continue;
                    }

                    IList<PointF> points = segment.GetFlattenedPointsInternal();
                    int len = points.Count;

                    if (len == 1)
                        session.DrawLine(points[0], points[0]);
                    else
                    {
                        for (int i = 1; i < len; i++)
                            session.DrawLine(points[i - 1], points[i]);
                    }

                    context.Progress?.Increment();
                }

                region?.SetCompleted();
                session.FinalizeSession();
                return !context.IsCancellationRequested;
            }
            catch (Exception)
            {
                region?.Reset();
                throw;
            }
            finally
            {
                if (context.IsCancellationRequested)
                    region?.Reset();
            }

            #region Local Methods

            bool NeedsRegion()
            {
                // If there can be blending, then we must use a region to prevent overblending issues at crossing lines.
                Debug.Assert(!drawingOptions.AntiAliasing);
                if (drawingOptions.AlphaBlending && HasAlpha)
                    return true;

                IQuantizer? quantizer = drawingOptions.Quantizer;
                IDitherer? ditherer = drawingOptions.Ditherer;
                if (quantizer != null || ditherer != null)
                {
                    bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

                    // If the quantizer or ditherer relies on the actual content, it would be no benefit in direct drawing in the first pass
                    // because the small advantage would be negligible due to the multiple passes anyway.
                    if (quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true)
                        return true;

                    // Region is needed for thin paths if the known ditherer requires sequential processing, or if we can't tell whether IDitheringSession.IsSequential is true.
                    if (ditherer != null && (ditherer is ErrorDiffusionDitherer or RandomNoiseDitherer { HasSeed: true } || ditherer.GetType().Assembly != typeof(IDitherer).Assembly))
                        return true;
                }

                // From this point it's not a must to use a region so we can decide on practical reasons.

                // Not using a region if its cost would be bigger than direct draw. This is a very rough estimation because
                // we don't check the length or the orientation of the lines.
                if (rawPath.TotalVertices < bitmapData.Width + bitmapData.Height)
                    return false;

                // Returning true only if the path is expected to be re-used, and the region is not too big.
                return cache && (long)pathBounds.Width * pathBounds.Height <= drawingOptions.CacheRegionLimit;
            }

            #endregion
        }

        #endregion

        #region Private Protected Methods

        private protected abstract FillPathSession CreateFillSession(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region);

        private protected virtual DrawThinPathSession? CreateDrawThinPathSession(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region) => region == null
            ? throw new InvalidOperationException(Res.InternalError($"{nameof(CreateDrawThinPathSession)} should be overridden to draw path without generating a region"))
            : new DrawIntoRegionSession(this, context, bitmapData, rawPath, bounds, drawingOptions, region);

        #endregion

        #region Private Methods

        private bool ApplyRegion(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle visibleBounds, DrawingOptions drawingOptions, Region region)
        {
            using FillPathSession session = CreateFillSession(context, bitmapData, rawPath, visibleBounds, drawingOptions, region);
            var applicator = new RegionApplicator(session);
            if (applicator.IsSingleThreaded || session.IsSingleThreaded)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, visibleBounds.Height);
                for (int y = visibleBounds.Top; y < visibleBounds.Bottom; y++)
                {
                    if (context.IsCancellationRequested)
                        return false;

                    applicator.ApplyScanline(y);
                    context.Progress?.Increment();
                }
            }
            else
            {
                if (!ParallelHelper.For(context, DrawingOperation.ProcessingPixels, visibleBounds.Top, visibleBounds.Bottom,
                    y => applicator.ApplyScanline(y)))
                {
                    return false;
                }
            }

            session.FinalizeSession();
            return !context.IsCancellationRequested;
        }

        #endregion

        #endregion

        #endregion
    }
}