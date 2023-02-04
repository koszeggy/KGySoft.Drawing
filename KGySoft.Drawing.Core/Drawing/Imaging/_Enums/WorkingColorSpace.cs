#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WorkingColorSpace.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

using System.Drawing;

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents the preferred working color space for various operations such as alpha blending, measuring distance of colors, interpolation,
    /// quantizing, dithering and some other operations. The working color space can be specified independently from the color space of the actual pixel format
    /// of an <see cref="IBitmapData"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>Most common color representations with limited values for the individual color channels (just like the <see cref="Color"/>
    /// or <see cref="Color32"/> types that use 8 bit color channels) use the sRGB color space that has gamma corrected color values.
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
    /// <h2>Alpha Blending</h2>
    /// <para>Blending partially transparent (alpha) colors in the sRGB color space ends up in incorrect results, though it is not always obvious.
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
    /// <h2>Quantizing</h2>
    /// TODO: measuring color distance, image examples
    /// <h2>Dithering</h2>
    /// TODO: image examples
    /// <h2>Resizing</h2>
    /// TODO: image examples
    /// </remarks>
    public enum WorkingColorSpace
    {
        /// <summary>
        /// Represents the default color space mode most optimal for the current operation or actual color space.
        /// When working with <see cref="Palette"/> entries or an <see cref="IQuantizingSession"/> the default option chooses always
        /// the sRGB color space. When working with an <see cref="IBitmapData"/> directly, then the selected working color space
        /// depends on the <see cref="IBitmapData.PixelFormat"/> of the target bitmap data.
        /// </summary>
        Default,

        /// <summary>
        /// Indicates that the linear color space should be used when performing operations on colors.
        /// If the actual color space of the source is sRGB, then the operation has some overhead due to the conversions back and forth.
        /// </summary>
        Linear,

        /// <summary>
        /// Indicates that the sRGB color space should be used when performing operations on colors.
        /// </summary>
        Srgb
    }
}