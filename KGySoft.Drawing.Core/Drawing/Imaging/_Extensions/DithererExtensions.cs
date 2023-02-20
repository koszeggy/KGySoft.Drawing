#if !NET35
#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DithererExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Threading.Tasks;

using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Contains extension methods for the <see cref="IDitherer"/> type.
    /// </summary>
    public static class DithererExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets an <see cref="IDitheringSession"/> instance potentially asynchronously that can be used to dither the result of the specified <see cref="IQuantizingSession"/>
        /// applied to the specified <paramref name="source"/>.
        /// If <paramref name="ditherer"/> is a known ditherer that can be evaluated quickly, then this method might be executed synchronously.
        /// <br/>This method is available in.NET Framework 4.0 and above.
        /// </summary>
        /// <param name="ditherer">An <see cref="IDitherer"/> instance to get an <see cref="IDitheringSession"/> for.</param>
        /// <param name="source">The dithering session to be initialized will be performed on the specified <see cref="IReadableBitmapData"/> instance.</param>
        /// <param name="quantizingSession">The <see cref="IQuantizingSession"/> to which the dithering should be applied.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as parallelization, cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. Its result is an <see cref="IDitheringSession"/> instance that can be used to dither the result of the specified <see cref="IQuantizingSession"/>
        /// applied to the specified <paramref name="source"/>, or <see langword="null"/>, if the operation was canceled and
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">The non-canceled <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method returned <see langword="null"/>.</exception>
        public static Task<IDitheringSession?> InitializeAsync(this IDitherer ditherer, IReadableBitmapData source, IQuantizingSession quantizingSession, TaskConfig? asyncConfig = null)
            // Actually every ditherer in this assembly is known to be fast-initialized (none of them use the IAsyncContext parameter)
            => ditherer is OrderedDitherer or ErrorDiffusionDitherer or RandomNoiseDitherer or InterleavedGradientNoiseDitherer
                ? AsyncHelper.FromResult(ditherer.Initialize(source, quantizingSession), asyncConfig)
                : AsyncHelper.DoOperationAsync(ctx => DoInitializeSessionAsync(ctx, ditherer, source, quantizingSession), asyncConfig);

        #endregion

        #region Private Methods

        private static IDitheringSession? DoInitializeSessionAsync(IAsyncContext context, IDitherer ditherer, IReadableBitmapData source, IQuantizingSession quantizingSession)
        {
            if (context.IsCancellationRequested)
                return null;
            IDitheringSession result = ditherer.Initialize(source, quantizingSession, context);
            if (context.IsCancellationRequested)
                return null;
            if (result == null)
                throw new InvalidOperationException(Res.ImagingDithererInitializeNull);
            return result;
        }

        #endregion

        #endregion
    }
}
#endif