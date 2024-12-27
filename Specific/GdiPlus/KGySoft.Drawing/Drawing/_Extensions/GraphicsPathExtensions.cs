#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GraphicsPathExtensions.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
#if NET
using System.Runtime.Versioning;
#endif

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="GraphicsPath"/> class.
    /// </summary>
#if NET7_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    internal static class GraphicsPathExtensions
    {
        #region Methods

        /// <summary>
        /// Adds a rounded rectangle to this <see cref="GraphicsPath"/>, applying the same corner radius to all corners.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> instance to add the rounded rectangle to.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="cornerRadius">The size of the corner radius of the rounded rectangle for all corners.</param>
        /// <remarks>
        /// <para>The rounded rectangle is added as a new closed figure.</para>
        /// <para>If <paramref name="cornerRadius"/> is negative, the absolute value will be used. If it is greater than the half of the smaller side of the bounding rectangle,
        /// it will be adjusted to the half of the smaller side, so the result will be an oval shape. If the <paramref name="cornerRadius"/> is 0, a simple rectangle will be added.</para>
        /// </remarks>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Intended. Even very small differences are handled properly.")]
        public static void AddRoundedRectangle(this GraphicsPath path, RectangleF bounds, float cornerRadius)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);

            if (cornerRadius == 0f)
            {
                path.AddRectangle(bounds);
                return;
            }

            bounds.Normalize();
            path.StartFigure();
            float diameter = Math.Min(Math.Abs(cornerRadius) * 2f, Math.Min(Math.Abs(bounds.Width), Math.Abs(bounds.Height)));
            var arc = new RectangleF(bounds.Location, new SizeF(diameter, diameter));

            // top left arc
            path.AddArc(arc, 180, 90);

            // top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
        }

        /// <summary>
        /// Adds a rounded rectangle to this <see cref="GraphicsPath"/>, applying a custom corner radius to each corner.
        /// </summary>
        /// <param name="path">The <see cref="GraphicsPath"/> instance to add the rounded rectangle to.</param>
        /// <param name="bounds">The bounding rectangle that defines the rounded rectangle.</param>
        /// <param name="radiusTopLeft">The size of the top-left corner radius of the rounded rectangle.</param>
        /// <param name="radiusTopRight">The size of the top-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomRight">The size of the bottom-right corner radius of the rounded rectangle.</param>
        /// <param name="radiusBottomLeft">The size of the bottom-left corner radius of the rounded rectangle.</param>
        /// <remarks>
        /// <para>The rounded rectangle is added as a new closed figure.</para>
        /// <para>If any of the corner radius parameters is negative, the absolute value will be used. If the sum of any adjacent corner radius parameters is greater
        /// than the corresponding side of the bounding rectangle, then all corner radius parameters will be scaled down proportionally to fit into the bounding rectangle.</para>
        /// </remarks>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Intended. Even very small differences are handled properly.")]
        public static void AddRoundedRectangle(this GraphicsPath path, RectangleF bounds, float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), PublicResources.ArgumentNull);

            bounds.Normalize();
            path.StartFigure();

            // Adjusting radii to the bounds of they are too large
            radiusTopLeft = Math.Abs(radiusTopLeft);
            radiusTopRight = Math.Abs(radiusTopRight);
            radiusBottomRight = Math.Abs(radiusBottomRight);
            radiusBottomLeft = Math.Abs(radiusBottomLeft);
            float maxDiameterWidth = Math.Max(radiusTopLeft + radiusTopRight, radiusBottomLeft + radiusBottomRight);
            float maxDiameterHeight = Math.Max(radiusTopLeft + radiusBottomLeft, radiusTopRight + radiusBottomRight);
            if (maxDiameterWidth > bounds.Width || maxDiameterHeight > bounds.Height)
            {
                float scale = Math.Min(bounds.Width / maxDiameterWidth, bounds.Height / maxDiameterHeight);
                radiusTopLeft *= scale;
                radiusTopRight *= scale;
                radiusBottomRight *= scale;
                radiusBottomLeft *= scale;
            }

            // top left arc
            var corner = new RectangleF(bounds.Location, new SizeF(radiusTopLeft * 2f, radiusTopLeft * 2f));
            if (radiusTopLeft > 0f)
                path.AddArc(corner, 180f, 90f);
            else
                path.AddLine(corner.Location, corner.Location);

            // top right
            if (radiusTopRight != radiusTopLeft)
                corner.Size = new SizeF(radiusTopRight * 2f, radiusTopRight * 2f);
            corner.X = bounds.Right - corner.Width;
            if (radiusTopRight > 0f)
                path.AddArc(corner, 270f, 90f);
            else
                path.AddLine(corner.Location, corner.Location);

            // bottom right arc
            if (radiusBottomRight != radiusTopRight)
            {
                corner.Size = new SizeF(radiusBottomRight * 2f, radiusBottomRight * 2f);
                corner.X = bounds.Right - corner.Width;
            }

            corner.Y = bounds.Bottom - corner.Height;
            if (radiusBottomRight > 0f)
                path.AddArc(corner, 0f, 90f);
            else
                path.AddLine(corner.Location, corner.Location);

            // bottom left arc
            if (radiusBottomLeft != radiusBottomRight)
            {
                corner.Size = new SizeF(radiusBottomLeft * 2f, radiusBottomLeft * 2f);
                corner.Y = bounds.Bottom - corner.Height;
            }

            corner.X = bounds.Left;
            if (radiusBottomLeft > 0f)
                path.AddArc(corner, 90f, 90f);
            else
                path.AddLine(corner.Location, corner.Location);

            path.CloseFigure();
        }


        #endregion
    }
}
