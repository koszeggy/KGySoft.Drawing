#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SolidBrush.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Collections;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    // NOTE: This class would be much smaller (and less performant) if we just combined a lot of cases, e.g. the separated int/float overloads,
    // or if we called the more primitive shape drawing methods earlier, letting them creating the accessors in each step again and again.
    // Also, we would not need the IBitmapDataAccessor and any of its implementations if we just used a delegate call in the generic classes.
    // But unfortunately the performance of delegates and function pointers is much worse than direct call or generics:
    // 
    //  ==[Draw thin path - Size: {Width=101, Height=101}; Vertices: 73 Results]================================================
    //  Test Time: 5 000 ms
    //  Warming up: Yes
    //  Test cases: 4
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
    [SuppressMessage("ReSharper", "InlineTemporaryVariable", Justification = "A local variable is faster than a class field")]
    internal sealed class SolidBrush : Brush
    {
        #region Nested classes

        #region Fill

        #region SolidFillSessionNoBlend<,,> class

        private sealed class SolidFillSessionNoBlend<TAccessor, TColor, TBaseColor> : FillPathSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, _>
            where TColor : unmanaged, IColor<TColor, TBaseColor>
            where TBaseColor : unmanaged, IColor<TBaseColor, TBaseColor>
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

            [SecuritySafeCritical]
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

            [SecuritySafeCritical]
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
            where TAccessor : struct, IBitmapDataAccessor<TColor, _>
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

            [SecuritySafeCritical]
            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert(Blend && !color.IsOpaque && (uint)scanline.RowIndex < (uint)BitmapData.Height);
                TColor c = color;
                if (c.IsTransparent)
                    return;

                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                var accessor = new TAccessor();
                accessor.InitRow(row);

                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < row.Width);

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) != 1)
                        continue;

                    int pos = x + left;
                    TColor backColor = accessor.GetColor(pos);
                    accessor.SetColor(pos, c.BlendSrgb(backColor));
                }
            }

            [SecuritySafeCritical]
            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert(Blend && (uint)scanline.RowIndex < (uint)BitmapData.Height);
                TColor c = color;
                if (c.IsTransparent)
                    return;

                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                var accessor = new TAccessor();
                accessor.InitRow(row);
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
            where TAccessor : struct, IBitmapDataAccessor<TColor, _>
            where TColor : unmanaged, IColor<TColor, TBaseColor>
            where TBaseColor : unmanaged, IColor<TBaseColor, TBaseColor>
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

            [SecuritySafeCritical]
            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert(Blend && !color.IsOpaque && (uint)scanline.RowIndex < (uint)BitmapData.Height);
                TColor c = color;
                if (c.IsTransparent)
                    return;

                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                var accessor = new TAccessor();
                accessor.InitRow(row);
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

            [SecuritySafeCritical]
            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert(Blend && (uint)scanline.RowIndex < (uint)BitmapData.Height);
                TColor c = color;
                if (c.IsTransparent)
                    return;

                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                var accessor = new TAccessor();
                accessor.InitRow(row);
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

            [SecuritySafeCritical]
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

            [SecuritySafeCritical]
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

            [SecuritySafeCritical]
            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert(scanline.RowIndex < BitmapData.Height);
                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                IQuantizingSession session = quantizingSession;

                if (!Blend)
                {
                    Color32 quantizedColor = session.GetQuantizedColor(c);
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                            row.DoSetColor32(x + left, quantizedColor);
                    }

                    return;
                }

                byte alphaThreshold = session.AlphaThreshold;
                if (c.A == Byte.MinValue)
                    return;

                var colorSpace = session.WorkingColorSpace;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) != 1)
                        continue;

                    Color32 colorSrc = c;
                    if (colorSrc.A != Byte.MaxValue)
                    {
                        Color32 colorDst = row.DoGetColor32(x + left);
                        if (colorDst.A != Byte.MinValue)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                ? colorSrc.BlendWithBackground(colorDst, colorSpace)
                                : colorSrc.BlendWith(colorDst, colorSpace);
                        }

                        if (colorSrc.A < alphaThreshold)
                            continue;
                    }

                    row.DoSetColor32(x + left, session.GetQuantizedColor(colorSrc));
                }
            }

            [SecuritySafeCritical]
            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)BitmapData.Height);
                IBitmapDataRowInternal row = BitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                IQuantizingSession session = quantizingSession;

                // no blending: writing even transparent result pixels
                if (!Blend)
                {
                    Color32 quantizedColor = session.GetQuantizedColor(c);
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                row.DoSetColor32(x + left, quantizedColor);
                                continue;
                            default:
                                row.DoSetColor32(x + left, session.GetQuantizedColor(Color32.FromArgb(c.A == Byte.MaxValue ? value : (byte)(value * c.A / Byte.MaxValue), c)));
                                continue;
                        }
                    }

                    return;
                }

                // From this point there is blending. Working in a compatible way with DrawInto (important to be consistent with TwoPassSolidSession):
                // fully transparent source is skipped, just like when the alpha of the blended result is smaller than the threshold
                byte alphaThreshold = session.AlphaThreshold;
                if (c.A == Byte.MinValue)
                    return;

                var colorSpace = session.WorkingColorSpace;
                if (c.A == Byte.MaxValue)
                {
                    Color32? quantizedColor = null;
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                quantizedColor ??= session.GetQuantizedColor(c);
                                row.DoSetColor32(x + left, quantizedColor.Value);
                                continue;
                            default:
                                int pos = x + left;
                                Color32 colorSrc = Color32.FromArgb(value, c).Blend(row.DoGetColor32(pos), colorSpace);
                                if (colorSrc.A >= alphaThreshold)
                                    row.DoSetColor32(pos, session.GetQuantizedColor(colorSrc));
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
                            Color32 colorSrc = c.Blend(row.DoGetColor32(pos), colorSpace);
                            if (colorSrc.A >= alphaThreshold)
                                row.DoSetColor32(pos, session.GetQuantizedColor(colorSrc));
                            continue;
                        default:
                            pos = x + left;
                            colorSrc = Color32.FromArgb((byte)(value * c.A / Byte.MaxValue), c).Blend(row.DoGetColor32(pos), colorSpace);
                            if (colorSrc.A >= alphaThreshold)
                                row.DoSetColor32(pos, session.GetQuantizedColor(colorSrc));
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

            [SecuritySafeCritical]
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
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) != 1)
                            continue;

                        int pos = x + left;
                        row.DoSetColor32(pos, session.GetDitheredColor(c, pos, scanline.RowIndex));
                    }

                    return;
                }

                byte alphaThreshold = quantizingSession.AlphaThreshold;
                if (c.A == Byte.MinValue)
                    return;

                var colorSpace = quantizingSession.WorkingColorSpace;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) != 1)
                        continue;

                    int pos = x + left;
                    Color32 colorSrc = c;
                    if (colorSrc.A != Byte.MaxValue)
                    {
                        Color32 colorDst = row.DoGetColor32(pos);
                        if (colorDst.A != Byte.MinValue)
                        {
                            colorSrc = colorDst.A == Byte.MaxValue
                                ? colorSrc.BlendWithBackground(colorDst, colorSpace)
                                : colorSrc.BlendWith(colorDst, colorSpace);
                        }

                        if (colorSrc.A < alphaThreshold)
                            continue;
                    }

                    row.DoSetColor32(pos, session.GetDitheredColor(colorSrc, pos, scanline.RowIndex));
                }
            }

            [SecuritySafeCritical]
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
                if (c.A == Byte.MinValue)
                    return;

                var colorSpace = quantizingSession.WorkingColorSpace;
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
                                row.DoSetColor32(pos, session.GetDitheredColor(c, pos, scanline.RowIndex));
                                continue;
                            default:
                                pos = x + left;
                                Color32 colorSrc = Color32.FromArgb(value, c).Blend(row.DoGetColor32(pos), colorSpace);
                                if (colorSrc.A >= alphaThreshold)
                                    row.DoSetColor32(pos, session.GetDitheredColor(colorSrc, pos, scanline.RowIndex));
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
                            Color32 colorSrc = c.Blend(row.DoGetColor32(pos), colorSpace);
                            if (colorSrc.A >= alphaThreshold)
                                row.DoSetColor32(pos, session.GetDitheredColor(colorSrc, pos, scanline.RowIndex));
                            continue;
                        default:
                            pos = x + left;
                            colorSrc = Color32.FromArgb((byte)(value * c.A / Byte.MaxValue), c).Blend(row.DoGetColor32(pos), colorSpace);
                            if (colorSrc.A >= alphaThreshold)
                                row.DoSetColor32(pos, session.GetDitheredColor(colorSrc, pos, scanline.RowIndex));
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

            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                #region Local Methods

                [SecuritySafeCritical]
                void ProcessNoBlending(in RegionScanline scanline)
                {
                    Color32 c = color;
                    int y = scanline.RowIndex - bounds.Top;
                    IBitmapDataRowInternal rowDst = firstSessionTarget.GetRowCached(y);
                    int offset = scanline.Left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                            continue;

                        rowDst.DoSetColor32(x + offset, c);
                    }
                }

                [SecuritySafeCritical]
                void ProcessWithBlending(in RegionScanline scanline)
                {
                    Debug.Assert(color.A != Byte.MaxValue);
                    Color32 c = color;
                    IBitmapDataRowInternal rowDst = firstSessionTarget.GetRowCached(scanline.RowIndex - bounds.Top);
                    IBitmapDataRowInternal rowBackground = BitmapData.GetRowCached(scanline.RowIndex);
                    WorkingColorSpace colorSpace = WorkingColorSpace;
                    int left = scanline.Left;
                    int offset = left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                            continue;

                        Color32 colorDst = rowBackground.DoGetColor32(x + left);
                        rowDst.DoSetColor32(x + offset, c.Blend(colorDst, colorSpace));
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

                if (color.A > Byte.MinValue)
                    ProcessWithBlending(scanline);
            }

            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                #region Local Methods

                [SecuritySafeCritical]
                void ProcessNoBlending(in RegionScanline scanline)
                {
                    Color32 c = color;
                    int y = scanline.RowIndex - bounds.Top;
                    IBitmapDataRowInternal rowDst = firstSessionTarget.GetRowCached(y);
                    ArraySection<byte> rowMask = mask[y];
                    int offset = scanline.Left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        int pos = x + offset;
                        ColorExtensions.Set1bppColorIndex(ref rowMask.GetElementReferenceUnchecked(pos >> 3), pos, value);

                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                rowDst.DoSetColor32(pos, c);
                                continue;
                            default:
                                rowDst.DoSetColor32(pos, Color32.FromArgb(c.A == Byte.MaxValue ? value : (byte)(value * c.A / Byte.MaxValue), c));
                                continue;
                        }
                    }

                }

                [SecuritySafeCritical]
                void ProcessWithBlendingSolid(in RegionScanline scanline)
                {
                    Color32 c = color;
                    int y = scanline.RowIndex - bounds.Top;
                    IBitmapDataRowInternal rowDst = firstSessionTarget.GetRowCached(y);
                    ArraySection<byte> rowMask = mask[y];
                    WorkingColorSpace colorSpace = WorkingColorSpace;
                    IBitmapDataRowInternal rowBackground = BitmapData.GetRowCached(scanline.RowIndex);
                    int left = scanline.Left;
                    int offset = left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        int pos = x + offset;
                        ColorExtensions.Set1bppColorIndex(ref rowMask.GetElementReferenceUnchecked(pos >> 3), pos, value);

                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                rowDst.DoSetColor32(pos, c);
                                continue;
                            default:
                                Color32 backColor = rowBackground.DoGetColor32(x + left);
                                rowDst.DoSetColor32(pos, Color32.FromArgb(value, c).Blend(backColor, colorSpace));
                                continue;
                        }
                    }
                }

                void ProcessWithBlendingAlpha(in RegionScanline scanline)
                {
                    Color32 c = color;
                    int y = scanline.RowIndex - bounds.Top;
                    IBitmapDataRowInternal rowDst = firstSessionTarget.GetRowCached(y);
                    ArraySection<byte> rowMask = mask[y];
                    WorkingColorSpace colorSpace = WorkingColorSpace;
                    IBitmapDataRowInternal rowBackground = BitmapData.GetRowCached(scanline.RowIndex);
                    int left = scanline.Left;
                    int offset = left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        int pos = x + offset;
                        ColorExtensions.Set1bppColorIndex(ref rowMask.GetElementReferenceUnchecked(pos >> 3), pos, value);

                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                Color32 backColor = rowBackground.DoGetColor32(x + left);
                                rowDst.DoSetColor32(pos, c.Blend(backColor, colorSpace));
                                continue;
                            default:
                                backColor = rowBackground.DoGetColor32(x + left);
                                rowDst.DoSetColor32(pos, Color32.FromArgb((byte)(value * c.A / Byte.MaxValue), c).Blend(backColor, colorSpace));
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

        #region SolidDrawSession<,,> class

        private sealed class SolidDrawSession<TAccessor, TColor, TArg> : DrawThinPathSession
            where TAccessor : struct, IBitmapDataAccessor<TColor, TArg>
            where TColor : unmanaged
        {
            #region Fields

            private readonly TColor color;
            private readonly TArg arg;
            private readonly Action? disposeCallback;

            #endregion

            #region Constructors

            internal SolidDrawSession(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath path, Rectangle bounds,
                DrawingOptions drawingOptions, TColor color, TArg arg = default!, Action? disposeCallback = null)
                : base(owner, context, bitmapData, path, bounds, drawingOptions, null)
            {
                Debug.Assert(!Blend);
                this.color = color;
                this.arg = arg;
                this.disposeCallback = disposeCallback;
            }

            #endregion

            #region Methods

            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal override void DrawLine(PointF start, PointF end)
                => DirectDrawer.GenericDrawer<TAccessor, TColor, TArg>.DrawLine(BitmapData, start, end, color, PixelOffset, arg);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal override void DrawEllipse(RectangleF bounds)
                => DirectDrawer.GenericDrawer<TAccessor, TColor, TArg>.DrawEllipse(BitmapData, bounds, color, PixelOffset, arg);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            internal override void DrawArc(ArcSegment arc)
                => DirectDrawer.GenericDrawer<TAccessor, TColor, TArg>.DrawArc(BitmapData, arc, color, PixelOffset, arg);

            protected override void Dispose(bool disposing)
            {
                disposeCallback?.Invoke();
                base.Dispose(disposing);
            }

            #endregion
        }

        #endregion

        #endregion

        #endregion

        #region Fields

        private Color32? color32;
        private Color64? color64;
        private ColorF? colorF;

        #endregion

        #region Properties

        internal override bool HasAlpha { get; }
        private Color32 Color32 => color32 ??= color64?.ToColor32() ?? colorF!.Value.ToColor32();
        private Color64 Color64 => color64 ??= colorF?.ToColor64() ?? color32!.Value.ToColor64();
        private ColorF ColorF => colorF ??= color64?.ToColorF() ?? color32!.Value.ToColorF();

        #endregion

        #region Constructors

        internal SolidBrush(Color32 color)
        {
            color32 = color;
            HasAlpha = color.A != Byte.MaxValue;
        }

        internal SolidBrush(Color64 color)
        {
            color64 = color;
            HasAlpha = color.A != UInt16.MaxValue;
        }

        internal SolidBrush(ColorF color)
        {
            colorF = color.Clip();
            HasAlpha = color.A < 1f;
        }

        #endregion

        #region Methods

        #region Internal Methods
        // These direct drawing methods perform thin line drawing without creating a session.

        internal void DrawLine(IReadWriteBitmapData bitmapData, Point p1, Point p2)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawLine(bitmap, p1, p2, ColorF.ToPColorF());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawLine(bitmap, p1, p2, ColorF);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawLine(bitmap, p1, p2, Color64.ToPColor64());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawLine(bitmap, p1, p2, Color64);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawLine(bitmap, p1, p2, Color32.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawLine(bitmap, p1, p2, bitmapData.Palette!.GetNearestColorIndex(Color32));
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawLine(bitmap, p1, p2, Color32);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawLine(bitmap, p1, p2, ColorF.ToPColorF());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawLine(bitmap, p1, p2, ColorF);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawLine(bitmap, p1, p2, Color64.ToPColor64());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawLine(bitmap, p1, p2, Color64);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawLine(bitmap, p1, p2, Color32.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawLine(bitmap, p1, p2, bitmapData.Palette!.GetNearestColorIndex(Color32));
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawLine(bitmap, p1, p2, Color32);
        }

        internal void DrawLine(IReadWriteBitmapData bitmapData, PointF p1, PointF p2, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawLine(bitmap, p1, p2, ColorF.ToPColorF(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawLine(bitmap, p1, p2, ColorF, offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawLine(bitmap, p1, p2, Color64.ToPColor64(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawLine(bitmap, p1, p2, Color64, offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawLine(bitmap, p1, p2, Color32.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawLine(bitmap, p1, p2, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawLine(bitmap, p1, p2, Color32, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawLine(bitmap, p1, p2, ColorF.ToPColorF(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawLine(bitmap, p1, p2, ColorF, offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawLine(bitmap, p1, p2, Color64.ToPColor64(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawLine(bitmap, p1, p2, Color64, offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawLine(bitmap, p1, p2, Color32.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawLine(bitmap, p1, p2, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawLine(bitmap, p1, p2, Color32, offset);
        }

        internal void DrawLines(IReadWriteBitmapData bitmapData, IEnumerable<Point> points)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawLines(bitmap, points, ColorF.ToPColorF());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawLines(bitmap, points, ColorF);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawLines(bitmap, points, Color64.ToPColor64());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawLines(bitmap, points, Color64);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawLines(bitmap, points, Color32.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawLines(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(Color32));
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawLines(bitmap, points, Color32);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawLines(bitmap, points, ColorF.ToPColorF());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawLines(bitmap, points, ColorF);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawLines(bitmap, points, Color64.ToPColor64());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawLines(bitmap, points, Color64);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawLines(bitmap, points, Color32.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawLines(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(Color32));
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawLines(bitmap, points, Color32);
        }

        internal void DrawLines(IReadWriteBitmapData bitmapData, IEnumerable<PointF> points, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawLines(bitmap, points, ColorF.ToPColorF(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawLines(bitmap, points, ColorF, offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawLines(bitmap, points, Color64.ToPColor64(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawLines(bitmap, points, Color64, offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawLines(bitmap, points, Color32.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawLines(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawLines(bitmap, points, Color32, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawLines(bitmap, points, ColorF.ToPColorF(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawLines(bitmap, points, ColorF, offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawLines(bitmap, points, Color64.ToPColor64(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawLines(bitmap, points, Color64, offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawLines(bitmap, points, Color32.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawLines(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawLines(bitmap, points, Color32, offset);
        }

        internal void DrawBeziers(IReadWriteBitmapData bitmapData, IList<PointF> points, float offset)
        {
            if (points.Count == 0)
                return;

            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawBeziers(bitmap, points, ColorF.ToPColorF(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawBeziers(bitmap, points, ColorF, offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawBeziers(bitmap, points, Color64.ToPColor64(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawBeziers(bitmap, points, Color64, offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawBeziers(bitmap, points, Color32.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawBeziers(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawBeziers(bitmap, points, Color32, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawBeziers(bitmap, points, ColorF.ToPColorF(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawBeziers(bitmap, points, ColorF, offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawBeziers(bitmap, points, Color64.ToPColor64(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawBeziers(bitmap, points, Color64, offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawBeziers(bitmap, points, Color32.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawBeziers(bitmap, points, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawBeziers(bitmap, points, Color32, offset);
        }

        internal void DrawPolygon(IReadWriteBitmapData bitmapData, IEnumerable<Point> points)
        {
            IList<Point> pointList = points as IList<Point> ?? new List<Point>(points);
            if (pointList.Count == 0)
                return;

            if (pointList[0] != pointList[pointList.Count - 1])
            {
                if (ReferenceEquals(points, pointList))
                    pointList = new List<Point>(pointList) { pointList[0] };
                else
                    pointList.Add(pointList[0]);
            }

            DrawLines(bitmapData, pointList);
        }

        internal void DrawPolygon(IReadWriteBitmapData bitmapData, IEnumerable<PointF> points, float offset)
        {
            IList<PointF> pointList = points as IList<PointF> ?? new List<PointF>(points);
            if (pointList.Count == 0)
                return;

            if (pointList[0] != pointList[pointList.Count - 1])
            {
                if (ReferenceEquals(points, pointList))
                    pointList = new List<PointF>(pointList) { pointList[0] };
                else
                    pointList.Add(pointList[0]);
            }

            DrawLines(bitmapData, pointList, offset);
        }

        internal void DrawRectangle(IReadWriteBitmapData bitmapData, Rectangle rectangle)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawRectangle(bitmap, rectangle, ColorF.ToPColorF());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawRectangle(bitmap, rectangle, ColorF);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawRectangle(bitmap, rectangle, Color64.ToPColor64());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawRectangle(bitmap, rectangle, Color64);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawRectangle(bitmap, rectangle, Color32.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawRectangle(bitmap, rectangle, bitmapData.Palette!.GetNearestColorIndex(Color32));
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawRectangle(bitmap, rectangle, Color32);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawRectangle(bitmap, rectangle, ColorF.ToPColorF());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawRectangle(bitmap, rectangle, ColorF);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawRectangle(bitmap, rectangle, Color64.ToPColor64());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawRectangle(bitmap, rectangle, Color64);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawRectangle(bitmap, rectangle, Color32.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawRectangle(bitmap, rectangle, bitmapData.Palette!.GetNearestColorIndex(Color32));
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawRectangle(bitmap, rectangle, Color32);
        }

        internal void DrawRectangle(IReadWriteBitmapData bitmapData, RectangleF rectangle, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawRectangle(bitmap, rectangle, ColorF.ToPColorF(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawRectangle(bitmap, rectangle, ColorF, offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawRectangle(bitmap, rectangle, Color64.ToPColor64(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawRectangle(bitmap, rectangle, Color64, offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawRectangle(bitmap, rectangle, Color32.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawRectangle(bitmap, rectangle, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawRectangle(bitmap, rectangle, Color32, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawRectangle(bitmap, rectangle, ColorF.ToPColorF(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawRectangle(bitmap, rectangle, ColorF, offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawRectangle(bitmap, rectangle, Color64.ToPColor64(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawRectangle(bitmap, rectangle, Color64, offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawRectangle(bitmap, rectangle, Color32.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawRectangle(bitmap, rectangle, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawRectangle(bitmap, rectangle, Color32, offset);
        }

        internal void DrawEllipse(IReadWriteBitmapData bitmapData, Rectangle bounds)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawEllipse(bitmap, bounds, ColorF.ToPColorF());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawEllipse(bitmap, bounds, ColorF);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawEllipse(bitmap, bounds, Color64.ToPColor64());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawEllipse(bitmap, bounds, Color64);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawEllipse(bitmap, bounds, Color32.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawEllipse(bitmap, bounds, bitmapData.Palette!.GetNearestColorIndex(Color32));
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawEllipse(bitmap, bounds, Color32);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawEllipse(bitmap, bounds, ColorF.ToPColorF());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawEllipse(bitmap, bounds, ColorF);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawEllipse(bitmap, bounds, Color64.ToPColor64());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawEllipse(bitmap, bounds, Color64);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawEllipse(bitmap, bounds, Color32.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawEllipse(bitmap, bounds, bitmapData.Palette!.GetNearestColorIndex(Color32));
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawEllipse(bitmap, bounds, Color32);
        }

        internal void DrawEllipse(IReadWriteBitmapData bitmapData, RectangleF bounds, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawEllipse(bitmap, bounds, ColorF.ToPColorF(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawEllipse(bitmap, bounds, ColorF, offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawEllipse(bitmap, bounds, Color64.ToPColor64(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawEllipse(bitmap, bounds, Color64, offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawEllipse(bitmap, bounds, Color32.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawEllipse(bitmap, bounds, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawEllipse(bitmap, bounds, Color32, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawEllipse(bitmap, bounds, ColorF.ToPColorF(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawEllipse(bitmap, bounds, ColorF, offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawEllipse(bitmap, bounds, Color64.ToPColor64(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawEllipse(bitmap, bounds, Color64, offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawEllipse(bitmap, bounds, Color32.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawEllipse(bitmap, bounds, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawEllipse(bitmap, bounds, Color32, offset);
        }

        internal void DrawArc(IReadWriteBitmapData bitmapData, Rectangle bounds, float startAngle, float sweepAngle)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, ColorF.ToPColorF());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, ColorF);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color64.ToPColor64());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color64);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color32.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(Color32));
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color32);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, ColorF.ToPColorF());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, ColorF);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color64.ToPColor64());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color64);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color32.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(Color32));
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color32);
        }

        internal void DrawArc(IReadWriteBitmapData bitmapData, RectangleF bounds, float startAngle, float sweepAngle, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, ColorF.ToPColorF(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, ColorF, offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color64.ToPColor64(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color64, offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color32.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color32, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, ColorF.ToPColorF(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, ColorF, offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color64.ToPColor64(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color64, offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color32.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawArc(bitmap, bounds, startAngle, sweepAngle, Color32, offset);
        }

        internal void DrawPie(IReadWriteBitmapData bitmapData, RectangleF bounds, float startAngle, float sweepAngle, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, ColorF.ToPColorF(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, ColorF, offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, Color64.ToPColor64(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, Color64, offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, Color32.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, Color32, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, ColorF.ToPColorF(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, ColorF, offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, Color64.ToPColor64(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, Color64, offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, Color32.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawPie(bitmap, bounds, startAngle, sweepAngle, Color32, offset);
        }

        internal void DrawRoundedRectangle(IReadWriteBitmapData bitmapData, Rectangle bounds, int cornerRadius)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, ColorF.ToPColorF());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, ColorF);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color64.ToPColor64());
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color64);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color32.ToPColor32());
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, bitmapData.Palette!.GetNearestColorIndex(Color32));
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color32);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, ColorF.ToPColorF());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, ColorF);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color64.ToPColor64());
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color64);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color32.ToPColor32());
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, bitmapData.Palette!.GetNearestColorIndex(Color32));
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color32);
        }

        internal void DrawRoundedRectangle(IReadWriteBitmapData bitmapData, RectangleF bounds, float cornerRadius, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, ColorF.ToPColorF(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, ColorF, offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color64.ToPColor64(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color64, offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color32.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color32, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, ColorF.ToPColorF(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, ColorF, offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color64.ToPColor64(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color64, offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color32.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, cornerRadius, Color32, offset);
        }

        internal void DrawRoundedRectangle(IReadWriteBitmapData bitmapData, RectangleF bounds,
            float radiusTopLeft, float radiusTopRight, float radiusBottomRight, float radiusBottomLeft, float offset)
        {
            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            if (bitmapData is ICustomBitmapData)
            {
                // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
                if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, ColorF.ToPColorF(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, ColorF, offset);
                    return;
                }

                if (pixelFormat.Prefers64BitColors)
                {
                    if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, Color64.ToPColor64(), offset);
                    else
                        DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, Color64, offset);
                    return;
                }

                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, Color32.ToPColor32(), offset);
                    return;
                }

                if (pixelFormat.Indexed)
                {
                    DirectDrawer.GenericDrawer<CustomBitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                    return;
                }

                DirectDrawer.GenericDrawer<CustomBitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, Color32, offset);
                return;
            }

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, ColorF.ToPColorF(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, ColorF, offset);
                return;
            }

            if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
                    DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, Color64.ToPColor64(), offset);
                else
                    DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, Color64, offset);
                return;
            }

            if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false })
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, Color32.ToPColor32(), offset);
                return;
            }

            if (pixelFormat.Indexed)
            {
                DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, bitmapData.Palette!.GetNearestColorIndex(Color32), offset);
                return;
            }

            DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.DrawRoundedRectangle(bitmap, bounds, radiusTopLeft, radiusTopRight, radiusBottomRight, radiusBottomLeft, Color32, offset);
        }

        internal bool FillRectangle(IAsyncContext context, IReadWriteBitmapData bitmapData, Rectangle rectangle)
        {
            rectangle = rectangle.IntersectSafe(new Rectangle(Point.Empty, bitmapData.Size));
            if (rectangle.IsEmpty())
                return !context.IsCancellationRequested;

            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;
            IBitmapDataInternal bitmap = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, false, true);

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true }
                    ? DirectDrawer.GenericDrawer<BitmapDataAccessorPColorF, PColorF, _>.FillRectangle(context, bitmap, ColorF.ToPColorF(), rectangle)
                    : DirectDrawer.GenericDrawer<BitmapDataAccessorColorF, ColorF, _>.FillRectangle(context, bitmap, ColorF, rectangle);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                    ? DirectDrawer.GenericDrawer<BitmapDataAccessorPColor64, PColor64, _>.FillRectangle(context, bitmap, Color64.ToPColor64(), rectangle)
                    : DirectDrawer.GenericDrawer<BitmapDataAccessorColor64, Color64, _>.FillRectangle(context, bitmap, Color64, rectangle);
            }

            return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                ? DirectDrawer.GenericDrawer<BitmapDataAccessorPColor32, PColor32, _>.FillRectangle(context, bitmap, Color32.ToPColor32(), rectangle)
                : pixelFormat.Indexed
                    ? DirectDrawer.GenericDrawer<BitmapDataAccessorIndexed, int, _>.FillRectangle(context, bitmap, bitmapData.Palette!.GetNearestColorIndex(Color32), rectangle)
                    : DirectDrawer.GenericDrawer<BitmapDataAccessorColor32, Color32, _>.FillRectangle(context, bitmap, Color32, rectangle);
        }

        #endregion

        #region Private Protected Methods

        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
            Justification = "Optimizations for special cases. Not extracting additional methods to prevent placing more frames on the call stack.")]
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
            bool linearBlending = bitmapData.IsLinearGamma();
            bool blend = drawingOptions.AlphaBlending && (HasAlpha || drawingOptions.AntiAliasing);

            if (pixelFormat.Indexed && (!blend || !HasAlpha))
                return new SolidFillSessionIndexed(this, context, bitmapData, rawPath, bounds, drawingOptions, region);

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || linearBlending && pixelFormat.LinearGamma)
            {
                // Using PColorF only if the actual pixel format really has linear gamma to prevent performance issues
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true } && (linearBlending || !blend)
                    ? !blend
                        ? new SolidFillSessionNoBlend<BitmapDataAccessorPColorF, PColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : new SolidFillSessionBlendLinear<BitmapDataAccessorPColorF, PColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : !blend
                        ? new SolidFillSessionNoBlend<BitmapDataAccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : linearBlending
                            ? new SolidFillSessionBlendLinear<BitmapDataAccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                            : new SolidFillSessionBlendSrgb<BitmapDataAccessorColorF, ColorF, ColorF>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false } && (!linearBlending || !blend)
                    ? !blend
                        ? new SolidFillSessionNoBlend<BitmapDataAccessorPColor64, PColor64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : new SolidFillSessionBlendSrgb<BitmapDataAccessorPColor64, PColor64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : !blend
                        ? new SolidFillSessionNoBlend<BitmapDataAccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : linearBlending
                            ? new SolidFillSessionBlendLinear<BitmapDataAccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                            : new SolidFillSessionBlendSrgb<BitmapDataAccessorColor64, Color64, Color64>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
            }

            // Unlike drawing, fill has no special SolidFillSessionColor32 because these split generic cases are actually faster
            return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false } && (!linearBlending || !blend)
                ? !blend
                    ? new SolidFillSessionNoBlend<BitmapDataAccessorPColor32, PColor32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : new SolidFillSessionBlendSrgb<BitmapDataAccessorPColor32, PColor32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                : !blend
                    ? new SolidFillSessionNoBlend<BitmapDataAccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                    : linearBlending
                        ? new SolidFillSessionBlendLinear<BitmapDataAccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region)
                        : new SolidFillSessionBlendSrgb<BitmapDataAccessorColor32, Color32, Color32>(this, context, bitmapData, rawPath, bounds, drawingOptions, region);
        }

        private protected override DrawThinPathSession? CreateDrawThinPathSession(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
        {
            if (region != null)
                return base.CreateDrawThinPathSession(context, bitmapData, rawPath, bounds, drawingOptions, region);

            Debug.Assert(!drawingOptions.AntiAliasing && (!drawingOptions.AlphaBlending || !HasAlpha));
            if (bitmapData is ICustomBitmapData)
                return CreateDrawThinPathSessionCustom(context, bitmapData, rawPath, bounds, drawingOptions);

            IQuantizer ? quantizer = drawingOptions.Quantizer;
            IDitherer? ditherer = drawingOptions.Ditherer;
            bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

            Debug.Assert(quantizer?.InitializeReliesOnContent != true && ditherer?.InitializeReliesOnContent != true && ditherer is not (ErrorDiffusionDitherer or RandomNoiseDitherer { HasSeed: true }));

            // With regular dithering (which implies quantizing, too)
            if (ditherer != null)
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                IQuantizingSession quantizingSession = quantizer!.Initialize(bitmapData, context);
                if (context.IsCancellationRequested)
                {
                    quantizingSession.Dispose();
                    return null;
                }

                context.Progress?.New(DrawingOperation.InitializingDitherer);
                IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession, context);

                return new SolidDrawSession<BitmapDataAccessorDithering, Color32, IDitheringSession>(this, context, bitmapData, rawPath, bounds, drawingOptions, Color32,
                    ditheringSession, () => { ditheringSession.Dispose(); quantizingSession.Dispose(); });
            }

            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;

            // Quantizing without dithering. As we don't have blending we need to quantize a single color that we can do before creating the drawing session.
            if (quantizer != null)
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                Color32 quantizedColor;
                using (var quantizingSession = quantizer.Initialize(bitmapData, context))
                    quantizedColor = quantizingSession.GetQuantizedColor(Color32);

                if (pixelFormat.Indexed)
                        return new SolidDrawSession<BitmapDataAccessorIndexed, int, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, bitmapData.Palette!.GetNearestColorIndex(quantizedColor));
                return new SolidDrawSession<BitmapDataAccessorColor32, Color32, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizedColor);
            }

            // There is no quantizing: picking the most appropriate way for the best quality and performance.

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true }
                    ? new SolidDrawSession<BitmapDataAccessorPColorF, PColorF, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, ColorF.ToPColorF())
                    : new SolidDrawSession<BitmapDataAccessorColorF, ColorF, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, ColorF);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                    ? new SolidDrawSession<BitmapDataAccessorPColor64, PColor64, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, Color64.ToPColor64())
                    : new SolidDrawSession<BitmapDataAccessorColor64, Color64, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, Color64);
            }

            return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                ? new SolidDrawSession<BitmapDataAccessorPColor32, PColor32, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, Color32.ToPColor32())
                : pixelFormat.Indexed
                    ? new SolidDrawSession<BitmapDataAccessorIndexed, int, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, bitmapData.Palette!.GetNearestColorIndex(Color32))
                    : new SolidDrawSession<BitmapDataAccessorColor32, Color32, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, Color32);
        }

        #endregion

        #region Private Methods

        private DrawThinPathSession? CreateDrawThinPathSessionCustom(IAsyncContext context, IReadWriteBitmapData bitmapData, RawPath rawPath, Rectangle bounds, DrawingOptions drawingOptions)
        {
            IQuantizer? quantizer = drawingOptions.Quantizer;
            IDitherer? ditherer = drawingOptions.Ditherer;
            bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

            Debug.Assert(quantizer?.InitializeReliesOnContent != true && ditherer?.InitializeReliesOnContent != true);

            // With regular dithering (which implies quantizing, too)
            if (ditherer != null)
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                IQuantizingSession quantizingSession = quantizer!.Initialize(bitmapData, context);
                if (context.IsCancellationRequested)
                {
                    quantizingSession.Dispose();
                    return null;
                }

                context.Progress?.New(DrawingOperation.InitializingDitherer);
                IDitheringSession ditheringSession = ditherer.Initialize(bitmapData, quantizingSession, context);

                return new SolidDrawSession<CustomBitmapDataAccessorDithering, Color32, IDitheringSession>(this, context, bitmapData, rawPath, bounds, drawingOptions, Color32,
                    ditheringSession, () => { ditheringSession.Dispose(); quantizingSession.Dispose(); });
            }

            PixelFormatInfo pixelFormat = bitmapData.PixelFormat;

            // Quantizing without dithering. As we don't have blending we need to quantize a single color that we can do before creating the drawing session.
            if (quantizer != null)
            {
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                Color32 quantizedColor;
                using (var quantizingSession = quantizer.Initialize(bitmapData, context))
                    quantizedColor = quantizingSession.GetQuantizedColor(Color32);

                if (pixelFormat.Indexed)
                    return new SolidDrawSession<CustomBitmapDataAccessorIndexed, int, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, bitmapData.Palette!.GetNearestColorIndex(quantizedColor));
                return new SolidDrawSession<CustomBitmapDataAccessorColor32, Color32, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, quantizedColor);
            }

            // There is no quantizing: picking the most appropriate way for the best quality and performance.

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true }
                    ? new SolidDrawSession<CustomBitmapDataAccessorPColorF, PColorF, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, ColorF.ToPColorF())
                    : new SolidDrawSession<CustomBitmapDataAccessorColorF, ColorF, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, ColorF);
            }

            if (pixelFormat.Prefers64BitColors)
            {
                return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                    ? new SolidDrawSession<CustomBitmapDataAccessorPColor64, PColor64, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, Color64.ToPColor64())
                    : new SolidDrawSession<CustomBitmapDataAccessorColor64, Color64, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, Color64);
            }

            return pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                ? new SolidDrawSession<CustomBitmapDataAccessorPColor32, PColor32, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, Color32.ToPColor32())
                : pixelFormat.Indexed
                    ? new SolidDrawSession<CustomBitmapDataAccessorIndexed, int, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, bitmapData.Palette!.GetNearestColorIndex(Color32))
                    : new SolidDrawSession<CustomBitmapDataAccessorColor32, Color32, _>(this, context, bitmapData, rawPath, bounds, drawingOptions, Color32);
        }

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