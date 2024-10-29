#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GradientBitmapData.cs
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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a read-only bitmap data of a gradient.
    /// As a public instance should be exposed as an <see cref="IReadableBitmapData"/>.
    /// As an <see cref="IBitmapDataInternal"/> it allows reading any pixel coordinates.
    /// </summary>
    internal sealed class GradientBitmapData<TInterpolation> : BitmapDataBase
        where TInterpolation : struct, IInterpolation
    {
        #region Row class

        private sealed class Row : BitmapDataRowBase
        {
            #region Methods

            #region Public Methods

            public override Color32 DoGetColor32(int x) => ((GradientBitmapData<TInterpolation>)BitmapData).DoGetColor32(x, Index);
            public override PColor32 DoGetPColor32(int x) => ((GradientBitmapData<TInterpolation>)BitmapData).DoGetPColor32(x, Index);
            public override void DoSetColor32(int x, Color32 c) => throw new NotSupportedException(PublicResources.NotSupported);
            [SecurityCritical] public override T DoReadRaw<T>(int x) => throw new NotSupportedException(PublicResources.NotSupported);
            [SecurityCritical] public override void DoWriteRaw<T>(int x, T data) => throw new NotSupportedException(PublicResources.NotSupported);

            #endregion

            #region Protected Methods

            protected override void DoMoveToIndex()
            {
            }

            #endregion

            #endregion
        }

        #endregion

        #region Fields

        // the colors here are actually in the working color space of the bitmap
        private readonly ColorF startColor;
        private readonly ColorF endColor;
        private readonly float rotationX;
        private readonly float rotationY;
        private readonly float start;
        private readonly float end;

        #endregion

        #region Constructors

        internal GradientBitmapData(Size size, PointF startPoint, PointF endPoint, ColorF startColor, ColorF endColor, bool isLinearColorSpace)
            : base(new BitmapDataConfig(size, KnownPixelFormat.Format128bppRgba.ToInfoInternal(), workingColorSpace: isLinearColorSpace ? WorkingColorSpace.Linear : WorkingColorSpace.Srgb))
        {
            // NOTE: The colors here are expected in the specified color space. If it was a public constructor, it would be cleaner if ColorF values were always
            // in the linear color space, but it would be slower because of the extra back-and-forth conversions (e.g. when a Color32 was specified originally
            // in the public factory method). The BitmapData/Row.GetColorF methods will always return linear color space results.
            this.startColor = startColor;
            this.endColor = endColor;

            // Using double for the partial results matters when using 90 or 180 degrees, because in radians they are not exactly representable.
            // (Only horizontal and vertical gradients with integer start/end points have optimized cases in LinearGradientBrush).
            double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
            rotationX = (float)Math.Cos(angle);
            rotationY = (float)Math.Sin(angle);
            start = startPoint.X * rotationX + startPoint.Y * rotationY;
            end = endPoint.X * rotationX + endPoint.Y * rotationY;
        }

        #endregion

        #region Methods

        #region Public Methods

        public override Color32 DoGetColor32(int x, int y) => DoGetColor(x, y).ToColor32(LinearWorkingColorSpace);
        public override Color64 DoGetColor64(int x, int y) => DoGetColor(x, y).ToColor64(LinearWorkingColorSpace);
        public override PColor64 DoGetPColor64(int x, int y) => DoGetColor(x, y).ToPColor64(LinearWorkingColorSpace);
        public override PColorF DoGetPColorF(int x, int y) => DoGetColor(x, y).ToPColorF(LinearWorkingColorSpace);
        public override ColorF DoGetColorF(int x, int y) => DoGetColor(x, y).ToColorF(LinearWorkingColorSpace);

        public override void DoSetColor32(int x, int y, Color32 c) => throw new NotSupportedException(PublicResources.NotSupported);
        [SecurityCritical]public override T DoReadRaw<T>(int x, int y) => throw new NotSupportedException(PublicResources.NotSupported);
        [SecurityCritical]public override void DoWriteRaw<T>(int x, int y, T data) => throw new NotSupportedException(PublicResources.NotSupported);

        #endregion

        #region Protected Methods

        protected override IBitmapDataRowInternal DoGetRow(int y) => new Row
        {
            BitmapData = this,
            Index = y,
        };

        #endregion

        #region Private Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private ColorF DoGetColor(int x, int y)
        {
            // Calculating the current position on the gradient. Using double for the intermediate steps to avoid precision issues (observable with
            // clipping interpolation). Would not be necessary if the range was normalized between (-1, 1) but the extra calculation would be slower.
            float current = (float)((double)x * rotationX + (double)y * rotationY);
            float pos = default(TInterpolation).GetValue((start - current) / (start - end));
            return startColor.Interpolate(endColor, pos);
        }

        #endregion

        #endregion

    }
}
