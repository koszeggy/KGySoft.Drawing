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
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.CompilerServices;

using KGySoft.Collections;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal abstract class TextureBasedBrush<TMapper> : TextureBasedBrush
        where TMapper : struct, TextureBasedBrush.ITextureMapper
    {
        #region Nested Classes

        #region Fill

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
                Texture = owner.GetTexture(context, rawPath, drawingOptions, out disposeTexture, out Point offset);
                mapper = new TMapper();
                mapper.InitTexture(Texture, offset);
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

        #region FillSessionNoBlend<,,> class

        private sealed class FillSessionNoBlend<TAccessor, TColor, TBaseColor> : TextureBasedFillSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, TBaseColor, _>
            where TColor : unmanaged, IColor<TColor, TBaseColor>
            where TBaseColor : unmanaged, IColor<TBaseColor, TBaseColor>
        {
            #region Constructors

            internal FillSessionNoBlend(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                Debug.Assert(!Blend);
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);

                TMapper mapper = Mapper;
                int dstY = scanline.RowIndex;
                int srcY = mapper.MapY(dstY);
                var accDst = new TAccessor();
                accDst.InitRow(BitmapData.GetRowCached(dstY));
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < BitmapData.Width);

                if (srcY < 0)
                {
                    // Blank texture row. As there is no blending here, setting transparent pixels
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                            accDst.SetColor(x + left, default);
                    }

                    return;
                }

                var accSrc = new TAccessor();
                accSrc.InitRow(Texture.GetRowCached(srcY));
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    int dstX = x + left;
                    int srcX = mapper.MapX(dstX);
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                        accDst.SetColor(dstX, srcX >= 0 ? accSrc.GetColor(srcX) : default);
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                TMapper mapper = Mapper;
                int dstY = scanline.RowIndex;
                int srcY = mapper.MapY(dstY);
                var accDst = new TAccessor();
                accDst.InitRow(BitmapData.GetRowCached(dstY));
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < BitmapData.Width);

                if (srcY < 0)
                {
                    // Blank texture row. As there is no blending here, setting transparent pixels where the scanline mask is not zero
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (scanline.Scanline.GetElementUnchecked(x) != 0)
                            accDst.SetColor(x + left, default);
                    }

                    return;
                }

                var accSrc = new TAccessor();
                accSrc.InitRow(Texture.GetRowCached(srcY));
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    switch (value)
                    {
                        case Byte.MinValue:
                            continue;
                        case Byte.MaxValue:
                            int dstX = x + left;
                            int srcX = mapper.MapX(dstX);
                            accDst.SetColor(dstX, srcX >= 0 ? accSrc.GetColor(srcX) : default);
                            continue;
                        default:
                            dstX = x + left;
                            srcX = mapper.MapX(dstX);
                            if (srcX < 0)
                                accDst.SetColor(dstX, default);
                            else
                            {
                                TBaseColor c = accSrc.GetBaseColor(srcX);
                                accDst.SetBaseColor(dstX, c.AdjustAlpha(value, c));
                            }

                            continue;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region FillSessionBlendSrgb<,,> class

        private sealed class FillSessionBlendSrgb<TAccessor, TColor, TBaseColor> : TextureBasedFillSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, TBaseColor, _>
            where TColor : unmanaged, IColor<TColor, TBaseColor>
            where TBaseColor : unmanaged, IColor<TBaseColor, TBaseColor>
        {
            #region Constructors

            internal FillSessionBlendSrgb(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                Debug.Assert(Blend && WorkingColorSpace == WorkingColorSpace.Srgb);
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert(Blend && (uint)scanline.RowIndex < (uint)BitmapData.Height);

                TMapper mapper = Mapper;
                int dstY = scanline.RowIndex;
                int srcY = mapper.MapY(dstY);

                if (srcY < 0)
                    return;

                var accDst = new TAccessor();
                accDst.InitRow(BitmapData.GetRowCached(dstY));
                var accSrc = new TAccessor();
                accSrc.InitRow(Texture.GetRowCached(srcY));
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < BitmapData.Width);

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    int dstX = x + left;
                    int srcX = mapper.MapX(dstX);
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                        continue;

                    TColor colorSrc = accSrc.GetColor(srcX);
                    if (!colorSrc.IsOpaque)
                    {
                        if (colorSrc.IsTransparent)
                            continue;

                        TColor colorDst = accDst.GetColor(dstX);
                        if (!colorDst.IsTransparent)
                            colorSrc = colorSrc.BlendSrgb(colorDst);
                    }

                    accDst.SetColor(dstX, colorSrc);
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert(Blend && (uint)scanline.RowIndex < (uint)BitmapData.Height);
                TMapper mapper = Mapper;
                int dstY = scanline.RowIndex;
                int srcY = mapper.MapY(dstY);

                if (srcY < 0)
                    return;

                var accDst = new TAccessor();
                accDst.InitRow(BitmapData.GetRowCached(dstY));
                var accSrc = new TAccessor();
                accSrc.InitRow(Texture.GetRowCached(srcY));
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < BitmapData.Width);

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    if (value == Byte.MinValue)
                        continue;

                    int dstX = x + left;
                    int srcX = mapper.MapX(dstX);
                    if (srcX < 0)
                        continue;

                    if (value == Byte.MaxValue)
                    {
                        TColor colorSrc = accSrc.GetColor(srcX);
                        if (colorSrc.IsOpaque)
                            accDst.SetColor(x + left, colorSrc);
                        else if (!colorSrc.IsTransparent)
                        {
                            TColor colorDst = accDst.GetColor(dstX);
                            accDst.SetColor(dstX, colorDst.IsTransparent ? colorSrc : colorSrc.BlendSrgb(colorDst));
                        }
                    }
                    else
                    {
                        TBaseColor colorSrc = accSrc.GetBaseColor(srcX);
                        if (colorSrc.IsOpaque)
                            colorSrc = colorSrc.WithAlpha(value, colorSrc);
                        else
                        {
                            colorSrc = colorSrc.AdjustAlpha(value, colorSrc);
                            if (colorSrc.IsTransparent)
                                continue;
                        }

                        TBaseColor colorDst = accDst.GetBaseColor(dstX);
                        accDst.SetBaseColor(dstX, colorDst.IsTransparent ? colorSrc : colorSrc.BlendSrgb(colorDst));
                    }
                }
            }

            #endregion
        }

        #endregion

        #region FillSessionBlendLinear<,,> class

        private sealed class FillSessionBlendLinear<TAccessor, TColor, TBaseColor> : TextureBasedFillSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, TBaseColor, _>
            where TColor : unmanaged, IColor<TColor, TBaseColor>
            where TBaseColor : unmanaged, IColor<TBaseColor, TBaseColor>
        {
            #region Constructors

            internal FillSessionBlendLinear(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                Debug.Assert(Blend && WorkingColorSpace == WorkingColorSpace.Linear);
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert(Blend && (uint)scanline.RowIndex < (uint)BitmapData.Height);

                TMapper mapper = Mapper;
                int dstY = scanline.RowIndex;
                int srcY = mapper.MapY(dstY);

                if (srcY < 0)
                    return;

                var accDst = new TAccessor();
                accDst.InitRow(BitmapData.GetRowCached(dstY));
                var accSrc = new TAccessor();
                accSrc.InitRow(Texture.GetRowCached(srcY));
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < BitmapData.Width);

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    int dstX = x + left;
                    int srcX = mapper.MapX(dstX);
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                        continue;

                    TColor colorSrc = accSrc.GetColor(srcX);
                    if (!colorSrc.IsOpaque)
                    {
                        if (colorSrc.IsTransparent)
                            continue;

                        TColor colorDst = accDst.GetColor(dstX);
                        if (!colorDst.IsTransparent)
                            colorSrc = colorSrc.BlendLinear(colorDst);
                    }

                    accDst.SetColor(dstX, colorSrc);
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert(Blend && (uint)scanline.RowIndex < (uint)BitmapData.Height);
                TMapper mapper = Mapper;
                int dstY = scanline.RowIndex;
                int srcY = mapper.MapY(dstY);

                if (srcY < 0)
                    return;

                var accDst = new TAccessor();
                accDst.InitRow(BitmapData.GetRowCached(dstY));
                var accSrc = new TAccessor();
                accSrc.InitRow(Texture.GetRowCached(srcY));
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < BitmapData.Width);

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    if (value == Byte.MinValue)
                        continue;

                    int dstX = x + left;
                    int srcX = mapper.MapX(dstX);
                    if (srcX < 0)
                        continue;

                    if (value == Byte.MaxValue)
                    {
                        TColor colorSrc = accSrc.GetColor(srcX);
                        if (colorSrc.IsOpaque)
                            accDst.SetColor(x + left, colorSrc);
                        else if (!colorSrc.IsTransparent)
                        {
                            TColor colorDst = accDst.GetColor(dstX);
                            accDst.SetColor(dstX, colorDst.IsTransparent ? colorSrc : colorSrc.BlendLinear(colorDst));
                        }
                    }
                    else
                    {
                        TBaseColor colorSrc = accSrc.GetBaseColor(srcX);
                        if (colorSrc.IsOpaque)
                            colorSrc = colorSrc.WithAlpha(value, colorSrc);
                        else
                        {
                            colorSrc = colorSrc.AdjustAlpha(value, colorSrc);
                            if (colorSrc.IsTransparent)
                                continue;
                        }

                        TBaseColor colorDst = accDst.GetBaseColor(dstX);
                        accDst.SetBaseColor(dstX, colorDst.IsTransparent ? colorSrc : colorSrc.BlendLinear(colorDst));
                    }
                }
            }

            #endregion
        }

        #endregion

        #region FillSessionWithQuantizing class

        private sealed class FillSessionWithQuantizing : TextureBasedFillSession
        {
            #region Fields

            private readonly IQuantizingSession quantizingSession;
            private readonly Color32 transparentColor;

            #endregion

            #region Constructors

            internal FillSessionWithQuantizing(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath,
                Rectangle bounds, DrawingOptions drawingOptions, IQuantizer quantizer, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                quantizingSession = quantizer.Initialize(bitmapData, context);
                WorkingColorSpace = quantizingSession.WorkingColorSpace;
                transparentColor = quantizingSession.GetQuantizedColor(default);
            }

            #endregion

            #region Methods

            #region Internal Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);

                TMapper mapper = Mapper;
                int dstY = scanline.RowIndex;
                int srcY = mapper.MapY(dstY);
                int left = scanline.Left;
                IBitmapDataRowInternal rowSrc;
                IBitmapDataRowInternal rowDst = BitmapData.GetRowCached(dstY);
                IQuantizingSession session = quantizingSession;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < BitmapData.Width);

                if (!Blend)
                {
                    if (srcY < 0)
                    {
                        // Blank texture row. As there is no blending here, setting transparent pixels
                        Color32 tr = transparentColor;
                        for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                        {
                            if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                                rowDst.DoSetColor32(x + left, tr);
                        }

                        return;
                    }

                    rowSrc = Texture.GetRowCached(srcY);
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        int dstX = x + left;
                        int srcX = mapper.MapX(dstX);
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                            rowDst.DoSetColor32(dstX, srcX >= 0 ? session.GetQuantizedColor(rowSrc.DoGetColor32(srcX)) : transparentColor);
                    }

                    return;
                }

                // From this point there is blending. Working in a compatible way with DrawInto (important to be consistent with TwoPassSession):
                // fully transparent source is skipped, just like when the alpha of the blended result is smaller than the threshold
                if (srcY < 0)
                    return;

                rowSrc = Texture.GetRowCached(srcY);
                var colorSpace = WorkingColorSpace;
                byte alphaThreshold = session.AlphaThreshold;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    int dstX = x + left;
                    int srcX = mapper.MapX(dstX);
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                        continue;

                    Color32 colorSrc = rowSrc.DoGetColor32(srcX);
                    if (colorSrc.A != Byte.MaxValue)
                    {
                        if (colorSrc.A == Byte.MinValue)
                            continue;
                        Color32 colorDst = rowDst.DoGetColor32(dstX);
                        if (colorDst.A != Byte.MinValue)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                ? colorSrc.BlendWithBackground(colorDst, colorSpace)
                                : colorSrc.BlendWith(colorDst, colorSpace);
                        }

                        if (colorSrc.A < alphaThreshold)
                            continue;
                    }

                    rowDst.DoSetColor32(dstX, session.GetQuantizedColor(colorSrc));
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                TMapper mapper = Mapper;
                int dstY = scanline.RowIndex;
                int srcY = mapper.MapY(dstY);
                int left = scanline.Left;
                IBitmapDataRowInternal rowSrc;
                IBitmapDataRowInternal rowDst = BitmapData.GetRowCached(dstY);
                IQuantizingSession session = quantizingSession;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < BitmapData.Width);

                if (!Blend)
                {
                    if (srcY < 0)
                    {
                        // Blank texture row. As there is no blending here, setting transparent pixels where the scanline mask is not zero
                        Color32 tr = transparentColor;
                        for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                        {
                            if (scanline.Scanline.GetElementUnchecked(x) != 0)
                                rowDst.SetColor32(x + left, tr);
                        }

                        return;
                    }

                    rowSrc = Texture.GetRowCached(srcY);
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        if (value == Byte.MinValue)
                            continue;

                        int dstX = x + left;
                        int srcX = mapper.MapX(dstX);
                        if (srcX < 0)
                            rowDst.DoSetColor32(dstX, transparentColor);
                        else
                        {
                            Color32 colorSrc = rowSrc.GetColor32(srcX);
                            if (value != Byte.MaxValue)
                                colorSrc = Color32.FromArgb(colorSrc.A == Byte.MaxValue ? value : (byte)(value * colorSrc.A / Byte.MaxValue), colorSrc);
                            rowDst.DoSetColor32(dstX, session.GetQuantizedColor(colorSrc));
                        }
                    }

                    return;
                }

                if (srcY < 0)
                    return;

                rowSrc = Texture.GetRowCached(srcY);
                var colorSpace = WorkingColorSpace;
                byte alphaThreshold = session.AlphaThreshold;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    if (value == Byte.MinValue)
                        continue;

                    int dstX = x + left;
                    int srcX = mapper.MapX(dstX);
                    if (srcX < 0)
                        continue;

                    Color32 colorSrc = rowSrc.GetColor32(srcX);
                    if (colorSrc.A == Byte.MinValue)
                        continue;

                    if (value != Byte.MaxValue)
                    {
                        if (colorSrc.A == Byte.MaxValue)
                            colorSrc = Color32.FromArgb(value, colorSrc);
                        else
                        {
                            colorSrc = Color32.FromArgb((byte)(value * colorSrc.A / Byte.MaxValue), colorSrc);
                            if (colorSrc.A == Byte.MinValue)
                                continue;
                        }
                    }

                    if (colorSrc.A != Byte.MaxValue)
                    {
                        Color32 colorDst = rowDst.DoGetColor32(dstX);
                        if (colorDst.A != Byte.MinValue)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                ? colorSrc.BlendWithBackground(colorDst, colorSpace)
                                : colorSrc.BlendWith(colorDst, colorSpace);
                        }

                        if (colorSrc.A < alphaThreshold)
                            continue;
                    }

                    rowDst.DoSetColor32(dstX, session.GetQuantizedColor(colorSrc));
                }
            }

            #endregion

            #region Protected Methods
            
            protected override void Dispose(bool disposing)
            {
                quantizingSession.Dispose();
                base.Dispose(disposing);
            }

            #endregion

            #endregion
        }

        #endregion

        #region FillSessionWithDithering class

        private sealed class FillSessionWithDithering : TextureBasedFillSession
        {
            #region Fields

            private readonly IQuantizingSession quantizingSession;
            private readonly IDitheringSession? ditheringSession;

            #endregion

            #region Properties

            internal override bool IsSingleThreaded => ditheringSession?.IsSequential == true;

            #endregion

            #region Constructors

            internal FillSessionWithDithering(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath,
                Rectangle bounds, DrawingOptions drawingOptions, IQuantizer quantizer, IDitherer ditherer, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                quantizingSession = quantizer.Initialize(bitmapData, context);
                WorkingColorSpace = quantizingSession.WorkingColorSpace;
                if (context.IsCancellationRequested)
                    return;

                context.Progress?.New(DrawingOperation.InitializingDitherer);
                ditheringSession = ditherer.Initialize(bitmapData, quantizingSession, context);
            }

            #endregion

            #region Methods

            #region Internal Methods
            
            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                IDitheringSession? session = ditheringSession;
                if (session == null)
                {
                    Debug.Fail("Dithering session is not expected to be null here");
                    return;
                }

                TMapper mapper = Mapper;
                int dstY = scanline.RowIndex;
                int srcY = mapper.MapY(dstY);
                int left = scanline.Left;
                IBitmapDataRowInternal rowSrc;
                IBitmapDataRowInternal rowDst = BitmapData.GetRowCached(dstY);
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < BitmapData.Width);

                if (!Blend)
                {
                    if (srcY < 0)
                    {
                        // Blank texture row. As there is no blending here, setting transparent pixels
                        for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                        {
                            if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                                continue;

                            int dstX = x + left;
                            rowDst.DoSetColor32(dstX, session.GetDitheredColor(default, dstX, dstY));
                        }
                    }
                    else
                    {
                        rowSrc = Texture.GetRowCached(srcY);
                        for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                        {
                            int dstX = x + left;
                            int srcX = mapper.MapX(dstX);
                            if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                                continue;

                            rowDst.DoSetColor32(dstX, session.GetDitheredColor(srcX >= 0 ? rowSrc.DoGetColor32(srcX) : default, dstX, dstY));
                        }
                    }

                    return;
                }

                // From this point there is blending. Working in a compatible way with DrawInto (important to be consistent with TwoPassSession):
                // fully transparent source is skipped, just like when the alpha of the blended result is smaller than the threshold
                if (srcY < 0)
                    return;

                rowSrc = Texture.GetRowCached(srcY);
                var colorSpace = WorkingColorSpace;
                byte alphaThreshold = quantizingSession.AlphaThreshold;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    int dstX = x + left;
                    int srcX = mapper.MapX(dstX);
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                        continue;

                    Color32 colorSrc = rowSrc.DoGetColor32(srcX);
                    if (colorSrc.A != Byte.MaxValue)
                    {
                        if (colorSrc.A == Byte.MinValue)
                            continue;
                        Color32 colorDst = rowDst.DoGetColor32(dstX);
                        if (colorDst.A != Byte.MinValue)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                ? colorSrc.BlendWithBackground(colorDst, colorSpace)
                                : colorSrc.BlendWith(colorDst, colorSpace);
                        }

                        if (colorSrc.A < alphaThreshold)
                            continue;
                    }

                    rowDst.DoSetColor32(dstX, session.GetDitheredColor(colorSrc, dstX, dstY));
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                IDitheringSession? session = ditheringSession;
                if (session == null)
                {
                    Debug.Fail("Dithering session is not expected to be null here");
                    return;
                }

                TMapper mapper = Mapper;
                int dstY = scanline.RowIndex;
                int srcY = mapper.MapY(dstY);
                int left = scanline.Left;
                IBitmapDataRowInternal rowSrc;
                IBitmapDataRowInternal rowDst = BitmapData.GetRowCached(dstY);
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < BitmapData.Width);

                if (!Blend)
                {
                    if (srcY < 0)
                    {
                        // Blank texture row. As there is no blending here, setting transparent pixels where the scanline mask is not zero
                        for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                        {
                            if (scanline.Scanline.GetElementUnchecked(x) == 0)
                                continue;

                            int dstX = x + left;
                            rowDst.SetColor32(dstX, session.GetDitheredColor(default, dstX, dstY));
                        }
                    }
                    else
                    {
                        rowSrc = Texture.GetRowCached(srcY);
                        for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                        {
                            byte value = scanline.Scanline.GetElementUnchecked(x);
                            if (value == Byte.MinValue)
                                continue;

                            int dstX = x + left;
                            int srcX = mapper.MapX(dstX);
                            if (srcX < 0)
                                rowDst.DoSetColor32(dstX, session.GetDitheredColor(default, dstX, dstY));
                            else
                            {
                                Color32 colorSrc = rowSrc.GetColor32(srcX);
                                if (value != Byte.MaxValue)
                                    colorSrc = Color32.FromArgb(colorSrc.A == Byte.MaxValue ? value : (byte)(value * colorSrc.A / Byte.MaxValue), colorSrc);
                                rowDst.DoSetColor32(dstX, session.GetDitheredColor(colorSrc, dstX, dstY));
                            }
                        }
                    }

                    return;
                }

                if (srcY < 0)
                    return;

                rowSrc = Texture.GetRowCached(srcY);
                var colorSpace = WorkingColorSpace;
                byte alphaThreshold = quantizingSession.AlphaThreshold;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    if (value == Byte.MinValue)
                        continue;

                    int dstX = x + left;
                    int srcX = mapper.MapX(dstX);
                    if (srcX < 0)
                        continue;

                    Color32 colorSrc = rowSrc.GetColor32(srcX);
                    if (colorSrc.A == Byte.MinValue)
                        continue;

                    if (value != Byte.MaxValue)
                    {
                        if (colorSrc.A == Byte.MaxValue)
                            colorSrc = Color32.FromArgb(value, colorSrc);
                        else
                        {
                            colorSrc = Color32.FromArgb((byte)(value * colorSrc.A / Byte.MaxValue), colorSrc);
                            if (colorSrc.A == Byte.MinValue)
                                continue;
                        }
                    }

                    if (colorSrc.A != Byte.MaxValue)
                    {
                        Color32 colorDst = rowDst.DoGetColor32(dstX);
                        if (colorDst.A != Byte.MinValue)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                ? colorSrc.BlendWithBackground(colorDst, colorSpace)
                                : colorSrc.BlendWith(colorDst, colorSpace);
                        }

                        if (colorSrc.A < alphaThreshold)
                            continue;
                    }

                    rowDst.DoSetColor32(dstX, session.GetDitheredColor(colorSrc, dstX, dstY));
                }
            }

            #endregion

            #region Protected Methods

            protected override void Dispose(bool disposing)
            {
                ditheringSession?.Dispose();
                quantizingSession.Dispose();
                base.Dispose(disposing);
            }

            #endregion

            #endregion
        }

        #endregion

        #region TwoPassFillSession class

        private sealed class TwoPassFillSession : TextureBasedFillSession
        {
            #region Fields

            private readonly IQuantizer quantizer;
            private readonly IDitherer? ditherer;
            private readonly IBitmapDataInternal firstSessionTarget;
            private readonly Rectangle bounds;
            private readonly bool isMaskGenerated;

            private Array2D<byte> mask;

            #endregion

            #region Constructors

            internal TwoPassFillSession(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath,
                Rectangle bounds, DrawingOptions drawingOptions, IQuantizer quantizer, IDitherer? ditherer, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                this.quantizer = quantizer;
                this.ditherer = ditherer;
                WorkingColorSpace = quantizer.WorkingColorSpace();

                // Note: not using GetPreferredFirstPassPixelFormat because the first step is not a cloning, and the small performance gain at PArgb blending
                //       is lost at FinalizeSession where the PColors are converted to Color32 due to the quantizing anyway
                firstSessionTarget = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bounds.Size, KnownPixelFormat.Format32bppArgb, WorkingColorSpace);
                isMaskGenerated = region?.IsAntiAliased == false;
                mask = isMaskGenerated ? region!.Mask : new Array2D<byte>(bounds.Height, KnownPixelFormat.Format1bppIndexed.GetByteWidth(bounds.Width));
                this.bounds = bounds;
            }

            #endregion

            #region Methods

            #region Internal Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                #region Local Methods

                void ProcessNoBlending(in RegionScanline scanline)
                {
                    TMapper mapper = Mapper;
                    int dstY = scanline.RowIndex - bounds.Top;
                    int srcY = mapper.MapY(scanline.RowIndex);

                    // Even though there is no blending, we can skip transparent pixels here because the 1st pass result is transparent initially.
                    if (srcY < 0)
                        return;

                    IBitmapDataRowInternal rowSrc = Texture.GetRowCached(srcY);
                    IBitmapDataRowInternal rowDst = firstSessionTarget.GetRowCached(dstY);
                    int offset = scanline.Left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        int srcX = mapper.MapX(x + scanline.Left);
                        if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                            continue;

                        Color32 colorSrc = rowSrc.DoGetColor32(srcX);
                        if (colorSrc.A != Byte.MinValue)
                            rowDst.DoSetColor32(x + offset, colorSrc);
                    }
                }

                void ProcessWithBlending(in RegionScanline scanline)
                {
                    TMapper mapper = Mapper;
                    int dstY = scanline.RowIndex - bounds.Top;
                    int srcY = mapper.MapY(scanline.RowIndex);

                    if (srcY < 0)
                        return;

                    IBitmapDataRowInternal rowSrc = Texture.GetRowCached(srcY);
                    IBitmapDataRowInternal rowDst = firstSessionTarget.GetRowCached(dstY);
                    IBitmapDataRowInternal rowBackground = BitmapData.GetRowCached(scanline.RowIndex);
                    WorkingColorSpace colorSpace = WorkingColorSpace;
                    int left = scanline.Left;
                    int offset = left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        int srcX = mapper.MapX(x + scanline.Left);
                        if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                            continue;

                        Color32 colorSrc = rowSrc.DoGetColor32(srcX);
                        if (colorSrc.A != Byte.MaxValue)
                        {
                            if (colorSrc.A == Byte.MinValue)
                                continue;
                            Color32 colorDst = rowBackground.DoGetColor32(x + left);
                            if (colorDst.A != Byte.MinValue)
                            {
                                colorSrc = colorDst.A == Byte.MaxValue
                                    ? colorSrc.BlendWithBackground(colorDst, colorSpace)
                                    : colorSrc.BlendWith(colorDst, colorSpace);
                            }
                        }

                        rowDst.DoSetColor32(x + offset, colorSrc);
                    }
                }

                #endregion

                // if mask is not generated, then we can be sure that scanline width is exactly the visible width
                Debug.Assert(isMaskGenerated || scanline.Scanline.Length == KnownPixelFormat.Format1bppIndexed.GetByteWidth(bounds.Width));
                if (!isMaskGenerated)
                    scanline.Scanline.CopyTo(mask[scanline.RowIndex - bounds.Top]);

                if (!Blend)
                    ProcessNoBlending(scanline);
                else
                    ProcessWithBlending(scanline);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                #region Local Methods

                void ProcessNoBlending(in RegionScanline scanline)
                {
                    TMapper mapper = Mapper;
                    int dstY = scanline.RowIndex - bounds.Top;
                    int srcY = mapper.MapY(dstY);

                    // Even though there is no blending, we can skip transparent pixels here because the 1st pass result is transparent initially.
                    if (srcY < 0)
                        return;

                    IBitmapDataRowInternal rowSrc = Texture.GetRowCached(srcY);
                    IBitmapDataRowInternal rowDst = firstSessionTarget.GetRowCached(dstY);
                    ArraySection<byte> rowMask = mask[dstY];
                    int offset = scanline.Left - bounds.Left;
                    
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        if (value == Byte.MinValue)
                            continue;

                        int srcX = mapper.MapX(x + scanline.Left);
                        if (srcX < 0)
                            continue;

                        int pos = x + offset;
                        ColorExtensions.Set1bppColorIndex(ref rowMask.GetElementReferenceUnchecked(pos >> 3), pos, 1);

                        Color32 colorSrc = rowSrc.DoGetColor32(srcX);
                        if (value != Byte.MaxValue)
                            colorSrc = Color32.FromArgb(colorSrc.A == Byte.MaxValue ? value : (byte)(value * colorSrc.A / Byte.MaxValue), colorSrc);
                        if (colorSrc.A == Byte.MinValue)
                            continue;

                        rowDst.DoSetColor32(pos, colorSrc);
                    }
                }

                void ProcessWithBlending(in RegionScanline scanline)
                {
                    TMapper mapper = Mapper;
                    int dstY = scanline.RowIndex - bounds.Top;
                    int srcY = mapper.MapY(dstY);

                    if (srcY < 0)
                        return;

                    IBitmapDataRowInternal rowSrc = Texture.GetRowCached(srcY);
                    IBitmapDataRowInternal rowDst = firstSessionTarget.GetRowCached(dstY);
                    IBitmapDataRowInternal rowBackground = BitmapData.GetRowCached(scanline.RowIndex);
                    WorkingColorSpace colorSpace = WorkingColorSpace;
                    ArraySection<byte> rowMask = mask[dstY];
                    int left = scanline.Left;
                    int offset = left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        if (value == Byte.MinValue)
                            continue;

                        int srcX = mapper.MapX(x + scanline.Left);
                        if (srcX < 0)
                            continue;

                        int pos = x + offset;
                        ColorExtensions.Set1bppColorIndex(ref rowMask.GetElementReferenceUnchecked(pos >> 3), pos, 1);

                        Color32 colorSrc = rowSrc.DoGetColor32(srcX);
                        if (colorSrc.A == Byte.MinValue)
                            continue;

                        if (value != Byte.MaxValue)
                        {
                            if (colorSrc.A == Byte.MaxValue)
                                colorSrc = Color32.FromArgb(value, colorSrc);
                            else
                            {
                                colorSrc = Color32.FromArgb((byte)(value * colorSrc.A / Byte.MaxValue), colorSrc);
                                if (colorSrc.A == Byte.MinValue)
                                    continue;
                            }
                        }

                        if (colorSrc.A != Byte.MaxValue)
                        {
                            Color32 colorDst = rowBackground.DoGetColor32(x + left);
                            if (colorDst.A != Byte.MinValue)
                            {
                                colorSrc = colorDst.A == Byte.MaxValue
                                    ? colorSrc.BlendWithBackground(colorDst, colorSpace)
                                    : colorSrc.BlendWith(colorDst, colorSpace);
                            }
                        }

                        rowDst.DoSetColor32(pos, colorSrc);
                    }
                }

                #endregion

                if (!Blend)
                    ProcessNoBlending(scanline);
                else
                    ProcessWithBlending(scanline);
            }

            internal override void FinalizeSession()
            {
                Point maskOffset = isMaskGenerated ? VisibleBounds.Location - new Size(Region!.Bounds.Location) : Point.Empty;
                firstSessionTarget.DoCopyTo(Context, BitmapData, bounds.Location, quantizer, ditherer, Blend, mask, maskOffset);
            }

            #endregion

            #region Protected Methods

            protected override void Dispose(bool disposing)
            {
                if (!isMaskGenerated)
                    mask.Dispose();
                firstSessionTarget.Dispose();
                base.Dispose(disposing);
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #region Draw

        #region DrawSession<,,> class

        private sealed class DrawSession<TAccessor, TColor, TArg> : DrawThinPathSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, TArg>
            where TColor : unmanaged, IColor<TColor>
        {
            #region Fields

            private readonly IBitmapDataInternal texture;
            private readonly bool disposeTexture;
            private readonly TMapper mapper;
            private readonly TAccessor accessorSrc;
            private readonly TAccessor accessorDst;
            private readonly TArg arg;
            private readonly Action? disposeCallback;

            #endregion

            #region Constructors

            internal DrawSession(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath path, Rectangle bounds,
                DrawingOptions drawingOptions, TArg arg = default!, Action? disposeCallback = null)
                : base(owner, context, bitmapData, path, bounds, drawingOptions, null)
            {
                texture = owner.GetTexture(context, path, drawingOptions, out disposeTexture, out Point offset);
                mapper = new TMapper();
                mapper.InitTexture(texture, offset);
                this.arg = arg;
                this.disposeCallback = disposeCallback;
                Debug.Assert(!Blend);
                accessorSrc = new TAccessor();
                accessorSrc.InitBitmapData(texture, arg);
                accessorDst = new TAccessor();
                accessorDst.InitBitmapData(BitmapData, arg);
            }

            #endregion

            #region Methods

            #region Internal Methods
            
            internal override void DrawLine(PointF start, PointF end)
            {
                Debug.Assert(Region == null && !DrawingOptions.AntiAliasing && !Blend);
                (Point p1, Point p2) = Round(start, end);
                TMapper map = mapper;
                TAccessor accSrc = accessorSrc;
                TAccessor accDst = accessorDst;
                Size size = BitmapData.Size;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(size.Height))
                        return;

                    accDst.InitRow(BitmapData.GetRowCached(p1.Y), arg);
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, size.Width - 1);
                    int srcY = map.MapY(p1.Y);

                    // blank line: as there is no blending here, setting transparent pixels
                    if (srcY < 0)
                    {
                        for (int x = Math.Max(p1.X, 0); x <= max; x++)
                            accDst.SetColor(x, default);
                        return;
                    }

                    accSrc.InitRow(texture.GetRowCached(srcY), arg);
                    for (int x = Math.Max(p1.X, 0); x <= max; x++)
                    {
                        int srcX = map.MapX(x);
                        accDst.SetColor(x, srcX < 0 ? default : accSrc.GetColor(srcX));
                    }

                    return;
                }

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(size.Width))
                        return;

                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, size.Height - 1);
                    int srcX = map.MapX(p1.Y);

                    // blank line: as there is no blending here, setting transparent pixels
                    if (srcX < 0)
                    {
                        for (int y = Math.Max(p1.Y, 0); y <= max; y++)
                            accDst.SetColor(p1.X, y, default);
                        return;
                    }

                    for (int y = Math.Max(p1.Y, 0); y <= max; y++)
                    {
                        int srcY = map.MapX(y);
                        accDst.SetColor(p1.X, y, srcY < 0 ? default : accSrc.GetColor(srcX, srcY));
                    }

                    return;
                }

                // general line
                long width = (p2.X - p1.X).Abs();
                long height = (p2.Y - p1.Y).Abs();

                if (width >= height)
                {
                    long numerator = width >> 1;
                    if (p1.X > p2.X)
                        (p1, p2) = (p2, p1);
                    int step = p2.Y > p1.Y ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible X coordinates
                    if (x < 0)
                    {
                        numerator = (numerator - height * x) % width;
                        y -= x * step;
                        x = 0;
                    }

                    int endX = Math.Min(p2.X, size.Width - 1);
                    int offY = step > 0 ? Math.Min(p2.Y, size.Height - 1) + 1 : Math.Max(p2.Y, 0) - 1;
                    for (; x <= endX; x++)
                    {
                        // Drawing only if Y is visible
                        if ((uint)y < (uint)size.Height)
                        {
                            int srcX = map.MapX(x);
                            int srcY = map.MapY(y);
                            accDst.SetColor(x, y, srcX < 0 || srcY < 0 ? default : accSrc.GetColor(srcX, srcY));
                        }

                        numerator += height;
                        if (numerator < width)
                            continue;

                        y += step;
                        if (y == offY)
                            return;
                        numerator -= width;
                    }
                }
                else
                {
                    long numerator = height >> 1;
                    if (p1.Y > p2.Y)
                        (p1, p2) = (p2, p1);
                    int step = p2.X > p1.X ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible Y coordinates
                    if (y < 0)
                    {
                        numerator = (numerator - width * y) % height;
                        x -= y * step;
                        y = 0;
                    }

                    int endY = Math.Min(p2.Y, size.Height - 1);
                    int offX = step > 0 ? Math.Min(p2.X, size.Width - 1) + 1 : Math.Max(p2.X, 0) - 1;
                    for (; y <= endY; y++)
                    {
                        // Drawing only if X is visible
                        if ((uint)x < (uint)size.Width)
                        {
                            int srcX = map.MapX(x);
                            int srcY = map.MapY(y);
                            accDst.SetColor(x, y, srcX < 0 || srcY < 0 ? default : accSrc.GetColor(srcX, srcY));
                        }

                        numerator += width;
                        if (numerator < height)
                            continue;

                        x += step;
                        if (x == offX)
                            return;
                        numerator -= height;
                    }
                }
            }

            internal override void DrawEllipse(RectangleF bounds)
            {
                (Point p1, Point p2) = Round(bounds.Location, bounds.Location + bounds.Size);
                Size size = BitmapData.Size;

                (int left, int right) = p2.X >= p1.X ? (p1.X, p2.X) : (p2.X, p1.X);
                (int top, int bottom) = p2.Y >= p1.Y ? (p1.Y, p2.Y) : (p2.Y, p1.Y);
                int width = right - left; // exclusive: the actual drawn width is width + 1
                int height = bottom - top; // exclusive: the actual drawn height is height + 1

                Debug.Assert(width <= ArcSegment.DrawAsLinesThreshold && height <= ArcSegment.DrawAsLinesThreshold);
                if (left >= size.Width || top >= size.Height || right < 0 || bottom < 0)
                    return;

                int oddHeightCorrection = height & 1;
                long widthSquared = (long)width * width;
                long heightSquared = (long)height * height;
                long stepX = 1L - width;
                stepX = (stepX * heightSquared) << 2; // should be checked(stepX * heightSquared * 4) if height could be larger than 916395
                long stepY = (oddHeightCorrection + 1L) * widthSquared;
                stepY <<= 2; // should be checked(stepY * 4) if width could be larger than 916396
                long err = oddHeightCorrection * widthSquared;
                err += stepX + stepY; //  should be checked(stepX + stepY + err) if size could be larger than 916396 x 916395

                bottom = top + ((height + 1) >> 1);
                top = bottom - oddHeightCorrection;
                long scaledWidth = widthSquared << 3;
                long scaledHeight = heightSquared << 3;

                TMapper map = mapper;
                TAccessor accSrc = accessorSrc;
                TAccessor accDst = accessorDst;

                do
                {
                    SetPixel(left, top);
                    SetPixel(right, top);
                    SetPixel(left, bottom);
                    SetPixel(right, bottom);

                    long err2 = err << 1; //should be checked(err * 2) if size could be larger than 916396 x 916395
                    if (err2 <= stepY)
                    {
                        top -= 1;
                        bottom += 1;
                        stepY += scaledWidth; //should be checked(stepY + scaledWidth) if width could be larger than 916396
                        err += stepY;
                    }

                    if (err2 >= stepX || err2 > stepY)
                    {
                        left += 1;
                        right -= 1;
                        stepX += scaledHeight; //should be checked(stepX + scaledHeight) if height could be larger than 916395
                        err += stepX;
                    }
                } while (left <= right);

                if (left > size.Width || right < -1 || top < 0 && bottom >= size.Height)
                    return;

                while (bottom - top <= height)
                {
                    SetPixel(left - 1, top);
                    SetPixel(right + 1, top);
                    top -= 1;
                    SetPixel(left - 1, bottom);
                    SetPixel(right + 1, bottom);
                    bottom += 1;
                }

                #region Local Methods

                [MethodImpl(MethodImpl.AggressiveInlining)]
                void SetPixel(int x, int y)
                {
                    if ((uint)x >= (uint)size.Width || (uint)y >= (uint)size.Height)
                        return;

                    int srcX = map.MapX(x);
                    int srcY = map.MapY(y);
                    accDst.SetColor(x, y, srcX < 0 || srcY < 0 ? default : accSrc.GetColor(srcX, srcY));
                }

                #endregion
            }

            internal override void DrawArc(ArcSegment arc)
            {
                RectangleF bounds = arc.Bounds;
                (Point p1, Point p2) = Round(bounds.Location, bounds.Location + bounds.Size);
                Size size = BitmapData.Size;

                (int left, int right) = p2.X >= p1.X ? (p1.X, p2.X) : (p2.X, p1.X);
                (int top, int bottom) = p2.Y >= p1.Y ? (p1.Y, p2.Y) : (p2.Y, p1.Y);
                int width = right - left; // exclusive: the actual drawn width is width + 1
                int height = bottom - top; // exclusive: the actual drawn height is height + 1

                Debug.Assert(width <= ArcSegment.DrawAsLinesThreshold && height <= ArcSegment.DrawAsLinesThreshold);
                if (left >= size.Width || top >= size.Height || right < 0 || bottom < 0)
                    return;

                int oddHeightCorrection = height & 1;
                long widthSquared = (long)width * width;
                long heightSquared = (long)height * height;
                long stepX = 1L - width;
                stepX = (stepX * heightSquared) << 2; // should be checked(stepX * heightSquared * 4) if height could be larger than 916395
                long stepY = (oddHeightCorrection + 1L) * widthSquared;
                stepY <<= 2; // should be checked(stepY * 4) if width could be larger than 916396
                long err = oddHeightCorrection * widthSquared;
                err += stepX + stepY; //  should be checked(stepX + stepY + err) if size could be larger than 916396 x 916395

                bottom = top + ((height + 1) >> 1);
                top = bottom - oddHeightCorrection;
                long scaledWidth = widthSquared << 3;
                long scaledHeight = heightSquared << 3;

                // Not using arc.RadiusX/Y here because that is shorter by a half pixel (even if there is no rounding error)
                // because ArcSegment has no concept of line width, and here we draw a 1px wide path.
                float centerX = (left + right + 1) / 2f;
                float radiusX = (width + 1) / 2f;
                float radiusY = (height + 1) / 2f;
                (float startRad, float endRad) = arc.GetStartEndRadians();
                ArcSegment.AdjustAngles(ref startRad, ref endRad, radiusX, radiusY);

                // To prevent calculating Atan2 for each pixel, we just calculate a valid start/end range once, and apply it based on the current sector attributes.
                BitVector32 sectors = arc.GetSectors();
                int startX = (int)(centerX + radiusX * MathF.Cos(startRad));
                int endX = (int)(centerX + radiusX * MathF.Cos(endRad));

                TMapper map = mapper;
                TAccessor accSrc = accessorSrc;
                TAccessor accDst = accessorDst;

                do
                {
                    SetPixel(right, bottom, 0);
                    SetPixel(left, bottom, 1);
                    SetPixel(left, top, 2);
                    SetPixel(right, top, 3);

                    long err2 = err << 1; //should be checked(err * 2) if size could be larger than 916396 x 916395
                    if (err2 <= stepY)
                    {
                        top -= 1;
                        bottom += 1;
                        stepY += scaledWidth; //should be checked(stepY + scaledWidth) if width could be larger than 916396
                        err += stepY;
                    }

                    if (err2 >= stepX || err2 > stepY)
                    {
                        left += 1;
                        right -= 1;
                        stepX += scaledHeight; //should be checked(stepX + scaledHeight) if height could be larger than 916395
                        err += stepX;
                    }
                } while (left <= right);

                if (left > size.Width || right < -1 || top < 0 && bottom >= size.Height)
                    return;

                while (top - bottom <= height)
                {
                    SetPixel(right + 1, bottom, 0);
                    SetPixel(left - 1, bottom, 1);
                    bottom += 1;
                    SetPixel(left - 1, top, 2);
                    SetPixel(right + 1, top, 3);
                    top -= 1;
                }

                #region Local Methods

                void SetPixel(int x, int y, int sector)
                {
                    if ((uint)x >= (uint)size.Width || (uint)y >= (uint)size.Height)
                        return;

                    int sectorType = sectors[ArcSegment.Sectors[sector]];
                    if (sectorType == ArcSegment.SectorNotDrawn)
                        return;

                    if (sectorType == ArcSegment.SectorFullyDrawn
                        || sector > 1 // positive sector point
                        && (sectorType == ArcSegment.SectorStart && x >= startX
                            || sectorType == ArcSegment.SectorEnd && x <= endX
                            || sectorType == ArcSegment.SectorStartEnd && x >= startX && x <= endX)
                        || sector <= 1 // negative sector point
                        && (sectorType == ArcSegment.SectorStart && x <= startX
                            || sectorType == ArcSegment.SectorEnd && x >= endX
                            || sectorType == ArcSegment.SectorStartEnd && x <= startX && x >= endX))
                    {
                        int srcX = map.MapX(x);
                        int srcY = map.MapY(y);
                        accDst.SetColor(x, y, srcX < 0 || srcY < 0 ? default : accSrc.GetColor(srcX, srcY));
                    }
                }

                #endregion
            }

            #endregion

            #region Protected Methods

            protected override void Dispose(bool disposing)
            {
                if (disposing && disposeTexture)
                    texture.Dispose();
                disposeCallback?.Invoke();
                base.Dispose(disposing);
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion

        #endregion

        #region Methods

        private protected sealed override FillPathSession CreateFillSession(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
        {
            IQuantizer? quantizer = drawingOptions.Quantizer;
            IDitherer? ditherer = drawingOptions.Ditherer;
            bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

            // If the quantizer or ditherer relies on the actual [possibly already blended] result we perform the operation in two passes
            if (quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true)
                return new TwoPassFillSession(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer, region);

            // With regular dithering (which implies quantizing, too)
            if (ditherer != null)
                return new FillSessionWithDithering(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer, region);

            // Quantizing without dithering
            if (quantizer != null)
                return new FillSessionWithQuantizing(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer, region);

            // There is no quantizing: picking the most appropriate way for the best quality and performance.
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            bool linearBlending = bitmapData.LinearBlending();
            bool blend = drawingOptions.AlphaBlending && (HasAlpha || drawingOptions.AntiAliasing);

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || linearBlending && pixelFormat.LinearGamma)
            {
                // Using PColorF only if the actual pixel format really has linear gamma to prevent performance issues
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true } && (linearBlending || !blend)
                    ? !blend
                        ? new FillSessionNoBlend<BitmapDataAccessorPColorF, PColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : new FillSessionBlendLinear<BitmapDataAccessorPColorF, PColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : !blend
                        ? new FillSessionNoBlend<BitmapDataAccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : linearBlending
                            ? new FillSessionBlendLinear<BitmapDataAccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                            : new FillSessionBlendSrgb<BitmapDataAccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false } && (!linearBlending || !blend)
                    ? !blend
                        ? new FillSessionNoBlend<BitmapDataAccessorPColor64, PColor64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : new FillSessionBlendSrgb<BitmapDataAccessorPColor64, PColor64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : !blend
                        ? new FillSessionNoBlend<BitmapDataAccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : linearBlending
                            ? new FillSessionBlendLinear<BitmapDataAccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                            : new FillSessionBlendSrgb<BitmapDataAccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
            }

            return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false } && (!linearBlending || !blend)
                ? !blend
                    ? new FillSessionNoBlend<BitmapDataAccessorPColor32, PColor32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : new FillSessionBlendSrgb<BitmapDataAccessorPColor32, PColor32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                : !blend
                    ? new FillSessionNoBlend<BitmapDataAccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : linearBlending
                        ? new FillSessionBlendLinear<BitmapDataAccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : new FillSessionBlendSrgb<BitmapDataAccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
        }

        private protected sealed override DrawThinPathSession? CreateDrawThinPathSession(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
        {
            if (region != null)
                return base.CreateDrawThinPathSession(context, bitmapData, rawPath, bounds, drawingOptions, region);

            Debug.Assert(!drawingOptions.AntiAliasing && (!drawingOptions.AlphaBlending || !HasAlpha));
            IQuantizer? quantizer = drawingOptions.Quantizer;
            IDitherer? ditherer = drawingOptions.Ditherer;
            bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

            Debug.Assert(quantizer?.InitializeReliesOnContent != true && ditherer?.InitializeReliesOnContent != true);

            // Quantizing with or without dithering
            if (quantizer != null)
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData, context);
                if (context.IsCancellationRequested)
                {
                    quantizingSession.Dispose();
                    return null;
                }

                if (ditherer == null)
                {
                    return new DrawSession<BitmapDataAccessorQuantizing, Color32, IQuantizingSession>(this, context, bitmapData, rawPath, bounds, drawingOptions,
                        quantizingSession, () => quantizingSession.Dispose());
                }

                // Quantizing with dithering
                context.Progress?.New(DrawingOperation.InitializingDitherer);
                var ditheringSession = ditherer.Initialize(bitmapData, quantizingSession, context);

                return new DrawSession<BitmapDataAccessorDithering, Color32, IDitheringSession>(this, context, bitmapData, rawPath, bounds, drawingOptions,
                    ditheringSession, () => { ditheringSession.Dispose(); quantizingSession.Dispose(); });

            }

            // There is no quantizing: picking the most appropriate way for the best quality and performance.
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true }
                    ? new DrawSession<BitmapDataAccessorPColorF, PColorF, _>(this, context, bitmapData, rawPath, bounds, drawingOptions)
                    : new DrawSession<BitmapDataAccessorColorF, ColorF, _>(this, context, bitmapData, rawPath, bounds, drawingOptions);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                    ? new DrawSession<BitmapDataAccessorPColor64, PColor64, _>(this, context, bitmapData, rawPath, bounds, drawingOptions)
                    : new DrawSession<BitmapDataAccessorColor64, Color64, _>(this, context, bitmapData, rawPath, bounds, drawingOptions);
            }

            return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                ? new DrawSession<BitmapDataAccessorPColor32, PColor32, _>(this, context, bitmapData, rawPath, bounds, drawingOptions)
                : new DrawSession<BitmapDataAccessorColor32, Color32, _>(this, context, bitmapData, rawPath, bounds, drawingOptions);
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

            void InitTexture(IBitmapDataInternal texture, Point offset);
            int MapY(int y);
            int MapX(int x);

            #endregion
        }

        #endregion

        #region Nested Structs

        #region TextureMapperTile struct
        
        private struct TextureMapperTile : ITextureMapper
        {
            #region Fields

            private Size size;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point _) => size = texture.Size;
            public int MapY(int y) => y % size.Height;
            public int MapX(int x) => x % size.Width;

            #endregion
        }

        #endregion

        #region TextureMapperTileOffset struct
        
        private struct TextureMapperTileOffset : ITextureMapper
        {
            #region Fields

            private Size size;
            private Point offset;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point textureOffset)
            {
                size = texture.Size;
                offset = textureOffset;
            }

            public int MapY(int y)
            {
                y += offset.Y;
                return (y %= size.Height) < 0 ? y + size.Height : y;
            }

            public int MapX(int x)
            {
                x += offset.X;
                return (x %= size.Width) < 0 ? x + size.Width : x;
            }

            #endregion
        }

        #endregion

        #region TextureMapperTileFlipX struct

        private struct TextureMapperTileFlipX : ITextureMapper
        {
            #region Fields

            private Size size;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point _) => size = texture.Size;
            public int MapY(int y) => y % size.Height;
            public int MapX(int x)
            {
                x %= size.Width << 1;
                return x < size.Width ? x : size.Width - (x - size.Width) - 1;
            }

            #endregion
        }

        #endregion

        #region TextureMapperTileFlipXOffset struct

        private struct TextureMapperTileFlipXOffset : ITextureMapper
        {
            #region Fields

            private Size size;
            private Point offset;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point textureOffset)
            {
                size = texture.Size;
                offset = textureOffset;
            }

            public int MapY(int y)
            {
                y += offset.Y;
                return (y %= size.Height) < 0 ? y + size.Height : y;
            }

            public int MapX(int x)
            {
                x += offset.X;
                x %= size.Width << 1;
                if (x < 0)
                    x += size.Width << 1;
                return x < size.Width ? x : size.Width - (x - size.Width) - 1;
            }

            #endregion
        }

        #endregion

        #region TextureMapperTileFlipY struct

        private struct TextureMapperTileFlipY : ITextureMapper
        {
            #region Fields

            private Size size;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point _) => size = texture.Size;
            public int MapX(int x) => x % size.Width;

            public int MapY(int y)
            {
                y %= size.Height << 1;
                return y < size.Height ? y : size.Height - (y - size.Height) - 1;
            }

            #endregion
        }

        #endregion

        #region TextureMapperTileFlipYOffset struct

        private struct TextureMapperTileFlipYOffset : ITextureMapper
        {
            #region Fields

            private Size size;
            private Point offset;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point textureOffset)
            {
                size = texture.Size;
                offset = textureOffset;
            }

            public int MapX(int x)
            {
                x += offset.X;
                return (x %= size.Width) < 0 ? x + size.Width : x;
            }

            public int MapY(int y)
            {
                y += offset.Y;
                y %= size.Height << 1;
                if (y < 0)
                    y += size.Height << 1;
                return y < size.Height ? y : size.Height - (y - size.Height) - 1;
            }

            #endregion
        }

        #endregion

        #region TextureMapperTileFlipXY struct

        private struct TextureMapperTileFlipXY : ITextureMapper
        {
            #region Fields

            private Size size;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point _) => size = texture.Size;

            public int MapY(int y)
            {
                y %= size.Height << 1;
                return y < size.Height ? y : size.Height - (y - size.Height) - 1;
            }

            public int MapX(int x)
            {
                x %= size.Width << 1;
                return x < size.Width ? x : size.Width - (x - size.Width) - 1;
            }

            #endregion
        }

        #endregion

        #region TextureMapperTileFlipXYOffset struct

        private struct TextureMapperTileFlipXYOffset : ITextureMapper
        {
            #region Fields

            private Size size;
            private Point offset;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point textureOffset)
            {
                size = texture.Size;
                offset = textureOffset;
            }

            public int MapY(int y)
            {
                y += offset.Y;
                y %= size.Height << 1;
                if (y < 0)
                    y += size.Height << 1;
                return y < size.Height ? y : size.Height - (y - size.Height) - 1;
            }

            public int MapX(int x)
            {
                x += offset.X;
                x %= size.Width << 1;
                if (x < 0)
                    x += size.Width << 1;
                return x < size.Width ? x : size.Width - (x - size.Width) - 1;
            }

            #endregion
        }

        #endregion

        #region TextureMapperClip struct

        private struct TextureMapperClip : ITextureMapper
        {
            #region Fields

            private Size size;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point _) => size = texture.Size;
            public int MapY(int y) => (uint)y < (uint)size.Height ? y : -1;
            public int MapX(int x) => (uint)x < (uint)size.Width ? x : -1;

            #endregion
        }

        #endregion

        #region TextureMapperExtend struct

        private struct TextureMapperExtend : ITextureMapper
        {
            #region Fields

            private Size size;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point _) => size = texture.Size;
            public int MapY(int y) => (uint)y < (uint)size.Height ? y : size.Height - 1;
            public int MapX(int x) => (uint)x < (uint)size.Width ? x : size.Width - 1;

            #endregion
        }

        #endregion

        #region TextureMapperOffset struct

        private struct TextureMapperOffset : ITextureMapper
        {
            #region Fields

            private Size size;
            private Point offset;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point textureOffset)
            {
                size = texture.Size;
                offset = textureOffset;
            }

            public int MapY(int y)
            {
                y += offset.Y;
                return (uint)y < (uint)size.Height ? y : -1;
            }

            public int MapX(int x)
            {
                x += offset.X;
                return (uint)x < (uint)size.Width ? x : -1;
            }

            #endregion
        }

        #endregion

        #region TextureMapperOffsetExtend struct

        private struct TextureMapperOffsetExtend : ITextureMapper
        {
            #region Fields

            private Size size;
            private Point offset;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point textureOffset)
            {
                size = texture.Size;
                offset = textureOffset;
            }

            public int MapY(int y)
            {
                y += offset.Y;
                return y < 0 ? 0
                    : y >= size.Height ? size.Height - 1
                    : y;
            }

            public int MapX(int x)
            {
                x += offset.X;
                return x < 0 ? 0
                    : x >= size.Width ? size.Width - 1
                    : x;
            }

            #endregion
        }

        #endregion

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        internal static TextureBasedBrush Create(IReadableBitmapData texture, TextureMapMode mapMode, bool hasAlphaHint, Point offset)
        {
            if (mapMode >= TextureMapMode.Center)
            {
                return mapMode == TextureMapMode.CenterExtend
                    ? new TextureBrush<TextureMapperOffsetExtend>(texture, hasAlphaHint, mapMode, offset)
                    : new TextureBrush<TextureMapperOffset>(texture, hasAlphaHint, mapMode, offset);
            }

            if (offset.IsEmpty)
            {
                return mapMode switch
                {
                    TextureMapMode.Tile => new TextureBrush<TextureMapperTile>(texture, hasAlphaHint, mapMode, offset),
                    TextureMapMode.TileFlipX => new TextureBrush<TextureMapperTileFlipX>(texture, hasAlphaHint, mapMode, offset),
                    TextureMapMode.TileFlipY => new TextureBrush<TextureMapperTileFlipY>(texture, hasAlphaHint, mapMode, offset),
                    TextureMapMode.TileFlipXY => new TextureBrush<TextureMapperTileFlipXY>(texture, hasAlphaHint, mapMode, offset),
                    TextureMapMode.Clip => new TextureBrush<TextureMapperClip>(texture, hasAlphaHint, mapMode, offset),
                    TextureMapMode.Extend => new TextureBrush<TextureMapperExtend>(texture, hasAlphaHint, mapMode, offset),
                    _ => throw new ArgumentOutOfRangeException(PublicResources.EnumOutOfRange(mapMode))
                };
            }

            return mapMode switch
            {
                TextureMapMode.Tile => new TextureBrush<TextureMapperTileOffset>(texture, hasAlphaHint, mapMode, offset),
                TextureMapMode.TileFlipX => new TextureBrush<TextureMapperTileFlipXOffset>(texture, hasAlphaHint, mapMode, offset),
                TextureMapMode.TileFlipY => new TextureBrush<TextureMapperTileFlipYOffset>(texture, hasAlphaHint, mapMode, offset),
                TextureMapMode.TileFlipXY => new TextureBrush<TextureMapperTileFlipXYOffset>(texture, hasAlphaHint, mapMode, offset),
                TextureMapMode.Clip => new TextureBrush<TextureMapperOffset>(texture, hasAlphaHint, mapMode, offset),
                TextureMapMode.Extend => new TextureBrush<TextureMapperOffsetExtend>(texture, hasAlphaHint, mapMode, offset),
                _ => throw new ArgumentOutOfRangeException(PublicResources.EnumOutOfRange(mapMode))
            };
        }

        #endregion

        #region Instance Methods

        private protected abstract IBitmapDataInternal GetTexture(IAsyncContext context, RawPath rawPath, DrawingOptions drawingOptions, out bool disposeTexture, out Point offset);

        #endregion

        #endregion
    }
}
