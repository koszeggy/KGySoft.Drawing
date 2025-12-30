#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CombineSession.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Security;

using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal struct CombineSession
    {
        #region Constants

        private const int parallelThreshold = 100;
        private const int ditheringScale = 2;

        #endregion

        #region Fields

        #region Internal Fields
        
        [AllowNull]internal IBitmapDataInternal Source1 = null!;
        [AllowNull]internal IBitmapDataInternal Source2 = null!;
        internal IBitmapDataInternal Target = null!;
        internal Point Source1Location;
        internal Point Source2Location;
        internal Point TargetLocation;
        internal Size Size;

        #endregion

        #region Private Fields

        private readonly IAsyncContext context;

        #endregion

        #endregion

        #region Properties

        internal readonly Rectangle Source1Rectangle => new Rectangle(Source1Location, Size);
        internal readonly Rectangle Source2Rectangle => new Rectangle(Source2Location, Size);
        internal readonly Rectangle TargetRectangle => new Rectangle(TargetLocation, Size);

        #endregion

        #region Constructors

        internal CombineSession(IAsyncContext context) => this.context = context;

        #endregion

        #region Methods

        #region Internal Methods

        [SecuritySafeCritical]
        internal void PerformCombine(Func<Color32, Color32, Color32> combineFunction, IQuantizer? quantizer, IDitherer? ditherer)
        {
            // processing with dithering
            if (ditherer != null)
            {
                Debug.Assert(quantizer is PredefinedColorsQuantizer, "When combining with dithering, the quantizer is expected to be created from the target bitmap data");
                Debug.Assert(!ditherer.InitializeReliesOnContent, "When dithering relies on content, the combine operation is not expected to use the ditherer");
                
                // Using a clipped source for quantizer/ditherer, just for the initialization.
                // NOTE: As it is asserted, the quantizer ignores it, and also the ditherer should not care, maybe except the size.
                IReadableBitmapData initSource = Size == Source1.Size
                    ? Source1
                    : Source1.Clip(Source1Rectangle);
                try
                {
                    context.Progress?.New(DrawingOperation.InitializingQuantizer);
                    using IQuantizingSession quantizingSession = quantizer!.Initialize(initSource, context);

                    // quantization with dithering
                    context.Progress?.New(DrawingOperation.InitializingDitherer);
                    using IDitheringSession ditheringSession = ditherer.Initialize(initSource, quantizingSession, context);
                    if (context.IsCancellationRequested)
                        return;
                    if (ditheringSession == null)
                        throw new InvalidOperationException(Res.ImagingDithererInitializeNull);
                    PerformCombineWithDithering(ditheringSession, combineFunction);
                    return;
                }
                finally
                {
                    if (!ReferenceEquals(initSource, Source1))
                        initSource.Dispose();
                }
            }

            // Sequential processing
            if (Size.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, Size.Height);
                IBitmapDataRowInternal rowSrc1 = Source1.GetRowCached(Source1Location.Y);
                IBitmapDataRowInternal rowSrc2 = Source2.GetRowCached(Source2Location.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetLocation.Y);
                for (int y = 0; y < Size.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < Size.Width; x++)
                        rowDst.DoSetColor32(x + TargetLocation.X, combineFunction.Invoke(rowSrc1.DoGetColor32(x + Source1Location.X), rowSrc2.DoGetColor32(x + Source2Location.X)));
                    rowSrc1.MoveNextRow();
                    rowSrc2.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source1 = Source1;
            IBitmapDataInternal source2 = Source2;
            IBitmapDataInternal target = Target;
            Point source1Location = Source1Location;
            Point source2Location = Source2Location;
            Point targetLocation = TargetRectangle.Location;
            int sizeWidth = Size.Width;

            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, Size.Height, ProcessRow);

            #region Local Methods

            [SecuritySafeCritical]
            void ProcessRow(int y)
            {
                IBitmapDataRowInternal rowSrc1 = source1.GetRowCached(source1Location.Y + y);
                IBitmapDataRowInternal rowSrc2 = source2.GetRowCached(source2Location.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc1 = source1Location.X;
                int offsetSrc2 = source2Location.X;
                int offsetDst = targetLocation.X;
                int width = sizeWidth;
                Func<Color32, Color32, Color32> f = combineFunction;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetColor32(x + offsetDst, f.Invoke(rowSrc1.DoGetColor32(x + offsetSrc1), rowSrc2.DoGetColor32(x + offsetSrc2)));
            }

            #endregion
        }

        [SecuritySafeCritical]
        internal void PerformCombine(Func<Color64, Color64, Color64> combineFunction)
        {
            // Sequential processing
            if (Size.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, Size.Height);
                IBitmapDataRowInternal rowSrc1 = Source1.GetRowCached(Source1Location.Y);
                IBitmapDataRowInternal rowSrc2 = Source2.GetRowCached(Source2Location.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetLocation.Y);
                for (int y = 0; y < Size.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < Size.Width; x++)
                        rowDst.DoSetColor64(x + TargetLocation.X, combineFunction.Invoke(rowSrc1.DoGetColor64(x + Source1Location.X), rowSrc2.DoGetColor64(x + Source2Location.X)));
                    rowSrc1.MoveNextRow();
                    rowSrc2.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source1 = Source1;
            IBitmapDataInternal source2 = Source2;
            IBitmapDataInternal target = Target;
            Point source1Location = Source1Location;
            Point source2Location = Source2Location;
            Point targetLocation = TargetRectangle.Location;
            int sizeWidth = Size.Width;

            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, Size.Height, ProcessRow);

            #region Local Methods

            [SecuritySafeCritical]
            void ProcessRow(int y)
            {
                IBitmapDataRowInternal rowSrc1 = source1.GetRowCached(source1Location.Y + y);
                IBitmapDataRowInternal rowSrc2 = source2.GetRowCached(source2Location.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc1 = source1Location.X;
                int offsetSrc2 = source2Location.X;
                int offsetDst = targetLocation.X;
                int width = sizeWidth;
                Func<Color64, Color64, Color64> f = combineFunction;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetColor64(x + offsetDst, f.Invoke(rowSrc1.DoGetColor64(x + offsetSrc1), rowSrc2.DoGetColor64(x + offsetSrc2)));
            }

            #endregion
        }

        [SecuritySafeCritical]
        internal void PerformCombine(Func<ColorF, ColorF, ColorF> combineFunction)
        {
            // Sequential processing
            if (Size.Width < parallelThreshold)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, Size.Height);
                IBitmapDataRowInternal rowSrc1 = Source1.GetRowCached(Source1Location.Y);
                IBitmapDataRowInternal rowSrc2 = Source2.GetRowCached(Source2Location.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetLocation.Y);
                for (int y = 0; y < Size.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;
                    for (int x = 0; x < Size.Width; x++)
                        rowDst.DoSetColorF(x + TargetLocation.X, combineFunction.Invoke(rowSrc1.DoGetColorF(x + Source1Location.X), rowSrc2.DoGetColorF(x + Source2Location.X)));
                    rowSrc1.MoveNextRow();
                    rowSrc2.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source1 = Source1;
            IBitmapDataInternal source2 = Source2;
            IBitmapDataInternal target = Target;
            Point source1Location = Source1Location;
            Point source2Location = Source2Location;
            Point targetLocation = TargetRectangle.Location;
            int sizeWidth = Size.Width;

            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, Size.Height, ProcessRow);

            #region Local Methods

            [SecuritySafeCritical]
            void ProcessRow(int y)
            {
                IBitmapDataRowInternal rowSrc1 = source1.GetRowCached(source1Location.Y + y);
                IBitmapDataRowInternal rowSrc2 = source2.GetRowCached(source2Location.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc1 = source1Location.X;
                int offsetSrc2 = source2Location.X;
                int offsetDst = targetLocation.X;
                int width = sizeWidth;
                Func<ColorF, ColorF, ColorF> f = combineFunction;
                for (int x = 0; x < width; x++)
                    rowDst.DoSetColorF(x + offsetDst, f.Invoke(rowSrc1.DoGetColorF(x + offsetSrc1), rowSrc2.DoGetColorF(x + offsetSrc2)));
            }

            #endregion
        }

        #endregion

        #region Private Methods

        [SecuritySafeCritical]
        private void PerformCombineWithDithering(IDitheringSession ditheringSession, Func<Color32, Color32, Color32> combineFunction)
        {
            // Sequential processing
            if (ditheringSession.IsSequential || Size.Width < parallelThreshold >> ditheringScale)
            {
                context.Progress?.New(DrawingOperation.ProcessingPixels, Size.Height);
                IBitmapDataRowInternal rowSrc1 = Source1.GetRowCached(Source1Location.Y);
                IBitmapDataRowInternal rowSrc2 = Source2.GetRowCached(Source2Location.Y);
                IBitmapDataRowInternal rowDst = Target.GetRowCached(TargetLocation.Y);
                for (int y = 0; y < Size.Height; y++)
                {
                    if (context.IsCancellationRequested)
                        return;

                    // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                    for (int x = 0; x < Size.Width; x++)
                        rowDst.DoSetColor32(x + TargetLocation.X, ditheringSession.GetDitheredColor(combineFunction.Invoke(rowSrc1.DoGetColor32(x + Source1Location.X), rowSrc2.DoGetColor32(x + Source2Location.X)), x, y));

                    rowSrc1.MoveNextRow();
                    rowSrc2.MoveNextRow();
                    rowDst.MoveNextRow();
                    context.Progress?.Increment();
                }

                return;
            }

            // Parallel processing
            IBitmapDataInternal source1 = Source1;
            IBitmapDataInternal source2 = Source2;
            IBitmapDataInternal target = Target;
            Point source1Location = Source1Location;
            Point source2Location = Source2Location;
            Point targetLocation = TargetRectangle.Location;
            int sizeWidth = Size.Width;

            ParallelHelper.For(context, DrawingOperation.ProcessingPixels, 0, Size.Height, ProcessRow);

            #region Local Methods

            [SecuritySafeCritical]
            void ProcessRow(int y)
            {
                IDitheringSession session = ditheringSession;
                IBitmapDataRowInternal rowSrc1 = source1.GetRowCached(source1Location.Y + y);
                IBitmapDataRowInternal rowSrc2 = source2.GetRowCached(source2Location.Y + y);
                IBitmapDataRowInternal rowDst = target.GetRowCached(targetLocation.Y + y);
                int offsetSrc1 = source1Location.X;
                int offsetSrc2 = source2Location.X;
                int offsetDst = targetLocation.X;
                int width = sizeWidth;
                Func<Color32, Color32, Color32> f = combineFunction;

                // we can pass x, y to the dithering session because if there is an offset it was initialized by a properly clipped rectangle
                for (int x = 0; x < width; x++)
                    rowDst.DoSetColor32(x + offsetDst, session.GetDitheredColor(f.Invoke(rowSrc1.DoGetColor32(x + offsetSrc1), rowSrc2.DoGetColor32(x + offsetSrc2)), x, y));
            }

            #endregion
        }

        #endregion

        #endregion
    }
}
