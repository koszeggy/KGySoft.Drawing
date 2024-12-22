#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GradientWrapMode.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

using KGySoft.Drawing.Imaging;

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents the mode how a gradient is treated when the gradient area is exceeded.
    /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/T_KGySoft_Drawing_Shapes_GradientWrapMode.htm">online help</a> for image examples.</div>
    /// </summary>
    /// <example>
    /// <para>The following table demonstrates the possible <see cref="GradientWrapMode"/> values and their effect:
    /// <table class="table is-hoverable"><thead><tr><th width="80%">Description</th><th width="20%">Image Example</th></tr></thead><tbody>
    /// <tr><td><see cref="Stop"/>: The gradient stops at the specified start and end points, and the exceeding area is simply filled with the specified end colors in both directions.
    /// In this example a horizontal gradient is drawn from 20 to 80, using <see cref="WorkingColorSpace.Linear"/> color space from red to blue.</td>
    /// <td><img src="../Help/Images/BrushLinearGradientWrapModeStop.png" alt="Linear gradient brush with Stop wrap mode. AntiAliasing = true, AlphaBlending = true."/></td></tr>
    /// <tr><td><see cref="Clip"/>: The gradient is clipped at the specified start and end points, as if the exceeding area was transparent. This example is the same gradient as above, but with <see cref="Clip"/> wrap mode.</td>
    /// <td><img src="../Help/Images/BrushLinearGradientWrapModeClip.png" alt="Linear gradient brush with Clip wrap mode. AntiAliasing = true, AlphaBlending = true."/></td></tr>
    /// <tr><td><see cref="Repeat"/>: The gradient is repeated beyond the specified start and end points.
    /// In this example a diagonal gradient is drawn from (-10, 10) to (10, -10), using <see cref="WorkingColorSpace.Srgb"/> color space from white to black.</td>
    /// <td><img src="../Help/Images/BrushLinearGradientWrapModeRepeat.png" alt="Linear gradient brush with Repeat wrap mode. AntiAliasing = true, AlphaBlending = true."/></td></tr>
    /// <tr><td><see cref="Mirror"/>: The gradient is mirrored beyond the specified start and end points. This example is the same gradient as above, but with <see cref="Mirror"/> wrap mode.</td>
    /// <td><img src="../Help/Images/BrushLinearGradientWrapModeMirror.png" alt="Linear gradient brush with Mirror wrap mode. AntiAliasing = true, AlphaBlending = true."/></td></tr>
    /// </tbody></table></para>
    /// <note>A <see cref="GradientWrapMode"/> value is used only when the gradient is specified by two points. When just an angle is specified, the gradient is always stretched to the bounds of the shape.
    /// <br/>See the <strong>Examples</strong> section of the <see cref="Brush.CreateLinearGradient(float,Color32,Color32,WorkingColorSpace)"/> method for an example for such a gradient.
    /// </note>
    /// </example>
    public enum GradientWrapMode
    {
        /// <summary>
        /// The gradient stops at the specified start and end points, and the exceeding area is simply filled with the specified end colors in both directions.
        /// </summary>
        Stop,

        /// <summary>
        /// The gradient is clipped at the specified start and end points, as if the exceeding area was transparent.
        /// </summary>
        Clip,

        /// <summary>
        /// The gradient is repeated beyond the specified start and end points.
        /// </summary>
        Repeat,

        /// <summary>
        /// The gradient is mirrored beyond the specified start and end points.
        /// </summary>
        Mirror
    }
}