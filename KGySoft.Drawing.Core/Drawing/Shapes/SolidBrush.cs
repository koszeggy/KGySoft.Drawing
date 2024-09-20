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
using System.Drawing;
using System.Runtime.CompilerServices;

using KGySoft.Collections;
using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal sealed class SolidBrush : Brush
    {
        #region Nested classes

        #region SolidFillSessionNoBlendin class

        // TODO: separate Color32/64/F sessions in by CreateSession by pixelformat preference
        private sealed class SolidFillSessionNoBlending/*Color32 TODO*/ : FillPathSession
        {
            #region Fields
            
            private readonly Color32 color;
            private readonly IBitmapDataInternal bitmapData;

            #endregion

            #region Constructors

            internal SolidFillSessionNoBlending(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(context, drawingOptions, bounds, region)
            {
                color = owner.Color32;
                this.bitmapData = (bitmapData as IBitmapDataInternal) ?? new BitmapDataWrapper(bitmapData, false, true);
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)bitmapData.Height);
                IBitmapDataRowInternal row = bitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                Debug.Assert(scanline.MinIndex + left >= 0 && scanline.MaxIndex + left < row.Width);
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 1)
                        row.DoSetColor32(x + left, c);
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline scanline)
            {
                Debug.Assert((uint)scanline.RowIndex < (uint)bitmapData.Height);
                IBitmapDataRowInternal row = bitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;

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
            }

            #endregion
        }

        #endregion

        #region SolidFillSessionWithBlending class

        private sealed class SolidFillSessionWithBlending : FillPathSession
        {
            #region Fields

            private readonly Color32 color;
            private readonly IBitmapDataInternal bitmapData;
            private readonly WorkingColorSpace workingColorSpace;

            #endregion

            #region Constructors

            internal SolidFillSessionWithBlending(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
                : base(context, drawingOptions, bounds, region)
            {
                color = owner.Color32;
                this.bitmapData = (bitmapData as IBitmapDataInternal) ?? new BitmapDataWrapper(bitmapData, true, true);
                workingColorSpace = bitmapData.GetPreferredColorSpace();
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert(scanline.RowIndex < bitmapData.Height);
                Debug.Assert(color.A < Byte.MaxValue);
                IBitmapDataRowInternal row = bitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                var colorSpace = workingColorSpace;

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
                Debug.Assert(scanline.RowIndex < bitmapData.Height);
                IBitmapDataRowInternal row = bitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                var colorSpace = workingColorSpace;

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

        #region SolidFillSessionWithQuntizing class

        private sealed class SolidFillSessionWithQuantizing : FillPathSession
        {
            #region Fields

            private readonly Color32 color;
            private readonly IBitmapDataInternal bitmapData;
            private readonly bool blend;
            private readonly IQuantizingSession quantizingSession;

            #endregion

            #region Constructors

            internal SolidFillSessionWithQuantizing(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, Rectangle bounds,
                DrawingOptions drawingOptions, IQuantizer quantizer, bool blend, Region? region)
                : base(context, drawingOptions, bounds, region)
            {
                this.blend = blend;
                color = owner.Color32;
                this.bitmapData = (bitmapData as IBitmapDataInternal) ?? new BitmapDataWrapper(bitmapData, true, true);
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                quantizingSession = quantizer.Initialize(bitmapData, context);
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert(scanline.RowIndex < bitmapData.Height);
                IBitmapDataRowInternal row = bitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                IQuantizingSession session = quantizingSession;

                if (!blend)
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
                int y = scanline.RowIndex - Bounds.Top;
                Debug.Assert(y < bitmapData.Height);
                IBitmapDataRowInternal row = bitmapData.GetRowCached(y);
                Color32 c = color;
                int left = scanline.Left;
                IQuantizingSession session = quantizingSession;

                // no blending: writing even transparent result pixels
                if (!blend)
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
            private readonly IBitmapDataInternal bitmapData;
            private readonly bool blend;
            private readonly IQuantizingSession quantizingSession;
            private readonly IDitheringSession? ditheringSession;

            #endregion

            #region Properties

            internal override bool IsSingleThreaded => ditheringSession?.IsSequential == true;

            #endregion

            #region Constructors

            internal SolidFillSessionWithDithering(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, Rectangle bounds,
                DrawingOptions drawingOptions, IQuantizer quantizer, IDitherer ditherer, bool blend, Region? region)
                : base(context, drawingOptions, bounds, region)
            {
                this.blend = blend;
                color = owner.Color32;
                this.bitmapData = (bitmapData as IBitmapDataInternal) ?? new BitmapDataWrapper(bitmapData, true, true);
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                quantizingSession = quantizer.Initialize(bitmapData, context);
                if (context.IsCancellationRequested)
                    return;

                context.Progress?.New(DrawingOperation.InitializingDitherer);
                ditheringSession = ditherer.Initialize(bitmapData, quantizingSession, context);
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline scanline)
            {
                Debug.Assert(scanline.RowIndex < bitmapData.Height);
                IDitheringSession? session = ditheringSession;
                if (session == null)
                {
                    Debug.Fail("Dithering session is not expected to be null here");
                    return;
                }

                IBitmapDataRowInternal row = bitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;

                if (!blend)
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
                Debug.Assert(scanline.RowIndex < bitmapData.Height);
                IDitheringSession? session = ditheringSession;
                if (session == null)
                {
                    Debug.Fail("Dithering session is not expected to be null here");
                    return;
                }

                IBitmapDataRowInternal row = bitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                var colorSpace = quantizingSession.WorkingColorSpace;

                // no blending: writing even transparent result pixels
                if (!blend)
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

        #region TwoPassSolitFillSession class

        private sealed class TwoPassSolidFillSession : FillPathSession
        {
            #region Fields

            private readonly IQuantizer quantizer;
            private readonly IDitherer? ditherer;
            private readonly IBitmapDataInternal firstSessionTarget;
            private readonly IBitmapDataInternal finalTarget;
            private readonly Rectangle bounds;
            private readonly WorkingColorSpace workingColorSpace;
            private readonly Color32 color;
            private readonly bool blend;
            private readonly bool isMaskGenerated;

            private Array2D<byte> mask;

            #endregion

            #region Constructors

            internal TwoPassSolidFillSession(SolidBrush owner, IAsyncContext context, IReadWriteBitmapData bitmapData, Rectangle bounds,
                DrawingOptions drawingOptions, IQuantizer quantizer, IDitherer? ditherer, bool blend, Region? region)
                : base(context, drawingOptions, bounds, region)
            {
                color = owner.Color32;
                finalTarget = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
                this.quantizer = quantizer;
                this.ditherer = ditherer;
                workingColorSpace = quantizer.WorkingColorSpace();

                // Note: not using GetPreferredFirstPassPixelFormat because the first step is not a cloning, and the small performance gain at PArgb blending
                //       is lost at FinalizeSession where the PColors are converted to Color32 due to the quantizing anyway
                firstSessionTarget = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bounds.Size, KnownPixelFormat.Format32bppArgb, workingColorSpace);
                isMaskGenerated = region?.IsAntiAliased == false;
                mask = isMaskGenerated ? region!.Mask : new Array2D<byte>(bounds.Height, KnownPixelFormat.Format1bppIndexed.GetByteWidth(bounds.Width));
                this.bounds = bounds;
                this.blend = blend;
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
                    int left = scanline.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                            continue;

                        targetRow.DoSetColor32(x + left, c);
                    }
                }

                void ProcessWithBlending(in RegionScanline scanline)
                {
                    Color32 c = color;
                    IBitmapDataRowInternal targetRow = firstSessionTarget.GetRowCached(scanline.RowIndex - bounds.Top);
                    WorkingColorSpace colorSpace = workingColorSpace;
                    IBitmapDataRowInternal sourceRow = finalTarget.GetRowCached(scanline.RowIndex);
                    int left = scanline.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnchecked(x >> 3), x) == 0)
                            continue;

                        int pos = x + left;
                        Color32 backColor = sourceRow.DoGetColor32(pos);
                        targetRow.DoSetColor32(pos, c.Blend(backColor, colorSpace));
                    }
                }

                #endregion

                // if mask is not generated, then we can be sure that scanline width is exactly the visible width
                Debug.Assert(isMaskGenerated || scanline.Scanline.Length == KnownPixelFormat.Format1bppIndexed.GetByteWidth(bounds.Width));
                if (!isMaskGenerated)
                    scanline.Scanline.CopyTo(mask[scanline.RowIndex - bounds.Top]);

                if (!blend || color.A == Byte.MaxValue)
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
                    int left = scanline.Left;
                    int maskOffset = left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        ColorExtensions.Set1bppColorIndex(ref maskRow.GetElementReferenceUnchecked((x + maskOffset) >> 3), x + maskOffset, value);

                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                targetRow.DoSetColor32(x + left, c);
                                continue;
                            default:
                                targetRow.DoSetColor32(x + left, Color32.FromArgb(c.A == Byte.MaxValue ? value : (byte)(value * c.A / Byte.MaxValue), c));
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
                    WorkingColorSpace colorSpace = workingColorSpace;
                    IBitmapDataRowInternal sourceRow = finalTarget.GetRowCached(scanline.RowIndex);
                    int left = scanline.Left;
                    int maskOffset = left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        ColorExtensions.Set1bppColorIndex(ref maskRow.GetElementReferenceUnchecked((x + maskOffset) >> 3), x + maskOffset, value);

                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                targetRow.DoSetColor32(x + left, c);
                                continue;
                            default:
                                int pos = x + left;
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
                    WorkingColorSpace colorSpace = workingColorSpace;
                    IBitmapDataRowInternal sourceRow = finalTarget.GetRowCached(scanline.RowIndex);
                    int left = scanline.Left;
                    int maskOffset = left - bounds.Left;

                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        byte value = scanline.Scanline.GetElementUnchecked(x);
                        ColorExtensions.Set1bppColorIndex(ref maskRow.GetElementReferenceUnchecked((x + maskOffset) >> 3), x + maskOffset, value);

                        switch (value)
                        {
                            case Byte.MinValue:
                                continue;
                            case Byte.MaxValue:
                                int pos = x + left;
                                Color32 backColor = sourceRow.DoGetColor32(pos);
                                targetRow.DoSetColor32(pos, c.Blend(backColor, colorSpace));
                                continue;
                            default:
                                pos = x + left;
                                backColor = sourceRow.DoGetColor32(pos);
                                targetRow.DoSetColor32(pos, Color32.FromArgb((byte)(value * c.A / Byte.MaxValue), c).Blend(backColor, colorSpace));
                                continue;
                        }
                    }
                }

                #endregion

                if (!blend)
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
                Point maskOffset = isMaskGenerated ? Bounds.Location - new Size(Region!.Bounds.Location) : Point.Empty;
                firstSessionTarget.DoCopyTo(Context, finalTarget, bounds.Location, quantizer, ditherer, blend, mask, maskOffset);
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

        #region Fields

        private Color64? colorSrgb;
        private ColorF? colorLinear;

        #endregion

        #region Properties

        private bool HasAlpha
        {
            get
            {
                if (colorSrgb.HasValue)
                    return colorSrgb.Value.A != UInt16.MaxValue;
                return colorLinear!.Value.A < 1f;
            }
        }

        private Color32 Color32 => (colorSrgb ??= colorLinear!.Value.ToColor64()).ToColor32();

        #endregion

        #region Constructors

        internal SolidBrush(Color32 color) => colorSrgb = color.ToColor64();
        internal SolidBrush(Color64 color) => colorSrgb = color;
        internal SolidBrush(ColorF color) => colorLinear = color;

        #endregion

        #region Methods

        private protected override FillPathSession CreateSession(IAsyncContext context, IReadWriteBitmapData bitmapData, Rectangle bounds, DrawingOptions drawingOptions, Region? region)
        {
            IQuantizer? quantizer = drawingOptions.Quantizer;
            IDitherer? ditherer = drawingOptions.Ditherer;
            bool antiAliasing = drawingOptions.AntiAliasing;
            bool blend = drawingOptions.AlphaBlending && (HasAlpha || antiAliasing);
            bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

            if (quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true)
                return new TwoPassSolidFillSession(this, context, bitmapData, bounds, drawingOptions, quantizer!, ditherer, blend, region);

            if (ditherer != null)
                return new SolidFillSessionWithDithering(this, context, bitmapData, bounds, drawingOptions, quantizer!, ditherer, blend, region);

            if (quantizer != null)
                return new SolidFillSessionWithQuantizing(this, context, bitmapData, bounds, drawingOptions, quantizer, blend, region);

            // TODO: && bitmapData.PixelFormat/WorkingColorSpace prefers Color32...
            return blend
                ? new SolidFillSessionWithBlending(this, context, bitmapData, bounds, drawingOptions, region)
                : new SolidFillSessionNoBlending(this, context, bitmapData, bounds, drawingOptions, region);
        }

        #endregion
    }
}