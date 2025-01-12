#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PathFactory.cs
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

using KGySoft.Collections;
using KGySoft.Drawing.Examples.Shared.Enums;
using KGySoft.Drawing.Shapes;

#endregion

#nullable enable

namespace KGySoft.Drawing.Examples.Shared
{
    internal static class PathFactory
    {
        #region Fields

        private static readonly IThreadSafeCacheAccessor<(Rectangle, PathShape, int), Path> pathCache
            = ThreadSafeCacheFactory.Create<(Rectangle, PathShape, int), Path>(CreatePath, new LockFreeCacheOptions { InitialCapacity = 1, ThresholdCapacity = 1 });

        #endregion

        #region Methods

        #region Internal Methods
        
        internal static Path? GetPath(Rectangle bounds, PathShape shape, int outlineWidth)
        {
            bounds.Inflate(-outlineWidth / 2 - 1, -outlineWidth / 2 - 1);
            if (bounds.Width <= 0 || bounds.Height <= 0 || shape == PathShape.None)
                return null;

            // Returning the path from a 1-element cache, so returning the same instance if the same parameters are requested repeatedly.
            // This way the inner cached path region can be re-used by the caller.
            return pathCache[(bounds, shape, outlineWidth)];
        }

        #endregion

        #region Private Methods

        private static Path CreatePath((Rectangle Bounds, PathShape Shape, int) key)
        {
            var (bounds, shape, _) = key;

            var result = new Path(preferCaching: true);
            switch (shape)
            {
                case PathShape.Ellipse:
                    return result.AddEllipse(bounds);

                case PathShape.RoundedRectangle:
                    return result.AddRoundedRectangle(bounds, 20);

                case PathShape.Star:
                    int size = Math.Min(bounds.Width, bounds.Height);
                    return result.TransformTranslation((bounds.Width >> 1) - (size >> 1) + bounds.Left, (bounds.Height >> 1) - (size >> 1) + bounds.Top)
                        .AddPolygon([new(size * 0.5f, 0f), new(size * 0.79f, size * 0.9f), new(size * 0.02f, size * 0.35f), new(size * 0.97f, size * 0.35f), new(size * 0.21f, size * 0.9f)]);
                
                case PathShape.Heart:
                    size = Math.Min(bounds.Width, bounds.Height);
                    return result.TransformTranslation((bounds.Width >> 1) - (size >> 1) + bounds.Left, (bounds.Height >> 1) - (size >> 1) + bounds.Top)
                        .AddArc(0f, 0f, size / 2f, size / 2f, 155f, 205f)
                        .AddArc(size / 2f, 0f, size / 2f, size / 2f, 180f, 205f)
                        .AddPoint(size / 2f, size)
                        .CloseFigure();
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(shape));
            }
        }

        #endregion

        #endregion
    }
}
