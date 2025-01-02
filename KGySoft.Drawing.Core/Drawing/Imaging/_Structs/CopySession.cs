#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CopySession.cs
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
using System.Security;

using KGySoft.Collections;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal struct CopySession
    {
        #region Constants

        private const int parallelThreshold = 100;
        private const int quantizingScale = 1;
        private const int ditheringScale = 2;

        #endregion

        #region Fields

        #region Internal Fields
        
        [AllowNull]internal IBitmapDataInternal Source = null!;
        [AllowNull]internal IBitmapDataInternal Target = null!;
        internal Rectangle SourceRectangle;
        internal Rectangle TargetRectangle;

        #endregion

        #region Private Fields

        private readonly IAsyncContext context;

        #endregion

        #endregion

        #region Constructors

        internal CopySession(IAsyncContext context) : this()
        {
            this.context = context;
        }

        internal CopySession(IAsyncContext context, IBitmapDataInternal sessionSource, IBitmapDataInternal sessionTarget, Rectangle actualSourceRectangle, Rectangle actualTargetRectangle)
        {
            this.context = context;
            Source = sessionSource;
            Target = sessionTarget;
            SourceRectangle = actualSourceRectangle;
            TargetRectangle = actualTargetRectangle;
        }

        #endregion

        #region Methods

        #region Internal Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal void PerformCopy()
        {
            if (TryPerformRawCopy())
                return;

            PixelFormatInfo pixelFormat = Target.PixelFormat;

            // For linear gamma assuming the best performance with [P]ColorF even if the preferred color type is smaller.
            if (pixelFormat.Prefers128BitColors || pixelFormat.LinearGamma)
            {
                // Using PColorF only if the actual pixel format really has linear gamma to prevent performance issues
                if (pixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true })
                    PerformCopyPColorF();
                else
                    PerformCopyColorF();
            }
            else if (pixelFormat.Prefers64BitColors)
            {
                if (pixelFormat.HasPremultipliedAlpha)
                    PerformCopyPColor64();
                else
                    PerformCopyColor64();
            }
            else if (pixelFormat.HasPremultipliedAlpha)
                PerformCopyPColor32();
            else
                PerformCopyColor32();
        }

        internal void PerformCopyWithQuantizer(IQuantizingSession quantizingSession)
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold >> quantizingScale)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < SourceRectangle.Width; x++)
                        rowDst.DoSetColor32(x + TargetRectangle.X, quantizingSession.GetQuantizedColor(rowSrc.DoGetColor32(x + SourceRectangle.X)));

                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IQuantizingSession session = quantizingSession;
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetColor32(x + offsetDst, session.GetQuantizedColor(rowSrc.DoGetColor32(x + offsetSrc)));
            });
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal void PerformCopyWithQuantizer(IQuantizingSession quantizingSession, bool skipTransparent, in Array2D<byte> mask, Point maskOffset)
        {
            if (!mask.IsNull)
            {
                PerformCopyWithQuantizerMasked(quantizingSession, skipTransparent, mask, maskOffset);
                return;
            }

            if (skipTransparent)
            {
                PerformCopyWithQuantizerSkipTransparent(quantizingSession);
                return;
            }

            PerformCopyWithQuantizer(quantizingSession);
        }

        internal void PerformCopyWithDithering(IQuantizingSession quantizingSession, IDitheringSession ditheringSession)
        {
            // Sequential processing
            if (ditheringSession.IsSequential || SourceRectangle.Width < parallelThreshold >> ditheringScale)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;

                    // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                    for (int x = 0; x < SourceRectangle.Width; x++)
                        rowDst.DoSetColor32(x + TargetRectangle.X, ditheringSession.GetDitheredColor(rowSrc.DoGetColor32(x + SourceRectangle.X), x, y));

                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IDitheringSession session = ditheringSession;
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;

                // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                for (int x = 0; x < width; x++)
                    rowDst.DoSetColor32(x + offsetDst, session.GetDitheredColor(rowSrc.DoGetColor32(x + offsetSrc), x, y));
            });
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal void PerformCopyWithDithering(IQuantizingSession quantizingSession, IDitheringSession ditheringSession, bool skipTransparent, in Array2D<byte> mask, Point maskOffset)
        {
            if (!mask.IsNull)
            {
                PerformCopyWithDitheringMasked(quantizingSession, ditheringSession, skipTransparent, mask, maskOffset);
                return;
            }

            if (skipTransparent)
            {
                PerformCopyWithDitheringSkipTransparent(quantizingSession, ditheringSession);
                return;
            }

            PerformCopyWithDithering(quantizingSession, ditheringSession);
        }

        internal void PerformDraw(IQuantizer? quantizer, IDitherer? ditherer)
        {
            if (quantizer == null)
            {
                PerformDrawDirect();
                return;
            }

            IReadableBitmapData initSource = SourceRectangle.Size == Source.Size
                ? Source
                : Source.Clip(SourceRectangle);

            try
            {
                Debug.Assert(!quantizer.InitializeReliesOnContent || !Source.HasMultiLevelAlpha(), "This draw performs blending on-the-fly but the used quantizer would require two-pass processing");
                context.Progress?.New(DrawingOperation.InitializingQuantizer);
                using (IQuantizingSession quantizingSession = quantizer.Initialize(initSource, context))
                {
                    if (context.IsCancellationRequested)
                        return;
                    if (quantizingSession == null)
                        throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);

                    // quantizing without dithering
                    if (ditherer == null)
                    {
                        PerformDrawWithQuantizer(quantizingSession);
                        return;
                    }

                    // quantizing with dithering
                    Debug.Assert(!ditherer.InitializeReliesOnContent || !Source.HasMultiLevelAlpha(), "This draw performs blending on-the-fly but the used ditherer would require two-pass processing");

                    context.Progress?.New(DrawingOperation.InitializingDitherer);
                    using IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession, context);
                    if (context.IsCancellationRequested)
                        return;
                    if (ditheringSession == null)
                        throw new InvalidOperationException(Res.ImagingDithererInitializeNull);

                    PerformDrawWithDithering(quantizingSession, ditheringSession);
                }
            }
            finally
            {
                if (!ReferenceEquals(initSource, Source))
                    initSource.Dispose();
            }
        }

        /// <summary>
        /// Drawing without a quantizer in 32bpp color space.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity",
            Justification = "False alarm, the new analyzer includes the complexity of local methods")]
        internal void PerformDrawDirect()
        {
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            bool linearBlending = target.LinearBlending();

            // Taking always the target pixel format for the best quality/performance.
            // For targets with linear gamma (when linear blending is used) assuming
            // the best performance with [P]ColorF even if the preferred color type is smaller.
            PixelFormatInfo targetPixelFormat = target.PixelFormat;
            Action<int> processRow = targetPixelFormat.Prefers128BitColors || linearBlending && targetPixelFormat.LinearGamma
                ? linearBlending && targetPixelFormat is { HasPremultipliedAlpha: true, LinearGamma: true }
                    ? ProcessRowPColorF
                    : ProcessRowColorF
                : targetPixelFormat.Prefers64BitColors
                    ? !linearBlending && targetPixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                        ? ProcessRowPColor64
                        : ProcessRowColor64
                    : !linearBlending && targetPixelFormat is { HasPremultipliedAlpha: true, LinearGamma: false }
                        ? ProcessRowPColor32
                        : ProcessRowColor32;
            
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    processRow.Invoke(y);
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, processRow);

            #region Local Methods

            void ProcessRowColor32(int y)
            {
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                byte alphaThreshold = target.PixelFormat.HasMultiLevelAlpha ? (byte)0 : target.AlphaThreshold;
                bool linear = linearBlending;
                int width = sourceWidth;

                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(x + offsetDst, colorSrc);
                        continue;
                    }

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // source here has a partial transparency: we need to read the target color
                    int pos = x + offsetDst;
                    Color32 colorDst = rowDst.DoGetColor32(pos);

                    // non-transparent target: blending
                    if (colorDst.A != 0)
                    {
                        colorSrc = colorDst.A == Byte.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackground(colorDst, linear)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst, linear);
                    }

                    // overwriting target color only if blended color has high enough alpha
                    if (colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(pos, colorSrc);
                }
            }

            void ProcessRowPColor32(int y)
            {
                Debug.Assert(!linearBlending && !target.PixelFormat.LinearGamma);

                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;

                for (int x = 0; x < width; x++)
                {
                    PColor32 colorSrc = rowSrc.DoGetPColor32(x + offsetSrc);

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetPColor32(x + offsetDst, colorSrc);
                        continue;
                    }

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // source here has a partial transparency: we need to read the target color
                    int pos = x + offsetDst;
                    PColor32 colorDst = rowDst.DoGetPColor32(pos);

                    // non-transparent target: blending
                    if (colorDst.A != 0)
                        colorSrc = colorSrc.BlendWithSrgb(colorDst);

                    rowDst.DoSetPColor32(pos, colorSrc);
                }
            }

            void ProcessRowColor64(int y)
            {
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                ushort alphaThreshold = ColorSpaceHelper.ToUInt16(target.PixelFormat.HasMultiLevelAlpha ? (byte)0 : target.AlphaThreshold);
                bool linear = linearBlending;
                int width = sourceWidth;

                for (int x = 0; x < width; x++)
                {
                    Color64 colorSrc = rowSrc.DoGetColor64(x + offsetSrc);

                    // fully solid source: overwrite
                    if (colorSrc.A == UInt16.MaxValue)
                    {
                        rowDst.DoSetColor64(x + offsetDst, colorSrc);
                        continue;
                    }

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // source here has a partial transparency: we need to read the target color
                    int pos = x + offsetDst;
                    Color64 colorDst = rowDst.DoGetColor64(pos);

                    // non-transparent target: blending
                    if (colorDst.A != 0)
                    {
                        colorSrc = colorDst.A == UInt16.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackground(colorDst, linear)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst, linear);
                    }

                    // overwriting target color only if blended color has high enough alpha
                    if (colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor64(pos, colorSrc);
                }
            }

            void ProcessRowPColor64(int y)
            {
                Debug.Assert(!linearBlending && !target.PixelFormat.LinearGamma);
       
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;

                for (int x = 0; x < width; x++)
                {
                    PColor64 colorSrc = rowSrc.DoGetPColor64(x + offsetSrc);

                    // fully solid source: overwrite
                    if (colorSrc.A == UInt16.MaxValue)
                    {
                        rowDst.DoSetPColor64(x + offsetDst, colorSrc);
                        continue;
                    }

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // source here has a partial transparency: we need to read the target color
                    int pos = x + offsetDst;
                    PColor64 colorDst = rowDst.DoGetPColor64(pos);

                    // non-transparent target: blending
                    if (colorDst.A != 0)
                        colorSrc = colorSrc.BlendWithSrgb(colorDst);

                    rowDst.DoSetPColor64(pos, colorSrc);
                }
            }

            void ProcessRowColorF(int y)
            {
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                float alphaThreshold = ColorSpaceHelper.ToFloat(target.PixelFormat.HasMultiLevelAlpha ? (byte)0 : target.AlphaThreshold);
                bool linear = linearBlending;
                int width = sourceWidth;

                for (int x = 0; x < width; x++)
                {
                    ColorF colorSrc = rowSrc.DoGetColorF(x + offsetSrc);

                    // fully solid source: overwrite
                    if (colorSrc.A >= 1f)
                    {
                        rowDst.DoSetColorF(x + offsetDst, colorSrc);
                        continue;
                    }

                    // fully transparent source: skip
                    if (colorSrc.A <= 0f)
                        continue;

                    // source here has a partial transparency: we need to read the target color
                    int pos = x + offsetDst;
                    ColorF colorDst = rowDst.DoGetColorF(pos);

                    // non-transparent target: blending
                    if (colorDst.A > 0f)
                    {
                        colorSrc = colorDst.A >= 1f
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackground(colorDst, linear)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst, linear);
                    }

                    // overwriting target color only if blended color has high enough alpha
                    if (colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColorF(pos, colorSrc);
                }
            }

            void ProcessRowPColorF(int y)
            {
                Debug.Assert(linearBlending && target.PixelFormat.LinearGamma);

                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;

                for (int x = 0; x < width; x++)
                {
                    PColorF colorSrc = rowSrc.DoGetPColorF(x + offsetSrc);

                    // fully solid source: overwrite
                    if (colorSrc.A >= 1f)
                    {
                        rowDst.DoSetPColorF(x + offsetDst, colorSrc);
                        continue;
                    }

                    // fully transparent source: skip
                    if (colorSrc.A <= 0f)
                        continue;

                    // source here has a partial transparency: we need to read the target color
                    int pos = x + offsetDst;
                    PColorF colorDst = rowDst.DoGetPColorF(pos);

                    // non-transparent target: blending
                    if (colorDst.A != 0)
                        colorSrc = colorSrc.BlendWithLinear(colorDst);

                    rowDst.DoSetPColorF(pos, colorSrc);
                }
            }

            #endregion
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Tries to perform a raw copy. If succeeds converts the horizontal dimensions to bytes from pixels.
        /// Note: Stride and origin is set from outside so we spare some casts and possible GCHandle uses.
        /// </summary>
        private bool TryPerformRawCopy()
        {
            // same non-custom pixel format is required
            if (Source.PixelFormat != Target.PixelFormat || Source.IsCustomPixelFormat || Target.IsCustomPixelFormat
                || Source is not (UnmanagedBitmapDataBase or ManagedBitmapDataBase)
                || Target is not (UnmanagedBitmapDataBase or ManagedBitmapDataBase))
            {
                return false;
            }

            int bpp = Source.PixelFormat.BitsPerPixel;

            if (bpp > 8)
            {
                int byteSize = bpp >> 3;
                SourceRectangle.X *= byteSize;
                TargetRectangle.X *= byteSize;
                SourceRectangle.Width = TargetRectangle.Width *= byteSize;
                PerformCopyRaw();
                return true;
            }

            // for indexed images we need some further checks
            // palette must be the same
            if (Source.Palette?.Equals(Target.Palette) != true)
                return false;

            if (bpp == 8)
            {
                PerformCopyRaw();
                return true;
            }

            // 1/4bpp: area to copy must be aligned correctly
            // Note: Actually we could raw-copy the middle part if a large enough area has the same source/target alignment and then copy the first/last columns by pixels...
            int alignmentMask = bpp == 1 ? 7 : 1;

            // left edge: must be aligned to byte boundary
            if (!((SourceRectangle.X & alignmentMask) == 0 && (TargetRectangle.X & alignmentMask) == 0
                // right edge: either byte boundary...
                && ((SourceRectangle.Width & alignmentMask) == 0
                    // ...or copying to the last column so the rest of the bits are padding
                    || SourceRectangle.Right == Source.Width && TargetRectangle.Right == Target.Width)))
            {
                return false;
            }

            // converting horizontal dimension to byte sized
            int shift = bpp == 1 ? 3 : 1;
            SourceRectangle.X >>= shift;
            TargetRectangle.X >>= shift;
            int width = SourceRectangle.Width >> shift;

            // right edge can be the part of line padding
            if ((SourceRectangle.Width & alignmentMask) != 0)
                width++;

            SourceRectangle.Width = TargetRectangle.Width = width;
            PerformCopyRaw();
            return true;
        }

        [SecuritySafeCritical]
        private unsafe void PerformCopyRaw()
        {
            Debug.Assert(Source is UnmanagedBitmapDataBase or ManagedBitmapDataBase);
            Debug.Assert(Target is UnmanagedBitmapDataBase or ManagedBitmapDataBase);

            if (Source is UnmanagedBitmapDataBase unmanagedSrc)
            {
                if (Target is UnmanagedBitmapDataBase nativeDst)
                {
                    DoCopyRaw(unmanagedSrc.Stride, nativeDst.Stride, (byte*)unmanagedSrc.Scan0, (byte*)nativeDst.Scan0);
                    return;
                }

                var managedDst = (ManagedBitmapDataBase)Target;
                fixed (byte* pDst = &managedDst.GetPinnableReference())
                    DoCopyRaw(unmanagedSrc.Stride, managedDst.RowSize, (byte*)unmanagedSrc.Scan0, pDst);
            }
            else
            {
                var managedSrc = (ManagedBitmapDataBase)Source;
                if (Target is UnmanagedBitmapDataBase unmanagedDst)
                {
                    fixed (byte* pSrc = &managedSrc.GetPinnableReference())
                        DoCopyRaw(managedSrc.RowSize, unmanagedDst.Stride, pSrc, (byte*)unmanagedDst.Scan0);
                    return;
                }

                var managedDst = (ManagedBitmapDataBase)Target;
                fixed (byte* pSrc = &managedSrc.GetPinnableReference())
                fixed (byte* pDst = &managedDst.GetPinnableReference())
                    DoCopyRaw(managedSrc.RowSize, managedDst.RowSize, pSrc, pDst);
            }
        }

        [SecurityCritical]
        private unsafe void DoCopyRaw(int sourceStride, int targetStride, byte* sourceOrigin, byte* targetOrigin)
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                byte* pSrc = sourceOrigin + SourceRectangle.Y * sourceStride + SourceRectangle.X;
                byte* pDst = targetOrigin + TargetRectangle.Y * targetStride + TargetRectangle.X;
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    MemoryHelper.CopyMemory(pSrc, pDst, SourceRectangle.Width);
                    pSrc += sourceStride;
                    pDst += targetStride;
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int width = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                byte* pSrc = sourceOrigin + (y + sourceLocation.Y) * sourceStride + sourceLocation.X;
                byte* pDst = targetOrigin + (y + targetLocation.Y) * targetStride + targetLocation.X;
                MemoryHelper.CopyMemory(pSrc, pDst, width);
            });
        }

        private void PerformCopyColor32()
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < SourceRectangle.Width; x++)
                        rowDst.DoSetColor32(x + TargetRectangle.X, rowSrc.DoGetColor32(x + SourceRectangle.X));
                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetColor32(x + offsetDst, rowSrc.DoGetColor32(x + offsetSrc));
            });
        }

        private void PerformCopyPColor32()
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < SourceRectangle.Width; x++)
                        rowDst.DoSetPColor32(x + TargetRectangle.X, rowSrc.DoGetPColor32(x + SourceRectangle.X));
                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetPColor32(x + offsetDst, rowSrc.DoGetPColor32(x + offsetSrc));
            });
        }

        private void PerformCopyColor64()
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < SourceRectangle.Width; x++)
                        rowDst.DoSetColor64(x + TargetRectangle.X, rowSrc.DoGetColor64(x + SourceRectangle.X));
                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetColor64(x + offsetDst, rowSrc.DoGetColor64(x + offsetSrc));
            });
        }

        private void PerformCopyPColor64()
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < SourceRectangle.Width; x++)
                        rowDst.DoSetPColor64(x + TargetRectangle.X, rowSrc.DoGetPColor64(x + SourceRectangle.X));
                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetPColor64(x + offsetDst, rowSrc.DoGetPColor64(x + offsetSrc));
            });
        }

        private void PerformCopyColorF()
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < SourceRectangle.Width; x++)
                        rowDst.DoSetColorF(x + TargetRectangle.X, rowSrc.DoGetColorF(x + SourceRectangle.X));
                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetColorF(x + offsetDst, rowSrc.DoGetColorF(x + offsetSrc));
            });
        }

        private void PerformCopyPColorF()
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < SourceRectangle.Width; x++)
                        rowDst.DoSetPColorF(x + TargetRectangle.X, rowSrc.DoGetPColorF(x + SourceRectangle.X));
                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetPColorF(x + offsetDst, rowSrc.DoGetPColorF(x + offsetSrc));
            });
        }

        private void PerformCopyWithQuantizerSkipTransparent(IQuantizingSession quantizingSession)
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold >> quantizingScale)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                byte alphaThreshold = Math.Max(quantizingSession.AlphaThreshold, (byte)1);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < SourceRectangle.Width; x++)
                    {
                        Color32 colorSrc = rowSrc.DoGetColor32(x + SourceRectangle.X);
                        if (colorSrc.A < alphaThreshold)
                            continue;

                        rowDst.DoSetColor32(x + TargetRectangle.X, quantizingSession.GetQuantizedColor(colorSrc));
                    }

                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IQuantizingSession session = quantizingSession;
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                byte alphaThreshold = Math.Max(session.AlphaThreshold, (byte)1);
                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);
                    if (colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(x + offsetDst, session.GetQuantizedColor(colorSrc));
                }
            });
        }

        private void PerformCopyWithQuantizerMasked(IQuantizingSession quantizingSession, bool skipTransparent, in Array2D<byte> mask, Point maskOffset)
        {
            Debug.Assert(mask.Height + maskOffset.Y >= SourceRectangle.Height && mask.Width * 8 + maskOffset.X >= SourceRectangle.Width);

            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold >> quantizingScale)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                byte alphaThreshold = Math.Max(quantizingSession.AlphaThreshold, (byte)1);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;

                    ArraySection<byte> maskRow = mask[y + maskOffset.Y];
                    for (int x = 0; x < SourceRectangle.Width; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(maskRow.GetElementUnchecked((x + maskOffset.X) >> 3), x + maskOffset.X) == 0)
                            continue;
                        Color32 colorSrc = rowSrc.DoGetColor32(x + SourceRectangle.X);
                        if (skipTransparent && colorSrc.A < alphaThreshold)
                            continue;

                        rowDst.DoSetColor32(x + TargetRectangle.X, quantizingSession.GetQuantizedColor(colorSrc));
                    }

                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            var maskLocal = mask;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IQuantizingSession session = quantizingSession;
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                byte alphaThreshold = Math.Max(session.AlphaThreshold, (byte)1);
                bool skip = skipTransparent;
                ArraySection<byte> maskRow = maskLocal[y + maskOffset.Y];
                for (int x = 0; x < width; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(maskRow.GetElementUnchecked((x + maskOffset.X) >> 3), x + maskOffset.X) == 0)
                        continue;
                    Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);
                    if (skip && colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(x + offsetDst, session.GetQuantizedColor(colorSrc));
                }
            });
        }

        private void PerformCopyWithDitheringSkipTransparent(IQuantizingSession quantizingSession, IDitheringSession ditheringSession)
        {
            // Sequential processing
            if (ditheringSession.IsSequential || SourceRectangle.Width < parallelThreshold >> ditheringScale)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                byte alphaThreshold = Math.Max(quantizingSession.AlphaThreshold, (byte)1);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;

                    // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                    for (int x = 0; x < SourceRectangle.Width; x++)
                    {
                        Color32 colorSrc = rowSrc.DoGetColor32(x + SourceRectangle.X);
                        if (colorSrc.A < alphaThreshold)
                            continue;

                        rowDst.DoSetColor32(x + TargetRectangle.X, ditheringSession.GetDitheredColor(colorSrc, x, y));
                    }

                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IDitheringSession session = ditheringSession;
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                byte alphaThreshold = Math.Max(quantizingSession.AlphaThreshold, (byte)1);

                // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);
                    if (colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(x + offsetDst, session.GetDitheredColor(colorSrc, x, y));
                }
            });
        }

        private void PerformCopyWithDitheringMasked(IQuantizingSession quantizingSession, IDitheringSession ditheringSession, bool skipTransparent, in Array2D<byte> mask, Point maskOffset)
        {
            Debug.Assert(mask.Height + maskOffset.Y >= SourceRectangle.Height && mask.Width * 8 + maskOffset.X >= SourceRectangle.Width);

            // Sequential processing
            if (ditheringSession.IsSequential || SourceRectangle.Width < parallelThreshold >> ditheringScale)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.GetRowCached(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetRectangle.Y);
                byte alphaThreshold = Math.Max(quantizingSession.AlphaThreshold, (byte)1);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;

                    ArraySection<byte> maskRow = mask[y + maskOffset.Y];

                    // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                    for (int x = 0; x < SourceRectangle.Width; x++)
                    {
                        if (ColorExtensions.Get1bppColorIndex(maskRow.GetElementUnchecked((x + maskOffset.X) >> 3), x + maskOffset.X) == 0)
                            continue;
                        Color32 colorSrc = rowSrc.DoGetColor32(x + SourceRectangle.X);
                        if (skipTransparent && colorSrc.A < alphaThreshold)
                            continue;

                        rowDst.DoSetColor32(x + TargetRectangle.X, ditheringSession.GetDitheredColor(colorSrc, x, y));
                    }

                    rowSrc.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Array2D<byte> maskLocal = mask;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, y =>
            {
                IDitheringSession session = ditheringSession;
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                ArraySection<byte> maskRow = maskLocal[y + maskOffset.Y];
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                byte alphaThreshold = Math.Max(quantizingSession.AlphaThreshold, (byte)1);
                bool skip = skipTransparent;

                // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                for (int x = 0; x < width; x++)
                {
                    if (ColorExtensions.Get1bppColorIndex(maskRow.GetElementUnchecked((x + maskOffset.X) >> 3), x + maskOffset.X) == 0)
                        continue;
                    Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);
                    if (skip && colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(x + offsetDst, session.GetDitheredColor(colorSrc, x, y));
                }
            });
        }

        private void PerformDrawWithQuantizer(IQuantizingSession quantizingSession)
        {
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;

            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold >> quantizingScale)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);

                if (quantizingSession.WorkingColorSpace == WorkingColorSpace.Linear)
                {
                    for (int y = 0; y < SourceRectangle.Height; y++)
                    {
                        if (context.IsCancellationRequested)
                            return;
                        ProcessRowLinear(y);
                        context.Progress?.Increment();
                    }
                }
                else
                {
                    for (int y = 0; y < SourceRectangle.Height; y++)
                    {
                        if (context.IsCancellationRequested)
                            return;
                        ProcessRowSrgb(y);
                        context.Progress?.Increment();
                    }
                }

                return;
            }

            // Parallel processing
            Action<int> processRow = quantizingSession.WorkingColorSpace == WorkingColorSpace.Linear ? ProcessRowLinear : ProcessRowSrgb;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, processRow);

            #region Local Methods

            void ProcessRowSrgb(int y)
            {
                IQuantizingSession session = quantizingSession;
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                byte alphaThreshold = session.AlphaThreshold;
                int width = sourceWidth;

                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(x + offsetDst, session.GetQuantizedColor(colorSrc));
                        continue;
                    }

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // source here has a partial transparency: we need to read the target color
                    int pos = x + offsetDst;
                    Color32 colorDst = rowDst.DoGetColor32(pos);

                    // non-transparent target: blending
                    if (colorDst.A != 0)
                    {
                        colorSrc = colorDst.A == Byte.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackgroundSrgb(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWithSrgb(colorDst);
                    }

                    // overwriting target color only if blended color has high enough alpha
                    if (colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(pos, session.GetQuantizedColor(colorSrc));
                }
            }

            void ProcessRowLinear(int y)
            {
                IQuantizingSession session = quantizingSession;
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                byte alphaThreshold = session.AlphaThreshold;
                int width = sourceWidth;

                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(x + offsetDst, session.GetQuantizedColor(colorSrc));
                        continue;
                    }

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // source here has a partial transparency: we need to read the target color
                    int pos = x + offsetDst;
                    Color32 colorDst = rowDst.DoGetColor32(pos);

                    // non-transparent target: blending
                    if (colorDst.A != 0)
                    {
                        colorSrc = colorDst.A == Byte.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackgroundLinear(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWithLinear(colorDst);
                    }

                    // overwriting target color only if blended color has high enough alpha
                    if (colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(pos, session.GetQuantizedColor(colorSrc));
                }
            }

            #endregion
        }

        private void PerformDrawWithDithering(IQuantizingSession quantizingSession, IDitheringSession ditheringSession)
        {
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;

            // Sequential processing
            if (ditheringSession.IsSequential || SourceRectangle.Width < parallelThreshold >> ditheringScale)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, SourceRectangle.Height);
                if (quantizingSession.WorkingColorSpace == WorkingColorSpace.Linear)
                {
                    for (int y = 0; y < SourceRectangle.Height; y++)
                    {
                        if (context.IsCancellationRequested)
                            return;
                        ProcessRowLinear(y);
                        context.Progress?.Increment();
                    } 
                }
                else
                {
                    for (int y = 0; y < SourceRectangle.Height; y++)
                    {
                        if (context.IsCancellationRequested)
                            return;
                        ProcessRowSrgb(y);
                        context.Progress?.Increment();
                    }
                }

                return;
            }

            // Parallel processing
            Action<int> processRow = quantizingSession.WorkingColorSpace == WorkingColorSpace.Linear ? ProcessRowLinear : ProcessRowSrgb;
            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, SourceRectangle.Height, processRow);

            #region Local Methods

            void ProcessRowSrgb(int y)
            {
                IDitheringSession session = ditheringSession;
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                byte alphaThreshold = quantizingSession.AlphaThreshold;
                int width = sourceWidth;

                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(x + offsetDst, session.GetDitheredColor(colorSrc, x, y));
                        continue;
                    }

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // source here has a partial transparency: we need to read the target color
                    int pos = x + offsetDst;
                    Color32 colorDst = rowDst.DoGetColor32(pos);

                    // non-transparent target: blending
                    if (colorDst.A != 0)
                    {
                        colorSrc = colorDst.A == Byte.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackgroundSrgb(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWithSrgb(colorDst);
                    }

                    // overwriting target color only if blended color has high enough alpha
                    if (colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(pos, session.GetDitheredColor(colorSrc, x, y));
                }
            }

            void ProcessRowLinear(int y)
            {
                IDitheringSession session = ditheringSession;
                IBitmapDataRowInternal rowSrc = source.GetRowCached(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                byte alphaThreshold = quantizingSession.AlphaThreshold;
                int width = sourceWidth;

                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32(x + offsetDst, session.GetDitheredColor(colorSrc, x, y));
                        continue;
                    }

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // source here has a partial transparency: we need to read the target color
                    int pos = x + offsetDst;
                    Color32 colorDst = rowDst.DoGetColor32(pos);

                    // non-transparent target: blending
                    if (colorDst.A != 0)
                    {
                        colorSrc = colorDst.A == Byte.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackgroundLinear(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWithLinear(colorDst);
                    }

                    // overwriting target color only if blended color has high enough alpha
                    if (colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(pos, session.GetDitheredColor(colorSrc, x, y));
                }
            }

            #endregion
        }

        #endregion

        #endregion
    }
}
