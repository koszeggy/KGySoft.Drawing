#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TextureBrush.cs
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

using System;
using System.Drawing;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal sealed class TextureBrush<TMapper> : TextureBasedBrush<TMapper>
        where TMapper : struct, TextureBasedBrush.ITextureMapper
    {
        #region Fields

        private readonly IBitmapDataInternal texture;

        #endregion

        #region Properties
        
        private protected override bool HasAlpha { get; }

        #endregion

        #region Constructors

        internal TextureBrush(IReadableBitmapData texture, bool hasAlphaHint/*, WrapMode wrapMode TODO*/)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture), PublicResources.ArgumentNull);
            this.texture = texture as IBitmapDataInternal ?? new BitmapDataWrapper(texture, true, false);
            HasAlpha = hasAlphaHint && texture.HasAlpha();
        }

        #endregion

        #region Methods

        private protected override IBitmapDataInternal GetTexture(RawPath rawPath, out bool disposeTexture)
        {
            disposeTexture = false;
            return texture;
        }

        #endregion
    }
}
