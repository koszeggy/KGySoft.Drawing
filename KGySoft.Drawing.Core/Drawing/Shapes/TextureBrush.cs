#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TextureBrush.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal sealed class TextureBrush<TMapper> : TextureBasedBrush<TMapper>
        where TMapper : struct, TextureBasedBrush.ITextureMapper
    {
        #region Constants

        private const int cachingThreshold = 1 << 24; // 16 MB

        #endregion

        #region Fields

        private readonly IBitmapDataInternal texture;
        private readonly TextureMapMode mapMode;
        private readonly Point displayOffset;

        // An exactly 4-item cache for resized textures (when mapMode is Stretch or Zoom).
        // Key is created from size, | AA (1 bit) | KeepAspect (1 bit), using the sign bits for the booleans.
        // As Brush is not disposable, we must ensure that cached items contain only managed resources, without using pooled arrays.
        // NOTE: Not using IThreadSafeCacheAccessor because we need to manually add items, but not using ThreadSafeDictionary either to maintain capacity.
        // The actual cache will be a Cache instance wrapped into a LockingDictionary.
        private LockingDictionary<ulong, IBitmapDataInternal>? textureCache;

        #endregion

        #region Properties

        #region Internal Properties
        
        internal override bool HasAlpha { get; }

        #endregion

        #region Private Properties

        private LockingDictionary<ulong, IBitmapDataInternal> TextureCache
        {
            get
            {
                // Using a Cache inside to maintain capacity
                if (textureCache == null)
                    Interlocked.CompareExchange(ref textureCache, new Cache<ulong, IBitmapDataInternal>(4).AsThreadSafe(), null);
                return textureCache;
            }
        }

        #endregion

        #endregion

        #region Constructors

        internal TextureBrush(IReadableBitmapData texture, bool hasAlphaHint, TextureMapMode mapMode, Point offset)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture), PublicResources.ArgumentNull);
            this.mapMode = mapMode;
            displayOffset = new Point(-offset.X, -offset.Y);
            this.texture = texture as IBitmapDataInternal ?? new BitmapDataWrapper(texture, true, false);
            HasAlpha = hasAlphaHint && texture.HasAlpha();
        }

        #endregion

        #region Methods

        private protected override IBitmapDataInternal GetTexture(IAsyncContext context, RawPath rawPath, DrawingOptions drawingOptions, out bool disposeTexture, out Point offset)
        {
            IBitmapDataInternal? result = texture;
            offset = displayOffset;

            if (mapMode is TextureMapMode.Stretch or TextureMapMode.Zoom && rawPath.Bounds.Size != texture.Size)
            {
                // as size cannot be negative, we can use the sign bits for the boolean components of the key
                ulong key = (((ulong)(uint)rawPath.Bounds.Width | (drawingOptions.AntiAliasing ? 1u << 31 : 0u)) << 32)
                    | ((uint)rawPath.Bounds.Height | (mapMode is TextureMapMode.Zoom ? 1u << 31 : 0u));

                if (TextureCache.TryGetValue(key, out result))
                    disposeTexture = false;
                else
                {
                    result = (IBitmapDataInternal)texture.DoResize(context, rawPath.Bounds.Size, drawingOptions.AntiAliasing ? ScalingMode.Auto : ScalingMode.NearestNeighbor, mapMode is TextureMapMode.Zoom)!;

                    // Result above is null if cancellation occurred. This is one of the very few cases where we throw an OperationCanceledException
                    // instead of returning null. This prevents accessing the texture in the caller as well as creating an unnecessary region scanner, etc.
                    context.ThrowIfCancellationRequested();

                    // As Brush is not disposable, caching only when the texture does not use pooled buffers.
                    // Using TryAdd with KeyValuePair to pick always the KGySoft.Collections extension method, which handles LockingDictionary properly.
                    bool canCache = result is ManagedBitmapDataBase { MayUsePooledBuffer: false } && (long)result.RowSize * result.Height < cachingThreshold;
                    if (canCache && !TextureCache.TryAdd(new KeyValuePair<ulong, IBitmapDataInternal>(key, result))) // another thread may have added the same key
                        canCache = false;
                    disposeTexture = !canCache;
                }
            }
            else
                disposeTexture = false;

            if (mapMode >= TextureMapMode.Center)
                offset -= new Size(((rawPath.Bounds.Width - result.Width) >> 1) + rawPath.Bounds.Left, ((rawPath.Bounds.Height - result.Height) >> 1) + rawPath.Bounds.Top);

            return result;
        }

        #endregion
    }
}
