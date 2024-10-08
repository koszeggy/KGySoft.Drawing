#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Pen.cs
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

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents a pen for drawing operations.
    /// </summary>
    public sealed class Pen
    {
        #region Fields

        private Brush brush;
        private float width;
        private LineJoinStyle lineJoin;
        private float miterLimit = 10f;
        private LineCapStyle startCap;
        private LineCapStyle endCap;

        #endregion

        #region Properties

        public Brush Brush
        {
            get => brush;
            set => brush = value ?? throw new ArgumentNullException(nameof(value), PublicResources.ArgumentNull);
        }

        public float Width
        {
            get => width;
            set
            {
                if (value <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentMustBeGreaterThan(0f));
                width = value;
            }
        }

        public LineJoinStyle LineJoin
        {
            get => lineJoin;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));
                lineJoin = value;
            }
        }

        public float MiterLimit
        {
            get => miterLimit;
            set
            {
                if (value < 0f || Single.IsNaN(value))
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentMustBeGreaterThanOrEqualTo(0f));
                miterLimit = value;
            }
        }

        public LineCapStyle StartCap
        {
            get => startCap;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));
                startCap = value;
            }
        }

        public LineCapStyle EndCap
        {
            get => endCap;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));
                endCap = value;
            }
        }

        #endregion

        #region Constructors

        public Pen() : this(Color32.Black)
        {
        }

        public Pen(Color32 color, float width = 1f)
        {
            if (width <= 0f)
                throw new ArgumentOutOfRangeException(nameof(width), PublicResources.ArgumentMustBeGreaterThan(0f));
            this.width = width;
            brush = new SolidBrush(color);
        }

        public Pen(Brush brush, float width = 1f)
        {
            this.brush = brush ?? throw new ArgumentNullException(nameof(brush), PublicResources.ArgumentNull);
            
            if (width <= 0f)
                throw new ArgumentOutOfRangeException(nameof(width), PublicResources.ArgumentMustBeGreaterThan(0f));
            this.width = width;
        }

        #endregion

        #region Methods

        internal void ApplyPath(IAsyncContext context, IReadWriteBitmapData bitmapData, Path path, DrawingOptions drawingOptions, bool cache)
        {
            RawPath rawPath = path.RawPath;

            // TODO: special handling if width <= 1[.5?]: not generating a new path but drawing thr raw lines of rawPath

            RawPath widePath = cache ? rawPath.GetCreateWidePath(this) : rawPath.WidenPath(this);
            
            brush.ApplyRawPath(context, bitmapData, widePath, drawingOptions.WithNonZeroFill, cache);
        }

        #endregion
    }
}