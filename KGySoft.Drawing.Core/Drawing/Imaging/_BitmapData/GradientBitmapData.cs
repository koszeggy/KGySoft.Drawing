#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GradientBitmapData.cs
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
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a read-only bitmap data of a gradient.
    /// As a public instance should be exposed as an <see cref="IReadableBitmapData"/>.
    /// As an <see cref="IBitmapDataInternal"/> it allows reading any pixel coordinates, regardless of the actual size.
    /// </summary>
    internal sealed class GradientBitmapData<TInterpolation> : BitmapDataBase
        where TInterpolation : struct, IInterpolation
    {
        #region Row class

        private sealed class Row : BitmapDataRowBase
        {
            #region Fields

            internal double CurrentY;

            #endregion

            #region Properties

            private new GradientBitmapData<TInterpolation> BitmapData => (GradientBitmapData<TInterpolation>)base.BitmapData;

            #endregion

            #region Methods

            #region Public Methods

            [SecurityCritical]public override Color32 DoGetColor32(int x) => DoGetColor(x).ToColor32(BitmapData.LinearWorkingColorSpace);
            [SecurityCritical]public override Color64 DoGetColor64(int x) => DoGetColor(x).ToColor64(BitmapData.LinearWorkingColorSpace);
            [SecurityCritical]public override PColor64 DoGetPColor64(int x) => DoGetColor(x).ToPColor64(BitmapData.LinearWorkingColorSpace);
            [SecurityCritical]public override ColorF DoGetColorF(int x) => DoGetColor(x).ToColorF(BitmapData.LinearWorkingColorSpace);
            [SecurityCritical]public override PColorF DoGetPColorF(int x) => DoGetColor(x).ToPColorF(BitmapData.LinearWorkingColorSpace);
            [SecurityCritical]public override void DoSetColor32(int x, Color32 c) => throw new NotSupportedException(PublicResources.NotSupported);
            [SecurityCritical]public override T DoReadRaw<T>(int x) => throw new NotSupportedException(PublicResources.NotSupported);
            [SecurityCritical]public override void DoWriteRaw<T>(int x, T data) => throw new NotSupportedException(PublicResources.NotSupported);

            #endregion

            #region Protected Methods

            protected override void DoMoveToIndex() => CurrentY = (double)Index * BitmapData.RotationY;

            #endregion

            #region Private Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            private ColorF DoGetColor(int x)
            {
                // The same logic as in GradientBitmapData.DoGetColor, see the comments there.
                var bitmap = BitmapData;
                float current = (float)((double)x * bitmap.RotationX + CurrentY);
                float pos = default(TInterpolation).GetValue((bitmap.Start - current) / (bitmap.Start - bitmap.End));
                return bitmap.StartColor.Interpolate(bitmap.EndColor, pos);
            }

            #endregion

            #endregion
        }

        #endregion

        #region Fields

        // the colors here are actually in the working color space of the bitmap
        internal readonly ColorF StartColor;
        internal readonly ColorF EndColor;
        internal readonly float RotationX;
        internal readonly float RotationY;
        internal readonly float Start;
        internal readonly float End;

        #endregion

        #region Constructors

        internal GradientBitmapData(Size size, PointF startPoint, PointF endPoint, ColorF startColor, ColorF endColor, bool isLinearColorSpace)
            : base(new BitmapDataConfig(size, KnownPixelFormat.Format128bppRgba.ToInfoInternal(), workingColorSpace: isLinearColorSpace ? WorkingColorSpace.Linear : WorkingColorSpace.Srgb))
        {
            // NOTE: The colors here are expected in the specified color space. If it was a public constructor, it would be cleaner if ColorF values were always
            // in the linear color space, but it would be slower because of the extra back-and-forth conversions (e.g. when a Color32 was specified originally
            // in the public factory method). The BitmapData/Row.GetColorF methods will always return linear color space results.
            StartColor = startColor;
            EndColor = endColor;

            // Using double for the partial results matters when using 90 or 180 degrees, because in radians they are not exactly representable.
            double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
            RotationX = (float)Math.Cos(angle);
            RotationY = (float)Math.Sin(angle);
            Start = startPoint.X * RotationX + startPoint.Y * RotationY;
            End = endPoint.X * RotationX + endPoint.Y * RotationY;
        }

        #endregion

        #region Methods

        #region Public Methods

        [SecurityCritical]public override Color32 DoGetColor32(int x, int y) => DoGetColor(x, y).ToColor32(LinearWorkingColorSpace);
        [SecurityCritical]public override Color64 DoGetColor64(int x, int y) => DoGetColor(x, y).ToColor64(LinearWorkingColorSpace);
        [SecurityCritical]public override PColor64 DoGetPColor64(int x, int y) => DoGetColor(x, y).ToPColor64(LinearWorkingColorSpace);
        [SecurityCritical]public override PColorF DoGetPColorF(int x, int y) => DoGetColor(x, y).ToPColorF(LinearWorkingColorSpace);
        [SecurityCritical]public override ColorF DoGetColorF(int x, int y) => DoGetColor(x, y).ToColorF(LinearWorkingColorSpace);

        [SecurityCritical]public override void DoSetColor32(int x, int y, Color32 c) => throw new NotSupportedException(PublicResources.NotSupported);
        [SecurityCritical]public override T DoReadRaw<T>(int x, int y) => throw new NotSupportedException(PublicResources.NotSupported);
        [SecurityCritical]public override void DoWriteRaw<T>(int x, int y, T data) => throw new NotSupportedException(PublicResources.NotSupported);

        #endregion

        #region Protected Methods

        protected override IBitmapDataRowInternal DoGetRow(int y) => new Row
        {
            BitmapData = this,
            Index = y,
            CurrentY = (double)y * RotationY
        };

        #endregion

        #region Private Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private ColorF DoGetColor(int x, int y)
        {
            // Calculating the current position on the gradient. Using double for the intermediate steps to avoid precision issues (observable with
            // clipping interpolation). Would not be necessary if the range was normalized between (-1, 1) but the extra calculation would be slower.
            float current = (float)((double)x * RotationX + (double)y * RotationY);
            float pos = default(TInterpolation).GetValue((Start - current) / (Start - End));
            return StartColor.Interpolate(EndColor, pos);
        }

        #endregion

        #endregion

    }
}
