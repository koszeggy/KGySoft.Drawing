#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DrawingOptions.cs
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

using KGySoft.CoreLibraries;
using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Shapes
{
    public sealed class DrawingOptions : IEquatable<DrawingOptions>
    {
        #region Fields

        #region Static Fields

        internal static readonly DrawingOptions Default = new DrawingOptions();
        internal static readonly DrawingOptions DefaultNonZero = new DrawingOptions { FillMode = ShapeFillMode.NonZero };

        #endregion

        #region Instance Fields

        private TransformationMatrix transformation;
        private ShapeFillMode fillMode;
        private PixelOffset scanPathPixelOffset = Shapes.PixelOffset.Half;
        private PixelOffset drawPathPixelOffset;

        #endregion

        #endregion

        #region Properties

        #region Public Properties
        
        // If not the identity matrix, it disables path region caching.
        // If you intend to use the same path with the same orientation, then apply the transformation to a cached Path instance, and use the identity matrix here instead.
        public TransformationMatrix Transformation
        {
            get => transformation;
            set => transformation = value;
        }

        public bool AntiAliasing { get; set; }

        public bool FastThinLines { get; set; } = true;

        public bool AlphaBlending { get; set; } // If false, alpha is written directly, which is usually not quite good with AntiAliasing also enabled, except for already transparent background docs: color space: target's WorkingColorSpace or Quantizer

        public ShapeFillMode FillMode
        {
            get => fillMode;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(PublicResources.EnumOutOfRange(value));
                fillMode = value;
            }
        }

        // TODO: add images
        public PixelOffset ScanPathPixelOffset
        {
            get => scanPathPixelOffset;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(PublicResources.EnumOutOfRange(value));
                scanPathPixelOffset = value;
            }
        }

        public PixelOffset DrawPathPixelOffset
        {
            get => drawPathPixelOffset;
            set
            {
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(PublicResources.EnumOutOfRange(value));
                drawPathPixelOffset = value;
            }
        }

        public IQuantizer? Quantizer { get; set; }

        public IDitherer? Ditherer { get; set; }

        #endregion

        #region Internal Properties

        /// <summary>
        /// To avoid the callers use options.Transformation.IsIdentity, which would copy the matrix.
        /// </summary>
        internal bool IsIdentityTransform => transformation.IsIdentity;

        internal DrawingOptions WithNonZeroFill => FillMode is ShapeFillMode.NonZero ? this
            : Equals(Default) ? DefaultNonZero
            : new DrawingOptions(this) { FillMode = ShapeFillMode.NonZero };

        internal float PixelOffset => DrawPathPixelOffset == Shapes.PixelOffset.Half ? 0.5f : 0f;

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors

        public DrawingOptions()
        {
            transformation = TransformationMatrix.Identity;
            AlphaBlending = true;
        }

        #endregion
        
        #region Private Constructors

        private DrawingOptions(DrawingOptions other)
        {
            transformation = other.transformation;
            AntiAliasing = other.AntiAliasing;
            AlphaBlending = other.AlphaBlending;
            FillMode = other.FillMode;
            scanPathPixelOffset = other.scanPathPixelOffset;
            drawPathPixelOffset = other.drawPathPixelOffset;
            Quantizer = other.Quantizer;
            Ditherer = other.Ditherer;
        }

        #endregion

        #endregion

        #region Methods

        public bool Equals(DrawingOptions? other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Transformation == other.Transformation
                && AntiAliasing == other.AntiAliasing
                && AlphaBlending == other.AlphaBlending
                && FillMode == other.FillMode
                && ScanPathPixelOffset == other.ScanPathPixelOffset
                && DrawPathPixelOffset == other.DrawPathPixelOffset
                && Quantizer == other.Quantizer
                && Ditherer == other.Ditherer;
        }

        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is DrawingOptions other && Equals(other);

        // TODO: do combinations like in PenOptions
        public override int GetHashCode() => (Transformation, AntiAliasing, AlphaBlending, FillMode, ScanPathPixelOffset, DrawPathPixelOffset, Quantizer, Ditherer).GetHashCode();

        #endregion
    }
}
