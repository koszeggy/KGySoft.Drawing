﻿#region Copyright

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

        // TODO: remove, it's actually not faster. But first, take it as a base for non-generic cases
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
                            if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                                rowDst.DoSetColor32(x + left, default);
                        }

                        return;
                    }

                    rowSrc = Texture.GetRowCached(srcY);
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        int srcX = mapper.MapX(x);
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                            rowDst.DoSetColor32(x + left, srcX >= 0 ? rowSrc.DoGetColor32(srcX) : default);
                    }

                    return;
                }

                if (srcY < 0)
                    return;

                rowSrc = Texture.GetRowCached(srcY);
                var colorSpace = WorkingColorSpace;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    int srcX = mapper.MapX(x);
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) != 1)
                        continue;

                    int pos = x + left;
                    Color32 backColor = rowDst.DoGetColor32(pos);
                    Color32 foreColor = rowSrc.DoGetColor32(srcX);
                    rowDst.DoSetColor32(pos, foreColor.A == Byte.MaxValue ? foreColor : foreColor.Blend(backColor, colorSpace));
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
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < BitmapData.Width);

                if (!Blend)
                {
                    if (srcY < 0)
                    {
                        // Blank texture row. As there is no blending here, setting transparent pixels where the scanline mask is not zero
                        for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                        {
                            if (scanline.Scanline.GetElementUnchecked(x) != 0)
                                rowDst.SetColor32(x + left, default);
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
                            rowDst.DoSetColor32(x + left, default);
                        else
                        {
                            Color32 c = rowSrc.GetColor32(srcX);
                            rowDst.DoSetColor32(x + left, value == Byte.MaxValue ? c : Color32.FromArgb(c.A == Byte.MaxValue ? value : (byte)(value * c.A / Byte.MaxValue), c));
                        }
                    }

                    return;
                }

                if (srcY < 0)
                    return;

                rowSrc = Texture.GetRowCached(srcY);
                var colorSpace = WorkingColorSpace;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    if (value == Byte.MinValue)
                        continue;

                    int srcX = mapper.MapX(x);
                    if (srcX < 0)
                        continue;

                    Color32 foreColor = rowSrc.GetColor32(srcX);
                    if (value == Byte.MaxValue)
                    {
                        if (foreColor.A == Byte.MaxValue)
                            rowDst.DoSetColor32(x + left, foreColor);
                        else
                        {
                            int pos = x + left;
                            Color32 backColor = rowDst.DoGetColor32(pos);
                            rowDst.DoSetColor32(pos, foreColor.Blend(backColor, colorSpace));
                        }
                    }
                    else
                    {
                        int pos = x + left;
                        Color32 backColor = rowDst.DoGetColor32(pos);
                        rowDst.DoSetColor32(pos, (foreColor.A == Byte.MaxValue
                            ? Color32.FromArgb(value, foreColor)
                            : Color32.FromArgb((byte)(value * foreColor.A / Byte.MaxValue), foreColor))
                            .Blend(backColor, colorSpace));
                    }
                }
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
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) != 1)
                        continue;

                    int pos = x + left;
                    TColor backColor = accDst.GetColor(pos);
                    TColor foreColor = accSrc.GetColor(srcX);
                    accDst.SetColor(pos, foreColor.IsOpaque ? foreColor : foreColor.BlendSrgb(backColor));
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
                        TColor foreColor = accSrc.GetColor(srcX);
                        if (foreColor.IsOpaque)
                            accDst.SetColor(x + left, foreColor);
                        else
                        {
                            int pos = x + left;
                            TColor backColor = accDst.GetColor(pos);
                            accDst.SetColor(pos, foreColor.BlendSrgb(backColor));
                        }
                    }
                    else
                    {
                        int pos = x + left;
                        TBaseColor foreColor = accSrc.GetBaseColor(srcX);
                        TBaseColor backColor = accDst.GetBaseColor(pos);
                        accDst.SetBaseColor(pos, (foreColor.IsOpaque
                            ? foreColor.WithAlpha(value, foreColor)
                            : foreColor.AdjustAlpha(value, foreColor))
                            .BlendSrgb(backColor));
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
                    if (srcX < 0 || ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) != 1)
                        continue;

                    int pos = x + left;
                    TColor backColor = accDst.GetColor(pos);
                    TColor foreColor = accSrc.GetColor(srcX);
                    accDst.SetColor(pos, foreColor.IsOpaque ? foreColor : foreColor.BlendLinear(backColor));
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
                        TColor foreColor = accSrc.GetColor(srcX);
                        if (foreColor.IsOpaque)
                            accDst.SetColor(x + left, foreColor);
                        else
                        {
                            int pos = x + left;
                            TColor backColor = accDst.GetColor(pos);
                            accDst.SetColor(pos, foreColor.BlendLinear(backColor));
                        }
                    }
                    else
                    {
                        int pos = x + left;
                        TBaseColor foreColor = accSrc.GetBaseColor(srcX);
                        TBaseColor backColor = accDst.GetBaseColor(pos);
                        accDst.SetBaseColor(pos, (foreColor.IsOpaque
                            ? foreColor.WithAlpha(value, foreColor)
                            : foreColor.AdjustAlpha(value, foreColor))
                            .BlendLinear(backColor));
                    }
                }
            }

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

        #endregion

        #endregion

        #region Methods

        private protected sealed override FillPathSession CreateFillSession(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
        {
            IQuantizer? quantizer = drawingOptions.Quantizer;
            IDitherer? ditherer = drawingOptions.Ditherer;
            bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

            // TODO:
            //// If the quantizer or ditherer relies on the actual [possibly already blended] result we perform the operation in two passes
            //if (quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true)
            //    return new TwoPassSolidFillSession(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer, region);

            //// With regular dithering (which implies quantizing, too)
            //if (ditherer != null)
            //    return new SolidFillSessionWithDithering(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer, region);

            //// Quantizing without dithering
            //if (quantizer != null)
            //    return new SolidFillSessionWithQuantizing(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer, region);

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

            //// With regular dithering (which implies quantizing, too)
            //if (ditherer != null)
            //    return new SolidDrawSessionWithDithering(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer);

            //// Quantizing without dithering
            //if (quantizer != null)
            //    return new SolidDrawSessionWithQuantizing(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer);

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
