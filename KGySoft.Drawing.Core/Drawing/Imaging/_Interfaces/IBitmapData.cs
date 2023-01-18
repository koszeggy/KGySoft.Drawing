#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents the raw data of a bitmap. To create a managed instance use the <see cref="BitmapDataFactory"/> class.
    /// To obtain a readable or writable instance for platform specific bitmaps you can either use the specific <c>GetReadableBitmapData</c>, <c>GetWritableBitmapData</c>
    /// or <c>GetReadWriteBitmapData</c> extension methods when applicable (see the <strong>Remarks</strong> section of the <see cref="N:KGySoft.Drawing"/> namespace for a list
    /// about the supported platforms). Otherwise, you can use the members of the <see cref="BitmapDataFactory"/> class to create a bitmap data for
    /// any managed or unmanaged preallocated buffer of any bitmap implementation.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
    /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for all sources.
    /// </summary>
    /// <seealso cref="IReadableBitmapData"/>
    /// <seealso cref="IWritableBitmapData"/>
    /// <seealso cref="IReadWriteBitmapData"/>
    public interface IBitmapData : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the height of the current <see cref="IBitmapData"/> instance in pixels.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the width of the current <see cref="IBitmapData"/> instance in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the size of the current <see cref="IBitmapData"/> instance in pixels.
        /// </summary>
#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0
        Size Size { get; }
#else
        Size Size => new Size(Width, Height);
#endif

        /// <summary>
        /// Gets a <see cref="PixelFormatInfo"/> of the current <see cref="IBitmapData"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>The value of this property determines how the raw underlying values should be interpreted if the pixels
        /// are accessed by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> or <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see>
        /// methods. Otherwise, in most cases using the members of the interfaces derived from the <see cref="IBitmapData"/> and <see cref="IBitmapDataRow"/> interfaces
        /// work seamlessly.</para>
        /// <para>If this property returns an indexed format (see <see cref="PixelFormatInfo.Indexed"/>),
        /// then the <see cref="Palette"/> property returns a non-<see langword="null"/> value.</para>
        /// </remarks>
        PixelFormatInfo PixelFormat { get; }

        /// <summary>
        /// Gets a <see cref="Imaging.Palette"/> instance representing the colors used in this <see cref="IBitmapData"/> if <see cref="PixelFormat"/> represents an indexed format.
        /// For indexed bitmaps <see cref="PixelFormatInfo.Indexed"/> property of <see cref="PixelFormat"/> returns <see langword="true"/>.
        /// </summary>
        Palette? Palette { get; }

        /// <summary>
        /// Gets the size of a row in bytes, or zero, if this <see cref="IBitmapData"/> instance does not have an actual raw buffer to access.
        /// </summary>
        /// <remarks>
        /// <para>This property can be useful when accessing the bitmap data by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> or <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods.</para>
        /// <para>As <see cref="IBitmapData"/> can represent any custom bitmap data, row size is not guaranteed to be a multiple of 4.</para>
        /// <note>
        /// <para>This property can return 0 if the current <see cref="IBitmapData"/> instance represents a bitmap data without actual raw data or represents a clipped
        /// region where the left edge of the clipping has an offset compared to the original bitmap data.</para>
        /// <para>Even if this property returns a nonzero value, it is possible that raw access does not cover the few last columns.
        /// This may occur in case of indexed <see cref="PixelFormat"/>s if the bitmap data is clipped and the right edge of the clipping does not fall at byte boundary.</para>
        /// </note>
        /// </remarks>
        int RowSize { get; }

        /// <summary>
        /// When accessing pixels of indexed bitmaps, or setting pixels of bitmaps without alpha support, gets the color of the background.
        /// For example, when setting color values with alpha, which are considered opaque, they will be blended with this color before setting the pixel.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> extension method for details and examples.
        /// </summary>
        Color32 BackColor { get; }

        /// <summary>
        /// If this <see cref="IBitmapData"/> represents a bitmap with single bit alpha or with a palette that has a transparent color,
        /// then gets a threshold value for the <see cref="Color32.A">Color32.A</see> property, under which the color is considered transparent. If 0,
        /// then the pixels to be set will never be transparent.
        /// </summary>
        byte AlphaThreshold { get; }

        /// <summary>
        /// Gets a hint indicating the preferred blending mode of this <see cref="IBitmapData"/> instance.
        /// Some operations, such as setting pixels, drawing another bitmap data into this instance and performing other operations
        /// consider the value of this property. Operations that use an <see cref="IQuantizer"/> instance may override the value of this property.
        /// <br/>Default value if not implemented: <see cref="Imaging.BlendingMode.Default"/>. (Only in .NET Core 3.0/.NET Standard 2.1 and above. In earlier targeted frameworks this member must be implemented.)
        /// </summary>
        /// <remarks>
        /// <para>Many pixel formats use the sRGB color space that has gamma corrected color values (just like the <see cref="Color"/> or <see cref="Color32"/> types).
        /// In a color type that uses the sRGB color space the consecutive RGB values don't represent linear light intensity increase.
        /// Instead, a gamma correction of approximately γ = 2.2 is applied to the actual light intensity, so it is adjusted for the perception of the human eye
        /// (in fact, the sRGB color space is linear for the darkest values and uses a γ = 2.4 correction above a threshold limit so the overall average is
        /// somewhere around 2.2). This representation helps distributing RGB values so that 50% represents the apparently half-bright tones:</para>
        /// <list type="table"><item><term>
        /// <div style="text-align:center;">
        /// <img src="../Help/Images/GrayShades.gif" alt="Grayscale color shades with different bit depths"/>
        /// <br/>Grayscale color shades. The difference of RGB values between the neighboring shades is constant in the sRGB color space,
        /// resulting an apparently linear gradient with half-gray tones at the middle.</div>
        /// </term></item></list>
        /// <para>Blending partially transparent (alpha) colors in the sRGB ends up in incorrect results, though it is not always obvious.
        /// The most apparent incorrect results are for colors that have distinct RGB values, for which the result is typically too dark.</para>
        /// <table class="table is-hoverable">
        /// <thead><tr><th width="50%"><div style="text-align:center;">Blending in the sRGB color space</div></th>
        /// <th width="50%"><div style="text-align:center;">Blending in the linear color space</div></th></tr></thead>
        /// <tbody>
        /// <tr><td><div style="text-align:center;">
        /// <img src="../Help/Images/BlendingExampleSrgb.png" alt="Blending colored stripes in the sRGB color space"/>
        /// <br/>Result of blending colors in the sRGB color space. The vertical bars are opaque, whereas the horizontal ones have 50% transparency.
        /// Blending distinct colors often produce too dark results.</div></td>
        /// <td><div style="text-align:center;">
        /// <img src="../Help/Images/BlendingExampleLinear.png" alt="Blending colored stripes in the linear color space"/>
        /// <br/>Result of blending colors in the linear color space. The result seems much more natural.
        /// Note that horizontal bars still have 50% transparency, though they seem brighter now.
        /// </div></td></tr>
        /// <tr><td colspan="2"><div style="text-align:center;">
        /// <img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
        /// <br/>Color hues with alpha gradient. As the image contains transparency the blending with the background is performed by your browser.
        /// <br/>(Side note: just like at the grayscale gradient above, the color gradients also have constant RGB differences between the
        /// horizontal neighboring pixels in the sRGB color space. The examples below are different only in alpha blending
        /// but all of them have the same sRGB color gradients.)</div></td></tr>
        /// <tr><td><div style="text-align:center;">
        /// <img src="../Help/Images/AlphaGradientRgb888White.png" alt="Color hues blended with white background in the sRGB color space"/>
        /// <br/>Color hues blended with white background in the sRGB color space. If the alpha image in the row above looks like this,
        /// then your browser blends colors in the sRGB color space.</div></td>
        /// <td><div style="text-align:center;">
        /// <img src="../Help/Images/AlphaGradientRgb888WhiteLinear.png" alt="Color hues blended with white background in the linear color space"/>
        /// <br/>Color hues blended with white background in the linear color space.
        /// </div></td></tr>
        /// <tr><td><div style="text-align:center;">
        /// <img src="../Help/Images/AlphaGradientRgb888Black.png" alt="Color hues blended with black background in the sRGB color space"/>
        /// <br/>Color hues blended with black background in the sRGB color space.</div></td>
        /// <td><div style="text-align:center;">
        /// <img src="../Help/Images/AlphaGradientRgb888BlackLinear.png" alt="Color hues blended with black background in the linear color space"/>
        /// <br/>Color hues blended with black background in the linear color space. Note that much more colors are visible in the darker regions.
        /// </div></td></tr>
        /// </tbody>
        /// </table>
        /// <para>To blend sRGB colors correctly, the source colors must be converted to the linear color space first,
        /// and then the blended result has to be converted back to the sRGB color space. This has an additional cost,
        /// which is often omitted even by professional image processing applications, libraries and even by web browsers.</para>
        /// <note>Even though blending in the sRGB color space is not quite correct, some pixel formats are optimized exactly for doing that.
        /// With premultiplied sRGB pixel formats sRGB blending is very fast, and forcing color correct blending is particularly expensive.
        /// Use this pixel format only when it is really justified, for example when this is the fastest or default format for
        /// a rendering engine (which is almost always the case, including GDI+, WPF, UWP, WinUI, Skia and many others).
        /// If you want to use color correct blending the best if you use pixel formats with linear gamma (indicated by
        /// the <see cref="PixelFormatInfo.LinearGamma"/> property), or for sRGB color spaces try to avoid premultiplied pixel formats.</note>
        /// <para>When <see cref="BlendingMode"/> is <see cref="Imaging.BlendingMode.Default"/>, the chosen blending mode is chosen based on the context.
        /// For example, when setting a pixel using the <see cref="Color32"/> type, the sRGB blending will be picked (unless <see cref="PixelFormat"/>
        /// has the <see cref="PixelFormatInfo.LinearGamma"/> flag enabled) because it is faster, and it is the default behavior for most applications.</para>
        /// <para>For some operations, such as drawing a bitmap data into another one by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see>
        /// methods choose the blending strategy based on the <see cref="BlendingMode"/> of the target bitmap data.</para>
        /// <para>If this bitmap data has an indexed format and its <see cref="Palette"/> is set, then the strategy is determined by the <see cref="Imaging.Palette"/>
        /// instance. The default strategy is <see cref="Imaging.BlendingMode.Srgb"/> because <see cref="Imaging.Palette"/> uses sRGB color values but you can
        /// create an instance by the appropriate <see cref="Imaging.Palette"/> constructors or factory methods that uses linear blending.</para>
        /// <para>When you use a quantizer for some operations, it may also suppress the value of this property. As quantizers are limited to the 32 bit ARGB color space,
        /// the predefined <see cref="IQuantizer"/> implementations in this library also use sRGB blending by default but you can override it by
        /// the <see cref="PredefinedColorsQuantizer.ConfigureBlendingMode">PredefinedColorsQuantizer.ConfigureBlendingMode</see>
        /// and <see cref="OptimizedPaletteQuantizer.ConfigureBlendingMode">OptimizedPaletteQuantizer.ConfigureBlendingMode</see> methods.</para>
        /// </remarks>
#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0
        BlendingMode BlendingMode { get; }
#else
        BlendingMode BlendingMode => BlendingMode.Default;
#endif

        /// <summary>
        /// Gets whether this <see cref="IBitmapData"/> instance is disposed.
        /// </summary>
        bool IsDisposed { get; }

        #endregion
    }
}
