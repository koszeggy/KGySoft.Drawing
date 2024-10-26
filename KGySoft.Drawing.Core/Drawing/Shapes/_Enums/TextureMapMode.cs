#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TextureMapMode.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Shapes
{
    /// <summary>
    /// Represents the possible modes how a texture is mapped when filling a shape by a texture <see cref="Brush"/>.
    /// </summary>
    public enum TextureMapMode
    {
        Tile,
        TileFlipX,
        TileFlipY,
        TileFlipXY,
        Clip,
        Extend,
        Center,
        CenterExtend,
        Stretch, // A new texture is generated for each session. Smoothing mode depends on the drawing options. WorkingColorSpace depends on the original texture.
        Zoom, // Same as Stretch but keeps aspect ratio. There is no ZoomExtend mode because when keeping the aspect ratio, the result can have transparent borders.
    }
}