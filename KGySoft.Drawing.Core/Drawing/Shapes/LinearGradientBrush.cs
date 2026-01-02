#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LinearGradientBrush.cs
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

using System;
using System.Drawing;

using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal sealed class LinearGradientBrush : TextureBasedBrush<LinearGradientBrush.IdentityMapper>
    {
        #region Nested Structs

        internal readonly struct IdentityMapper : ITextureMapper
        {
            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point offset) { }
            public int MapY(int y) => y;
            public int MapX(int x) => x;

            #endregion
        }

        #endregion

        #region Fields

        private readonly float angle;
        private readonly ColorF startColor;
        private readonly ColorF endColor;
        private readonly bool isLinearColorSpace;
        private readonly IBitmapDataInternal? texture;

        #endregion

        #region Properties

        internal override bool HasAlpha { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// This constructor creates a gradient texture with specific start/end points that is not bound to any path.
        /// </summary>
        internal LinearGradientBrush(PointF startPoint, PointF endPoint, ColorF startColor, ColorF endColor, GradientWrapMode wrapMode, bool isLinearColorSpace)
        {
            this.startColor = startColor;
            this.endColor = endColor;
            this.isLinearColorSpace = isLinearColorSpace;
            HasAlpha = startColor.A < 1f || endColor.A < 1f || wrapMode == GradientWrapMode.Clip;

            // the actual size does not matter because as an IBitmapDataInternal any pixel coordinate can be read without range check
            var size = new Size(1, 1);
            texture = wrapMode switch
            {
                GradientWrapMode.Stop => new GradientBitmapData<StoppingInterpolation>(size, startPoint, endPoint, startColor, endColor, isLinearColorSpace),
                GradientWrapMode.Clip => new GradientBitmapData<ClippingInterpolation>(size, startPoint, endPoint, startColor, endColor, isLinearColorSpace),
                GradientWrapMode.Repeat => new GradientBitmapData<RepeatingInterpolation>(size, startPoint, endPoint, startColor, endColor, isLinearColorSpace),
                GradientWrapMode.Mirror => new GradientBitmapData<MirroringInterpolation>(size, startPoint, endPoint, startColor, endColor, isLinearColorSpace),
                _ => throw new ArgumentOutOfRangeException(PublicResources.EnumOutOfRange(wrapMode))
            };
        }

        /// <summary>
        /// This constructor is to create a gradient with a specific angle that will be applied to each path.
        /// </summary>
        internal LinearGradientBrush(float angle, ColorF startColor, ColorF endColor, bool isLinearColorSpace)
        {
            this.angle = angle;
            this.startColor = startColor;
            this.endColor = endColor;
            this.isLinearColorSpace = isLinearColorSpace;
            HasAlpha = startColor.A < 1f || endColor.A < 1f;
        }

        #endregion

        #region Methods

        private protected override IBitmapDataInternal GetTexture(IAsyncContext context, RawPath rawPath, DrawingOptions drawingOptions, out bool disposeTexture, out Point offset)
        {
            offset = default;

            // If the texture was created by the corresponding constructor, we can return it directly
            if (texture != null)
            {
                disposeTexture = false;
                return texture;
            }

            // though it will not contain anything to dispose...
            disposeTexture = true;

            // Here we generate a gradient specifically for the path so it perfectly completely covers it, without exceeding its bounds.
            return BitmapDataFactory.CreateLinearGradientBitmapData(rawPath.Bounds, angle, startColor, endColor, isLinearColorSpace);
        }

        #endregion
    }
}
