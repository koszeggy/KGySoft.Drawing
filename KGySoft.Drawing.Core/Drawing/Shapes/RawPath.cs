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
    /// The raw version of <see cref="Path"/> where everything is represented by simple points (line segments).
    /// </summary>
    internal sealed class RawPath
    {
        #region Nested Types

        [Flags]
        private enum RegionsCacheKey
        {
            None,
            NonZeroFillMode = 1,
            AntiAliasing = 1 << 1,
        }

        #endregion

        #region Fields

        private readonly List<RawFigure> figures;

        private Rectangle bounds;
        private int totalVertices;
        private int maxVertices;
        private IThreadSafeCacheAccessor<int, Region>? regionsCache;


        #endregion

        #region Properties

        internal Rectangle Bounds => bounds;
        internal int TotalVertices => totalVertices;
        internal int MaxVertices => maxVertices;
        internal List<RawFigure> Figures => figures;

        #endregion

        #region Constructors

        internal RawPath(int capacity) => figures = new List<RawFigure>(capacity);

        #endregion

        #region Methods

        #region Internal Methods
        
        internal void AddRawFigure(IList<PointF> points, bool optimize)
        {
            if (points.Count == 0)
                return;
            var figure = new RawFigure(points, optimize);
            bounds = figures.Count == 0 ? figure.Bounds : Rectangle.Union(bounds, figure.Bounds);
            figures.Add(figure);
            totalVertices += figure.Vertices.Length - 1;
            maxVertices = Math.Max(maxVertices, figure.Vertices.Length - 1);
            regionsCache = null;
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

        #endregion

        #region Private Methods

        private Region CreateRegion(int key) => new Region(bounds, ((RegionsCacheKey)key & RegionsCacheKey.AntiAliasing) != 0);

        #endregion

        #endregion
    }
}