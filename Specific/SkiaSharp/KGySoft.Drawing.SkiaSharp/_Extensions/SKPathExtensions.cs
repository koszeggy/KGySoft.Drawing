#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKPathExtensions.cs
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

using KGySoft.Drawing.Shapes;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Contains extension methods for the <see cref="SKPath"/> class.
    /// </summary>
    public static class SKPathExtensions
    {
        #region Methods

        /// <summary>
        /// Converts the specified <see cref="SKPath"/> to a <see cref="Path"/> object.
        /// </summary>
        /// <param name="path">The <see cref="SKPath"/> instance to convert to a <see cref="Path"/>.</param>
        /// <returns>A <see cref="Path"/> object that represents the same geometric path as the specified <see cref="SKPath"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> is <see langword="null"/>.</exception>
        public static Path ToPath(this SKPath path)
        {
            #region Local Methods

            static (PointF ControlPoint1, PointF ControlPoint2) GetCubicControlPointsFromQuadraticBezier(PointF start, PointF controlPoint, PointF end)
                => (new PointF(start.X + 2f / 3f * (controlPoint.X - start.X),
                        start.Y + 2f / 3f * (controlPoint.Y - start.Y)),
                    new PointF(end.X + 2f / 3f * (controlPoint.X - end.X),
                        end.Y + 2f / 3f * (controlPoint.Y - end.Y)));

            static (PointF ControlPoint1, PointF ControlPoint2) GetCubicControlPointsFromConicCurve(PointF start, PointF controlPoint, PointF end, float weight)
            {
                // Though SKPath has a ConvertConicToQuads, there is no need to approximate the curve by quadratic Béziers, because a single cubic Bézier curve always can represent a conic curve.
                // The problem is that SkiaSharp has no API to convert a conic curve to a cubic Bézier.
                // Credit to this paper where I finally managed to find the solution: https://www.mn.uio.no/math/english/people/aca/michaelf/papers/g4.pdf
                float lambda = weight * 4f / 3f / (1 + weight);
                return (new PointF((1 - lambda) * start.X + lambda * controlPoint.X,
                        (1 - lambda) * start.Y + lambda * controlPoint.Y),
                    new PointF((1 - lambda) * end.X + lambda * controlPoint.X,
                        (1 - lambda) * end.Y + lambda * controlPoint.Y));
            }

            #endregion

            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);

            var result = new Path();
            int count = path.PointCount;
            if (count == 0)
                return result;

            // As usual, the official SkiaSharp documentation is not quite helpful.
            // But this old Xamarin page has a good example for processing SKPath segments: https://learn.microsoft.com/en-us/previous-versions/xamarin/xamarin-forms/user-interface/graphics/skiasharp/curves/information#enumerating-the-path
            bool lastPointAdded = false;
            using SKPath.RawIterator iterator = path.CreateRawIterator();
            SKPoint[] buf = new SKPoint[4];
            SKPathVerb verb;
            while ((verb = iterator.Next(buf)) != SKPathVerb.Done)
            {
                switch (verb)
                {
                    case SKPathVerb.Move:
                        // Not adding the new start point here, because KGy SOFT's Path would render even a single point.
                        // Instead, adding it only when a section is actually added. NOTE: no need to store the start point (buf[0])
                        // because it is always repeated as the first point of the current section.
                        result.StartFigure();
                        lastPointAdded = false;
                        break;

                    case SKPathVerb.Line:
                        if (!lastPointAdded)
                        {
                            result.AddLine(buf[0].AsPointF(), buf[1].AsPointF());
                            lastPointAdded = true;
                        }
                        else
                            result.AddPoint(buf[1].AsPointF());
                        break;

                    case SKPathVerb.Cubic:
                        result.AddBezier(buf[0].AsPointF(), buf[1].AsPointF(), buf[2].AsPointF(), buf[3].AsPointF());
                        lastPointAdded = true;
                        break;

                    case SKPathVerb.Quad:
                        PointF startPoint = buf[0].AsPointF();
                        PointF endPoint = buf[2].AsPointF();
                        (PointF cp1, PointF cp2) = GetCubicControlPointsFromQuadraticBezier(startPoint, buf[1].AsPointF(), endPoint);
                        result.AddBezier(startPoint, cp1, cp2, endPoint);
                        lastPointAdded = true;
                        break;

                    case SKPathVerb.Conic:
                        startPoint = buf[0].AsPointF();
                        endPoint = buf[2].AsPointF();
                        (cp1, cp2) = GetCubicControlPointsFromConicCurve(startPoint, buf[1].AsPointF(), endPoint, iterator.ConicWeight());
                        result.AddBezier(startPoint, cp1, cp2, endPoint);
                        lastPointAdded = true;
                        break;

                    case SKPathVerb.Close:
                        result.CloseFigure();
                        lastPointAdded = false;
                        break;

                    default:
                        throw new InvalidOperationException(Res.InternalError($"Unexpected verb: {verb}"));
                }
            }

            return result;
        }

        #endregion
    }
}
