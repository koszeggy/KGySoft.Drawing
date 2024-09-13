#region Copyright

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

            internal SolidFillSessionNoBlending(SolidBrush owner, IWritableBitmapData bitmapData)
            {
                color = owner.Color32;
                this.bitmapData = (bitmapData as IBitmapDataInternal) ?? new BitmapDataWrapper(bitmapData, false, true);
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline<byte> scanline)
            {
                Debug.Assert(scanline.RowIndex < bitmapData.Height);
                IBitmapDataRowInternal row = bitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnsafe(x >> 3), x) == 1)
                        row.DoSetColor32(x + left, c);
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline<float> scanline)
            {
                Debug.Assert(scanline.RowIndex < bitmapData.Height);
                IBitmapDataRowInternal row = bitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    float value = scanline.Scanline.GetElementUnsafe(x);
                    switch (value)
                    {
                        case <= 0f:
                            continue;
                        case >= 1f:
                            row.DoSetColor32(x + left, c);
                            continue;
                        default:
                            row.DoSetColor32(x + left, Color32.FromArgb(ColorSpaceHelper.ToByte(c.A == Byte.MaxValue ? value : value * ColorSpaceHelper.ToFloat(c.A)), c));
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

            internal SolidFillSessionWithBlending(SolidBrush owner, IReadWriteBitmapData bitmapData)
            {
                color = owner.Color32;
                this.bitmapData = (bitmapData as IBitmapDataInternal) ?? new BitmapDataWrapper(bitmapData, true, true);
                workingColorSpace = bitmapData.GetPreferredColorSpace();
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline<byte> scanline)
            {
                Debug.Assert(scanline.RowIndex < bitmapData.Height);
                Debug.Assert(color.A < Byte.MaxValue);
                IBitmapDataRowInternal row = bitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                var colorSpace = workingColorSpace;

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnsafe(x >> 3), x) == 1)
                    {
                        int pos = x + left;
                        Color32 backColor = row.DoGetColor32(pos);
                        row.DoSetColor32(pos, c.Blend(backColor, colorSpace));
                    }
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline<float> scanline)
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
                        float value = scanline.Scanline.GetElementUnsafe(x);
                        switch (value)
                        {
                            case <= 0f:
                                continue;
                            case >= 1f:
                                row.DoSetColor32(x + left, c);
                                continue;
                            default:
                                int pos = x + left;
                                Color32 backColor = row.DoGetColor32(pos);
                                row.DoSetColor32(pos, Color32.FromArgb(ColorSpaceHelper.ToByte(value), c).Blend(backColor, colorSpace));
                                continue;
                        }
                    }

                    return;
                }

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    float value = scanline.Scanline.GetElementUnsafe(x);
                    switch (value)
                    {
                        case <= 0f:
                            continue;
                        case >= 1f:
                            int pos = x + left;
                            Color32 backColor = row.DoGetColor32(pos);
                            row.DoSetColor32(pos, c.Blend(backColor, colorSpace));
                            continue;
                        default:
                            pos = x + left;
                            backColor = row.DoGetColor32(pos);
                            row.DoSetColor32(pos, Color32.FromArgb(ColorSpaceHelper.ToByte(value * ColorSpaceHelper.ToFloat(c.A)), c).Blend(backColor, colorSpace));
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

            internal SolidFillSessionWithQuantizing(IAsyncContext context, SolidBrush owner, IReadWriteBitmapData bitmapData, IQuantizer quantizer, bool blend)
            {
                this.blend = blend;
                color = owner.Color32;
                this.bitmapData = (bitmapData as IBitmapDataInternal) ?? new BitmapDataWrapper(bitmapData, true, true);
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                quantizingSession = quantizer.Initialize(bitmapData, context);
            }

            #endregion

            #region Methods

            internal override void ApplyScanlineSolid(in RegionScanline<byte> scanline)
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
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnsafe(x >> 3), x) == 1)
                            row.DoSetColor32(x + left, session.GetQuantizedColor(c));
                    }
                }

                var colorSpace = session.WorkingColorSpace;
                byte alphaThreshold = session.AlphaThreshold;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnsafe(x >> 3), x) == 1)
                    {
                        int pos = x + left;
                        Color32 quantizedColor = session.GetQuantizedColor(c.Blend(row.DoGetColor32(pos), colorSpace));
                        if (quantizedColor.A >= alphaThreshold)
                            row.DoSetColor32(pos, quantizedColor);
                    }
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline<float> scanline)
            {
                Debug.Assert(scanline.RowIndex < bitmapData.Height);
                IBitmapDataRowInternal row = bitmapData.GetRowCached(scanline.RowIndex);
                Color32 c = color;
                int left = scanline.Left;
                IQuantizingSession session = quantizingSession;

                // no blending: writing even transparent result pixels
                if (!blend)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        float value = scanline.Scanline.GetElementUnsafe(x);
                        switch (value)
                        {
                            case <= 0f:
                                continue;
                            case >= 1f:
                                row.DoSetColor32(x + left, session.GetQuantizedColor(c));
                                continue;
                            default:
                                row.DoSetColor32(x + left, session.GetQuantizedColor(Color32.FromArgb(ColorSpaceHelper.ToByte(c.A == Byte.MaxValue ? value : value * ColorSpaceHelper.ToFloat(c.A)), c)));
                                continue;
                        }
                    }
                }

                // blending: skipping too transparent pixels
                var colorSpace = session.WorkingColorSpace;
                byte alphaThreshold = session.AlphaThreshold;
                if (c.A == Byte.MaxValue)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        float value = scanline.Scanline.GetElementUnsafe(x);
                        switch (value)
                        {
                            case <= 0f:
                                continue;
                            case >= 1f:
                                Color32 quantizedColor = session.GetQuantizedColor(c);
                                if (quantizedColor.A >= alphaThreshold)
                                    row.DoSetColor32(x + left, quantizedColor);
                                continue;
                            default:
                                int pos = x + left;
                                quantizedColor = session.GetQuantizedColor(Color32.FromArgb(ColorSpaceHelper.ToByte(value), c).Blend(row.DoGetColor32(pos), colorSpace));
                                if (quantizedColor.A >= alphaThreshold)
                                    row.DoSetColor32(pos, quantizedColor);
                                continue;
                        }
                    }

                    return;
                }

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    float value = scanline.Scanline.GetElementUnsafe(x);
                    switch (value)
                    {
                        case <= 0f:
                            continue;
                        case >= 1f:
                            int pos = x + left;
                            Color32 quantizedColor = session.GetQuantizedColor(c.Blend(row.DoGetColor32(pos), colorSpace));
                            if (quantizedColor.A >= alphaThreshold)
                                row.DoSetColor32(pos, quantizedColor);
                            continue;
                        default:
                            pos = x + left;
                            quantizedColor = session.GetQuantizedColor(Color32.FromArgb(ColorSpaceHelper.ToByte(value * ColorSpaceHelper.ToFloat(c.A)), c).Blend(row.DoGetColor32(pos), colorSpace));
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

            internal SolidFillSessionWithDithering(IAsyncContext context, SolidBrush owner, IReadWriteBitmapData bitmapData, IQuantizer quantizer, IDitherer ditherer, bool blend)
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

            internal override void ApplyScanlineSolid(in RegionScanline<byte> scanline)
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
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnsafe(x >> 3), x) == 1)
                            row.DoSetColor32(x + left, session.GetDitheredColor(c, x, scanline.RowIndex));
                    }
                }

                var colorSpace = quantizingSession.WorkingColorSpace;
                byte alphaThreshold = quantizingSession.AlphaThreshold;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnsafe(x >> 3), x) == 1)
                    {
                        int pos = x + left;
                        Color32 ditheredColor = session.GetDitheredColor(c.Blend(row.DoGetColor32(pos), colorSpace), x, scanline.RowIndex);
                        if (ditheredColor.A >= alphaThreshold)
                            row.DoSetColor32(pos, ditheredColor);
                    }
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline<float> scanline)
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
                        float value = scanline.Scanline.GetElementUnsafe(x);
                        switch (value)
                        {
                            case <= 0f:
                                continue;
                            case >= 1f:
                                row.DoSetColor32(x + left, session.GetDitheredColor(c, x, scanline.RowIndex));
                                continue;
                            default:
                                row.DoSetColor32(x + left, session.GetDitheredColor(Color32.FromArgb(ColorSpaceHelper.ToByte(c.A == Byte.MaxValue ? value : value * ColorSpaceHelper.ToFloat(c.A)), c), x, scanline.RowIndex));
                                continue;
                        }
                    }
                }

                // blending: skipping too transparent pixels
                byte alphaThreshold = quantizingSession.AlphaThreshold;
                if (c.A == Byte.MaxValue)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        float value = scanline.Scanline.GetElementUnsafe(x);
                        switch (value)
                        {
                            case <= 0f:
                                continue;
                            case >= 1f:
                                Color32 ditheredColor = session.GetDitheredColor(c, x, scanline.RowIndex);
                                if (ditheredColor.A >= alphaThreshold)
                                    row.DoSetColor32(x + left, ditheredColor);
                                continue;
                            default:
                                int pos = x + left;
                                ditheredColor = session.GetDitheredColor(Color32.FromArgb(ColorSpaceHelper.ToByte(value), c).Blend(row.DoGetColor32(pos), colorSpace), x, scanline.RowIndex);
                                if (ditheredColor.A >= alphaThreshold)
                                    row.DoSetColor32(x + left, ditheredColor);
                                continue;
                        }
                    }

                    return;
                }

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    float value = scanline.Scanline.GetElementUnsafe(x);
                    switch (value)
                    {
                        case <= 0f:
                            continue;
                        case >= 1f:
                            int pos = x + left;
                            Color32 ditheredColor = session.GetDitheredColor(c.Blend(row.DoGetColor32(pos), colorSpace), x, scanline.RowIndex);
                            if (ditheredColor.A >= alphaThreshold)
                                row.DoSetColor32(pos, ditheredColor);
                            continue;
                        default:
                            pos = x + left;
                            ditheredColor = session.GetDitheredColor(Color32.FromArgb(ColorSpaceHelper.ToByte(value * ColorSpaceHelper.ToFloat(c.A)), c).Blend(row.DoGetColor32(pos), colorSpace), x, scanline.RowIndex);
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

            private readonly IAsyncContext context;
            private readonly IQuantizer quantizer;
            private readonly IDitherer? ditherer;
            private readonly IBitmapDataInternal? firstSessionTarget;
            private readonly IBitmapDataInternal finalTarget;
            private readonly Rectangle bounds;
            private readonly Color32 color;
            private readonly WorkingColorSpace workingColorSpace;
            private readonly bool blend;

            private Array2D<byte> mask;

            #endregion

            #region Constructors

            internal TwoPassSolidFillSession(IAsyncContext context, SolidBrush owner, IReadWriteBitmapData bitmapData, Rectangle bounds, IQuantizer quantizer, IDitherer? ditherer, bool blend)
            {
                this.context = context;
                color = owner.Color32;
                finalTarget = bitmapData as IBitmapDataInternal ?? new BitmapDataWrapper(bitmapData, true, true);
                this.quantizer = quantizer;
                this.ditherer = ditherer;
                firstSessionTarget = (IBitmapDataInternal)BitmapDataFactory.CreateBitmapData(bounds.Size/*, TODO bitmapData.GetPreferredFirstPassPixelFormat(workingColorSpace)*/);
                if (firstSessionTarget == null)
                    return;

                mask = new Array2D<byte>(bounds.Height, KnownPixelFormat.Format1bppIndexed.GetByteWidth(bounds.Width));
                workingColorSpace = quantizer.WorkingColorSpace();
                this.bounds = bounds;
                this.blend = blend;
            }

            #endregion

            #region Methods

            #region Internal Methods
            
            internal override void ApplyScanlineSolid(in RegionScanline<byte> scanline)
            {
                Color32 c = color;
                int y = scanline.RowIndex - bounds.Top;
                IBitmapDataRowInternal targetRow = firstSessionTarget!.GetRowCached(y);
                scanline.Scanline.CopyTo(mask[y]);

                if (!blend || c.A == Byte.MaxValue)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnsafe(x >> 3), x) == 0)
                            continue;

                        targetRow.DoSetColor32(x, c);
                    }

                    return;
                }

                WorkingColorSpace colorSpace = workingColorSpace;
                IBitmapDataRowInternal sourceRow = finalTarget.GetRowCached(scanline.RowIndex);
                int left = scanline.Left;
                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(scanline.Scanline.GetElementUnsafe(x >> 3), x) == 0)
                        continue;

                    Color32 backColor = sourceRow.DoGetColor32(x + left);
                    targetRow.DoSetColor32(x, c.Blend(backColor, colorSpace));
                }
            }

            internal override void ApplyScanlineAntiAliasing(in RegionScanline<float> scanline)
            {
                Color32 c = color;
                int y = scanline.RowIndex - bounds.Top;
                IBitmapDataRowInternal targetRow = firstSessionTarget!.GetRowCached(y);
                ArraySection<byte> maskRow = mask[y];

                if (!blend)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        float value = scanline.Scanline.GetElementUnsafe(x);
                        ColorExtensions.Set1bppColorIndex(ref maskRow.GetElementReferenceUnchecked(x >> 3), x, value <= 0f ? 0 : 1);

                        switch (value)
                        {
                            case <= 0f:
                                continue;
                            case >= 1f:
                                targetRow.DoSetColor32(x, c);
                                continue;
                            default:
                                targetRow.DoSetColor32(x, Color32.FromArgb(ColorSpaceHelper.ToByte(c.A == Byte.MaxValue ? value : value * ColorSpaceHelper.ToFloat(c.A)), c));
                                continue;
                        }
                    }

                    return;
                }

                WorkingColorSpace colorSpace = workingColorSpace;
                IBitmapDataRowInternal sourceRow = finalTarget.GetRowCached(scanline.RowIndex);
                int left = scanline.Left;

                if (c.A == Byte.MaxValue)
                {
                    for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                    {
                        float value = scanline.Scanline.GetElementUnsafe(x);
                        ColorExtensions.Set1bppColorIndex(ref maskRow.GetElementReferenceUnchecked(x >> 3), x, value <= 0f ? 0 : 1);

                        switch (value)
                        {
                            case <= 0f:
                                continue;
                            case >= 1f:
                                targetRow.DoSetColor32(x, c);
                                continue;
                            default:
                                Color32 backColor = sourceRow.DoGetColor32(x + left);
                                targetRow.DoSetColor32(x, Color32.FromArgb(ColorSpaceHelper.ToByte(value), c).Blend(backColor, colorSpace));
                                continue;
                        }
                    }

                    return;
                }

                for (int x = scanline.MinIndex; x <= scanline.MaxIndex; x++)
                {
                    float value = scanline.Scanline.GetElementUnsafe(x);
                    ColorExtensions.Set1bppColorIndex(ref maskRow.GetElementReferenceUnchecked(x >> 3), x, value <= 0f ? 0 : 1);

                    switch (value)
                    {
                        case <= 0f:
                            continue;
                        case >= 1f:
                            Color32 backColor = sourceRow.DoGetColor32(x + left);
                            targetRow.DoSetColor32(x, c.Blend(backColor, colorSpace));
                            continue;
                        default:
                            backColor = sourceRow.DoGetColor32(x + left);
                            targetRow.DoSetColor32(x, Color32.FromArgb(ColorSpaceHelper.ToByte(value * ColorSpaceHelper.ToFloat(c.A)), c).Blend(backColor, colorSpace));
                            continue;
                    }
                }
            }

            internal override void FinalizeSession() => firstSessionTarget?.DoCopyTo(context, finalTarget, bounds.Location, quantizer, ditherer, blend, mask);

            #endregion

            #region Protected Methods

            protected override void Dispose(bool disposing)
            {
                mask.Dispose();
                firstSessionTarget?.Dispose();
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

        private protected override FillPathSession CreateSession(IAsyncContext context, IReadWriteBitmapData bitmapData, Rectangle bounds, DrawingOptions drawingOptions)
        {
            IQuantizer? quantizer = drawingOptions.Quantizer;
            IDitherer? ditherer = drawingOptions.Ditherer;
            bool blend = drawingOptions.AlphaBlending && (HasAlpha || drawingOptions.AntiAliasing);
            bitmapData.AdjustQuantizerAndDitherer(ref quantizer, ref ditherer);

            if(quantizer?.InitializeReliesOnContent == true || ditherer?.InitializeReliesOnContent == true)
                return new TwoPassSolidFillSession(context, this, bitmapData, bounds, quantizer!, ditherer, blend);

            if (ditherer != null)
                return new SolidFillSessionWithDithering(context, this, bitmapData, quantizer!, ditherer, blend);

            if (quantizer != null)
                return new SolidFillSessionWithQuantizing(context, this, bitmapData, quantizer, blend);

            // TODO: && bitmapData.PixelFormat/WorkingColorSpace prefers Color32...
            return blend
                ? new SolidFillSessionWithBlending(this, bitmapData)
                : new SolidFillSessionNoBlending(this, bitmapData);
        }

        #endregion
    }
}