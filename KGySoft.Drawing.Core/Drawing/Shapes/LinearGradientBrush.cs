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
    internal sealed class LinearGradientBrush : TextureBasedBrush<LinearGradientBrush.OffsetMapper>
    {
        #region Nested Structs

        #region TextureMapperOffset struct

        internal struct OffsetMapper : ITextureMapper
        {
            #region Fields

            private Point offset;

            #endregion

            #region Methods

            public void InitTexture(IBitmapDataInternal texture, Point textureOffset) => offset = textureOffset;
            public int MapY(int y) => y + offset.Y;
            public int MapX(int x) => x + offset.X;

            #endregion
        }

        #endregion

        #region StoppingInterpolation struct

        private struct StoppingInterpolation : IInterpolation
        {
            #region Methods
            
            public float GetValue(float value) => value < 0f ? 0f : value > 1f ? 1f : value;

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

        private readonly Color32 startColor;
        private readonly Color32 endColor;
        private readonly GradientMapMode mapMode;
        private readonly WorkingColorSpace workingColorSpace;
        private readonly IBitmapDataInternal? texture;

        #endregion

        #region Properties

        private protected override bool HasAlpha { get; }

        #endregion

        #region Constructors

        private LinearGradientBrush(PointF startPoint, PointF endPoint, Color32 startColor, Color32 endColor, GradientMapMode mapMode, WorkingColorSpace workingColorSpace)
        {
            this.startColor = startColor;
            this.endColor = endColor;
            this.mapMode = mapMode;
            this.workingColorSpace = workingColorSpace;
            HasAlpha = startColor.A != 255 || endColor.A != 255 || mapMode == GradientMapMode.Clip;

            // the actual size does not matter because as an IBitmapDataInternal any pixel coordinate can be read from GradientBitmapData
            var size = new Size(1, 1);
            texture = mapMode switch
            {
                GradientMapMode.Stop => new GradientBitmapData<StoppingInterpolation>(size, startPoint, endPoint, startColor, endColor, workingColorSpace),
                GradientMapMode.Clip => new GradientBitmapData<ClippingInterpolation>(size, startPoint, endPoint, startColor, endColor, workingColorSpace),
                GradientMapMode.Repeat => new GradientBitmapData<RepeatingInterpolation>(size, startPoint, endPoint, startColor, endColor, workingColorSpace),
                GradientMapMode.Mirror => new GradientBitmapData<MirroringInterpolation>(size, startPoint, endPoint, startColor, endColor, workingColorSpace),
                _ => throw new ArgumentOutOfRangeException(PublicResources.EnumOutOfRange(mapMode))
            };
        }

        #endregion

        #region Methods

        #region Static Methods

        internal static TextureBasedBrush Create(PointF startPoint, PointF endPoint, Color32 startColor, Color32 endColor, GradientMapMode mapMode, WorkingColorSpace workingColorSpace)
        {
            // if horizontal or vertical: TODO return new TextureBrush<Horizontal/VerticalGradient>(gradientTexture, hasAlphaHint, mapMode)
            return new LinearGradientBrush(startPoint, endPoint, startColor, endColor, mapMode, workingColorSpace);
        }

        #endregion

        private protected override IBitmapDataInternal GetTexture(IAsyncContext context, RawPath rawPath, DrawingOptions drawingOptions, out bool disposeTexture, out Point offset)
        {
            if (texture != null)
            {
                disposeTexture = false;
                offset = Point.Empty;
                return texture;
            }

            // TODO: dynamic gradient for path
            throw new NotImplementedException();
        }

        #endregion
    }
}
