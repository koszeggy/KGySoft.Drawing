using System.Drawing;
using System.Drawing.Imaging;

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents a quantization for the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Quantization,Dithering)">ConvertPixelFormat</see> extension method.
    /// </summary>
    public enum Quantization
    {
        /// <summary>
        /// If the target <see cref="PixelFormat"/> is an indexed one with less colors than the source format, then a default fix palette will be used.
        /// </summary>
        DefaultColors,

        /// <summary>
        /// If the target <see cref="PixelFormat"/> is an indexed one, then an optimized palette will be generated.
        /// </summary>
        OptimizePalette
    }
}
