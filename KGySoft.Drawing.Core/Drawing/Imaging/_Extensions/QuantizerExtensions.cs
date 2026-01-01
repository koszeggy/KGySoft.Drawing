#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: QuantizerExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

#if !NET35
using System; 
#endif
using System.Drawing;
#if !NET35
using System.Threading.Tasks;
#endif

#if !NET35
using KGySoft.Threading;
#endif

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Contains extension methods for the <see cref="IQuantizer"/> type.
    /// </summary>
    public static class QuantizerExtensions
    {
        #region Methods

        #region Public Methods
#if !NET35

        /// <summary>
        /// Gets an <see cref="IQuantizingSession"/> instance potentially asynchronously that can be used to quantize the colors of the specified <see cref="IReadableBitmapData"/> instance.
        /// If <paramref name="quantizer"/> is a known quantizer that can be evaluated quickly, then this method might be executed synchronously.
        /// <br/>This method is available in.NET Framework 4.0 and above.
        /// </summary>
        /// <param name="quantizer">An <see cref="IQuantizer"/> instance to get an <see cref="IQuantizingSession"/> for.</param>
        /// <param name="source">The quantizing session to be initialized will be performed on the specified <see cref="IReadableBitmapData"/> instance.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="IQuantizingSession"/> instance that can be used to quantize the colors of the specified <see cref="IReadableBitmapData"/> instance
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://koszeggy.github.io/docs/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">The non-canceled <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method returned <see langword="null"/>.</exception>
        public static Task<IQuantizingSession?> InitializeAsync(this IQuantizer quantizer, IReadableBitmapData source, TaskConfig? asyncConfig = null)
            // PredefinedColorsQuantizer is known to be fast so initializing it synchronously
            => quantizer is PredefinedColorsQuantizer
                ? AsyncHelper.FromResult(quantizer.Initialize(source), asyncConfig)
                : AsyncHelper.DoOperationAsync(ctx => DoInitializeSessionAsync(ctx, source, quantizer), asyncConfig);

#endif
        #endregion

        #region Internal Methods

        internal static WorkingColorSpace WorkingColorSpace(this IQuantizer? quantizer)
        {
            switch (quantizer)
            {
                case null:
                    return Imaging.WorkingColorSpace.Default;
                case PredefinedColorsQuantizer predefined:
                    return predefined.WorkingColorSpace;
                case OptimizedPaletteQuantizer optimized:
                    return optimized.WorkingColorSpace;
                default:
                    // not a built-in one: testing with a single pixel bitmap
                    using (var session = quantizer.Initialize(new SolidBitmapData(new Size(1, 1), default)))
                        return session.WorkingColorSpace;
            }
        }

        #endregion

        #region Private Methods
#if !NET35
        
        private static IQuantizingSession? DoInitializeSessionAsync(IAsyncContext context, IReadableBitmapData source, IQuantizer quantizer)
        {
            if (context.IsCancellationRequested)
                return null;
            IQuantizingSession result = quantizer.Initialize(source, context);
            if (context.IsCancellationRequested)
                return null;
            if (result == null)
                throw new InvalidOperationException(Res.ImagingQuantizerInitializeNull);
            return result;
        }

#endif
        #endregion

        #endregion
    }
}
