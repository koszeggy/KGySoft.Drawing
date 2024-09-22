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

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Shapes
{
    public sealed class DrawingOptions : IEquatable<DrawingOptions>
    {
        #region Fields

        #region Static Fields
        
        internal static readonly DrawingOptions Default = new DrawingOptions();

        #endregion

        #region Instance Fields

        private TransformationMatrix transformation;

        #endregion

        #endregion

        #region Properties

        #region Public Properties
        
        // If not the identity matrix, it disables path region caching.
        // If you intend to use the same path with the same orientation, then apply the transformation to a cached Path instance, and use the identity matrix here intead.
        public TransformationMatrix Transformation
        {
            get => transformation;
            set => transformation = value;
        }

        public bool AntiAliasing { get; set; }

        public bool AlphaBlending { get; set; } // If false, alpha is written directly, which is usually not quite good with AntiAliasing also enabled, except for already transparent background docs: color space: target's WorkingColorSpace or Quantizer

        public IQuantizer? Quantizer { get; set; }

        public IDitherer? Ditherer { get; set; }

        public ShapeFillMode FillMode { get; set; }

        #endregion

        #region Internal Properties

        // To avoid using options.Transformation.IsIdentity, which would copy the matrix.
        internal bool IsIdentityTransform => transformation.IsIdentity;

        #endregion

        #endregion

        #region Constructors

        public DrawingOptions()
        {
            transformation = TransformationMatrix.Identity;
            AlphaBlending = true;
        }

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
                && Quantizer == other.Quantizer
                && Ditherer == other.Ditherer;
        }

        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is DrawingOptions other && Equals(other);

        public override int GetHashCode() => (Transformation, AntiAliasing, AlphaBlending, FillMode, Quantizer, Ditherer).GetHashCode();

        #endregion
    }
}
