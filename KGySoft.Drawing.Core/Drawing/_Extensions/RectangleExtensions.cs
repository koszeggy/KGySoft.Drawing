using System.Drawing;

namespace KGySoft.Drawing
{
    internal static class RectangleExtensions
    {
        /// <summary>
        /// Gets whether the rectangle has zero Width OR Height.
        /// Not just faster than the IsEmpty property but also works better when Intersect returns a non-default practically zero rectangle.
        /// </summary>
        internal static bool IsEmpty(this Rectangle rect) => rect.Width == 0 || rect.Height == 0;
    }
}
