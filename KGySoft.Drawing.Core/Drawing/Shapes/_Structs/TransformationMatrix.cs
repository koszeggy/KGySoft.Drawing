#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TransformationMatrix.cs
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
using System.Numerics;

#endregion

namespace KGySoft.Drawing.Shapes
{
    public struct TransformationMatrix : IEquatable<TransformationMatrix>
    {
        #region Fields

        #region Static Fields

        public static readonly TransformationMatrix Identity = new TransformationMatrix(Matrix3x2.Identity);

        #endregion

        #region Instance Fields

        private Matrix3x2 matrix;

        #endregion

        #endregion

        #region Operators

        public static bool operator ==(TransformationMatrix a, TransformationMatrix b) => a.Equals(b);

        public static bool operator !=(TransformationMatrix a, TransformationMatrix b) => !(a == b);

        #endregion

        #region Constructors

        public TransformationMatrix(Matrix3x2 matrix)
        {
            this.matrix = matrix;
        }

        #endregion

        #region Methods

        public bool Equals(TransformationMatrix other)
        {
            return matrix.Equals(other.matrix);
        }

        public override bool Equals(object? obj)
        {
            return obj is TransformationMatrix other && Equals(other);
        }

        public override int GetHashCode()
        {
            return matrix.GetHashCode();
        }

        #endregion
    }
}