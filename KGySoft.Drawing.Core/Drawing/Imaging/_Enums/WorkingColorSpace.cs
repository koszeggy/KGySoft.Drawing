#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WorkingColorSpace.cs
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

using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents the preferred working color space for various operations such as alpha blending, measuring distance of colors, interpolation,
    /// quantizing, dithering and some other operations. The working color space can be specified independently from the color space of the actual pixel format
    /// of an <see cref="IBitmapData"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>The working color space can be specified at various places in KGy SOFT Drawing Libraries:
    /// <list type="bullet">
    /// <item>At the lowest level, you can specify it when blending colors manually, for example by
    /// the <see cref="ColorExtensions.Blend(Color32, Color32, WorkingColorSpace)">ColorExtensions.Blend</see> method.</item>
    /// <item><see cref="IBitmapData"/> instances have a <see cref="IBitmapData.WorkingColorSpace"/> property, which can be set by the factory methods
    /// such as the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataFactory.CreateBitmapData">BitmapDataFactory.CreateBitmapData</see> methods
    /// or by the <c>GetReadWriteBitmapData</c> methods for the technology-specific bitmap types.
    /// The <see cref="IBitmapData.WorkingColorSpace">IBitmapData.WorkingColorSpace</see> property is considered when setting pixels with transparency
    /// and the pixel format of the bitmap data does not support transparency so the color has to be blended with <see cref="IBitmapData.BackColor">IBitmapData.BackColor</see>,
    /// or when bitmap data instances are drawn into each other.</item>
    /// <item>The <see cref="Palette"/> class also has a <see cref="Palette.WorkingColorSpace"/> property, which is used by the <see cref="Palette.GetNearestColor"/>
    /// and <see cref="Palette.GetNearestColorIndex"/> methods.</item>
    /// <item>Quantizers also have their own working color space configuration. You can use the <see cref="PredefinedColorsQuantizer.ConfigureColorSpace">PredefinedColorsQuantizer.ConfigureColorSpace</see>
    /// and <see cref="OptimizedPaletteQuantizer.ConfigureColorSpace">OptimizedPaletteQuantizer.ConfigureColorSpace</see> methods to configure the
    /// working color space of the built-in quantizers of KGy SOFT Drawing Libraries.</item>
    /// <item>Ditherers may also have specific behavior for the different color spaces. The ditherer implementations in KGy SOFT Drawing Libraries
    /// always take the working color space of the corresponding quantizer exposed by the <see cref="IQuantizingSession.WorkingColorSpace">IQuantizingSession.WorkingColorSpace</see> property.</item>
    /// </list></para>
    /// <para>Most common color representations (just like the <see cref="Color"/> or <see cref="Color32"/> types that use 8 bit color channels) use the sRGB color space
    /// that has gamma corrected color values. In a color type that uses the sRGB color space the consecutive RGB values don't represent linear light intensity increase.
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
    /// The most apparent incorrect results are for colors that have disjunct RGB values, for which the result is typically too dark.</para>
    /// <table class="table is-hoverable">
    /// <thead><tr><th width="50%"><div style="text-align:center;">Blending in the sRGB color space</div></th>
    /// <th width="50%"><div style="text-align:center;">Blending in the linear color space</div></th></tr></thead>
    /// <tbody>
    /// <tr><td><div style="text-align:center;">
    /// <img src="../Help/Images/BlendingExampleSrgb.png" alt="Blending colored stripes in the sRGB color space"/>
    /// <br/>Result of blending colors in the sRGB color space. The vertical bars are opaque, whereas the horizontal ones have 50% transparency.
    /// Blending colors with disjunct RGB components often produce too dark results.</div></td>
    /// <td><div style="text-align:center;">
    /// <img src="../Help/Images/BlendingExampleLinear.png" alt="Blending colored stripes in the linear color space"/>
    /// <br/>Result of blending colors in the linear color space. The result seems much more natural.
    /// Note that horizontal bars still have 50% transparency, though they seem brighter now.</div></td>
    /// </tr>
    /// <tr><td colspan="2"><div style="text-align:center;">
    /// <img src="../Help/Images/AlphaGradient.png" alt="Color hues with alpha gradient"/>
    /// <br/>Color hues with alpha gradient. As the image contains transparency the blending with the background is performed by your browser.
    /// <br/>(Side note: just like at the grayscale gradient above, the color gradients also have constant RGB differences between the
    /// horizontal neighboring pixels in the sRGB color space. The examples below are different only in alpha blending
    /// but all of them have the same sRGB color gradients.)</div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <img src="../Help/Images/AlphaGradientRgb888White.png" alt="Color hues blended with white background in the sRGB color space"/>
    /// <br/>Color hues blended with white background in the sRGB color space. If the alpha image in the row above looks like this,
    /// then your browser blends colors in the sRGB color space.</div></td>
    /// <td><div style="text-align:center;">
    /// <img src="../Help/Images/AlphaGradientRgb888WhiteLinear.png" alt="Color hues blended with white background in the linear color space"/>
    /// <br/>Color hues blended with white background in the linear color space.</div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <img src="../Help/Images/AlphaGradientRgb888Black.png" alt="Color hues blended with black background in the sRGB color space"/>
    /// <br/>Color hues blended with black background in the sRGB color space.</div></td>
    /// <td><div style="text-align:center;">
    /// <img src="../Help/Images/AlphaGradientRgb888BlackLinear.png" alt="Color hues blended with black background in the linear color space"/>
    /// <br/>Color hues blended with black background in the linear color space. Note that much more colors are visible in the darker regions.</div></td>
    /// </tr>
    /// </tbody></table>
    /// <para>To blend sRGB colors correctly, the source colors must be converted to the linear color space first,
    /// and then the blended result has to be converted back to the sRGB color space. This has an additional cost,
    /// which is often omitted even by professional image processing applications, libraries and even by web browsers.</para>
    /// <note>Even though blending in the sRGB color space is not quite correct, some pixel formats are optimized exactly for doing that.
    /// With premultiplied sRGB pixel formats sRGB blending is very fast, and forcing color correct blending is particularly expensive.
    /// Use such pixel formats only when it is really justified, for example when this is the fastest or default format for
    /// a rendering engine (which is almost always the case, including GDI+, WPF, UWP, WinUI, Skia and many other engines).
    /// If you want to use color correct blending the best if you use pixel formats with linear gamma (indicated by
    /// the <see cref="PixelFormatInfo.LinearGamma"/> property), or for sRGB pixel formats try to avoid premultiplied formats
    /// to prevent the overhead of unnecessary conversions back and forth.</note>
    /// <h2>Quantizing</h2>
    /// <para>When reducing the number of colors to some specified set of colors (either by using a <see cref="Palette"/> of predefined entries or some custom
    /// quantizer function) measuring the distance between colors may return different results depending on the used color space.
    /// Looking up for the nearest colors in the sRGB color space tends to turn the darker shades too bright, though
    /// the more colors the quantizer uses the less noticeable the difference is.</para>
    /// <table class="table is-hoverable">
    /// <thead><tr><th width="50%"><div style="text-align:center;">Quantizing in the sRGB color space</div></th>
    /// <th width="50%"><div style="text-align:center;">Quantizing in the linear color space</div></th></tr></thead>
    /// <tbody>
    /// <tr><td colspan="2"><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarring.png" alt="Test image &quot;Girl with a Pearl Earring&quot;"/>
    /// <br/>Original test image "Girl with a Pearl Earring"</div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringBWSrgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with back and white palette, quantized in the sRGB color space"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white</see> palette in the sRGB color space.</div></td>
    /// <td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringBWLinear.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with back and white palette, quantized in the linear color space"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white</see> palette in the linear color space.</div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringRgb111Srgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with RGB111 palette, quantized in the sRGB color space"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.FromCustomPalette(Color[],Color,byte)">RGB111</see> palette in the sRGB color space.</div></td>
    /// <td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringRgb111Linear.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with RGB111 palette, quantized in the linear color space"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.FromCustomPalette(Color[],Color,byte)">RGB111</see> palette in the linear color space.</div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringDefault8bppSrgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with system default 8 BPP palette, quantized in the sRGB color space"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP</see> palette in the sRGB color space.
    /// For more colors the difference is much less noticeable but the background is slightly brighter than in the original image.</div></td>
    /// <td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringDefault8bppLinear.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with system default 8 BPP  palette, quantized in the linear color space"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP</see> palette in the linear color space.
    /// For more colors the difference is much less noticeable but the background is slightly darker than in the sRGB result.</div></td>
    /// </tr>
    /// </tbody></table>
    /// <h2>Dithering</h2>
    /// <para>When quantizing is combined with dithering, the ditherers may also respect the working color space of the quantizers. Similarly to the non-dithered results,
    /// the difference between working in the sRGB or linear color space gets less and less noticeable with using more and more colors.</para>
    /// <table class="table is-hoverable">
    /// <thead><tr><th width="50%"><div style="text-align:center;">Quantizing with dithering in the sRGB color space</div></th>
    /// <th width="50%"><div style="text-align:center;">Quantizing with dithering in the linear color space</div></th></tr></thead>
    /// <tbody>
    /// <tr><td colspan="2"><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarring.png" alt="Test image &quot;Girl with a Pearl Earring&quot;"/>
    /// <br/>Original test image "Girl with a Pearl Earring"</div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringBWDitheredB8Srgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with back and white palette, quantized in the sRGB color space using Bayer 8x8 dithering"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white</see> palette in the sRGB color space using <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering.
    /// The result is too bright. Please note though that if your browser resizes the image in the sRGB color space it might appear correct, in which case make sure you open it in a new tab and set the zoom to 100%.</div></td>
    /// <td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringBWDitheredB8Linear.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with back and white palette, quantized in the linear color space using Bayer 8x8 dithering"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.BlackAndWhite">black and white</see> palette in the linear color space using <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering.
    /// The result has correct brightness. Please note though that if your browser resizes the image in the sRGB color space it might appear too dark, in which case make sure you open it in a new tab and set the zoom to 100%.</div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringRgb111DitheredFSSrgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with RGB111 palette, quantized in the sRGB color space using Floyd-Steinberg dithering"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.FromCustomPalette(Color[],Color,byte)">RGB111</see> palette in the sRGB color space using <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering.
    /// The result is too bright. Please note though that if your browser resizes the image in the sRGB color space it might appear correct, in which case make sure you open it in a new tab and set the zoom to 100%.</div></td>
    /// <td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringRgb111DitheredFSLinear.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with RGB111 palette, quantized in the linear color space using Floyd-Steinberg dithering"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.FromCustomPalette(Color[],Color,byte)">RGB111</see> palette in the linear color space using <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering.
    /// The result has correct brightness. Please note though that if your browser resizes the image in the sRGB color space it might appear too dark, in which case make sure you open it in a new tab and set the zoom to 100%.</div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringDefault8bppDitheredFSSrgb.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with system default 8 BPP palette, quantized in the sRGB color space using Floyd-Steinberg dithering"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP</see> palette in the sRGB color space using <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering.
    /// The background is slightly brighter than in the original image but due to the number of colors the difference is barely noticeable.</div></td>
    /// <td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringDefault8bppDitheredFSLinear.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with system default 8 BPP  palette, quantized in the linear color space using Floyd-Steinberg dithering"/>
    /// <br/>Quantizing by <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette">system default 8 BPP</see> palette in the linear color space using <see cref="ErrorDiffusionDitherer.FloydSteinberg">Floyd-Steinberg</see> dithering.
    /// Due to the number of colors the difference is barely noticeable between the sRGB and the linear result.</div></td>
    /// </tr>
    /// </tbody></table>
    /// <h2>Resizing</h2>
    /// <para>When resizing images with some interpolation the working color space may also affect the result. When resizing true color images the difference might be negligible;
    /// however, if the source image has only a few colors with high frequency areas (typically dithered images as above) the working color space used for the resizing makes a noticeable difference.</para>
    /// <table class="table is-hoverable">
    /// <thead><tr><th width="50%"><div style="text-align:center;">Resizing in the sRGB color space</div></th>
    /// <th width="50%"><div style="text-align:center;">Resizing in the linear color space</div></th></tr></thead>
    /// <tbody>
    /// <tr><td colspan="2"><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringRgb111DitheredB8Linear.gif" alt="Test image &quot;Girl with a Pearl Earring&quot; with RGB111 palette, quantized in the linear color space using Bayer 8x8 dithering"/>
    /// <br/>Test image "Girl with a Pearl Earring" quantized by <see cref="PredefinedColorsQuantizer.FromCustomPalette(Color[],Color,byte)">RGB111</see> palette in the linear color space
    /// using <see cref="OrderedDitherer.Bayer8x8">Bayer 8x8</see> dithering.</div></td>
    /// </tr>
    /// <tr><td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringRgb111ResizedSrgb.png" alt="Quantized test image &quot;Girl with a Pearl Earring&quot; resized in the sRGB color space"/>
    /// <br/>Resizing the quantized image with bicubic interpolation in the sRGB color space. The result is too dark.
    /// Ironically, if the source image would have been quantized in the sRGB color space so it was too bright, then the resized result would seem quite correct.</div></td>
    /// <td><div style="text-align:center;">
    /// <img src="../Help/Images/GirlWithAPearlEarringRgb111ResizedLinear.png" alt="Quantized test image &quot;Girl with a Pearl Earring&quot; resized in the linear color space"/>
    /// <br/>Resizing the quantized image with bicubic interpolation in the linear color space. The result preserved the original brightness.</div></td>
    /// </tr>
    /// </tbody></table>
    /// </remarks>
    public enum WorkingColorSpace
    {
        /// <summary>
        /// Represents the default color space mode most optimal for the current operation or actual color space.
        /// When working with <see cref="Palette"/> entries or an <see cref="IQuantizingSession"/> the default option always chooses
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