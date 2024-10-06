﻿#region Copyright

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

        #endregion

        #region Constructors

        public Pen() : this(Color32.Black, 1)
        {
        }

        public Pen(Color32 color, float width = 1f)
        {
            if (width <= 0f)
                throw new ArgumentOutOfRangeException(nameof(width), PublicResources.ArgumentMustBeGreaterThan(0f));
            this.width = width;
            brush = new SolidBrush(color);
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