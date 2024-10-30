#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LinearGradientBrush.cs
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

using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal sealed class LinearGradientBrush : TextureBasedBrush<LinearGradientBrush.IdentityMapper>
    {
        #region Nested Structs

        #region IdentityMapper struct

        internal struct IdentityMapper : ITextureMapper
        {
            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point offset) { }
            public int MapY(int y) => y;
            public int MapX(int x) => x;

            #endregion
        }

        #endregion

        #region StoppingInterpolation struct

        private struct StoppingInterpolation : IInterpolation
        {
            #region Methods
            
            public float GetValue(float value) => value <= 0f ? 0f : value >= 1f ? 1f : value;

            #endregion
        }

        #endregion

        #region ClippingInterpolation struct

        private struct ClippingInterpolation : IInterpolation
        {
            #region Methods
            
            public float GetValue(float value) => value is < 0f or > 1f ? Single.NaN : value;
        
            #endregion
        }

        #endregion

        #region RepeatingInterpolation struct

        private struct RepeatingInterpolation : IInterpolation
        {
            #region Methods
            
            public float GetValue(float value) => value - MathF.Floor(value);

            #endregion
        }

        #endregion

        #region MirroringInterpolation struct

        private struct MirroringInterpolation : IInterpolation
        {
            #region Methods
            
            public float GetValue(float value)
            {
                value = Math.Abs(value) % 2f;
                return value > 1f ? 2f - value : value;
            }

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly float angle;
        private readonly ColorF startColor;
        private readonly ColorF endColor;
        private readonly bool isLinearColorSpace;
        private readonly IBitmapDataInternal? texture;

        #endregion

        #region Properties

        private protected override bool HasAlpha { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// This constructor creates a gradient texture with specific start/end points that is not bound to any path.
        /// </summary>
        internal LinearGradientBrush(PointF startPoint, PointF endPoint, ColorF startColor, ColorF endColor, GradientMapMode mapMode, bool isLinearColorSpace)
        {
            this.startColor = startColor;
            this.endColor = endColor;
            this.isLinearColorSpace = isLinearColorSpace;
            HasAlpha = startColor.A < 1f || endColor.A < 1f || mapMode == GradientMapMode.Clip;

            // the actual size does not matter because as an IBitmapDataInternal any pixel coordinate can be read without range check
            var size = new Size(1, 1);
            texture = mapMode switch
            {
                GradientMapMode.Stop => new GradientBitmapData<StoppingInterpolation>(size, startPoint, endPoint, startColor, endColor, isLinearColorSpace),
                GradientMapMode.Clip => new GradientBitmapData<ClippingInterpolation>(size, startPoint, endPoint, startColor, endColor, isLinearColorSpace),
                GradientMapMode.Repeat => new GradientBitmapData<RepeatingInterpolation>(size, startPoint, endPoint, startColor, endColor, isLinearColorSpace),
                GradientMapMode.Mirror => new GradientBitmapData<MirroringInterpolation>(size, startPoint, endPoint, startColor, endColor, isLinearColorSpace),
                _ => throw new ArgumentOutOfRangeException(PublicResources.EnumOutOfRange(mapMode))
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
            #region Local Methods

            static float ProjectPointOntoLine(PointF point, PointF direction) => point.X * direction.X + point.Y * direction.Y;

            #endregion

            offset = default;

            // If the texture is already created, we can return it directly
            if (texture != null)
            {
                disposeTexture = false;
                return texture;
            }

            // Here we generate a gradient specifically for the path so it perfectly completely covers it, without exceeding its bounds.
            // We could do a shortcut for horizontal and vertical gradients but they are handled in the factory method anyway.
            Rectangle bounds = rawPath.Bounds;
            PointF startPoint = default;
            PointF endPoint = default;
            PointF center = new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f);
            float rad = angle.ToRadian();

            float directionX = (float)Math.Cos(rad);
            float directionY = (float)Math.Sin(rad);

            // Projecting all corners onto the gradient direction and finding min/max projections
            PointF[] corners = new PointF[]
            {
                new PointF(bounds.Left, bounds.Top),
                new PointF(bounds.Right, bounds.Top),
                new PointF(bounds.Right, bounds.Bottom),
                new PointF(bounds.Left, bounds.Bottom)
            };

            float minProjection = float.MaxValue;
            float maxProjection = float.MinValue;
            foreach (var corner in corners)
            {
                float projection = ProjectPointOntoLine(new PointF(corner.X - center.X, corner.Y - center.Y), new PointF(directionX, directionY));
                if (projection < minProjection)
                {
                    minProjection = projection;
                    startPoint = new PointF(center.X + projection * directionX, center.Y + projection * directionY);
                }

                if (projection > maxProjection)
                {
                    maxProjection = projection;
                    endPoint = new PointF(center.X + projection * directionX, center.Y + projection * directionY);
                }
            }

            // though it doesn't contain anything to dispose...
            disposeTexture = true;

            // actually any size would do it, because as an IBitmapDataInternal any pixel coordinate can be read without range check
            return new GradientBitmapData<StoppingInterpolation>(bounds.Size, startPoint, endPoint, startColor, endColor, isLinearColorSpace);
        }

        #endregion
    }
}
