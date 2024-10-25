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

        #region FillSessionNoBlend<,,> class

        private sealed class FillSessionNoBlend<TAccessor, TColor, TBaseColor> : TextureBasedFillSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, TBaseColor>
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
                    int srcX = mapper.MapX(x);
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                        accDst.SetColor(x + left, srcX >= 0 ? accSrc.GetColor(srcX) : default);
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
                            int srcX = mapper.MapX(x);
                            accDst.SetColor(x + left, srcX >= 0 ? accSrc.GetColor(srcX) : default);
                            continue;
                        default:
                            srcX = mapper.MapX(x);
                            if (srcX < 0)
                                accDst.SetColor(x + left, default);
                            else
                            {
                                TBaseColor c = accSrc.GetBaseColor(srcX);
                                accDst.SetBaseColor(x + left, c.AdjustAlpha(value, c));
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
            where TAccessor : struct, IBitmapDataAccessor<TColor, TBaseColor>
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
                    int srcX = mapper.MapX(x);
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                        continue;

                    int dstX = x + left;
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

                    int srcX = mapper.MapX(x);
                    if (srcX < 0)
                        continue;

                    if (value == Byte.MaxValue)
                    {
                        TColor colorSrc = accSrc.GetColor(srcX);
                        if (colorSrc.IsOpaque)
                            accDst.SetColor(x + left, colorSrc);
                        else if (!colorSrc.IsTransparent)
                        {
                            int dstX = x + left;
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

                        int dstX = x + left;
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
            where TAccessor : struct, IBitmapDataAccessor<TColor, TBaseColor>
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
                    int srcX = mapper.MapX(x);
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                        continue;

                    int dstX = x + left;
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

                    int srcX = mapper.MapX(x);
                    if (srcX < 0)
                        continue;

                    if (value == Byte.MaxValue)
                    {
                        TColor colorSrc = accSrc.GetColor(srcX);
                        if (colorSrc.IsOpaque)
                            accDst.SetColor(x + left, colorSrc);
                        else if (!colorSrc.IsTransparent)
                        {
                            int dstX = x + left;
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

                        int dstX = x + left;
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
                        int srcX = mapper.MapX(x);
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                            rowDst.DoSetColor32(x + left, srcX >= 0 ? session.GetQuantizedColor(rowSrc.DoGetColor32(srcX)) : transparentColor);
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
                    int srcX = mapper.MapX(x);
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                        continue;

                    Color32 colorSrc = rowSrc.DoGetColor32(srcX);
                    if (colorSrc.A != Byte.MaxValue)
                    {
                        if (colorSrc.A == Byte.MinValue)
                            continue;
                        Color32 colorDst = rowDst.DoGetColor32(x + left);
                        if (colorDst.A != Byte.MinValue)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                ? colorSrc.BlendWithBackground(colorDst, colorSpace)
                                : colorSrc.BlendWith(colorDst, colorSpace);
                        }

                        if (colorSrc.A < alphaThreshold)
                            continue;
                    }

                    rowDst.DoSetColor32(x + left, session.GetQuantizedColor(colorSrc));
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

                        int srcX = mapper.MapX(x);
                        if (srcX < 0)
                            rowDst.DoSetColor32(x + left, transparentColor);
                        else
                        {
                            Color32 colorSrc = rowSrc.GetColor32(srcX);
                            if (value != Byte.MaxValue)
                                colorSrc = Color32.FromArgb(colorSrc.A == Byte.MaxValue ? value : (byte)(value * colorSrc.A / Byte.MaxValue), colorSrc);
                            rowDst.DoSetColor32(x + left, session.GetQuantizedColor(colorSrc));
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

                    int srcX = mapper.MapX(x);
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
                        Color32 colorDst = rowDst.DoGetColor32(x + left);
                        if (colorDst.A != Byte.MinValue)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                ? colorSrc.BlendWithBackground(colorDst, colorSpace)
                                : colorSrc.BlendWith(colorDst, colorSpace);
                        }

                        if (colorSrc.A < alphaThreshold)
                            continue;
                    }

                    rowDst.DoSetColor32(x + left, session.GetQuantizedColor(colorSrc));
                }
            }

            protected override void Dispose(bool disposing)
            {
                quantizingSession.Dispose();
                base.Dispose(disposing);
            }

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
                            int srcX = mapper.MapX(x);
                            if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                                continue;

                            int dstX = x + left;
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
                    int srcX = mapper.MapX(x);
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                        continue;

                    Color32 colorSrc = rowSrc.DoGetColor32(srcX);
                    int dstX = x + left;
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
                            int srcX = mapper.MapX(x);
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

                    int srcX = mapper.MapX(x);
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

                    int dstX = x + left;
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

            protected override void Dispose(bool disposing)
            {
                ditheringSession?.Dispose();
                quantizingSession.Dispose();
                base.Dispose(disposing);
            }

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
                        int pos = x + offset;
                        int srcX = mapper.MapX(pos);
                        if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                            continue;

                        Color32 colorSrc = rowSrc.DoGetColor32(srcX);
                        if (colorSrc.A != Byte.MinValue)
                            rowDst.DoSetColor32(pos, colorSrc);
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
                        int pos = x + offset;
                        int srcX = mapper.MapX(pos);
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

                        rowDst.DoSetColor32(pos, colorSrc);
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

                        int pos = x + offset;
                        int srcX = mapper.MapX(pos);
                        if (srcX < 0)
                            continue;

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

                        int pos = x + offset;
                        int srcX = mapper.MapX(pos);
                        if (srcX < 0)
                            continue;

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

        #region TextureBasedFillSession

        private abstract class TextureBasedDrawSession : DrawThinPathSession
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

            protected TextureBasedDrawSession(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
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

        // TODO: delete after creating other specific cases, not faster than the generic version
        #region DrawSessionColor32 class

        private sealed class DrawSessionColor32 : TextureBasedDrawSession
        {
            #region Constructors

            internal DrawSessionColor32(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
                : base(owner, context, bitmapData, path, bounds, drawingOptions, null)
            {
            }

            #endregion

            #region Methods

            internal override void DrawLine(PointF start, PointF end)
            {
                Debug.Assert(Region == null && !DrawingOptions.AntiAliasing && !Blend);
                Rectangle bounds = VisibleBounds;
                (Point p1, Point p2) = Round(start, end);
                TMapper mapper = Mapper;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(bounds.Bottom))
                        return;

                    IBitmapDataRowInternal rowDst = BitmapData.GetRowCached(p1.Y);
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, bounds.Right - 1);
                    int srcY = mapper.MapY(p1.Y);
                    
                    // blank line: as there is no blending here, setting transparent pixels
                    if (srcY < 0)
                    {
                        for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                            rowDst.DoSetColor32(x, default);
                        return;
                    }

                    IBitmapDataRowInternal rowSrc = Texture.GetRowCached(srcY);
                    for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                    {
                        int srcX = mapper.MapX(x);
                        rowDst.DoSetColor32(x, srcX < 0 ? default : rowSrc.GetColor32(srcX));
                    }

                    return;
                }

                IBitmapDataInternal bmpSrc = Texture;
                IBitmapDataInternal bmpDst = BitmapData;

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(bounds.Right))
                        return;

                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, bounds.Bottom - 1);
                    int srcX = mapper.MapX(p1.Y);

                    // blank line: as there is no blending here, setting transparent pixels
                    if (srcX < 0)
                    {
                        for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                            bmpDst.DoSetColor32(p1.X, y, default);
                        return;
                    }

                    for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                    {
                        int srcY = mapper.MapX(y);
                        bmpDst.DoSetColor32(p1.X, y, srcY < 0 ? default : bmpSrc.GetColor32(srcX, srcY));
                    }

                    return;
                }

                // general line
                int width = (p2.X - p1.X).Abs();
                int height = (p2.Y - p1.Y).Abs();

                if (width > height)
                {
                    int numerator = width >> 1;
                    if (p1.X > p2.X)
                        (p1, p2) = (p2, p1);
                    int step = p2.Y > p1.Y ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible X coordinates
                    if (x < bounds.Left)
                    {
                        int diff = bounds.Left - x;
                        numerator = (int)((numerator + ((long)height * diff)) % width);
                        x = bounds.Left;
                        y += diff * step;
                    }

                    int endX = Math.Min(p2.X, bounds.Right - 1);
                    int offY = step > 0 ? Math.Min(p2.Y, bounds.Bottom - 1) + 1 : Math.Max(p2.Y, bounds.Top) - 1;
                    for (; x <= endX; x++)
                    {
                        // Drawing only if Y is visible
                        if ((uint)y < (uint)bounds.Bottom)
                        {
                            int srcX = mapper.MapX(x);
                            int srcY = mapper.MapY(y);
                            bmpDst.DoSetColor32(x, y, srcX < 0 || srcY < 0 ? default : bmpSrc.GetColor32(srcX, srcY));
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
                    int numerator = height >> 1;
                    if (p1.Y > p2.Y)
                        (p1, p2) = (p2, p1);
                    int step = p2.X > p1.X ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible Y coordinates
                    if (y < bounds.Top)
                    {
                        int diff = bounds.Top - y;
                        numerator = (int)((numerator + ((long)width * diff)) % height);
                        x += diff * step;
                        y = bounds.Top;
                    }

                    int endY = Math.Min(p2.Y, bounds.Bottom - 1);
                    int offX = step > 0 ? Math.Min(p2.X, bounds.Right - 1) + 1 : Math.Max(p2.X, bounds.Left) - 1;
                    for (; y <= endY; y++)
                    {
                        // Drawing only if X is visible
                        if ((uint)(x - bounds.Left) < (uint)bounds.Right)
                        {
                            int srcX = mapper.MapX(x);
                            int srcY = mapper.MapY(y);
                            bmpDst.DoSetColor32(x, y, srcX < 0 || srcY < 0 ? default : bmpSrc.GetColor32(srcX, srcY));
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

            #endregion
        }

        #endregion

        #region DrawSession<,,> class

        private sealed class DrawSession<TAccessor, TColor, TBaseColor> : TextureBasedDrawSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, TBaseColor>
            where TColor : unmanaged, IColor<TColor, TBaseColor>
            where TBaseColor : unmanaged, IColor<TBaseColor, TBaseColor>
        {
            #region Fields

            private readonly TAccessor accessorSrc;
            private readonly TAccessor accessorDst;

            #endregion

            #region Constructors

            internal DrawSession(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
                : base(owner, context, bitmapData, path, bounds, drawingOptions, null)
            {
                Debug.Assert(!Blend);
                accessorSrc = new TAccessor();
                accessorSrc.InitBitmapData(Texture);
                accessorDst = new TAccessor();
                accessorDst.InitBitmapData(BitmapData);
            }

            #endregion

            #region Methods

            internal override void DrawLine(PointF start, PointF end)
            {
                Debug.Assert(Region == null && !DrawingOptions.AntiAliasing && !Blend);
                Rectangle bounds = VisibleBounds;
                (Point p1, Point p2) = Round(start, end);
                TMapper mapper = Mapper;
                TAccessor accSrc = accessorSrc;
                TAccessor accDst = accessorDst;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(bounds.Bottom))
                        return;

                    accDst.InitRow(BitmapData.GetRowCached(p1.Y));
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, bounds.Right - 1);
                    int srcY = mapper.MapY(p1.Y);
                    
                    // blank line: as there is no blending here, setting transparent pixels
                    if (srcY < 0)
                    {
                        for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                            accDst.SetColor(x, default);
                        return;
                    }

                    accSrc.InitRow(Texture.GetRowCached(srcY));
                    for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                    {
                        int srcX = mapper.MapX(x);
                        accDst.SetColor(x, srcX < 0 ? default : accSrc.GetColor(srcX));
                    }

                    return;
                }

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(bounds.Right))
                        return;

                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, bounds.Bottom - 1);
                    int srcX = mapper.MapX(p1.Y);

                    // blank line: as there is no blending here, setting transparent pixels
                    if (srcX < 0)
                    {
                        for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                            accDst.SetColor(p1.X, y, default);
                        return;
                    }

                    for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                    {
                        int srcY = mapper.MapX(y);
                        accDst.SetColor(p1.X, y, srcY < 0 ? default : accSrc.GetColor(srcX, srcY));
                    }

                    return;
                }

                // general line
                int width = (p2.X - p1.X).Abs();
                int height = (p2.Y - p1.Y).Abs();

                if (width > height)
                {
                    int numerator = width >> 1;
                    if (p1.X > p2.X)
                        (p1, p2) = (p2, p1);
                    int step = p2.Y > p1.Y ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible X coordinates
                    if (x < bounds.Left)
                    {
                        int diff = bounds.Left - x;
                        numerator = (int)((numerator + ((long)height * diff)) % width);
                        x = bounds.Left;
                        y += diff * step;
                    }

                    int endX = Math.Min(p2.X, bounds.Right - 1);
                    int offY = step > 0 ? Math.Min(p2.Y, bounds.Bottom - 1) + 1 : Math.Max(p2.Y, bounds.Top) - 1;
                    for (; x <= endX; x++)
                    {
                        // Drawing only if Y is visible
                        if ((uint)y < (uint)bounds.Bottom)
                        {
                            int srcX = mapper.MapX(x);
                            int srcY = mapper.MapY(y);
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
                    int numerator = height >> 1;
                    if (p1.Y > p2.Y)
                        (p1, p2) = (p2, p1);
                    int step = p2.X > p1.X ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible Y coordinates
                    if (y < bounds.Top)
                    {
                        int diff = bounds.Top - y;
                        numerator = (int)((numerator + ((long)width * diff)) % height);
                        x += diff * step;
                        y = bounds.Top;
                    }

                    int endY = Math.Min(p2.Y, bounds.Bottom - 1);
                    int offX = step > 0 ? Math.Min(p2.X, bounds.Right - 1) + 1 : Math.Max(p2.X, bounds.Left) - 1;
                    for (; y <= endY; y++)
                    {
                        // Drawing only if X is visible
                        if ((uint)(x - bounds.Left) < (uint)bounds.Right)
                        {
                            int srcX = mapper.MapX(x);
                            int srcY = mapper.MapY(y);
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

            #endregion
        }

        #endregion

        #region DrawSessionWithQuantizing class

        private sealed class DrawSessionWithQuantizing : TextureBasedDrawSession
        {
            #region Fields

            private readonly IQuantizingSession quantizingSession;
            private readonly Color32 transparentColor;

            #endregion

            #region Constructors

            internal DrawSessionWithQuantizing(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath path, Rectangle bounds,
                DrawingOptions drawingOptions, IQuantizer quantizer)
                : base(owner, context, bitmapData, path, bounds, drawingOptions, null)
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                quantizingSession = quantizer.Initialize(bitmapData, context);
                WorkingColorSpace = quantizingSession.WorkingColorSpace;
                transparentColor = quantizingSession.GetQuantizedColor(default);
            }

            #endregion

            #region Methods

            internal override void DrawLine(PointF start, PointF end)
            {
                Debug.Assert(Region == null && !DrawingOptions.AntiAliasing && !Blend);
                Rectangle bounds = VisibleBounds;
                (Point p1, Point p2) = Round(start, end);
                TMapper mapper = Mapper;
                IQuantizingSession session = quantizingSession;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(bounds.Bottom))
                        return;

                    IBitmapDataRowInternal rowDst = BitmapData.GetRowCached(p1.Y);
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, bounds.Right - 1);
                    int srcY = mapper.MapY(p1.Y);

                    // blank line: as there is no blending here, setting quantized transparent pixels
                    if (srcY < 0)
                    {
                        Color32 tr = transparentColor;
                        for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                            rowDst.DoSetColor32(x, tr);

                        return;
                    }

                    IBitmapDataRowInternal rowSrc = Texture.GetRowCached(srcY);
                    for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                    {
                        int srcX = mapper.MapX(x);
                        rowDst.DoSetColor32(x, srcX >= 0 ? session.GetQuantizedColor(rowSrc.GetColor32(srcX)) : transparentColor);
                    }

                    return;
                }

                IBitmapDataInternal bmpSrc = Texture;
                IBitmapDataInternal bmpDst = BitmapData;

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(bounds.Right))
                        return;

                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, bounds.Bottom - 1);
                    int srcX = mapper.MapX(p1.Y);

                    // blank line: as there is no blending here, setting quantized transparent pixels
                    if (srcX < 0)
                    {
                        Color32 tr = transparentColor;
                        for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                            bmpDst.DoSetColor32(p1.X, y, tr);

                        return;
                    }

                    for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                    {
                        int srcY = mapper.MapX(y);
                        bmpDst.DoSetColor32(p1.X, y, srcY >= 0 ? session.GetQuantizedColor(bmpSrc.GetColor32(srcX, srcY)) : transparentColor);
                    }

                    return;
                }

                // general line
                int width = (p2.X - p1.X).Abs();
                int height = (p2.Y - p1.Y).Abs();

                if (width > height)
                {
                    int numerator = width >> 1;
                    if (p1.X > p2.X)
                        (p1, p2) = (p2, p1);
                    int step = p2.Y > p1.Y ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible X coordinates
                    if (x < bounds.Left)
                    {
                        int diff = bounds.Left - x;
                        numerator = (int)((numerator + ((long)height * diff)) % width);
                        x = bounds.Left;
                        y += diff * step;
                    }

                    int endX = Math.Min(p2.X, bounds.Right - 1);
                    int offY = step > 0 ? Math.Min(p2.Y, bounds.Bottom - 1) + 1 : Math.Max(p2.Y, bounds.Top) - 1;
                    for (; x <= endX; x++)
                    {
                        // Drawing only if Y is visible
                        if ((uint)y < (uint)bounds.Bottom)
                        {
                            int srcX = mapper.MapX(x);
                            int srcY = mapper.MapY(y);
                            bmpDst.DoSetColor32(x, y, srcX < 0 || srcY < 0 ? transparentColor : session.GetQuantizedColor(bmpSrc.GetColor32(srcX, srcY)));
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
                    int numerator = height >> 1;
                    if (p1.Y > p2.Y)
                        (p1, p2) = (p2, p1);
                    int step = p2.X > p1.X ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible Y coordinates
                    if (y < bounds.Top)
                    {
                        int diff = bounds.Top - y;
                        numerator = (int)((numerator + ((long)width * diff)) % height);
                        x += diff * step;
                        y = bounds.Top;
                    }

                    int endY = Math.Min(p2.Y, bounds.Bottom - 1);
                    int offX = step > 0 ? Math.Min(p2.X, bounds.Right - 1) + 1 : Math.Max(p2.X, bounds.Left) - 1;
                    for (; y <= endY; y++)
                    {
                        // Drawing only if X is visible
                        if ((uint)(x - bounds.Left) < (uint)bounds.Right)
                        {
                            int srcX = mapper.MapX(x);
                            int srcY = mapper.MapY(y);
                            bmpDst.DoSetColor32(x, y, srcX < 0 || srcY < 0 ? transparentColor : session.GetQuantizedColor(bmpSrc.GetColor32(srcX, srcY)));
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

            #endregion
        }

        #endregion

        #region DrawSessionWithDithering class

        private sealed class DrawSessionWithDithering : TextureBasedDrawSession
        {
            #region Fields

            private readonly IDitheringSession? ditheringSession;

            #endregion

            #region Constructors

            internal DrawSessionWithDithering(TextureBasedBrush<TMapper> owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath path, Rectangle bounds,
                DrawingOptions drawingOptions, IQuantizer quantizer, IDitherer ditherer)
                : base(owner, context, bitmapData, path, bounds, drawingOptions, null)
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                IQuantizingSession quantizingSession = quantizer.Initialize(bitmapData, context);
                WorkingColorSpace = quantizingSession.WorkingColorSpace;
                if (context.IsCancellationRequested)
                    return;

                context.Progress?.New(DrawingOperation.InitializingDitherer);
                ditheringSession = ditherer.Initialize(bitmapData, quantizingSession, context);
            }

            #endregion

            #region Methods

            internal override void DrawLine(PointF start, PointF end)
            {
                Debug.Assert(Region == null && !DrawingOptions.AntiAliasing && !Blend);
                Rectangle bounds = VisibleBounds;
                (Point p1, Point p2) = Round(start, end);
                TMapper mapper = Mapper;
                IDitheringSession? session = ditheringSession;
                if (session == null)
                {
                    Debug.Fail("Dithering session is not expected to be null here");
                    return;
                }

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(bounds.Bottom))
                        return;

                    IBitmapDataRowInternal rowDst = BitmapData.GetRowCached(p1.Y);
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, bounds.Right - 1);
                    int srcY = mapper.MapY(p1.Y);

                    // blank line: as there is no blending here, setting dithered transparent pixels
                    if (srcY < 0)
                    {
                        for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                            rowDst.DoSetColor32(x, session.GetDitheredColor(default, x, p1.Y));

                        return;
                    }

                    IBitmapDataRowInternal rowSrc = Texture.GetRowCached(srcY);
                    for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                    {
                        int srcX = mapper.MapX(x);
                        rowDst.DoSetColor32(x, session.GetDitheredColor(srcX >= 0 ? rowSrc.GetColor32(srcX) : default, x, p1.Y));
                    }

                    return;
                }

                IBitmapDataInternal bmpSrc = Texture;
                IBitmapDataInternal bmpDst = BitmapData;

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(bounds.Right))
                        return;

                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, bounds.Bottom - 1);
                    int srcX = mapper.MapX(p1.Y);

                    // blank line: as there is no blending here, setting quantized transparent pixels
                    if (srcX < 0)
                    {
                        for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                            bmpDst.DoSetColor32(p1.X, y, session.GetDitheredColor(default, p1.X, y));

                        return;
                    }

                    for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                    {
                        int srcY = mapper.MapX(y);
                        bmpDst.DoSetColor32(p1.X, y, session.GetDitheredColor(srcY >= 0 ? bmpSrc.GetColor32(srcX, srcY) : default, p1.X, y));
                    }

                    return;
                }

                // general line
                int width = (p2.X - p1.X).Abs();
                int height = (p2.Y - p1.Y).Abs();

                if (width > height)
                {
                    int numerator = width >> 1;
                    if (p1.X > p2.X)
                        (p1, p2) = (p2, p1);
                    int step = p2.Y > p1.Y ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible X coordinates
                    if (x < bounds.Left)
                    {
                        int diff = bounds.Left - x;
                        numerator = (int)((numerator + ((long)height * diff)) % width);
                        x = bounds.Left;
                        y += diff * step;
                    }

                    int endX = Math.Min(p2.X, bounds.Right - 1);
                    int offY = step > 0 ? Math.Min(p2.Y, bounds.Bottom - 1) + 1 : Math.Max(p2.Y, bounds.Top) - 1;
                    for (; x <= endX; x++)
                    {
                        // Drawing only if Y is visible
                        if ((uint)y < (uint)bounds.Bottom)
                        {
                            int srcX = mapper.MapX(x);
                            int srcY = mapper.MapY(y);
                            bmpDst.DoSetColor32(x, y, session.GetDitheredColor(srcX < 0 || srcY < 0 ? default : bmpSrc.GetColor32(srcX, srcY), x, y));
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
                    int numerator = height >> 1;
                    if (p1.Y > p2.Y)
                        (p1, p2) = (p2, p1);
                    int step = p2.X > p1.X ? 1 : -1;
                    int x = p1.X;
                    int y = p1.Y;

                    // skipping invisible Y coordinates
                    if (y < bounds.Top)
                    {
                        int diff = bounds.Top - y;
                        numerator = (int)((numerator + ((long)width * diff)) % height);
                        x += diff * step;
                        y = bounds.Top;
                    }

                    int endY = Math.Min(p2.Y, bounds.Bottom - 1);
                    int offX = step > 0 ? Math.Min(p2.X, bounds.Right - 1) + 1 : Math.Max(p2.X, bounds.Left) - 1;
                    for (; y <= endY; y++)
                    {
                        // Drawing only if X is visible
                        if ((uint)(x - bounds.Left) < (uint)bounds.Right)
                        {
                            int srcX = mapper.MapX(x);
                            int srcY = mapper.MapY(y);
                            bmpDst.DoSetColor32(x, y, session.GetDitheredColor(srcX < 0 || srcY < 0 ? default : bmpSrc.GetColor32(srcX, srcY), x, y));
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
                        ? new FillSessionNoBlend<AccessorPColorF, PColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : new FillSessionBlendLinear<AccessorPColorF, PColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : !blend
                        ? new FillSessionNoBlend<AccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : linearBlending
                            ? new FillSessionBlendLinear<AccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                            : new FillSessionBlendSrgb<AccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false } && (!linearBlending || !blend)
                    ? !blend
                        ? new FillSessionNoBlend<AccessorPColor64, PColor64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : new FillSessionBlendSrgb<AccessorPColor64, PColor64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : !blend
                        ? new FillSessionNoBlend<AccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : linearBlending
                            ? new FillSessionBlendLinear<AccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                            : new FillSessionBlendSrgb<AccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
            }

            return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false } && (!linearBlending || !blend)
                ? !blend
                    ? new FillSessionNoBlend<AccessorPColor32, PColor32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : new FillSessionBlendSrgb<AccessorPColor32, PColor32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                : !blend
                    ? new FillSessionNoBlend<AccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : linearBlending
                        ? new FillSessionBlendLinear<AccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : new FillSessionBlendSrgb<AccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
        }

        private protected sealed override DrawThinPathSession CreateDrawThinPathSession(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
        {
            if (region != null)
                return base.CreateDrawThinPathSession(context, bitmapData, rawPath, bounds, drawingOptions, region);

            Debug.Assert(!drawingOptions.AntiAliasing && (!drawingOptions.AlphaBlending || !HasAlpha));
            IQuantizer? quantizer = drawingOptions.Quantizer;
            IDitherer? ditherer = drawingOptions.Ditherer;
            bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

            Debug.Assert(quantizer?.InitializeReliesOnContent != true && ditherer?.InitializeReliesOnContent != true);

            // With regular dithering (which implies quantizing, too)
            if (ditherer != null)
                return new DrawSessionWithDithering(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer);

            // Quantizing without dithering
            if (quantizer != null)
                return new DrawSessionWithQuantizing(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer);

            // There is no quantizing: picking the most appropriate way for the best quality and performance.
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true }
                    ? new DrawSession<AccessorPColorF, PColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions)
                    : new DrawSession<AccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                    ? new DrawSession<AccessorPColor64, PColor64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions)
                    : new DrawSession<AccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions);
            }

            return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                ? new DrawSession<AccessorPColor32, PColor32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions)
                : new DrawSession<AccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions);
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
