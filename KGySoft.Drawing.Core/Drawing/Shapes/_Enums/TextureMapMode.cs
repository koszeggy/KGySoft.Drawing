#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TextureMapMode.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Diagnostics.CodeAnalysis;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents the possible modes how a texture is mapped when filling a shape by a texture <see cref="Brush"/>.
    /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_TextureMapMode.htm">online help</a> for image examples.</div>
    /// </summary>
    /// <example>
    /// <para>The following table demonstrates the possible <see cref="TextureMapMode"/> values and their effect:
    /// <table class="table is-hoverable"><thead><tr><th width="80%">Description</th><th width="20%">Image Example</th></tr></thead><tbody>
    /// <tr><td><see cref="Tile"/>: The texture is tiled using the same orientation. <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see>
    /// is <see langword="true"/>, so the transparent pixels of the texture don't affect the background.</td>
    /// <td><img src="../Help/Images/BrushTextureTile.png" alt="Texture brush with Tile map mode. AntiAliasing = true, AlphaBlending = true."/></td></tr>
    /// <tr><td><see cref="TileFlipX"/>: The texture is tiled, mirroring the adjacent tiles horizontally in each row. <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see>
    /// is <see langword="true"/>, so the transparent pixels of the texture don't affect the background.</td>
    /// <td><img src="../Help/Images/BrushTextureTileFlipX.png" alt="Texture brush with TileFlipX map mode. AntiAliasing = true, AlphaBlending = true."/></td></tr>
    /// <tr><td><see cref="TileFlipY"/>: The texture is tiled, mirroring the adjacent tiles vertically in each column. <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see>
    /// is <see langword="true"/>, so the transparent pixels of the texture don't affect the background.</td>
    /// <td><img src="../Help/Images/BrushTextureTileFlipY.png" alt="Texture brush with TileFlipY map mode. AntiAliasing = true, AlphaBlending = true."/></td></tr>
    /// <tr><td><see cref="TileFlipXY"/>: The texture is tiled, mirroring the adjacent tiles horizontally in each row and vertically in each column. <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see>
    /// is <see langword="true"/>, so the transparent pixels of the texture don't affect the background.</td>
    /// <td><img src="../Help/Images/BrushTextureTileFlipXY.png" alt="Texture brush with TileFlipXY map mode. AntiAliasing = true, AlphaBlending = true."/></td></tr>
    /// <tr><td><see cref="Clip"/>: The texture is clipped to the shape without tiling. As in this example the texture does not cover the entire shape, the exceeding regions are simply handled as if they were transparent.
    /// As no offset is specified, the texture's location is the top-left corner of the target <see cref="IReadWriteBitmapData"/>, so it can be seen only in the topmost ellipse.
    /// <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see> is <see langword="false"/>, so the transparent ellipses indicate the regions of the fill operations.</td>
    /// <td><img src="../Help/Images/BrushTextureClip.png" alt="Texture brush with Clip map mode. AntiAliasing = false, AlphaBlending = false."/></td></tr>
    /// <tr><td><see cref="Extend"/>: Similar to <see cref="Clip"/>, except that the exceeding regions are filled by extending the texture's edge pixels.
    /// In this example the texture does not have homogeneous colors at the edges, hence the stripes in the exceeding regions.
    /// <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see> is <see langword="false"/>, so the transparent ellipses indicate the regions of the fill operations.</td>
    /// <td><img src="../Help/Images/BrushTextureExtend.png" alt="Texture brush with Extend map mode. AntiAliasing = false, AlphaBlending = false."/></td></tr>
    /// <tr><td><see cref="Center"/>: Same as <see cref="Clip"/>, but the texture is always centered in the currently filled shape.
    /// <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see> is <see langword="false"/>, so the transparent ellipses indicate the regions of the fill operations.</td>
    /// <td><img src="../Help/Images/BrushTextureCenter.png" alt="Texture brush with Center map mode. AntiAliasing = false, AlphaBlending = false."/></td></tr>
    /// <tr><td><see cref="CenterExtend"/>: Similar to <see cref="Extend"/>, but the texture is always centered in the currently filled shape.
    /// In this example the left edge of the texture is completely transparent, so we don't see pixels extended to the left.
    /// <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see> is <see langword="false"/>, so the transparent ellipses indicate the regions of the fill operations.</td>
    /// <td><img src="../Help/Images/BrushTextureCenterExtend.png" alt="Texture brush with CenterExtend map mode. AntiAliasing = false, AlphaBlending = false."/></td></tr>
    /// <tr><td><see cref="Stretch"/>: The texture is stretched to fill the entire shape. The texture's aspect ratio is not preserved.
    /// <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see> is <see langword="true"/>, so the small transparent regions didn't affect the background.
    /// <see cref="DrawingOptions.AntiAliasing">DrawingOptions.AntiAliasing</see> is also <see langword="true"/>, so the stretching was performed with using an interpolation.</td>
    /// <td><img src="../Help/Images/BrushTextureStretch.png" alt="Texture brush with Stretch map mode. AntiAliasing = true, AlphaBlending = true."/></td></tr>
    /// <tr><td><see cref="Zoom"/>: Similar to <see cref="Stretch"/>, but the texture's aspect ratio is preserved.
    /// <see cref="DrawingOptions.AlphaBlending">DrawingOptions.AlphaBlending</see> is <see langword="false"/>, so the transparent ellipses indicate the regions of the fill operations.
    /// <see cref="DrawingOptions.AntiAliasing">DrawingOptions.AntiAliasing</see> is <see langword="false"/>, so the zooming was performed with the nearest neighbor algorithm.</td>
    /// <td><img src="../Help/Images/BrushTextureZoom.png" alt="Texture brush with Zoom map mode. AntiAliasing = false, AlphaBlending = false."/></td></tr>
    /// </tbody></table></para>
    /// </example>
    public enum TextureMapMode
    {
        /// <summary>
        /// The texture is tiled, using the same orientation for each tile.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_TextureMapMode.htm">online help</a> for image examples.</div>
        /// </summary>
        Tile,

        /// <summary>
        /// The texture is tiled, mirroring the adjacent tiles horizontally in each row.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_TextureMapMode.htm">online help</a> for image examples.</div>
        /// </summary>
        TileFlipX,

        /// <summary>
        /// The texture is tiled, mirroring the adjacent tiles vertically in each column.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_TextureMapMode.htm">online help</a> for image examples.</div>
        /// </summary>
        TileFlipY,

        /// <summary>
        /// The texture is tiled, mirroring the adjacent tiles horizontally in each row and vertically in each column.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_TextureMapMode.htm">online help</a> for image examples.</div>
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "No, it should NOT be TileFlipXy")]
        TileFlipXY,

        /// <summary>
        /// The texture is clipped to the shape without tiling. If the texture does not cover the entire shape, the exceeding regions are simply handled as if they were transparent.
        /// If no offset is specified, the texture's location is the top-left corner of the target <see cref="IReadWriteBitmapData"/>, regardless of the shape's location.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_TextureMapMode.htm">online help</a> for image examples.</div>
        /// </summary>
        Clip,

        /// <summary>
        /// The texture is clipped to the shape without tiling. If the texture does not cover the entire shape, the exceeding regions are filled by extending the texture's edge pixels.
        /// If no offset is specified, the texture's location is the top-left corner of the target <see cref="IReadWriteBitmapData"/>, regardless of the shape's location.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_TextureMapMode.htm">online help</a> for image examples.</div>
        /// </summary>
        Extend,

        /// <summary>
        /// Similar to <see cref="Clip"/>, but the texture is always centered in the currently filled shape.
        /// If there is an offset specified, it is applied relative to the center of the shape.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_TextureMapMode.htm">online help</a> for image examples.</div>
        /// </summary>
        Center,

        /// <summary>
        /// Similar to <see cref="Extend"/>, but the texture is always centered in the currently filled shape.
        /// If there is an offset specified, it is applied relative to the center of the shape.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_TextureMapMode.htm">online help</a> for image examples.</div>
        /// </summary>
        CenterExtend,

        /// <summary>
        /// The texture is stretched to fill the entire shape. The texture's aspect ratio is not preserved.
        /// if <see cref="DrawingOptions.AntiAliasing">DrawingOptions.AntiAliasing</see> is <see langword="true"/>, then the texture is resized with an
        /// automatically selected interpolation; otherwise, the texture is resized using the nearest neighbor algorithm. The used color space for resizing depends
        /// on the <see cref="IBitmapData.WorkingColorSpace"/> of the original texture.
        /// Please note that this generates a new texture for each shape, so it can be slow for large shapes (though a cache with very limited size is used internally).
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_TextureMapMode.htm">online help</a> for image examples.</div>
        /// </summary>
        Stretch,

        /// <summary>
        /// Similar to <see cref="Stretch"/>, but the texture's aspect ratio is preserved.
        /// If the texture does not cover the entire shape, the exceeding regions are simply handled as if they were transparent.
        /// <div style="display: none;"><br/>See the <a href="https://koszeggy.github.io/docs/drawing/html/T_KGySoft_Drawing_Shapes_TextureMapMode.htm">online help</a> for image examples.</div>
        /// </summary>
        Zoom,
    }
}