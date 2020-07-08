#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.Copy.cs
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
using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    partial class BitmapDataExtensions
    {
        #region Nested structs

        #region CopySession struct

        private struct CopySession
        {
            #region Fields

            internal IBitmapDataInternal Source;
            internal IBitmapDataInternal Target;
            internal Rectangle SourceRectangle;
            internal Rectangle TargetRectangle;

            #endregion

            #region Methods

            #region Internal Methods

            /// <summary>
            /// Tries to perform a raw copy. If succeeds converts the horizontal dimensions to bytes from pixels.
            /// Note: Stride and origin is set from outside so we spare some casts and possible GCHandle uses.
            /// </summary>
            internal bool TryPerformRawCopy()
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
                    PerformCopyRaw();
                    return true;
                }

                // for indexed images we need some further checks
                // palette must be the same (only a reference check for better performance)
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
                if ((width & alignmentMask) != 0)
                    width++;

                SourceRectangle.Width = TargetRectangle.Width = width;
                PerformCopyRaw();
                return true;
            }

            internal void PerformCopyDirect()
            {
                // Sequential processing
                if (SourceRectangle.Width < parallelThreshold)
                {
                    IBitmapDataRowInternal rowSrc = Source.GetRow(SourceRectangle.Y);
                    IBitmapDataRowInternal rowDst = Target.GetRow(TargetRectangle.Y);
                    for (int y = 0; y < SourceRectangle.Height; y++)
                    {
                        for (int x = 0; x < SourceRectangle.Width; x++)
                            rowDst.DoSetColor32(x + TargetRectangle.X, rowSrc.DoGetColor32(x + SourceRectangle.X));
                        rowSrc.MoveNextRow();
                        rowDst.MoveNextRow();
                    }

                    return;
                }

                // Parallel processing
                IBitmapDataInternal source = Source;
                IBitmapDataInternal target = Target;
                Point sourceLocation = SourceRectangle.Location;
                Point targetLocation = TargetRectangle.Location;
                int width = SourceRectangle.Width;
                ParallelHelper.For(0, SourceRectangle.Height, y =>
                {
                    IBitmapDataRowInternal rowSrc = source.GetRow(sourceLocation.Y + y);
                    IBitmapDataRowInternal rowDst = target.GetRow(targetLocation.Y + y);
                    int offsetSrc = sourceLocation.X;
                    int offsetDst = targetLocation.X;
                    int len = width;
                    for (int x = 0; x < len; x++)
                        rowDst.DoSetColor32(x + offsetDst, rowSrc.DoGetColor32(x + offsetSrc));
                });
            }

            internal void PerformDrawDirect()
            {
                IBitmapDataInternal source = Source;
                IBitmapDataInternal target = Target;
                Point sourceLocation = SourceRectangle.Location;
                Point targetLocation = TargetRectangle.Location;
                int width = SourceRectangle.Width;

                // Sequential processing
                if (SourceRectangle.Width < parallelThreshold)
                {
                    if (Target.PixelFormat == PixelFormat.Format32bppPArgb)
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
                Action<int> processRow = Target.PixelFormat == PixelFormat.Format32bppPArgb
                    ? ProcessRowPremultiplied
                    : (Action<int>)ProcessRowStraight;

                ParallelHelper.For(0, SourceRectangle.Height, processRow);

                #region Local Methods

                void ProcessRowStraight(int y)
                {
                    IBitmapDataRowInternal rowSrc = source.GetRow(sourceLocation.Y + y);
                    IBitmapDataRowInternal rowDst = target.GetRow(targetLocation.Y + y);
                    int offsetSrc = sourceLocation.X;
                    int offsetDst = targetLocation.X;
                    int len = width;

                    for (int x = 0; x < len; x++)
                    {
                        Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            rowDst.DoSetColor32(x + offsetDst, colorSrc);
                            continue;
                        }

                        // fully transparent source: skip
                        if (colorSrc.A == 0 || colorSrc.A < target.AlphaThreshold)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        int pos = x + offsetDst;
                        Color32 colorDst = rowDst.DoGetColor32(pos);

                        // fully transparent target: we can overwrite with source
                        if (colorDst.A == 0)
                        {
                            rowDst.DoSetColor32(pos, colorSrc);
                            continue;
                        }

                        colorSrc = colorDst.A == Byte.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackground(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst);

                        rowDst.DoSetColor32(pos, colorSrc);
                    }
                }

                void ProcessRowPremultiplied(int y)
                {
                    IBitmapDataRowInternal rowSrc = source.GetRow(sourceLocation.Y + y);
                    IBitmapDataRowInternal rowDst = target.GetRow(targetLocation.Y + y);
                    int offsetSrc = sourceLocation.X;
                    int offsetDst = targetLocation.X;
                    int len = width;
                    bool isPremultipliedSource = source.PixelFormat == PixelFormat.Format32bppPArgb;

                    for (int x = 0; x < len; x++)
                    {
                        Color32 colorSrc = isPremultipliedSource
                                ? rowSrc.DoReadRaw<Color32>(x + offsetSrc)
                                : rowSrc.DoGetColor32(x + offsetSrc).ToPremultiplied();

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            rowDst.DoWriteRaw(x + offsetDst, colorSrc);
                            continue;
                        }

                        // fully transparent source: skip
                        if (colorSrc.A == 0)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        int pos = x + offsetDst;
                        Color32 colorDst = rowDst.DoReadRaw<Color32>(pos);

                        // fully transparent target: we can overwrite with source
                        if (colorDst.A == 0)
                        {
                            rowDst.DoWriteRaw(pos, colorSrc);
                            continue;
                        }

                        rowDst.DoWriteRaw(pos, colorSrc.BlendWithPremultiplied(colorDst));
                    }
                }

                #endregion
            }

            internal void PerformCopyWithQuantizer(IQuantizingSession quantizingSession)
            {
                // Sequential processing
                if (SourceRectangle.Width < (parallelThreshold >> 1))
                {
                    IBitmapDataRowInternal rowSrc = Source.GetRow(SourceRectangle.Y);
                    IBitmapDataRowInternal rowDst = Target.GetRow(TargetRectangle.Y);
                    for (int y = 0; y < SourceRectangle.Height; y++)
                    {
                        for (int x = 0; x < SourceRectangle.Width; x++)
                            rowDst.DoSetColor32(x + TargetRectangle.X, quantizingSession.GetQuantizedColor(rowSrc.DoGetColor32(x + SourceRectangle.X)));
                        rowSrc.MoveNextRow();
                        rowDst.MoveNextRow();
                    }

                    return;
                }

                IBitmapDataInternal source = Source;
                IBitmapDataInternal target = Target;
                Point sourceLocation = SourceRectangle.Location;
                Point targetLocation = TargetRectangle.Location;
                int width = SourceRectangle.Width;
                ParallelHelper.For(0, SourceRectangle.Height, y =>
                {
                    IQuantizingSession session = quantizingSession;
                    IBitmapDataRowInternal rowSrc = source.GetRow(sourceLocation.Y + y);
                    IBitmapDataRowInternal rowDst = target.GetRow(targetLocation.Y + y);
                    int offsetSrc = sourceLocation.X;
                    int offsetDst = targetLocation.X;
                    int len = width;
                    for (int x = 0; x < len; x++)
                        rowDst.DoSetColor32(x + offsetDst, session.GetQuantizedColor(rowSrc.DoGetColor32(x + offsetSrc)));
                });
            }

            internal void PerformDrawWithQuantizer(IQuantizingSession quantizingSession)
            {
                IBitmapDataInternal source = Source;
                IBitmapDataInternal target = Target;
                Point sourceLocation = SourceRectangle.Location;
                Point targetLocation = TargetRectangle.Location;
                int width = SourceRectangle.Width;

                // Sequential processing
                if (SourceRectangle.Width < parallelThreshold)
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
                    IBitmapDataRowInternal rowSrc = source.GetRow(sourceLocation.Y + y);
                    IBitmapDataRowInternal rowDst = target.GetRow(targetLocation.Y + y);
                    int offsetSrc = sourceLocation.X;
                    int offsetDst = targetLocation.X;
                    byte alphaThreshold = session.AlphaThreshold;
                    int len = width;

                    for (int x = 0; x < len; x++)
                    {
                        Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            rowDst.DoSetColor32(x + offsetDst, session.GetQuantizedColor(colorSrc));
                            continue;
                        }

                        // fully transparent source: skip
                        if (colorSrc.A == 0 || colorSrc.A < alphaThreshold)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        int pos = x + offsetDst;
                        Color32 colorDst = rowDst.DoGetColor32(pos);

                        // fully transparent target: we can overwrite with source
                        if (colorDst.A == 0)
                        {
                            rowDst.DoSetColor32(pos, session.GetQuantizedColor(colorSrc));
                            continue;
                        }

                        colorSrc = colorDst.A == Byte.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackground(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst);

                        rowDst.DoSetColor32(pos, session.GetQuantizedColor(colorSrc));
                    }
                }

                #endregion
            }

            internal void PerformCopyWithDithering(IDitheringSession ditheringSession)
            {
                // Sequential processing
                if (SourceRectangle.Width < (parallelThreshold >> 2) || ditheringSession.IsSequential)
                {
                    IBitmapDataRowInternal rowSrc = Source.GetRow(SourceRectangle.Y);
                    IBitmapDataRowInternal rowDst = Target.GetRow(TargetRectangle.Y);
                    for (int y = 0; y < SourceRectangle.Height; y++)
                    {
                        // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                        for (int x = 0; x < SourceRectangle.Width; x++)
                            rowDst.DoSetColor32(x + TargetRectangle.X, ditheringSession.GetDitheredColor(rowSrc.DoGetColor32(x + SourceRectangle.X), x, y));
                        rowSrc.MoveNextRow();
                        rowDst.MoveNextRow();
                    }

                    return;
                }

                // Parallel processing
                IBitmapDataInternal source = Source;
                IBitmapDataInternal target = Target;
                Point sourceLocation = SourceRectangle.Location;
                Point targetLocation = TargetRectangle.Location;
                int width = SourceRectangle.Width;
                ParallelHelper.For(0, SourceRectangle.Height, y =>
                {
                    IBitmapDataRowInternal rowSrc = source.GetRow(sourceLocation.Y + y);
                    IBitmapDataRowInternal rowDst = target.GetRow(targetLocation.Y + y);
                    int offsetSrc = sourceLocation.X;
                    int offsetDst = targetLocation.X;
                    int len = width;

                    // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                    for (int x = 0; x < len; x++)
                        rowDst.DoSetColor32(x + offsetDst, ditheringSession.GetDitheredColor(rowSrc.DoGetColor32(x + offsetSrc), x, y));
                });
            }

            internal void PerformDrawWithDithering(IQuantizingSession quantizingSession, IDitheringSession ditheringSession)
            {
                IBitmapDataInternal source = Source;
                IBitmapDataInternal target = Target;
                Point sourceLocation = SourceRectangle.Location;
                Point targetLocation = TargetRectangle.Location;
                int width = SourceRectangle.Width;

                // Sequential processing
                if (SourceRectangle.Width < (parallelThreshold >> 2) || ditheringSession.IsSequential)
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
                    IBitmapDataRowInternal rowSrc = source.GetRow(sourceLocation.Y + y);
                    IBitmapDataRowInternal rowDst = target.GetRow(targetLocation.Y + y);
                    int offsetSrc = sourceLocation.X;
                    int offsetDst = targetLocation.X;
                    byte alphaThreshold = quantizingSession.AlphaThreshold;
                    int len = width;

                    for (int x = 0; x < len; x++)
                    {
                        Color32 colorSrc = rowSrc.DoGetColor32(x + offsetSrc);

                        // fully solid source: overwrite
                        if (colorSrc.A == Byte.MaxValue)
                        {
                            rowDst.DoSetColor32(x + offsetDst, session.GetDitheredColor(colorSrc, x, y));
                            continue;
                        }

                        // fully transparent source: skip
                        if (colorSrc.A == 0 || colorSrc.A < alphaThreshold)
                            continue;

                        // source here has a partial transparency: we need to read the target color
                        int pos = x + offsetDst;
                        Color32 colorDst = rowDst.DoGetColor32(pos);

                        // fully transparent target: we can overwrite with source
                        if (colorDst.A == 0)
                        {
                            rowDst.DoSetColor32(pos, session.GetDitheredColor(colorSrc, x, y));
                            continue;
                        }

                        colorSrc = colorDst.A == Byte.MaxValue
                            // target pixel is fully solid: simple blending
                            ? colorSrc.BlendWithBackground(colorDst)
                            // both source and target pixels are partially transparent: complex blending
                            : colorSrc.BlendWith(colorDst);

                        rowDst.DoSetColor32(pos, session.GetDitheredColor(colorSrc, x, y));
                    }
                }

                #endregion
            }

            #endregion

            #region Private Methods

            private unsafe void PerformCopyRaw()
            {
                Debug.Assert(Source is NativeBitmapDataBase || Source is ManagedBitmapDataBase);
                Debug.Assert(Target is NativeBitmapDataBase || Target is ManagedBitmapDataBase);

                if (Source is NativeBitmapDataBase nativeSrc)
                {
                    if (Target is NativeBitmapDataBase nativeDst)
                    {
                        DoCopyRaw(nativeSrc.Stride, nativeDst.Stride, (byte*)nativeSrc.Scan0, (byte*)nativeDst.Scan0);
                        return;
                    }

                    var managedDst = (ManagedBitmapDataBase)Target;
                    fixed (byte* pDst = &managedDst.GetPinnableReference())
                        DoCopyRaw(nativeSrc.Stride, managedDst.RowSize, (byte*)nativeSrc.Scan0, pDst);
                }
                else
                {
                    var managedSrc = (ManagedBitmapDataBase)Source;
                    if (Target is NativeBitmapDataBase nativeDst)
                    {
                        fixed (byte* pSrc = &managedSrc.GetPinnableReference())
                            DoCopyRaw(managedSrc.RowSize, nativeDst.Stride, pSrc, (byte*)nativeDst.Scan0);
                        return;
                    }

                    var managedDst = (ManagedBitmapDataBase)Target;
                    fixed (byte* pSrc = &managedSrc.GetPinnableReference())
                    fixed (byte* pDst = &managedDst.GetPinnableReference())
                        DoCopyRaw(managedSrc.RowSize, managedDst.RowSize, pSrc, pDst);
                }
            }

            private unsafe void DoCopyRaw(int sourceStride, int targetStride, byte* sourceOrigin, byte* targetOrigin)
            {
                // Sequential processing
                if (SourceRectangle.Width < parallelThreshold)
                {
                    byte* pSrc = sourceOrigin + SourceRectangle.Y * sourceStride + SourceRectangle.X;
                    byte* pDst = targetOrigin + TargetRectangle.Y * targetStride + TargetRectangle.X;
                    for (int y = 0; y < SourceRectangle.Height; y++)
                    {
                        MemoryHelper.CopyMemory(pSrc, pDst, SourceRectangle.Width);
                        pSrc += sourceStride;
                        pDst += targetStride;
                    }
                    return;
                }

                // Parallel processing
                Point sourceLocation = SourceRectangle.Location;
                Point targetLocation = TargetRectangle.Location;
                int width = SourceRectangle.Width;
                ParallelHelper.For(0, SourceRectangle.Height, y =>
                {
                    byte* pSrc = sourceOrigin + (y + sourceLocation.Y) * sourceStride + sourceLocation.X;
                    byte* pDst = targetOrigin + (y + targetLocation.Y) * targetStride + targetLocation.X;
                    MemoryHelper.CopyMemory(pSrc, pDst, width);
                });
            }

            #endregion

            #endregion
        }

        #endregion

        #endregion
    }
}
