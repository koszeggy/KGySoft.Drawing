using System.Runtime.InteropServices;
namespace KGySoft.Drawing.WinApi
{
    using System;
    using System.Drawing;

    /// <summary>
    /// The RGBQUAD structure describes a color consisting of relative intensities of red, green, and blue.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RGBQUAD
    {
        /// <summary>
        /// The intensity of blue in the color.
        /// </summary>
        internal byte rgbBlue;

        /// <summary>
        /// The intensity of green in the color.
        /// </summary>
        internal byte rgbGreen;

        /// <summary>
        /// The intensity of red in the color.
        /// </summary>
        internal byte rgbRed;

        /// <summary>
        /// This member is reserved and must be zero.
        /// </summary>
        internal byte rgbReserved;

        internal RGBQUAD(Color color)
        {
            rgbRed = color.R;
            rgbGreen = color.G;
            rgbBlue = color.B;
            rgbReserved = 0;
        }

        internal bool EqualsWithColor(Color color)
        {
            return color != Color.Empty && rgbRed == color.R && rgbGreen == color.G && rgbBlue == color.B;
        }
    }
}
