#region Used namespaces

using System.Drawing;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        #region Fields

        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        #endregion

        #region Constructors

        internal RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        internal RECT(Rectangle r)
        {
            Left = r.Left;
            Top = r.Top;
            Right = r.Width;
            Bottom = r.Height;
        }

        #endregion

        #region Methods

        internal static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x, y, x + width, y + height);
        }

        internal Rectangle ToRectangle()
        {
            return Rectangle.FromLTRB(Left, Top, Right, Bottom);
        }

        #endregion
    }
}
