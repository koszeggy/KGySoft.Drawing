#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TextureBasedBrush.cs
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
    internal abstract class TextureBasedBrush<TMapper> : TextureBasedBrush
        where TMapper : struct, TextureBasedBrush.ITextureMapper
    {
        #region Nested Classes

        #region TextureBasedFillSession

        private abstract class TextureBasedFillSession : FillPathSession
        {
            #region Fields

            private readonly bool disposeTexture;
            private readonly TMapper mapper;

            #endregion

            #region Properties

            private protected IBitmapDataInternal Texture { get; }
            private protected TMapper Mapper => mapper;

            #endregion

            #region Constructors

            protected TextureBasedFillSession(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                Texture = owner.GetTexture(rawPath, out disposeTexture);
                mapper = new TMapper();
                mapper.InitTexture(Texture);
            }

            #endregion

            #region Methods

            protected override void Dispose(bool disposing)
            {
                if (disposing && disposeTexture)
                    Texture.Dispose();
                base.Dispose(disposing);
            }

            #endregion
        }

        #endregion

        #region FillSessionColor32 class

        private sealed class FillSessionColor32 : TextureBasedFillSession
        {
            #region Constructors

            internal FillSessionColor32(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);

                TMapper mapper = Mapper;
                int dstY = scanline.RowIndex;
                int srcY = mapper.MapY(dstY);
                if (srcY < 0)
                    return;

                int left = scanline.Left;
                IBitmapDataRowInternal rowSrc = Texture.GetRowCached(srcY);
                IBitmapDataRowInternal rowDst = BitmapData.GetRowCached(dstY);

                if (!Blend)
                {
                    Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < rowDst.Width);
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        int srcX = mapper.MapX(x);
                        if (srcX >= 0 && ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                            rowDst.DoSetColor32(x + left,  rowSrc.DoGetColor32(srcX));
                    }

                    return;
                }

                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < rowDst.Width);
                var colorSpace = WorkingColorSpace;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    int srcX = mapper.MapX(x);
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) != 1)
                        continue;

                    int pos = x + left;
                    Color32 backColor = rowDst.DoGetColor32(pos);
                    Color32 c = rowSrc.DoGetColor32(srcX);
                    rowDst.DoSetColor32(pos, c.A == Byte.MaxValue ? c : c.Blend(backColor, colorSpace));
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                throw null;
                //Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                //int y = scanline.RowIndex;
                //int left = scanline.Left;
                //TMapper textureRow = GetTextureRow(y);
                //IBitmapDataRowInternal row;

                //if (!Blend)
                //{
                //    row = BitmapData.GetRowCached(y);
                //    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                //    {
                //        byte value = scanline.Scanline.GetElementUnchecked(x);
                //        if (value == Byte.MinValue)
                //            continue;

                //        Color32 c = textureRow.GetColor32(x);
                //        row.DoSetColor32(x + left, value == Byte.MaxValue ? c : Color32.FromArgb(c.A == Byte.MaxValue ? value : (byte)(value * c.A / Byte.MaxValue), c));
                //    }

                //    return;
                //}

                //if (textureRow.IsBlank)
                //    return;

                //row = BitmapData.GetRowCached(y);
                //var colorSpace = WorkingColorSpace;
                //for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                //{
                //    byte value = scanline.Scanline.GetElementUnchecked(x);
                //    if (value == Byte.MinValue)
                //        continue;

                //    Color32 c = textureRow.GetColor32(x);
                //    if (value == Byte.MaxValue)
                //    {
                //        if (c.A == Byte.MaxValue)
                //            row.DoSetColor32(x + left, c);
                //        else
                //        {
                //            int pos = x + left;
                //            Color32 backColor = row.DoGetColor32(pos);
                //            row.DoSetColor32(pos, c.Blend(backColor, colorSpace));
                //        }
                //    }
                //    else
                //    {
                //        int pos = x + left;
                //        Color32 backColor = row.DoGetColor32(pos);
                //        row.DoSetColor32(pos, (c.A == Byte.MaxValue
                //            ? Color32.FromArgb(value, c)
                //            : Color32.FromArgb((byte)(value * c.A / Byte.MaxValue), c))
                //            .Blend(backColor, colorSpace));
                //    }
                //}
            }

            #endregion
        }

        #endregion

        #endregion

        #region Methods

        private protected sealed override FillPathSession CreateFillSession(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
        {
            //IQuantizer? quantizer = drawingOptions.Quantizer;
            //IDitherer? ditherer = drawingOptions.Ditherer;
            //bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

            //// If the quantizer or ditherer relies on the actual [possibly already blended] result we perform the operation in two passes
            //if (quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true)
            //    return new TwoPassSolidFillSession(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer, region);

            //// With regular dithering (which implies quantizing, too)
            //if (ditherer != null)
            //    return new SolidFillSessionWithDithering(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer, region);

            //// Quantizing without dithering
            //if (quantizer != null)
            //    return new SolidFillSessionWithQuantizing(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer, region);

            //// There is no quantizing: picking the most appropriate way for the best quality and performance.
            //PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            //bool linearBlending = bitmapData.LinearBlending();
            //bool blend = drawingOptions.AlphaBlending && (HasAlpha || drawingOptions.AntiAliasing);

            //if (pixelFormat.Indexed && (!blend || !HasAlpha))
            //    return new SolidFillSessionIndexed(this, context, bitmapData, rawPath, bounds, drawingOptions, region);

            //// For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            //if (pixelFormat.Prefers128BitColors || linearBlending && pixelFormat.LinearGamma)
            //{
            //    // Using PColorF only if the actual pixel format really has linear gamma to prevent performance issues
            //    return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true } && (linearBlending || !blend)
            //        ? new SolidFillSessionPColorF(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
            //        : new SolidFillSessionColorF(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
            //}

            //if (pixelFormat.Prefers64BitColors)
            //{
            //    return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false } && (!linearBlending || !blend)
            //        ? new SolidFillSessionPColor64(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
            //        : new SolidFillSessionColor64(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
            //}

            //return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false } && (!linearBlending || !blend)
            //    ? new SolidFillSessionPColor32(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
            //    : new FillSessionColor32(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
            return new FillSessionColor32(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
        }

        private protected sealed override DrawThinPathSession CreateDrawThinPathSession(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
        {
            if (region != null)
                return base.CreateDrawThinPathSession(context, bitmapData, rawPath, bounds, drawingOptions, region);

            //Debug.Assert(!drawingOptions.AntiAliasing && (!drawingOptions.AlphaBlending || !HasAlpha));
            //IQuantizer? quantizer = drawingOptions.Quantizer;
            //IDitherer? ditherer = drawingOptions.Ditherer;
            //bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

            //Debug.Assert(quantizer?.InitializeReliesOnContent != true && ditherer?.InitializeReliesOnContent != true);

            //// With regular dithering (which implies quantizing, too)
            //if (ditherer != null)
            //    return new SolidDrawSessionWithDithering(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer);

            //// Quantizing without dithering
            //if (quantizer != null)
            //    return new SolidDrawSessionWithQuantizing(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer);

            //// There is no quantizing: picking the most appropriate way for the best quality and performance.
            //PixelFormatInfo pixelFormat = bitmapData.PixelFormat;

            //if (pixelFormat.Indexed)
            //    return new SolidDrawSessionIndexed(this, context, bitmapData, rawPath, bounds, drawingOptions);

            //// For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            //if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            //{
            //    return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true }
            //        ? new SolidDrawSessionPColorF(this, context, bitmapData, rawPath, bounds, drawingOptions)
            //        : new SolidDrawSessionColorF(this, context, bitmapData, rawPath, bounds, drawingOptions);
            //}

            //if (pixelFormat.Prefers64BitColors)
            //{
            //    return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
            //        ? new SolidDrawSessionPColor64(this, context, bitmapData, rawPath, bounds, drawingOptions)
            //        : new SolidDrawSessionColor64(this, context, bitmapData, rawPath, bounds, drawingOptions);
            //}

            //return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
            //    ? new SolidDrawSessionPColor32(this, context, bitmapData, rawPath, bounds, drawingOptions)
            //    : new SolidDrawSessionColor32(this, context, bitmapData, rawPath, bounds, drawingOptions);
            throw null;
        }

        #endregion
    }

    /// <summary>
    /// Base class for coordinate-based brushes. See the comment in <see cref="SolidBrush"/> for the reason of the many nested classes.
    /// </summary>
    internal abstract class TextureBasedBrush : Brush
    {
        #region Nested Types

        #region Nested Interfaces

        internal interface ITextureMapper
        {
            #region Methods

            void InitTexture(IBitmapDataInternal texture);
            int MapY(int y);
            int MapX(int x);

            #endregion
        }

        #endregion

        #region Nested classes

        #region Fill

        #region TextureBasedFillSession

        //private abstract class TextureBasedFillSession : FillPathSession
        //{
        //    #region Fields

        //    private readonly IBitmapDataInternal texture;
        //    private readonly bool disposeTexture;

        //    #endregion

        //    #region Constructors

        //    protected TextureBasedFillSession(TextureBasedBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
        //        : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
        //    {
        //        texture = owner.GetTexture(rawPath, out disposeTexture);
        //    }

        //    #endregion

        //    #region Methods

        //    //protected Color32 GetColor32(int x, int y)
        //    //{
        //    //    // TODO: offset, wrap mode
        //    //    return texture.DoGetColor32(x % texture.Width, y % texture.Height);
        //    //}

        //    protected override void Dispose(bool disposing)
        //    {
        //        if (disposing && disposeTexture)
        //            texture.Dispose();
        //        base.Dispose(disposing);
        //    }

        //    #endregion
        //}

        #endregion

        #region FillSessionColor32 class

        //private sealed class FillSessionColor32 : TextureBasedFillSession
        //{
        //    #region Constructors

        //    internal FillSessionColor32(TextureBasedBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
        //        : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
        //    {
        //    }

        //    #endregion

        //    #region Methods

        //    internal override void ApplyScanlineSolid(in RegionScanline scanline)
        //    {
        //        Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);

        //        int y = scanline.RowIndex;
        //        IBitmapDataRowInternal row = BitmapData.GetRowCached(y);
        //        int left = scanline.Left;
        //        Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < row.Width);

        //        if (!Blend)
        //        {
        //            for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
        //            {
        //                if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
        //                    row.DoSetColor32(x + left, GetColor32(x, y));
        //            }

        //            return;
        //        }

        //        var colorSpace = WorkingColorSpace;
        //        for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
        //        {
        //            if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) != 1)
        //                continue;

        //            int pos = x + left;
        //            Color32 backColor = row.DoGetColor32(pos);
        //            Color32 c = GetColor32(x, y);
        //            row.DoSetColor32(pos, c.A == Byte.MaxValue ? c : c.Blend(backColor, colorSpace));
        //        }
        //    }

        //    internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
        //    {
        //        Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
        //        int y = scanline.RowIndex;
        //        IBitmapDataRowInternal row = BitmapData.GetRowCached(y);
        //        int left = scanline.Left;

        //        if (!Blend)
        //        {
        //            for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
        //            {
        //                byte value = scanline.Scanline.GetElementUnchecked(x);
        //                if (value == Byte.MinValue)
        //                    continue;

        //                Color32 c = GetColor32(x, y);
        //                row.DoSetColor32(x + left, value == Byte.MaxValue ? c : Color32.FromArgb(c.A == Byte.MaxValue ? value : (byte)(value * c.A / Byte.MaxValue), c));
        //            }

        //            return;
        //        }

        //        var colorSpace = WorkingColorSpace;
        //        for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
        //        {
        //            byte value = scanline.Scanline.GetElementUnchecked(x);
        //            if (value == Byte.MinValue)
        //                continue;

        //            Color32 c = GetColor32(x, y);
        //            if (value == Byte.MaxValue)
        //            {
        //                if (c.A == Byte.MaxValue)
        //                    row.DoSetColor32(x + left, c);
        //                else
        //                {
        //                    int pos = x + left;
        //                    Color32 backColor = row.DoGetColor32(pos);
        //                    row.DoSetColor32(pos, c.Blend(backColor, colorSpace));
        //                }
        //            }
        //            else
        //            {
        //                int pos = x + left;
        //                Color32 backColor = row.DoGetColor32(pos);
        //                row.DoSetColor32(pos, (c.A == Byte.MaxValue
        //                    ? Color32.FromArgb(value, c)
        //                    : Color32.FromArgb((byte)(value * c.A / Byte.MaxValue), c))
        //                    .Blend(backColor, colorSpace));
        //            }
        //        }
        //    }

        //    #endregion
        //}

        #endregion

        #endregion

        #endregion

        #region Nested Structs

        private protected struct TextureMapperTile : ITextureMapper
        {
            #region Fields

            private Size size;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture) => size = texture.Size;
            public int MapY(int y) => y % size.Height;
            public int MapX(int x) => x % size.Width;
            
            #endregion
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        internal static TextureBasedBrush Create(IReadableBitmapData texture, WrapMode wrapMode = WrapMode.Tile, bool hasAlphaHint = true)
        {
            switch (wrapMode)
            {
                case WrapMode.Tile:
                    return new TextureBrush<TextureMapperTile>(texture, hasAlphaHint);
                default:
                    throw new InvalidOperationException(Res.InternalError($"Unhandled wrap mode: {wrapMode}"));
            }
        }

        #endregion

        #region Instance Methods
        
        private protected abstract IBitmapDataInternal GetTexture(RawPath rawPath, out bool disposeTexture);

        #endregion

        #endregion
    }
}
