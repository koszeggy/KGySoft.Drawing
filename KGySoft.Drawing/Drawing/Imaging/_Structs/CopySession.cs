#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CopySession.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security;

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

        internal IBitmapDataInternal Source;
        internal IBitmapDataInternal Target;
        internal Rectangle SourceRectangle;
        internal Rectangle TargetRectangle;

        #endregion

        #region Constructors

        internal CopySession(IBitmapDataInternal sessionSource, IBitmapDataInternal sessionTarget, Rectangle actualSourceRectangle, Rectangle actualTargetRectangle)
        {
            Source = sessionSource;
            Target = sessionTarget;
            SourceRectangle = actualSourceRectangle;
            TargetRectangle = actualTargetRectangle;
        }

        #endregion

        #region Methods

        #region Internal Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal void PerformCopy(IAsyncContext context)
        {
            if (!TryPerformRawCopy(context))
            {
                if (Target.IsFastPremultiplied())
                    PerformCopyDirectPremultiplied(context);
                else
                    PerformCopyDirectStraight(context);
            }
        }

        internal void PerformCopyWithQuantizer(IAsyncContext context, IQuantizingSession quantizingSession, bool skipTransparent)
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold >> quantizingScale)
            {
                context.Progress?.New(DrawingOperation.CopyWithQuantizer, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.DoGetRow(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.DoGetRow(TargetRectangle.Y);
                byte alphaThreshold = quantizingSession.AlphaThreshold;
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < SourceRectangle.Width; x++)
                    {
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
            ParallelHelper.For(context, DrawingOperation.CopyWithQuantizer, 0, SourceRectangle.Height, y =>
            {
                IQuantizingSession session = quantizingSession;
                IBitmapDataRowInternal rowSrc = source.DoGetRow(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.DoGetRow(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                byte alphaThreshold = session.AlphaThreshold;
                bool skip = skipTransparent;
                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);
                    if (skip && colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(x + offsetDst, session.GetQuantizedColor(colorSrc));
                }
            });
        }

        internal void PerformCopyWithDithering(IAsyncContext context, IQuantizingSession quantizingSession, IDitheringSession ditheringSession, bool skipTransparent)
        {
            // Sequential processing
            if (ditheringSession.IsSequential || SourceRectangle.Width < parallelThreshold >> ditheringScale)
            {
                context.Progress?.New(DrawingOperation.CopyWithDithering, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.DoGetRow(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.DoGetRow(TargetRectangle.Y);
                byte alphaThreshold = quantizingSession.AlphaThreshold;
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;

                    // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                    for (int x = 0; x < SourceRectangle.Width; x++)
                    {
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
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;
            ParallelHelper.For(context, DrawingOperation.CopyWithDithering, 0, SourceRectangle.Height, y =>
            {
                IDitheringSession session = ditheringSession;
                IBitmapDataRowInternal rowSrc = source.DoGetRow(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.DoGetRow(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                byte alphaThreshold = quantizingSession.AlphaThreshold;
                bool skip = skipTransparent;

                // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);
                    if (skip && colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(x + offsetDst, session.GetDitheredColor(colorSrc, x, y));
                }
            });
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "False alarm, initSource is disposed if needed")]
        internal void PerformDraw(IQuantizer quantizer, IDitherer ditherer)
        {
            if (quantizer == null)
            {
                PerformDrawDirect();
                return;
            }

            IReadableBitmapData initSource = SourceRectangle.Size == Source.GetSize()
                ? Source
                : Source.Clip(SourceRectangle);

            try
            {
                Debug.Assert(!quantizer.InitializeReliesOnContent || !Source.HasMultiLevelAlpha(), "This draw performs blending on-the-fly but the used quantizer would require two-pass processing");
                using (IQuantizingSession quantizingSession = quantizer.Initialize(initSource, AsyncHelper.Null) ?? throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull))
                {
                    // quantizing without dithering
                    if (ditherer == null)
                    {
                        PerformDrawWithQuantizer(quantizingSession);
                        return;
                    }

                    // quantizing with dithering
                    Debug.Assert(!ditherer.InitializeReliesOnContent || !Source.HasMultiLevelAlpha(), "This draw performs blending on-the-fly but the used ditherer would require two-pass processing");

                    using IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession, AsyncHelper.Null) ?? throw new InvalidOperationException(Res.ImagingDithererInitializeNull);
                    PerformDrawWithDithering(quantizingSession, ditheringSession);
                }
            }
            finally
            {
                if (!ReferenceEquals(initSource, Source))
                    initSource.Dispose();
            }
        }

        internal void PerformDrawDirect()
        {
            IBitmapDataInternal source = Source;
            IBitmapDataInternal target = Target;
            Point sourceLocation = SourceRectangle.Location;
            Point targetLocation = TargetRectangle.Location;
            int sourceWidth = SourceRectangle.Width;

            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                if (Target.IsFastPremultiplied())
                {
                    for (int y = 0; y < SourceRectangle.Height; y++)
                        ProcessRowPremultiplied(y);
                }
                else
                {
                    for (int y = 0; y < SourceRectangle.Height; y++)
                        ProcessRowStraight(y);
                }

                return;
            }

            // Parallel processing
            Action<int> processRow = Target.IsFastPremultiplied()
                ? ProcessRowPremultiplied
                : (Action<int>)ProcessRowStraight;

            ParallelHelper.For(0, SourceRectangle.Height, processRow);

            #region Local Methods

            void ProcessRowStraight(int y)
            {
                IBitmapDataRowInternal rowSrc = source.DoGetRow(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.DoGetRow(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                byte alphaThreshold = target.AlphaThreshold;
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
                            ? colorSrc.BlendWithBackground(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst);
                    }

                    // overwriting target color only if blended color has high enough alpha
                    if (colorSrc.A < alphaThreshold)
                        continue;

                    rowDst.DoSetColor32(pos, colorSrc);
                }
            }

            void ProcessRowPremultiplied(int y)
            {
                IBitmapDataRowInternal rowSrc = source.DoGetRow(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.DoGetRow(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;

                for (int x = 0; x < width; x++)
                {
                    Color32 colorSrc = rowSrc.DoGetColor32Premultiplied(x + offsetSrc);

                    // fully solid source: overwrite
                    if (colorSrc.A == Byte.MaxValue)
                    {
                        rowDst.DoSetColor32Premultiplied(x + offsetDst, colorSrc);
                        continue;
                    }

                    // fully transparent source: skip
                    if (colorSrc.A == 0)
                        continue;

                    // source here has a partial transparency: we need to read the target color
                    int pos = x + offsetDst;
                    Color32 colorDst = rowDst.DoGetColor32Premultiplied(pos);

                    // non-transparent target: blending
                    if (colorDst.A != 0)
                        colorSrc = colorSrc.BlendWithPremultiplied(colorDst);

                    rowDst.DoSetColor32Premultiplied(pos, colorSrc);
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
        private bool TryPerformRawCopy(IAsyncContext context)
        {
            // same pixel format and a well-known bitmap data type is required
            if (Source.PixelFormat != Target.PixelFormat
                || !(Source is NativeBitmapDataBase || Source is ManagedBitmapDataBase)
                || !(Target is NativeBitmapDataBase || Target is ManagedBitmapDataBase))
            {
                return false;
            }

            int bpp = Source.PixelFormat.ToBitsPerPixel();

            if (bpp > 8)
            {
                // wide colors: allowing raw copy only if wide color transformations are the same on managed and native bitmap data
                if (bpp > 32 && Source.GetType() != Target.GetType() && ColorExtensions.Max16BppValue != UInt16.MaxValue)
                    return false;

                int byteSize = bpp >> 3;
                SourceRectangle.X *= byteSize;
                TargetRectangle.X *= byteSize;
                SourceRectangle.Width = TargetRectangle.Width *= byteSize;
                PerformCopyRaw(context);
                return true;
            }

            // for indexed images we need some further checks
            // palette must be the same (only a reference check for better performance)
            if (Source.Palette?.Equals(Target.Palette) != true)
                return false;

            if (bpp == 8)
            {
                PerformCopyRaw(context);
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
            PerformCopyRaw(context);
            return true;
        }

        [SecuritySafeCritical]
        private unsafe void PerformCopyRaw(IAsyncContext context)
        {
            Debug.Assert(Source is NativeBitmapDataBase || Source is ManagedBitmapDataBase);
            Debug.Assert(Target is NativeBitmapDataBase || Target is ManagedBitmapDataBase);

            if (Source is NativeBitmapDataBase nativeSrc)
            {
                if (Target is NativeBitmapDataBase nativeDst)
                {
                    DoCopyRaw(nativeSrc.Stride, nativeDst.Stride, (byte*)nativeSrc.Scan0, (byte*)nativeDst.Scan0, context);
                    return;
                }

                var managedDst = (ManagedBitmapDataBase)Target;
                fixed (byte* pDst = &managedDst.GetPinnableReference())
                    DoCopyRaw(nativeSrc.Stride, managedDst.RowSize, (byte*)nativeSrc.Scan0, pDst, context);
            }
            else
            {
                var managedSrc = (ManagedBitmapDataBase)Source;
                if (Target is NativeBitmapDataBase nativeDst)
                {
                    fixed (byte* pSrc = &managedSrc.GetPinnableReference())
                        DoCopyRaw(managedSrc.RowSize, nativeDst.Stride, pSrc, (byte*)nativeDst.Scan0, context);
                    return;
                }

                var managedDst = (ManagedBitmapDataBase)Target;
                fixed (byte* pSrc = &managedSrc.GetPinnableReference())
                fixed (byte* pDst = &managedDst.GetPinnableReference())
                    DoCopyRaw(managedSrc.RowSize, managedDst.RowSize, pSrc, pDst, context);
            }
        }

        [SecurityCritical]
        [SuppressMessage("Microsoft.Security", "CA2140:Transparent code must not reference security critical items",
            Justification = "False alarm, SecurityCritical is applied**")]
        private unsafe void DoCopyRaw(int sourceStride, int targetStride, byte* sourceOrigin, byte* targetOrigin, IAsyncContext context)
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.RawCopy, SourceRectangle.Height);
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
            ParallelHelper.For(context, DrawingOperation.RawCopy, 0, SourceRectangle.Height, y =>
            {
                byte* pSrc = sourceOrigin + (y + sourceLocation.Y) * sourceStride + sourceLocation.X;
                byte* pDst = targetOrigin + (y + targetLocation.Y) * targetStride + targetLocation.X;
                MemoryHelper.CopyMemory(pSrc, pDst, width);
            });
        }

        private void PerformCopyDirectStraight(IAsyncContext context)
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.StraightCopy, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.DoGetRow(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.DoGetRow(TargetRectangle.Y);
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
            ParallelHelper.For(context, DrawingOperation.StraightCopy, 0, SourceRectangle.Height, y =>
            {
                IBitmapDataRowInternal rowSrc = source.DoGetRow(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.DoGetRow(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetColor32(x + offsetDst, rowSrc.DoGetColor32(x + offsetSrc));
            });
        }

        private void PerformCopyDirectPremultiplied(IAsyncContext context)
        {
            // Sequential processing
            if (SourceRectangle.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.PremultipliedCopy, SourceRectangle.Height);
                IBitmapDataRowInternal rowSrc = Source.DoGetRow(SourceRectangle.Y);
                IBitmapDataRowInternal rowDst = Target.DoGetRow(TargetRectangle.Y);
                for (int y = 0; y < SourceRectangle.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < SourceRectangle.Width; x++)
                        rowDst.DoSetColor32Premultiplied(x + TargetRectangle.X, rowSrc.DoGetColor32Premultiplied(x + SourceRectangle.X));
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
            ParallelHelper.For(context, DrawingOperation.PremultipliedCopy, 0, SourceRectangle.Height, y =>
            {
                IBitmapDataRowInternal rowSrc = source.DoGetRow(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.DoGetRow(targetLocation.Y + y);
                int offsetSrc = sourceLocation.X;
                int offsetDst = targetLocation.X;
                int width = sourceWidth;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetColor32Premultiplied(x + offsetDst, rowSrc.DoGetColor32Premultiplied(x + offsetSrc));
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
                for (int y = 0; y < SourceRectangle.Height; y++)
                    ProcessRow(y);

                return;
            }

            // Parallel processing
            ParallelHelper.For(0, SourceRectangle.Height, ProcessRow);

            #region Local Methods

            void ProcessRow(int y)
            {
                IQuantizingSession session = quantizingSession;
                IBitmapDataRowInternal rowSrc = source.DoGetRow(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.DoGetRow(targetLocation.Y + y);
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
                            ? colorSrc.BlendWithBackground(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst);
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
                for (int y = 0; y < SourceRectangle.Height; y++)
                    ProcessRow(y);

                return;
            }

            // Parallel processing
            ParallelHelper.For(0, SourceRectangle.Height, ProcessRow);

            #region Local Methods

            void ProcessRow(int y)
            {
                IDitheringSession session = ditheringSession;
                IBitmapDataRowInternal rowSrc = source.DoGetRow(sourceLocation.Y + y);
                IBitmapDataRowInternal rowDst = target.DoGetRow(targetLocation.Y + y);
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
                            ? colorSrc.BlendWithBackground(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst);
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
