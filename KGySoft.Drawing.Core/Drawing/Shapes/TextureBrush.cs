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
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal sealed class TextureBrush<TMapper> : TextureBasedBrush<TMapper>
        where TMapper : struct, TextureBasedBrush.ITextureMapper
    {
        #region Fields

        private readonly IBitmapDataInternal texture;
        private readonly TextureMapMode mapMode;

        #endregion

        #region Properties
        
        private protected override bool HasAlpha { get; }

        #endregion

        #region Constructors

        internal TextureBrush(IReadableBitmapData texture, bool hasAlphaHint, TextureMapMode mapMode)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture), PublicResources.ArgumentNull);
            this.mapMode = mapMode;
            this.texture = texture as IBitmapDataInternal ?? new BitmapDataWrapper(texture, true, false);
            HasAlpha = hasAlphaHint && texture.HasAlpha();
        }

        #endregion

        #region Methods

        private protected override IBitmapDataInternal GetTexture(IAsyncContext context, RawPath rawPath, DrawingOptions drawingOptions, out bool disposeTexture, out Point offset)
        {
            IBitmapDataInternal result = texture;

            if (mapMode is TextureMapMode.Stretch or TextureMapMode.Zoom)
            {
                // TODO: 1 item cache (key: size+AA+KeepAspect) - make sure DoResize does not use array pooling so no dispose is needed
                result = (IBitmapDataInternal)texture.DoResize(context, rawPath.Bounds.Size, drawingOptions.AntiAliasing ? ScalingMode.Auto : ScalingMode.NearestNeighbor, mapMode is TextureMapMode.Zoom)!;
                
                // Result above is null if cancellation occurred. This is one of the very few cases where we throw an OperationCanceledException
                // instead of returning null. This prevents accessing the texture in the caller as well as creating an unnecessary region scanner, etc.
                context.ThrowIfCancellationRequested();
            }

            offset = mapMode >= TextureMapMode.Center
                ? new Point(-(((rawPath.Bounds.Width - result.Width) >> 1) + rawPath.Bounds.Left), -(((rawPath.Bounds.Height - result.Height) >> 1) + rawPath.Bounds.Top))
                : Point.Empty;

            disposeTexture = result != texture;
            return result;
        }

        #endregion
    }
}
