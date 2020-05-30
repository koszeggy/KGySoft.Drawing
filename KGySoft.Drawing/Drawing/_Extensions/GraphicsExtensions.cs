#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GraphicsExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security;

using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="Graphics"/> type.
    /// </summary>
    public static class GraphicsExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Draws a rounded rectangle specified by a bounding <see cref="Rectangle"/> and four corner radius values.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> instance to draw on.</param>
        /// <param name="pen">The <see cref="Pen"/> instance to be used for the drawing.</param>
        /// <param name="bounds">A <see cref="Rectangle"/> that bounds the rounded rectangle.</param>
        /// <param name="radiusTopLeft">Size of the top-left radius.</param>
        /// <param name="radiusTopRight">Size of the top-right radius.</param>
        /// <param name="radiusBottomRight">Size of the bottom-right radius.</param>
        /// <param name="radiusBottomLeft">Size of the bottom-left radius.</param>
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics), PublicResources.ArgumentNull);
            if (pen == null)
                throw new ArgumentNullException(nameof(pen), PublicResources.ArgumentNull);

            using (GraphicsPath path = CreateRoundedRectangle(bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft))
            {
                graphics.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// Draws a rounded rectangle specified by a bounding <see cref="Rectangle"/> and a common corner radius value for each corners.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> instance to draw on.</param>
        /// <param name="pen">The <see cref="Pen"/> instance to be used for the drawing.</param>
        /// <param name="bounds">A <see cref="Rectangle"/> that bounds the rounded rectangle.</param>
        /// <param name="cornerRadius">Size of the corner radius for each corners.</param>
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics), PublicResources.ArgumentNull);
            if (pen == null)
                throw new ArgumentNullException(nameof(pen), PublicResources.ArgumentNull);

            using (GraphicsPath path = CreateRoundedRectangle(bounds, cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// Fills a rounded rectangle specified by a bounding <see cref="Rectangle"/> and four custom corner radius values.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> instance to be used for the drawing.</param>
        /// <param name="bounds">A <see cref="Rectangle"/> that bounds the rounded rectangle.</param>
        /// <param name="radiusTopLeft">Size of the top-left radius.</param>
        /// <param name="radiusTopRight">Size of the top-right radius.</param>
        /// <param name="radiusBottomRight">Size of the bottom-right radius.</param>
        /// <param name="radiusBottomLeft">Size of the bottom-left radius.</param>
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics), PublicResources.ArgumentNull);
            if (brush == null)
                throw new ArgumentNullException(nameof(brush), PublicResources.ArgumentNull);

            using (GraphicsPath path = CreateRoundedRectangle(bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft))
            {
                graphics.FillPath(brush, path);
            }
        }

        /// <summary>
        /// Fills a rounded rectangle specified by a bounding <see cref="Rectangle"/> and a common corner radius value for each corners.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> instance to draw on.</param>
        /// <param name="brush">The <see cref="Brush"/> instance to be used for the drawing.</param>
        /// <param name="bounds">A <see cref="Rectangle"/> that bounds the rounded rectangle.</param>
        /// <param name="cornerRadius">Size of the corner radius for each corners.</param>
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics), PublicResources.ArgumentNull);
            if (brush == null)
                throw new ArgumentNullException(nameof(brush), PublicResources.ArgumentNull);

            using (GraphicsPath path = CreateRoundedRectangle(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }

        /// <summary>
        /// Copies the  <see cref="Graphics"/> object provided in <paramref name="graphics"/> parameter to a <see cref="Bitmap"/> instance.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> instance to be converted.</param>
        /// <param name="visibleClipOnly">When <see langword="true"/>, the result will contain only the area represented by <see cref="Graphics.VisibleClipBounds"/> property. When <see langword="false"/>,
        /// the result will contain the image of the whole container source (when a container object is found), where the visible clip bounds can be identified by <see cref="Graphics.VisibleClipBounds"/> in pixels.</param>
        /// <returns>A <see cref="Bitmap"/> object that contains the image content of the source <see cref="Graphics"/> object, or <see langword="null"/>, when the required area of
        /// <paramref name="graphics"/> is empty.</returns>
        /// <remarks>
        /// <note>This method is supported on Windows only.</note>
        /// </remarks>
        /// <exception cref="PlatformNotSupportedException">This method is supported on Windows only.</exception>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        public static Bitmap ToBitmap(this Graphics graphics, bool visibleClipOnly)
        {
            if (!OSUtils.IsWindows)
                throw new PlatformNotSupportedException(Res.RequiresWindows);

            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics), PublicResources.ArgumentNull);

            if (visibleClipOnly && graphics.IsVisibleClipEmpty)
                return null;

            Bitmap result;
            RectangleF visibleRect;
            int sourceLeft, sourceTop, targetWidth, targetHeight;
            GraphicsState state = graphics.Save();
            try
            {
                // resetting the identity matrix so VisibleClipBounds will be correct
                graphics.Transform = new Matrix();

                // obtaining size in pixels
                graphics.PageUnit = GraphicsUnit.Pixel;
                visibleRect = graphics.VisibleClipBounds;
                sourceLeft = (int)visibleRect.Left;
                sourceTop = (int)visibleRect.Top;
                targetWidth = (int)visibleRect.Width;
                targetHeight = (int)visibleRect.Height;

                // there is a source image: copying so transparency is preserved
                if (graphics.GetBackingImage() is Bitmap imgSource)
                {
                    if (!visibleClipOnly)
                        return (Bitmap)imgSource.Clone();

                    result = new Bitmap(targetWidth, targetHeight);
                    using (Graphics graphicsTarget = Graphics.FromImage(result))
                    {
                        graphicsTarget.DrawImage(imgSource, new RectangleF(0f, 0f, targetWidth, targetHeight), visibleRect, GraphicsUnit.Pixel);
                        return result;
                    }
                } 
            }
            finally
            {
                graphics.Restore(state);
            }

            IntPtr dcSource = graphics.GetHdc();
            if (!visibleClipOnly)
            {
                sourceLeft = 0;
                sourceTop = 0;

                // obtaining container Window
                IntPtr hwnd = User32.WindowFromDC(dcSource);
                if (hwnd != IntPtr.Zero)
                {
                    //// Show in whole screen
                    //RECT rect;
                    //GetWindowRect(hwnd, out rect); // the full rect of self control on screen
                    //// Show in screen
                    //GetWindowRect(hwnd, out rect);
                    //left = -rect.Left;
                    //top = -rect.Top;
                    //width = GetDeviceCaps(dcSource, DeviceCap.HORZRES);
                    //height = GetDeviceCaps(dcSource, DeviceCap.VERTRES);
                    //visibleRect.Offset(rect.Left, rect.Top);

                    //// Show in parent control
                    //IntPtr hwndParent = GetParent(hwnd);
                    //if (hwndParent != IntPtr.Zero)
                    //{
                    //    RECT rectParent;
                    //    GetWindowRect(hwndParent, out rectParent);
                    //    left = rectParent.Left - rect.Left;
                    //    top = rectParent.Top - rect.Top;
                    //    width = rectParent.Right - rectParent.Left;
                    //    height = rectParent.Bottom - rectParent.Top;
                    //    visibleRect.Offset(-left, -top);
                    //}
                    //else

                    // Show in container control
                    Rectangle rect = User32.GetClientRect(hwnd);
                    if (rect.Right < visibleRect.Right && rect.Bottom < visibleRect.Bottom)
                    {
                        // Visible rect is larger than client rect: calculating from full size.
                        // This is usually the case when Graphics is created for nonclient area
                        rect = User32.GetWindowRect(hwnd);
                    }

                    targetWidth = rect.Right - rect.Left;
                    targetHeight = rect.Bottom - rect.Top;
                }
                else if (visibleRect.Location != Point.Empty)
                {
                    // no window: surrounding symmetrically or max 100 px
                    targetWidth = (int)(visibleRect.Right + Math.Min(visibleRect.Left, 100f));
                    targetHeight = (int)(visibleRect.Bottom + Math.Min(visibleRect.Top, 100f));
                }
            }

            // the container control is too small
            if (targetWidth <= 0 || targetHeight <= 0)
            {
                graphics.ReleaseHdc(dcSource);
                return null;
            }

            // creating a compatible bitmap
            IntPtr dcTarget = Gdi32.CreateCompatibleDC(dcSource);
            IntPtr hbmResult = Gdi32.CreateCompatibleBitmap(dcSource, targetWidth, targetHeight);
            Gdi32.SelectObject(dcTarget, hbmResult);

            // Copy content
            Gdi32.BitBlt(dcTarget, 0, 0, targetWidth, targetHeight, dcSource, sourceLeft, sourceTop);
            result = Image.FromHbitmap(hbmResult);

            //cleanup
            graphics.ReleaseHdc(dcSource);
            Gdi32.DeleteDC(dcTarget);
            Gdi32.DeleteObject(hbmResult);

            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns the path for a rounded rectangle specified by a bounding <see cref="Rectangle"/> structure and four corner radius values.
        /// </summary>
        /// <param name="bounds">A <see cref="Rectangle"/> structure that bounds the rounded rectangle.</param>
        /// <param name="radiusTopLeft">Size of the top-left radius.</param>
        /// <param name="radiusTopRight">Size of the top-right radius.</param>
        /// <param name="radiusBottomRight">Size of the bottom-right radius.</param>
        /// <param name="radiusBottomLeft">Size of the bottom-left radius.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radiusTopLeft, int radiusTopRight, int radiusBottomRight, int radiusBottomLeft)
        {
            Size size = new Size(radiusTopLeft << 1, radiusTopLeft << 1);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            // top left arc
            if (radiusTopLeft == 0)
                path.AddLine(arc.Location, arc.Location);
            else
                path.AddArc(arc, 180, 90);

            // top right arc
            if (radiusTopRight != radiusTopLeft)
            {
                size = new Size(radiusTopRight << 1, radiusTopRight << 1);
                arc.Size = size;
            }

            arc.X = bounds.Right - size.Width;
            if (radiusTopRight == 0)
                path.AddLine(arc.Location, arc.Location);
            else
                path.AddArc(arc, 270, 90);

            // bottom right arc
            if (radiusTopRight != radiusBottomRight)
            {
                size = new Size(radiusBottomRight << 1, radiusBottomRight << 1);
                arc.X = bounds.Right - size.Width;
                arc.Size = size;
            }

            arc.Y = bounds.Bottom - size.Height;
            if (radiusBottomRight == 0)
                path.AddLine(arc.Location, arc.Location);
            else
                path.AddArc(arc, 0, 90);

            // bottom left arc
            if (radiusBottomRight != radiusBottomLeft)
            {
                arc.Size = new Size(radiusBottomLeft << 1, radiusBottomLeft << 1);
                arc.Y = bounds.Bottom - arc.Height;
            }

            arc.X = bounds.Left;
            if (radiusBottomLeft == 0)
                path.AddLine(arc.Location, arc.Location);
            else
                path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Returns the path for a rounded rectangle specified by a bounding <see cref="Rectangle"/> structure and a common corner radius value for each corners.
        /// </summary>
        /// <param name="bounds">A <see cref="Rectangle"/> structure that bounds the rounded rectangle.</param>
        /// <param name="radius">Size of the corner radius for each corners.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The result must not be disposed.")]
        private static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);

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
            return path;
        }

        #endregion

        #endregion
    }
}
