#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RectangleExtensions.cs
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
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing
{
    internal static class RectangleExtensions
    {
        #region Methods
        
        /// <summary>
        /// Gets whether the rectangle has zero Width OR Height.
        /// Not just faster than the IsEmpty property but also works better when Intersect returns a non-default practically zero rectangle.
        /// </summary>
        internal static bool IsEmpty(this Rectangle rect) => rect.Width == 0 || rect.Height == 0;

        /// <summary>
        /// Like Rectangle.Intersect, but works with big ranges, and returns Rectangle.Empty if the result would be a practically zero rectangle.
        /// </summary>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Rectangle IntersectSafe(this Rectangle a, Rectangle b)
        {
            int x1 = Math.Max(a.X, b.X);
            long x2 = Math.Min((long)a.X + a.Width, (long)b.X + b.Width);
            int y1 = Math.Max(a.Y, b.Y);
            long y2 = Math.Min((long)a.Y + a.Height, (long)b.Y + b.Height);

            // The original Rectangle.Intersect method has >= checks, which can return non-default zero height or width rectangles.
            if (x2 > x1 && y2 > y1)
                // The (int) cast is safe because the result is guaranteed to be in the int range as intersection can only reduce height and width.
                return new Rectangle(x1, y1, (int)(x2 - x1), (int)(y2 - y1));

            return Rectangle.Empty;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static int RightChecked(this Rectangle rectangle) => checked(rectangle.X + rectangle.Width);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static int BottomChecked(this Rectangle rectangle) => checked(rectangle.Y + rectangle.Height);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static void Normalize(this ref Rectangle rect)
        {
            checked
            {
                if (rect.Width < 0)
                {
                    rect.X += rect.Width;
                    rect.Width = -rect.Width;
                }

                if (rect.Height < 0)
                {
                    rect.Y += rect.Height;
                    rect.Height = -rect.Height;
                }
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static void Normalize(this ref RectangleF rect)
        {
            if (rect.Width < 0f)
            {
                rect.X += rect.Width;
                rect.Width = -rect.Width;
            }

            if (rect.Height < 0f)
            {
                rect.Y += rect.Height;
                rect.Height = -rect.Height;
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Rectangle TruncateChecked(this RectangleF rect)
        {
            checked
            {
                int x = (int)rect.X;
                int y = (int)rect.Y;
                int width = (int)rect.Width;
                int height = (int)rect.Height;
                return new Rectangle(x, y, width, height);
            }
        }

        #endregion

    }
}
