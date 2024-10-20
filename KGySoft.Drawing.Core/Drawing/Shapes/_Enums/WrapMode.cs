﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WrapMode.cs
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
    public enum WrapMode
    {
        Tile,
        TileFlipX,
        TileFlipY,
        TileFlipXY,
        NoTile,
        // TODO: Center, Stretch, Shrink, Zoom - in the case not WrapMode but TextureMode?
    }
}