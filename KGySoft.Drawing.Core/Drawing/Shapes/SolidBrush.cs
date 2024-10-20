﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SolidBrush.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;

using KGySoft.Collections;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    // NOTE: The base class would be somewhat smaller (no nested interfaces and structs) if we just used a delegate call in the generic classes.
    // But unfortunately the performance of delegates and function pointers is much worse than direct call or generics:
    // 
    //  ==[Draw thin path - Size: {Width=101, Height=101}; Vertices: 73 Results]================================================
    //  Test Time: 5 000 ms
    //  Warming up: Yes
    //  Test cases: 5
    //  Repeats: 3
    //  Calling GC.Collect: Yes
    //  Forced CPU Affinity: No
    //  Cases are sorted by fulfilled iterations (the most first)
    //  --------------------------------------------------
    //  1. Direct SetColor32: 3 994 990 iterations in 15 000,01 ms. Adjusted for 5 000 ms: 1 331 662,72
    //    #1  1 334 847 iterations in 5 000,00 ms. Adjusted: 1 334 846,23	 <---- Best
    //    #2  1 332 692 iterations in 5 000,00 ms. Adjusted: 1 332 691,87
    //    #3  1 327 451 iterations in 5 000,00 ms. Adjusted: 1 327 450,07	 <---- Worst
    //    Worst-Best difference: 7 396,16 (0,56%)
    //  2. Generic accessor: 3 779 629 iterations in 15 000,00 ms. Adjusted for 5 000 ms: 1 259 875,96 (-71 804,76 / 94,61%)
    //    #1  1 252 877 iterations in 5 000,00 ms. Adjusted: 1 252 876,32	 <---- Worst
    //    #2  1 259 760 iterations in 5 000,00 ms. Adjusted: 1 259 759,80
    //    #3  1 266 992 iterations in 5 000,00 ms. Adjusted: 1 266 991,77	 <---- Best
    //    Worst-Best difference: 14 115,45 (1,13%)
    //  3. Function pointer: 2 585 604 iterations in 15 000,01 ms. Adjusted for 5 000 ms: 861 867,48 (-469 795,24 / 64,72%)
    //    #1  830 838 iterations in 5 000,00 ms. Adjusted: 830 837,68	 <---- Worst
    //    #2  861 045 iterations in 5 000,00 ms. Adjusted: 861 044,76
    //    #3  893 721 iterations in 5 000,01 ms. Adjusted: 893 720,00	 <---- Best
    //    Worst-Best difference: 62 882,31 (7,57%)
    //  4. Delegate: 1 921 980 iterations in 15 000,01 ms. Adjusted for 5 000 ms: 640 659,74 (-691 002,99 / 48,11%)
    //    #1  641 274 iterations in 5 000,00 ms. Adjusted: 641 273,88	 <---- Best
    //    #2  639 767 iterations in 5 000,00 ms. Adjusted: 639 766,48	 <---- Worst
    //    #3  640 939 iterations in 5 000,00 ms. Adjusted: 640 938,85
    //    Worst-Best difference: 1 507,41 (0,24%)
    //  5. Cached region: 894 508 iterations in 15 000,03 ms. Adjusted for 5 000 ms: 298 168,71 (-1 033 494,01 / 22,39%)
    //    #1  297 908 iterations in 5 000,01 ms. Adjusted: 297 907,48
    //    #2  297 593 iterations in 5 000,01 ms. Adjusted: 297 592,49	 <---- Worst
    //    #3  299 007 iterations in 5 000,01 ms. Adjusted: 299 006,16	 <---- Best
    //    Worst-Best difference: 1 413,67 (0,48%)
    internal sealed class SolidBrush : Brush
    {
        #region Nested Types

        #region Nested Classes

        #region Fill

        #region SolidFillSessionColor32 class

        private sealed class SolidFillSessionColor32 : FillPathSession
        {
            #region Fields

            private readonly Color32 color;

            #endregion

            #region Constructors

            internal SolidFillSessionColor32(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                color = owner.Color32;
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);

                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < row.Width);

                if (!Blend)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                            row.DoSetColor32(x + left, c);
                    }

                    return;
                }

                Debug.Assert(color.A < Byte.MaxValue);
                var colorSpace = WorkingColorSpace;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                    {
                        int pos = x + left;
                        Color32 backColor = row.DoGetColor32(pos);
                        row.DoSetColor32(pos, c.Blend(backColor, colorSpace));
                    }
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;

                if (!Blend)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                row.DoSetColor32(x + left, c);
                                continue;
                            default:
                                row.DoSetColor32(x + left, Color32.FromArgb(c.A == Byte.MaxValue ? value : (byte)(value * c.A / Byte.MaxValue), c));
                                continue;
                        }
                    }

                    return;
                }

                var colorSpace = WorkingColorSpace;
                if (c.A == Byte.MaxValue)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                row.DoSetColor32(x + left, c);
                                continue;
                            default:
                                int pos = x + left;
                                Color32 backColor = row.DoGetColor32(pos);
                                row.DoSetColor32(pos, Color32.FromArgb(value, c).Blend(backColor, colorSpace));
                                continue;
                        }
                    }

                    return;
                }

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    switch (value)
                    {
                        case Byte.MinValue:
                            continue;
                        case Byte.MaxValue:
                            int pos = x + left;
                            Color32 backColor = row.DoGetColor32(pos);
                            row.DoSetColor32(pos, c.Blend(backColor, colorSpace));
                            continue;
                        default:
                            pos = x + left;
                            backColor = row.DoGetColor32(pos);
                            row.DoSetColor32(pos, Color32.FromArgb((byte)(value * c.A / Byte.MaxValue), c).Blend(backColor, colorSpace));
                            continue;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region SolidFillSessionNoBlend<,,> class

        private sealed class SolidFillSessionNoBlend<TAccessor, TColor, TBaseColor> : FillPathSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, TBaseColor>
            where TColor : unmanaged, IColor<TColor, TBaseColor>
        {
            #region Fields

            private readonly TColor color;
            private readonly TBaseColor baseColor;

            #endregion

            #region Constructors

            internal SolidFillSessionNoBlend(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                Debug.Assert(!Blend);
                color = owner.GetColor<TColor>();
                baseColor = owner.GetColor<TBaseColor>();
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                var accessor = new TAccessor();
                accessor.InitRow(row);
                TColor c = color;
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < row.Width);

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                        accessor.SetColor(x + left, c);
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                var accessor = new TAccessor();
                accessor.InitRow(row);
                TColor c = color;
                TBaseColor bc = baseColor;
                int left = scanline.Left;

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    switch (value)
                    {
                        case Byte.MinValue:
                            continue;
                        case Byte.MaxValue:
                            accessor.SetColor(x + left, c);
                            continue;
                        default:
                            accessor.SetColor(x + left, c.AdjustAlpha(value, bc));
                            continue;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region SolidFillSessionBlendSrgb<,,> class

        private sealed class SolidFillSessionBlendSrgb<TAccessor, TColor, TBaseColor> : FillPathSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, TBaseColor>
            where TColor : unmanaged, IColor<TColor, TBaseColor>
        {
            #region Fields

            private readonly TColor color;
            private readonly TBaseColor baseColor;

            #endregion

            #region Constructors

            internal SolidFillSessionBlendSrgb(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                Debug.Assert(Blend && WorkingColorSpace == WorkingColorSpace.Srgb);
                color = owner.GetColor<TColor>();
                baseColor = owner.GetColor<TBaseColor>();
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert(Blend && (uint)scanline.RowIndex < (uint)BitmapData.Height);
                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                var accessor = new TAccessor();
                accessor.InitRow(row);
                TColor c = color;
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < row.Width);

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                    {
                        int pos = x + left;
                        TColor backColor = accessor.GetColor(pos);
                        accessor.SetColor(pos, c.BlendSrgb(backColor));
                    }
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert(Blend && (uint)scanline.RowIndex < (uint)BitmapData.Height);
                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                var accessor = new TAccessor();
                accessor.InitRow(row);
                TColor c = color;
                TBaseColor bc = baseColor;
                int left = scanline.Left;

                if (c.IsOpaque)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                accessor.SetColor(x + left, c);
                                continue;
                            default:
                                int pos = x + left;
                                TColor backColor = accessor.GetColor(pos);
                                accessor.SetColor(pos, c.WithAlpha(value, bc).BlendSrgb(backColor));
                                continue;
                        }
                    }

                    return;
                }

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    switch (value)
                    {
                        case Byte.MinValue:
                            continue;
                        case Byte.MaxValue:
                            int pos = x + left;
                            TColor backColor = accessor.GetColor(pos);
                            accessor.SetColor(pos, c.BlendSrgb(backColor));
                            continue;
                        default:
                            pos = x + left;
                            backColor = accessor.GetColor(pos);
                            accessor.SetColor(pos, c.AdjustAlpha(value, bc).BlendSrgb(backColor));
                            continue;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region SolidFillSessionBlendLinear<,,> class

        private sealed class SolidFillSessionBlendLinear<TAccessor, TColor, TBaseColor> : FillPathSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, TBaseColor>
            where TColor : unmanaged, IColor<TColor, TBaseColor>
        {
            #region Fields

            private readonly TColor color;
            private readonly TBaseColor baseColor;

            #endregion

            #region Constructors

            internal SolidFillSessionBlendLinear(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                Debug.Assert(Blend && WorkingColorSpace == WorkingColorSpace.Linear);
                color = owner.GetColor<TColor>();
                baseColor = owner.GetColor<TBaseColor>();
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                var accessor = new TAccessor();
                accessor.InitRow(row);
                TColor c = color;
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < row.Width);

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                    {
                        int pos = x + left;
                        TColor backColor = accessor.GetColor(pos);
                        accessor.SetColor(pos, c.BlendLinear(backColor));
                    }
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert(Blend && (uint)scanline.RowIndex < (uint)BitmapData.Height);
                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                var accessor = new TAccessor();
                accessor.InitRow(row);
                TColor c = color;
                TBaseColor bc = baseColor;
                int left = scanline.Left;

                if (c.IsOpaque)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                accessor.SetColor(x + left, c);
                                continue;
                            default:
                                int pos = x + left;
                                TColor backColor = accessor.GetColor(pos);
                                accessor.SetColor(pos, c.WithAlpha(value, bc).BlendLinear(backColor));
                                continue;
                        }
                    }

                    return;
                }

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    switch (value)
                    {
                        case Byte.MinValue:
                            continue;
                        case Byte.MaxValue:
                            int pos = x + left;
                            TColor backColor = accessor.GetColor(pos);
                            accessor.SetColor(pos, c.BlendLinear(backColor));
                            continue;
                        default:
                            pos = x + left;
                            backColor = accessor.GetColor(pos);
                            accessor.SetColor(pos, c.AdjustAlpha(value, bc).BlendLinear(backColor));
                            continue;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region SolidFillSessionIndexed class

        private sealed class SolidFillSessionIndexed : FillPathSession
        {
            #region Fields

            private readonly Color32 color;
            private readonly int colorIndex;

            #endregion

            #region Constructors

            internal SolidFillSessionIndexed(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                Debug.Assert(bitmapData.PixelFormat.Indexed && (!Blend || !owner.HasAlpha));
                color = owner.Color32;
                colorIndex = bitmapData.Palette!.GetNearestColorIndex(color);
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                Debug.Assert(!Blend);

                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                int i = colorIndex;
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < row.Width);

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                        row.DoSetColorIndex(x + left, i);
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                Debug.Assert(!Blend || Owner is SolidBrush { HasAlpha: false });

                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int i = colorIndex;
                int left = scanline.Left;

                if (!Blend)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                row.DoSetColorIndex(x + left, i);
                                continue;
                            default:
                                row.DoSetColor32(x + left, Color32.FromArgb(c.A == Byte.MaxValue ? value : (byte)(value * c.A / Byte.MaxValue), c));
                                continue;
                        }
                    }

                    return;
                }

                var colorSpace = WorkingColorSpace;
                Debug.Assert(c.A == Byte.MaxValue);
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    switch (value)
                    {
                        case Byte.MinValue:
                            continue;
                        case Byte.MaxValue:
                            row.DoSetColorIndex(x + left, i);
                            continue;
                        default:
                            int pos = x + left;
                            Color32 backColor = row.DoGetColor32(pos);
                            row.DoSetColor32(pos, Color32.FromArgb(value, c).Blend(backColor, colorSpace));
                            continue;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region SolidFillSessionWithQuantizing class

        private sealed class SolidFillSessionWithQuantizing : FillPathSession
        {
            #region Fields

            private readonly Color32 color;
            private readonly IQuantizingSession quantizingSession;

            #endregion

            #region Constructors

            internal SolidFillSessionWithQuantizing(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath,
                Rectangle bounds, DrawingOptions drawingOptions, IQuantizer quantizer, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                color = owner.Color32;
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                quantizingSession = quantizer.Initialize(bitmapData, context);
                WorkingColorSpace = quantizingSession.WorkingColorSpace;
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert(scanline.RowIndex < BitmapData.Height);
                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                IQuantizingSession session = quantizingSession;

                if (!Blend)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                            row.DoSetColor32(x + left, session.GetQuantizedColor(c));
                    }

                    return;
                }

                var colorSpace = session.WorkingColorSpace;
                byte alphaThreshold = session.AlphaThreshold;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                    {
                        int pos = x + left;
                        Color32 quantizedColor = session.GetQuantizedColor(c.Blend(row.DoGetColor32(pos), colorSpace));
                        if (quantizedColor.A >= alphaThreshold)
                            row.DoSetColor32(pos, quantizedColor);
                    }
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                int y = scanline.RowIndex - VisibleBounds.Top;
                Debug.Assert(y < BitmapData.Height);
                IBitmapDataRowInternal row = BitmapData.GetRowCached(y);
                Color32 c = color;
                int left = scanline.Left;
                IQuantizingSession session = quantizingSession;

                // no blending: writing even transparent result pixels
                if (!Blend)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                row.DoSetColor32(x + left, session.GetQuantizedColor(c));
                                continue;
                            default:
                                row.DoSetColor32(x + left, session.GetQuantizedColor(Color32.FromArgb(c.A == Byte.MaxValue ? value : (byte)(value * c.A / Byte.MaxValue), c)));
                                continue;
                        }
                    }

                    return;
                }

                // blending: skipping too transparent pixels
                var colorSpace = session.WorkingColorSpace;
                byte alphaThreshold = session.AlphaThreshold;
                if (c.A == Byte.MaxValue)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                Color32 quantizedColor = session.GetQuantizedColor(c);
                                if (quantizedColor.A >= alphaThreshold)
                                    row.DoSetColor32(x + left, quantizedColor);
                                continue;
                            default:
                                int pos = x + left;
                                quantizedColor = session.GetQuantizedColor(Color32.FromArgb(value, c).Blend(row.DoGetColor32(pos), colorSpace));
                                if (quantizedColor.A >= alphaThreshold)
                                    row.DoSetColor32(pos, quantizedColor);
                                continue;
                        }
                    }

                    return;
                }

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    switch (value)
                    {
                        case Byte.MinValue:
                            continue;
                        case Byte.MaxValue:
                            int pos = x + left;
                            Color32 quantizedColor = session.GetQuantizedColor(c.Blend(row.DoGetColor32(pos), colorSpace));
                            if (quantizedColor.A >= alphaThreshold)
                                row.DoSetColor32(pos, quantizedColor);
                            continue;
                        default:
                            pos = x + left;
                            quantizedColor = session.GetQuantizedColor(Color32.FromArgb((byte)(value * c.A / Byte.MaxValue), c).Blend(row.DoGetColor32(pos), colorSpace));
                            if (quantizedColor.A >= alphaThreshold)
                                row.DoSetColor32(pos, quantizedColor);
                            continue;
                    }
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

        #region SolidFillSessionWithDithering class

        private sealed class SolidFillSessionWithDithering : FillPathSession
        {
            #region Fields

            private readonly Color32 color;
            private readonly IQuantizingSession quantizingSession;
            private readonly IDitheringSession? ditheringSession;

            #endregion

            #region Properties

            internal override bool IsSingleThreaded => ditheringSession?.IsSequential == true;

            #endregion

            #region Constructors

            internal SolidFillSessionWithDithering(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath,
                Rectangle bounds, DrawingOptions drawingOptions, IQuantizer quantizer, IDitherer ditherer, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                color = owner.Color32;
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
                Debug.Assert(scanline.RowIndex < BitmapData.Height);
                IDitheringSession? session = ditheringSession;
                if (session == null)
                {
                    Debug.Fail("Dithering session is not expected to be null here");
                    return;
                }

                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;

                if (!Blend)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        int pos = x + left;
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                            row.DoSetColor32(pos, session.GetDitheredColor(c, pos, scanline.RowIndex));
                    }

                    return;
                }

                var colorSpace = quantizingSession.WorkingColorSpace;
                byte alphaThreshold = quantizingSession.AlphaThreshold;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                    {
                        int pos = x + left;
                        Color32 ditheredColor = session.GetDitheredColor(c.Blend(row.DoGetColor32(pos), colorSpace), pos, scanline.RowIndex);
                        if (ditheredColor.A >= alphaThreshold)
                            row.DoSetColor32(pos, ditheredColor);
                    }
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert(scanline.RowIndex < BitmapData.Height);
                IDitheringSession? session = ditheringSession;
                if (session == null)
                {
                    Debug.Fail("Dithering session is not expected to be null here");
                    return;
                }

                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                var colorSpace = quantizingSession.WorkingColorSpace;

                // no blending: writing even transparent result pixels
                if (!Blend)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                int pos = x + left;
                                row.DoSetColor32(pos, session.GetDitheredColor(c, pos, scanline.RowIndex));
                                continue;
                            default:
                                pos = x + left;
                                row.DoSetColor32(pos, session.GetDitheredColor(Color32.FromArgb(c.A == Byte.MaxValue ? value : (byte)(value * c.A / Byte.MaxValue), c), pos, scanline.RowIndex));
                                continue;
                        }
                    }

                    return;
                }

                // blending: skipping too transparent pixels
                byte alphaThreshold = quantizingSession.AlphaThreshold;
                if (c.A == Byte.MaxValue)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                int pos = x + left;
                                Color32 ditheredColor = session.GetDitheredColor(c, pos, scanline.RowIndex);
                                if (ditheredColor.A >= alphaThreshold)
                                    row.DoSetColor32(pos, ditheredColor);
                                continue;
                            default:
                                pos = x + left;
                                ditheredColor = session.GetDitheredColor(Color32.FromArgb(value, c).Blend(row.DoGetColor32(pos), colorSpace), pos, scanline.RowIndex);
                                if (ditheredColor.A >= alphaThreshold)
                                    row.DoSetColor32(pos, ditheredColor);
                                continue;
                        }
                    }

                    return;
                }

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    byte value = scanline.Scanline.GetElementUnchecked(x);
                    switch (value)
                    {
                        case Byte.MinValue:
                            continue;
                        case Byte.MaxValue:
                            int pos = x + left;
                            Color32 ditheredColor = session.GetDitheredColor(c.Blend(row.DoGetColor32(pos), colorSpace), pos, scanline.RowIndex);
                            if (ditheredColor.A >= alphaThreshold)
                                row.DoSetColor32(pos, ditheredColor);
                            continue;
                        default:
                            pos = x + left;
                            ditheredColor = session.GetDitheredColor(Color32.FromArgb((byte)(value * c.A / Byte.MaxValue), c).Blend(row.DoGetColor32(pos), colorSpace), pos, scanline.RowIndex);
                            if (ditheredColor.A >= alphaThreshold)
                                row.DoSetColor32(pos, ditheredColor);
                            continue;
                    }
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

        #region TwoPassSolidFillSession class

        private sealed class TwoPassSolidFillSession : FillPathSession
        {
            #region Fields

            private readonly IQuantizer quantizer;
            private readonly IDitherer? ditherer;
            private readonly IBitmapDataInternal firstSessionTarget;
            private readonly Rectangle bounds;
            private readonly Color32 color;
            private readonly bool isMaskGenerated;

            private Array2D<byte> mask;

            #endregion

            #region Constructors

            internal TwoPassSolidFillSession(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath,
                Rectangle bounds, DrawingOptions drawingOptions, IQuantizer quantizer, IDitherer? ditherer, Region? region)
                : base(owner, context, bitmapData, rawPath, bounds, drawingOptions, region)
            {
                color = owner.Color32;
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
                    Color32 c = color;
                    int y = scanline.RowIndex - bounds.Top;
                    IBitmapDataRowInternal targetRow = firstSessionTarget.GetRowCached(y);
                    int offset = scanline.Left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                            continue;

                        targetRow.DoSetColor32(x + offset, c);
                    }
                }

                void ProcessWithBlending(in RegionScanline scanline)
                {
                    Color32 c = color;
                    IBitmapDataRowInternal targetRow = firstSessionTarget.GetRowCached(scanline.RowIndex - bounds.Top);
                    WorkingColorSpace colorSpace = WorkingColorSpace;
                    IBitmapDataRowInternal sourceRow = BitmapData.GetRowCached(scanline.RowIndex);
                    int offset = scanline.Left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                            continue;

                        int pos = x + offset;
                        Color32 backColor = sourceRow.DoGetColor32(pos);
                        targetRow.DoSetColor32(pos, c.Blend(backColor, colorSpace));
                    }
                }

                #endregion

                // if mask is not generated, then we can be sure that scanline width is exactly the visible width
                Debug.Assert(isMaskGenerated || scanline.Scanline.Length == KnownPixelFormat.Format1bppIndexed.GetByteWidth(bounds.Width));
                if (!isMaskGenerated)
                    scanline.Scanline.CopyTo(mask[scanline.RowIndex - bounds.Top]);

                if (!Blend || color.A == Byte.MaxValue)
                {
                    ProcessNoBlending(scanline);
                    return;
                }

                ProcessWithBlending(scanline);
            }

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                #region Local Methods

                void ProcessNoBlending(in RegionScanline scanline)
                {
                    Color32 c = color;
                    int y = scanline.RowIndex - bounds.Top;
                    IBitmapDataRowInternal targetRow = firstSessionTarget.GetRowCached(y);
                    ArraySection<byte> maskRow = mask[y];
                    int offset = scanline.Left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        int pos = x + offset;
                        ColorExtensions.Set1bppColorIndex(ref maskRow.GetElementReferenceUnchecked(pos >> 3), pos, value);

                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                targetRow.DoSetColor32(pos, c);
                                continue;
                            default:
                                targetRow.DoSetColor32(pos, Color32.FromArgb(c.A == Byte.MaxValue ? value : (byte)(value * c.A / Byte.MaxValue), c));
                                continue;
                        }
                    }

                }

                void ProcessWithBlendingSolid(in RegionScanline scanline)
                {
                    Color32 c = color;
                    int y = scanline.RowIndex - bounds.Top;
                    IBitmapDataRowInternal targetRow = firstSessionTarget.GetRowCached(y);
                    ArraySection<byte> maskRow = mask[y];
                    WorkingColorSpace colorSpace = WorkingColorSpace;
                    IBitmapDataRowInternal sourceRow = BitmapData.GetRowCached(scanline.RowIndex);
                    int offset = scanline.Left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        int pos = x + offset;
                        ColorExtensions.Set1bppColorIndex(ref maskRow.GetElementReferenceUnchecked(pos >> 3), pos, value);

                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                targetRow.DoSetColor32(pos, c);
                                continue;
                            default:
                                Color32 backColor = sourceRow.DoGetColor32(pos);
                                targetRow.DoSetColor32(pos, Color32.FromArgb(value, c).Blend(backColor, colorSpace));
                                continue;
                        }
                    }
                }

                void ProcessWithBlendingAlpha(in RegionScanline scanline)
                {
                    Color32 c = color;
                    int y = scanline.RowIndex - bounds.Top;
                    IBitmapDataRowInternal targetRow = firstSessionTarget.GetRowCached(y);
                    ArraySection<byte> maskRow = mask[y];
                    WorkingColorSpace colorSpace = WorkingColorSpace;
                    IBitmapDataRowInternal sourceRow = BitmapData.GetRowCached(scanline.RowIndex);
                    int offset = scanline.Left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        int pos = x + offset;
                        ColorExtensions.Set1bppColorIndex(ref maskRow.GetElementReferenceUnchecked(pos >> 3), pos, value);

                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                Color32 backColor = sourceRow.DoGetColor32(pos);
                                targetRow.DoSetColor32(pos, c.Blend(backColor, colorSpace));
                                continue;
                            default:
                                backColor = sourceRow.DoGetColor32(pos);
                                targetRow.DoSetColor32(pos, Color32.FromArgb((byte)(value * c.A / Byte.MaxValue), c).Blend(backColor, colorSpace));
                                continue;
                        }
                    }
                }

                #endregion

                if (!Blend)
                {
                    ProcessNoBlending(scanline);
                    return;
                }

                if (color.A == Byte.MaxValue)
                {
                    ProcessWithBlendingSolid(scanline);
                    return;
                }

                ProcessWithBlendingAlpha(scanline);
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

        #region SolidDrawSessionColor32 class

        private sealed class SolidDrawSessionColor32 : DrawThinPathSession
        {
            #region Fields

            private readonly Color32 color;

            #endregion

            #region Constructors

            internal SolidDrawSessionColor32(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
                : base(owner, context, bitmapData, path, bounds, drawingOptions, null)
            {
                color = owner.Color32;
            }

            #endregion

            #region Methods

            [SuppressMessage("ReSharper", "InlineTemporaryVariable", Justification = "A local variable is faster than a class field")]
            internal override void DrawLine(PointF start, PointF end)
            {
                Debug.Assert(Region == null && !DrawingOptions.AntiAliasing && !Blend);
                Rectangle bounds = VisibleBounds;
                (Point p1, Point p2) = Round(start, end);
                Color32 c = color;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(bounds.Bottom))
                        return;

                    IBitmapDataRowInternal row = BitmapData.GetRowCached(p1.Y);
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, bounds.Right - 1);
                    for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                        row.DoSetColor32(x, c);

                    return;
                }

                IBitmapDataInternal bitmapData = BitmapData;

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(bounds.Right))
                        return;

                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, bounds.Bottom - 1);
                    for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                        bitmapData.DoSetColor32(p1.X, y, c);

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
                            bitmapData.DoSetColor32(x, y, c);
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
                            bitmapData.DoSetColor32(x, y, c);
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

        #region SolidDrawSession class

        private sealed class SolidDrawSession<TAccessor, TColor, TBaseColor> : DrawThinPathSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, TBaseColor>
            where TColor : unmanaged, IColor<TColor, TBaseColor>
        {
            #region Fields

            private readonly TColor color;
            private readonly TAccessor accessor;

            #endregion

            #region Constructors

            internal SolidDrawSession(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
                : base(owner, context, bitmapData, path, bounds, drawingOptions, null)
            {
                Debug.Assert(!Blend);
                color = owner.GetColor<TColor>();
                accessor = new TAccessor();
                accessor.InitBitmapData(BitmapData);
            }

            #endregion

            #region Methods

            [SuppressMessage("ReSharper", "InlineTemporaryVariable", Justification = "A local variable is faster than a class field")]
            internal override void DrawLine(PointF start, PointF end)
            {
                Debug.Assert(Region == null && !DrawingOptions.AntiAliasing && !Blend);
                Rectangle bounds = VisibleBounds;
                (Point p1, Point p2) = Round(start, end);
                TColor c = color;
                TAccessor acc = accessor;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(bounds.Bottom))
                        return;

                    acc.InitRow(BitmapData.GetRowCached(p1.Y));
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, bounds.Right - 1);
                    for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                        acc.SetColor(x, c);

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
                    for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                        acc.SetColor(p1.X, y, c);

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
                            acc.SetColor(x, y, c);
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
                            acc.SetColor(x, y, c);
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

        #region SolidDrawSessionIndexed class

        private sealed class SolidDrawSessionIndexed : DrawThinPathSession
        {
            #region Fields

            private readonly int colorIndex;

            #endregion

            #region Constructors

            internal SolidDrawSessionIndexed(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath path, Rectangle bounds, DrawingOptions drawingOptions)
                : base(owner, context, bitmapData, path, bounds, drawingOptions, null)
            {
                colorIndex = bitmapData.Palette!.GetNearestColorIndex(owner.Color32);
            }

            #endregion

            #region Methods

            [SuppressMessage("ReSharper", "InlineTemporaryVariable", Justification = "A local variable is faster than a class field")]
            internal override void DrawLine(PointF start, PointF end)
            {
                Debug.Assert(Region == null && !DrawingOptions.AntiAliasing && !Blend);
                Rectangle bounds = VisibleBounds;
                (Point p1, Point p2) = Round(start, end);
                int c = colorIndex;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(bounds.Bottom))
                        return;

                    IBitmapDataRowInternal row = BitmapData.GetRowCached(p1.Y);
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, bounds.Right - 1);
                    for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                        row.DoSetColorIndex(x, c);

                    return;
                }

                IBitmapDataInternal bitmapData = BitmapData;

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(bounds.Right))
                        return;

                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, bounds.Bottom - 1);
                    for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                        bitmapData.DoSetColorIndex(p1.X, y, c);

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
                            bitmapData.DoSetColorIndex(x, y, c);
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
                            bitmapData.DoSetColorIndex(x, y, c);
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

        #region SolidDrawSessionWithQuantizing class

        private sealed class SolidDrawSessionWithQuantizing : DrawThinPathSession
        {
            #region Fields

            private readonly Color32 color;
            private readonly IQuantizingSession quantizingSession;

            #endregion

            #region Constructors

            internal SolidDrawSessionWithQuantizing(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath path, Rectangle bounds,
                DrawingOptions drawingOptions, IQuantizer quantizer)
                : base(owner, context, bitmapData, path, bounds, drawingOptions, null)
            {
                color = owner.Color32;
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                quantizingSession = quantizer.Initialize(bitmapData, context);
                WorkingColorSpace = quantizingSession.WorkingColorSpace;
            }

            #endregion

            #region Methods

            [SuppressMessage("ReSharper", "InlineTemporaryVariable", Justification = "A local variable is faster than a class field")]
            internal override void DrawLine(PointF start, PointF end)
            {
                Debug.Assert(Region == null && !DrawingOptions.AntiAliasing && !Blend);
                Rectangle bounds = VisibleBounds;
                (Point p1, Point p2) = Round(start, end);
                Color32 c = color;
                IQuantizingSession session = quantizingSession;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(bounds.Bottom))
                        return;

                    IBitmapDataRowInternal row = BitmapData.GetRowCached(p1.Y);
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, bounds.Right - 1);
                    for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                        row.DoSetColor32(x, session.GetQuantizedColor(c));

                    return;
                }

                IBitmapDataInternal bitmapData = BitmapData;

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(bounds.Right))
                        return;

                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, bounds.Bottom - 1);
                    for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                        bitmapData.DoSetColor32(p1.X, y, session.GetQuantizedColor(c));

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
                            bitmapData.DoSetColor32(x, y, session.GetQuantizedColor(c));
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
                            bitmapData.DoSetColor32(x, y, session.GetQuantizedColor(c));
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

        #region SolidDrawSessionWithDithering class

        private sealed class SolidDrawSessionWithDithering : DrawThinPathSession
        {
            #region Fields

            private readonly Color32 color;
            private readonly IDitheringSession? ditheringSession;

            #endregion

            #region Constructors

            internal SolidDrawSessionWithDithering(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath path, Rectangle bounds,
                DrawingOptions drawingOptions, IQuantizer quantizer, IDitherer ditherer)
                : base(owner, context, bitmapData, path, bounds, drawingOptions, null)
            {
                color = owner.Color32;
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

            [SuppressMessage("ReSharper", "InlineTemporaryVariable", Justification = "A local variable is faster than a class field")]
            internal override void DrawLine(PointF start, PointF end)
            {
                Debug.Assert(Region == null && !DrawingOptions.AntiAliasing && !Blend);
                IDitheringSession? session = ditheringSession;
                if (session == null)
                {
                    Debug.Fail("Dithering session is not expected to be null here");
                    return;
                }

                Rectangle bounds = VisibleBounds;
                (Point p1, Point p2) = Round(start, end);
                Color32 c = color;

                // horizontal line (or a single point)
                if (p1.Y == p2.Y)
                {
                    if ((uint)p1.Y >= (uint)(bounds.Bottom))
                        return;

                    IBitmapDataRowInternal row = BitmapData.GetRowCached(p1.Y);
                    if (p1.X > p2.X)
                        (p1.X, p2.X) = (p2.X, p1.X);

                    int max = Math.Min(p2.X, bounds.Right - 1);
                    for (int x = Math.Max(p1.X, bounds.Left); x <= max; x++)
                        row.DoSetColor32(x, session.GetDitheredColor(c, x, p1.Y));

                    return;
                }

                IBitmapDataInternal bitmapData = BitmapData;

                // vertical line
                if (p1.X == p2.X)
                {
                    if ((uint)p1.X >= (uint)(bounds.Right))
                        return;

                    if (p1.Y > p2.Y)
                        (p1.Y, p2.Y) = (p2.Y, p1.Y);

                    int max = Math.Min(p2.Y, bounds.Bottom - 1);
                    for (int y = Math.Max(p1.Y, bounds.Top); y <= max; y++)
                        bitmapData.DoSetColor32(p1.X, y, session.GetDitheredColor(c, p1.X, y));

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
                            bitmapData.DoSetColor32(x, y, session.GetDitheredColor(c, x, y));
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
                            bitmapData.DoSetColor32(x, y, session.GetDitheredColor(c, x, y));
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

        #endregion

        #region Fields

        private Color64? colorSrgb;
        private ColorF? colorLinear;

        #endregion

        #region Properties

        private protected override bool HasAlpha
        {
            get
            {
                if (colorSrgb.HasValue)
                    return colorSrgb.Value.A != UInt16.MaxValue;
                return colorLinear!.Value.A < 1f;
            }
        }

        private Color64 Color64 => colorSrgb ??= colorLinear!.Value.ToColor64();
        private ColorF ColorF => colorLinear ??= colorSrgb!.Value.ToColorF();
        private Color32 Color32 => Color64.ToColor32();

        #endregion

        #region Constructors

        internal SolidBrush(Color32 color) => colorSrgb = color.ToColor64();
        internal SolidBrush(Color64 color) => colorSrgb = color;
        internal SolidBrush(ColorF color) => colorLinear = color.Clip();

        #endregion

        #region Methods

        #region Private Protected Methods
        
        private protected override FillPathSession CreateFillSession(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
        {
            IQuantizer? quantizer = drawingOptions.Quantizer;
            IDitherer? ditherer = drawingOptions.Ditherer;
            bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

            // If the quantizer or ditherer relies on the actual [possibly already blended] result we perform the operation in two passes
            if (quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true)
                return new TwoPassSolidFillSession(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer, region);

            // With regular dithering (which implies quantizing, too)
            if (ditherer != null)
                return new SolidFillSessionWithDithering(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer, region);

            // Quantizing without dithering
            if (quantizer != null)
                return new SolidFillSessionWithQuantizing(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer, region);

            // There is no quantizing: picking the most appropriate way for the best quality and performance.
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            bool linearBlending = bitmapData.LinearBlending();
            bool blend = drawingOptions.AlphaBlending && (HasAlpha || drawingOptions.AntiAliasing);

            if (pixelFormat.Indexed && (!blend || !HasAlpha))
                return new SolidFillSessionIndexed(this, context, bitmapData, rawPath, bounds, drawingOptions, region);

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || linearBlending && pixelFormat.LinearGamma)
            {
                // Using PColorF only if the actual pixel format really has linear gamma to prevent performance issues
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true } && (linearBlending || !blend)
                    ? !blend
                        ? new SolidFillSessionNoBlend<AccessorPColorF, PColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : new SolidFillSessionBlendLinear<AccessorPColorF, PColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : !blend
                        ? new SolidFillSessionNoBlend<AccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : linearBlending
                            ? new SolidFillSessionBlendLinear<AccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                            : new SolidFillSessionBlendSrgb<AccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false } && (!linearBlending || !blend)
                    ? !blend
                        ? new SolidFillSessionNoBlend<AccessorPColor64, PColor64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : new SolidFillSessionBlendSrgb<AccessorPColor64, PColor64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : !blend
                        ? new SolidFillSessionNoBlend<AccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : linearBlending
                            ? new SolidFillSessionBlendLinear<AccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                            : new SolidFillSessionBlendSrgb<AccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
            }

            // As Color32 is used much more often than any other formats, using a dedicated class for that due to performance reasons - see the table at the top
            return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false } && (!linearBlending || !blend)
                ? !blend
                    ? new SolidFillSessionNoBlend<AccessorPColor32, PColor32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : new SolidFillSessionBlendSrgb<AccessorPColor32, PColor32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                //: !blend
                //    ? new SolidFillSessionNoBlend<AccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                //    : linearBlending
                //        ? new SolidFillSessionBlendLinear<AccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                //        : new SolidFillSessionBlendSrgb<AccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
                : new SolidFillSessionColor32(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
        }

        private protected override DrawThinPathSession CreateDrawThinPathSession(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
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
                return new SolidDrawSessionWithDithering(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer!, ditherer);

            // Quantizing without dithering
            if (quantizer != null)
                return new SolidDrawSessionWithQuantizing(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizer);

            // There is no quantizing: picking the most appropriate way for the best quality and performance.
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;

            if (pixelFormat.Indexed)
                return new SolidDrawSessionIndexed(this, context, bitmapData, rawPath, bounds, drawingOptions);

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true }
                    ? new SolidDrawSession<AccessorPColorF, PColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions)
                    : new SolidDrawSession<AccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                    ? new SolidDrawSession<AccessorPColor64, PColor64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions)
                    : new SolidDrawSession<AccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions);
            }

            // As Color32 is used much more often than any other formats, using a dedicated class for that due to performance reasons - see the table at the top
            return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                ? new SolidDrawSession<AccessorPColor32, PColor32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions)
                //: new SolidDrawSession<AccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions);
                : new SolidDrawSessionColor32(this, context, bitmapData, rawPath, bounds, drawingOptions);
        }

        #endregion

        #region Private Methods

        // The invalid branches will be optimized away by the JIT compiler
        private TColor GetColor<TColor>() => typeof(TColor) == typeof(Color32) ? (TColor)(object)Color32
            : typeof(TColor) == typeof(PColor32) ? (TColor)(object)Color32.ToPColor32()
            : typeof(TColor) == typeof(Color64) ? (TColor)(object)Color64
            : typeof(TColor) == typeof(PColor64) ? (TColor)(object)Color64.ToPColor64()
            : typeof(TColor) == typeof(ColorF) ? (TColor)(object)ColorF
            : typeof(TColor) == typeof(PColorF) ? (TColor)(object)ColorF.ToPColorF()
            : throw new InvalidOperationException(Res.InternalError($"Unhandled color {typeof(TColor)}"));

        #endregion

        #endregion
    }
}