#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PathFactory.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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

        private static readonly IThreadSafeCacheAccessor<(Rectangle, PathShape), Path> pathCache
            = ThreadSafeCacheFactory.Create<(Rectangle, PathShape), Path>(CreatePath, new LockFreeCacheOptions { InitialCapacity = 1, ThresholdCapacity = 1 });

        #endregion

        #region Properties

        internal static Func<Rectangle, Path>? GetTextPathCallback { get; set; }

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
            return pathCache[(bounds, shape)];
        }

        #endregion

        #region Private Methods

        private static Path CreatePath((Rectangle Bounds, PathShape Shape) key)
        {
            var (bounds, shape) = key;

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

                case PathShape.Text:
                    // If a project has a specific text -> Path conversion, then using that one
                    if (GetTextPathCallback is Func<Rectangle, Path> callback)
                        return callback.Invoke(bounds);

                    // Otherwise, just returning the capital letter "A", converted from the following SVG (original source: https://www.svgrepo.com/svg/120099/letter-a-text-variant)
                    // <svg fill="#000000" version="1.1" id="Capa_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="100px" height="100px" viewBox="0 0 32.181 32.18" xml:space="preserve"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <g>
                    // <path d="M30.037,29.924c-0.67-0.611-1.525-2.094-2.567-4.447L16.539,0.625h-0.424L5.276,24.805 c-1.028,2.311-1.893,3.834-2.593,4.57c-0.7,0.738-1.595,1.189-2.683,1.352v0.828h10.079v-0.828 C8.5,30.608,7.511,30.43,7.107,30.192c-0.687-0.401-1.029-1.026-1.029-1.877c0-0.641,0.209-1.453,0.627-2.438l1.273-2.949h10.705 l1.608,3.777c0.416,0.98,0.642,1.541,0.67,1.676c0.091,0.283,0.135,0.559,0.135,0.826c0,0.447-0.163,0.791-0.49,1.029 c-0.479,0.328-1.305,0.488-2.48,0.488h-0.604v0.828h14.659v-0.828C31.261,30.653,30.545,30.385,30.037,29.924z M8.764,21.274 l4.647-10.436l4.516,10.436H8.764z"></path>
                    // </g></g></svg>

                    size = Math.Min(bounds.Width, bounds.Height);
                    return result.TransformTranslation((bounds.Width >> 1) - (size >> 1) + bounds.Left, (bounds.Height >> 1) - (size >> 1) + bounds.Top)
                        .TransformScale(size / 32.181f, size / 32.18f)
                        .AddBezier(30.037f, 29.924f, // M30.037,29.924
                            29.367f, 29.313f, 28.512f, 27.83f, 27.47f, 25.477f) // c -0.67 -0.611 -1.525 -2.094 -2.567 -4.447 (relative cubic)
                        .AddLines(new PointF(16.539f, 0.625f), // L16.539,0.625
                            new PointF(16.115f, 0.625f), // h-0.424
                            new PointF(5.276f, 24.805f)) // L5.276,24.805
                        .AddBeziers(new PointF(5.276f, 24.805f), // same as previous end point
                            new PointF(4.248f, 27.116f), new PointF(3.383f, 28.639f), new PointF(2.683f, 29.375f), // c -1.028,2.311 -1.893,3.834 -2.593,4.57
                            new PointF(1.983f, 30.113f), new PointF(1.088f, 30.564f), new PointF(0f, 30.727f)) // c -0.7,0.738 -1.595,1.189 -2.683,1.352
                        .AddLines(new PointF(0f, 31.555f), // v0.828
                            new PointF(10.079f, 31.555f), // h10.079
                            new PointF(10.079f, 30.727f)) // v-0.828
                        .AddBeziers(new PointF(10.079f, 30.727f), // same as previous end point
                            new PointF(8.5f, 30.608f), new PointF(7.511f, 30.43f), new PointF(7.107f, 30.192f), // C8.5,30.608,7.511,30.43,7.107,30.192  (absolute cubic)
                            new PointF(6.42f, 29.791f), new PointF(6.078f, 29.166f), new PointF(6.078f, 28.315f), // c -0.687,-0.401 -1.029,-1.026 -1.029,-1.877 (relative)
                            new PointF(6.078f, 27.674f), new PointF(6.287f, 26.862f), new PointF(6.705f, 25.877f)) // c 0,-0.641 0.209,-1.453 0.627,-2.438
                        .AddLines(new PointF(7.978f, 22.928f), // l1.273,-2.949
                            new PointF(18.683f, 22.928f), // h10.705
                            new PointF(20.291f, 26.705f)) // l1.608,3.777
                        .AddBeziers(new PointF(20.291f, 26.705f), // same as previous end point
                            new PointF(20.707f, 27.685f), new PointF(20.933f, 28.246f), new PointF(20.961f, 28.381f), // c 0.416,0.98 0.642,1.541 0.67,1.676
                            new PointF(21.052f, 28.664f), new PointF(21.096f, 28.94f), new PointF(21.096f, 29.207f), // c 0.091,0.283 0.135,0.559 0.135,0.826
                            new PointF(21.096f, 29.654f), new PointF(20.933f, 29.998f), new PointF(20.606f, 30.236f), // c 0,0.447 -0.163,0.791 -0.49,1.029
                            new PointF(20.127f, 30.564f), new PointF(19.301f, 30.724f), new PointF(18.126f, 30.724f)) // c -0.479,0.328 -1.305,0.488 -2.48,0.488
                        .AddLines(new PointF(17.522f, 30.724f), // h-0.604
                            new PointF(17.522f, 31.552f), // v0.828
                            new PointF(32.181f, 31.552f), // h14.659
                            new PointF(32.181f, 30.724f)) // v-0.828
                        .AddBezier(32.181f, 30.724f, 31.261f, 30.653f, 30.545f, 30.385f, 30.037f, 29.924f) // C31.261,30.653,30.545,30.385,30.037,29.924
                        .AddPolygon( // z (AddPolygon contains an implicit CloseFigure + StartFigure)
                            new PointF(8.764f, 21.274f), // M8.764,21.274
                            new PointF(13.411f, 10.838f), // l4.647-10.436
                            new PointF(17.927f, 21.274f) // l4.516,10.436
                        ); // H8.764 z // AddPolygon implicitly contains adding the start point again and closing the figure
                default:
                    throw new ArgumentOutOfRangeException(nameof(shape));
            }
        }

        #endregion

        #endregion
    }
}
