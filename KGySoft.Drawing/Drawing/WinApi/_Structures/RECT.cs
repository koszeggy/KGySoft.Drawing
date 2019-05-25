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

        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;

        #endregion

        #region Methods

        internal Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);

        #endregion
    }
}
